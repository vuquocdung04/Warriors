using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Equipment Database")]
public class EquipmentDatabase : ScriptableObject
{
    [Header("Equipment (3 loại)")]
    public TextAsset meleeJson;
    public TextAsset rangeJson;
    public TextAsset shieldJson;

    [Header("Config")]
    public TextAsset statJson;
    public TextAsset rankJson;
    public TextAsset gachaLevelJson;
    public TextAsset gachaRateJson;

    private Dictionary<string, EquipmentData> _melee;
    private Dictionary<string, EquipmentData> _range;
    private Dictionary<string, EquipmentData> _shield;
    private Dictionary<string, StatData> _stats;
    private Dictionary<string, RankData> _ranks;
    private Dictionary<int, GachaLevelData> _gachaLevels;
    private Dictionary<int, GachaRateData> _gachaRates;

    public void Init()
    {
        _melee = LoadEquip(meleeJson);
        _range = LoadEquip(rangeJson);
        _shield = LoadEquip(shieldJson);

        _stats = new Dictionary<string, StatData>();
        foreach (var s in Load<StatData>(statJson))
            if (!string.IsNullOrEmpty(s.id)) _stats[s.id] = s;

        _ranks = new Dictionary<string, RankData>();
        foreach (var r in Load<RankData>(rankJson))
            if (!string.IsNullOrEmpty(r.rank)) _ranks[r.rank] = r;

        _gachaLevels = new Dictionary<int, GachaLevelData>();
        foreach (var g in Load<GachaLevelData>(gachaLevelJson))
            _gachaLevels[g.level] = g;

        _gachaRates = new Dictionary<int, GachaRateData>();
        foreach (var g in Load<GachaRateData>(gachaRateJson))
            _gachaRates[g.level] = g;
    }

    Dictionary<string, EquipmentData> LoadEquip(TextAsset json)
    {
        var dict = new Dictionary<string, EquipmentData>();
        foreach (var e in Load<EquipmentData>(json))
            if (!string.IsNullOrEmpty(e.id)) dict[e.id] = e;
        return dict;
    }

    List<T> Load<T>(TextAsset json)
    {
        if (json == null) { Debug.LogError("[EquipDB] thiếu json"); return new List<T>(); }
        return JsonConvert.DeserializeObject<List<T>>(json.text) ?? new List<T>();
    }

    // ---- TRA CỨU ----
    public EquipmentData GetMelee(string id) => Get(_melee, id);
    public EquipmentData GetRange(string id) => Get(_range, id);
    public EquipmentData GetShield(string id) => Get(_shield, id);

    public List<EquipmentData> AllMelee() => new List<EquipmentData>(_melee.Values);
    public List<EquipmentData> AllRange() => new List<EquipmentData>(_range.Values);
    public List<EquipmentData> AllShield() => new List<EquipmentData>(_shield.Values);

    public StatData GetStat(string id) => Get(_stats, id);
    public RankData GetRank(string rank) => Get(_ranks, rank);
    public GachaLevelData GetGachaLevel(int level) => _gachaLevels.TryGetValue(level, out var g) ? g : null;
    public GachaRateData GetGachaRate(int level) => _gachaRates.TryGetValue(level, out var g) ? g : null;

    static EquipmentData Get(Dictionary<string, EquipmentData> d, string id)
        => d != null && d.TryGetValue(id, out var v) ? v : null;
    static StatData Get(Dictionary<string, StatData> d, string id)
        => d != null && d.TryGetValue(id, out var v) ? v : null;
    static RankData Get(Dictionary<string, RankData> d, string id)
        => d != null && d.TryGetValue(id, out var v) ? v : null;



    // thêm vào EquipmentDatabase

    // 3 unit của civ -> 3 stat đã cộng equipment đang đeo
    public List<UnitCombatStats> BuildStats(string civId)
    {
        var units = DataRepo.Instance.unitDatabase.GetCivUnits(civId);   // [0]=melee [1]=range [2]=shield
        var result = new List<UnitCombatStats>();

        string[] equippedIds =
        {
        UseProfile.EquippedMelee.Value,
        UseProfile.EquippedRange.Value,
        UseProfile.EquippedShield.Value,
    };
        EquipmentData[] equips =
        {
        GetMelee(equippedIds[0]),
        GetRange(equippedIds[1]),
        GetShield(equippedIds[2]),
    };

        for (int i = 0; i < units.Count; i++)
        {
            var stats = new UnitCombatStats(units[i]);
            if (i < equips.Length && equips[i] != null)
                ApplyEquipment(stats, equips[i]);
            result.Add(stats);
        }
        return result;
    }

    void ApplyEquipment(UnitCombatStats stats, EquipmentData equip)
    {
        int level = EquipmentSave.Get(equip.id).level;   // level món hiện tại

        // gom flat & percent theo stat id
        var flat = new Dictionary<string, float>();
        var percent = new Dictionary<string, float>();

        foreach (var s in equip.stats)
        {
            if (level < s.levelUnlock) continue;          // stat chưa mở theo level

            int idx = Mathf.Clamp(level - 1, 0, s.levels.Count - 1);
            float value = s.levels[idx];                  // giá trị stat ở level hiện tại

            StatData def = GetStat(s.statType);
            string calc = def != null ? def.calcType : "flat";
            string baseStat = StripSuffix(s.statType);    // atk_percent -> atk

            switch (calc)
            {
                case "percent":
                    percent[baseStat] = (percent.TryGetValue(baseStat, out var p) ? p : 0f) + value;
                    break;
                case "rate":
                    AddRate(stats, s.statType, value);     // cộng thẳng vào tỉ lệ
                    break;
                default: // flat
                    flat[baseStat] = (flat.TryGetValue(baseStat, out var f) ? f : 0f) + value;
                    break;
            }
        }

        // áp flat + percent: final = (base + flat) * (1 + percent)
        stats.atk = Combine(stats.atk, flat, percent, "atk");
        stats.hp = Combine(stats.hp, flat, percent, "hp");
        stats.moveSpeed = Combine(stats.moveSpeed, flat, percent, "move_speed");
        stats.attackSpeed = Combine(stats.attackSpeed, flat, percent, "attack_speed");
        stats.attackRangeInCells = Combine(stats.attackRangeInCells, flat, percent, "attack_range");
    }

    float Combine(float baseVal, Dictionary<string, float> flat, Dictionary<string, float> percent, string key)
    {
        float f = flat.TryGetValue(key, out var fv) ? fv : 0f;
        float p = percent.TryGetValue(key, out var pv) ? pv : 0f;
        return (baseVal + f) * (1f + p / 100f);   // percent dạng 50 = +50%
    }

    void AddRate(UnitCombatStats stats, string statType, float value)
    {
        switch (statType)
        {
            case "critical_chance": stats.criticalChance += value; break;
            case "life_steal": stats.lifeSteal += value; break;
            case "push_chance": stats.pushChance += value; break;
            case "poison_chance": stats.poisonChance += value; break;
            case "burn_chance": stats.burnChance += value; break;
            case "freeze_chance": stats.freezeChance += value; break;
        }
    }

    static string StripSuffix(string statType)   // atk_percent -> atk, hp_percent -> hp
    {
        if (statType.EndsWith("_percent")) return statType.Substring(0, statType.Length - "_percent".Length);
        return statType;
    }

}