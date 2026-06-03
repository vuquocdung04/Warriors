using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class EquipmentIcon
{
    public string id;
    [PreviewField(50, ObjectFieldAlignment.Left)]
    public Sprite sprite;
}
[System.Serializable]
public class StatIcon
{
    public string statType;
    [PreviewField(50, ObjectFieldAlignment.Left)]
    public Sprite sprite;
}

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

    [Header("Prefab item (1 prefab / loại)")]
    public EquipmentItem meleeItemPrefab;
    public EquipmentItem rangeItemPrefab;
    public EquipmentItem shieldItemPrefab;

    [Header("Icon theo id (mỗi loại 1 list)")]
    [TableList] public List<EquipmentIcon> meleeIcons;
    [TableList] public List<EquipmentIcon> rangeIcons;
    [TableList] public List<EquipmentIcon> shieldIcons;


    [Header("Màu nền theo rank")]
    public Color commonColor = Color.white;
    public Color rareColor = Color.blue;
    public Color epicColor = Color.magenta;
    public Color legendColor = Color.yellow;


    [TableList] public List<StatIcon> statIcons;
    private Dictionary<string, Sprite> _statIcon;

    private Dictionary<string, Sprite> _meleeIcon;
    private Dictionary<string, Sprite> _rangeIcon;
    private Dictionary<string, Sprite> _shieldIcon;

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

        _meleeIcon = BuildIconDict(meleeIcons);
        _rangeIcon = BuildIconDict(rangeIcons);
        _shieldIcon = BuildIconDict(shieldIcons);

        _statIcon = new Dictionary<string, Sprite>();
        foreach (var e in statIcons)
            if (e != null && !string.IsNullOrEmpty(e.statType)) _statIcon[e.statType] = e.sprite;


    }
    Dictionary<string, Sprite> BuildIconDict(List<EquipmentIcon> list)
    {
        var dict = new Dictionary<string, Sprite>();
        foreach (var e in list)
            if (e != null && !string.IsNullOrEmpty(e.id)) dict[e.id] = e.sprite;
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
        EquipType[] types = { EquipType.Melee, EquipType.Range, EquipType.Shield };

        for (int i = 0; i < units.Count; i++)
        {
            var stats = new UnitStats(units[i]);
            if (i < equips.Length && equips[i] != null) ApplyEquipment(stats, equips[i], types[i]);
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


    void ApplyEquipment(UnitStats stats, EquipmentData equip, EquipType type)
    {
        int level = EquipmentSave.Get(type, equip.id).level;

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
        float v = value / 100f;
        switch (statType)
        {
            case "critical_chance": stats.criticalChance += v; break;
            case "life_steal": stats.lifeSteal += v; break;
            case "push_chance": stats.pushChance += v; break;
            case "poison_chance": stats.poisonChance += v; break;
            case "burn_chance": stats.burnChance += v; break;
            case "freeze_chance": stats.freezeChance += v; break;
        }
    }
    static string StripSuffix(string statType)   // atk_percent -> atk, hp_percent -> hp
    {
        if (statType.EndsWith("_percent")) return statType.Substring(0, statType.Length - "_percent".Length);
        return statType;
    }
    public Sprite GetMeleeIcon(string id) => GetIcon(_meleeIcon, id);
    public Sprite GetRangeIcon(string id) => GetIcon(_rangeIcon, id);
    public Sprite GetShieldIcon(string id) => GetIcon(_shieldIcon, id);

    static Sprite GetIcon(Dictionary<string, Sprite> d, string id)
        => d != null && d.TryGetValue(id, out var s) ? s : null;

    // getter prefab item 1/loại
    public EquipmentItem GetMeleeItemPrefab() => meleeItemPrefab;
    public EquipmentItem GetRangeItemPrefab() => rangeItemPrefab;
    public EquipmentItem GetShieldItemPrefab() => shieldItemPrefab;



    public Color GetRankColor(string rank)
    {
        switch (rank)
        {
            case "rare": return rareColor;
            case "epic": return epicColor;
            case "legend": return legendColor;
            default: return commonColor;   // common
        }
    }

    public Sprite GetStatIcon(string statType)
    => _statIcon != null && _statIcon.TryGetValue(statType, out var s) ? s : null;

    public int GetMaxGachaLevel()
    {
        int max = 1;
        foreach (var lv in _gachaLevels.Keys)
            if (lv > max) max = lv;
        return max;
    }

}