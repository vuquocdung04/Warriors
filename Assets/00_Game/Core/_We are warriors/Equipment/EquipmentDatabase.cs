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

    [Header("Item prefabs (UI)")]
    public List<EquipmentItem> meleeItemPrefabs;
    public List<EquipmentItem> rangeItemPrefabs;
    public List<EquipmentItem> shieldItemPrefabs;

    private Dictionary<string, EquipmentItem> _meleeItem;
    private Dictionary<string, EquipmentItem> _rangeItem;
    private Dictionary<string, EquipmentItem> _shieldItem;

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

        _meleeItem = BuildItemDict(meleeItemPrefabs);
        _rangeItem = BuildItemDict(rangeItemPrefabs);
        _shieldItem = BuildItemDict(shieldItemPrefabs);

    }
    Dictionary<string, EquipmentItem> BuildItemDict(List<EquipmentItem> prefabs)
    {
        var dict = new Dictionary<string, EquipmentItem>();
        foreach (var p in prefabs)
        {
            if (p == null) continue;
            if (!string.IsNullOrEmpty(p.id)) dict[p.id] = p;
        }
        return dict;
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
    public List<UnitStats> BuildStats(string civId)
    {
        var units = DataRepo.Instance.unitDatabase.GetCivUnits(civId);
        var result = new List<UnitStats>();

        EquipmentData[] equips =
        {
        GetMelee(UseProfile.EquippedMelee.Value),
        GetRange(UseProfile.EquippedRange.Value),
        GetShield(UseProfile.EquippedShield.Value),
    };

        for (int i = 0; i < units.Count; i++)
        {
            var stats = new UnitStats(units[i]);
            if (i < equips.Length && equips[i] != null) ApplyEquipment(stats, equips[i]);
            stats.maxHp = stats.hp;
            result.Add(stats);
        }
        return result;
    }

    public List<UnitStats> BuildBaseStats(string civId)
    {
        var units = DataRepo.Instance.unitDatabase.GetCivUnits(civId);
        var result = new List<UnitStats>();
        foreach (var u in units)
        {
            var s = new UnitStats(u);
            s.maxHp = s.hp;
            result.Add(s);
        }
        return result;
    }

    void ApplyEquipment(UnitStats stats, EquipmentData equip)
    {
        int level = EquipmentSave.Get(equip.id).level;

        var flat = new Dictionary<string, float>();
        var percent = new Dictionary<string, float>();

        foreach (var s in equip.stats)
        {
            if (level < s.levelUnlock) continue;          // stat chưa mở theo level

            float value = s.value;                         // cố định, không theo level nữa
            StatData def = GetStat(s.statType);
            string calc = def != null ? def.calcType : "flat";
            string baseStat = StripSuffix(s.statType);

            switch (calc)
            {
                case "percent":
                    percent[baseStat] = (percent.TryGetValue(baseStat, out var p) ? p : 0f) + value;
                    break;
                case "rate":
                    AddRate(stats, s.statType, value);
                    break;
                default:
                    flat[baseStat] = (flat.TryGetValue(baseStat, out var f) ? f : 0f) + value;
                    break;
            }
        }

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

    void AddRate(UnitStats stats, string statType, float value)
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
    public EquipmentItem GetMeleeItemPrefab(string id) => Get(_meleeItem, id);
    public EquipmentItem GetRangeItemPrefab(string id) => Get(_rangeItem, id);
    public EquipmentItem GetShieldItemPrefab(string id) => Get(_shieldItem, id);

    static EquipmentItem Get(Dictionary<string, EquipmentItem> d, string id)
        => d != null && d.TryGetValue(id, out var p) ? p : null;


}