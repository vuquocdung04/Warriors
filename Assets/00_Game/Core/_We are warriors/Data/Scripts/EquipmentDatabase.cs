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

    static EquipmentData Get(Dictionary<string, EquipmentData> d, string id)
        => d != null && d.TryGetValue(id, out var v) ? v : null;
    static StatData Get(Dictionary<string, StatData> d, string id)
        => d != null && d.TryGetValue(id, out var v) ? v : null;
    static RankData Get(Dictionary<string, RankData> d, string id)
        => d != null && d.TryGetValue(id, out var v) ? v : null;

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

}
