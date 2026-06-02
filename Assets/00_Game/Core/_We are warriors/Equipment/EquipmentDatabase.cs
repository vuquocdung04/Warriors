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
}