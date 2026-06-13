using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Gacha Database")]
public class GachaDatabase : ScriptableObject
{
    public TextAsset gachaLevelJson;

    private Dictionary<int, GachaLevelData> _gachaLevels;

    public void Init()
    {
        _gachaLevels = new Dictionary<int, GachaLevelData>();
        foreach (var g in Load<GachaLevelData>(gachaLevelJson))
            _gachaLevels[g.level] = g;
    }

    List<T> Load<T>(TextAsset json)
    {
        if (json == null) { Debug.LogError("[GachaDB] thiếu json"); return new List<T>(); }
        return JsonConvert.DeserializeObject<List<T>>(json.text) ?? new List<T>();
    }

    public GachaLevelData GetGachaLevel(int level) => _gachaLevels.TryGetValue(level, out var g) ? g : null;

    public int GetMaxGachaLevel()
    {
        int max = 1;
        foreach (var lv in _gachaLevels.Keys)
            if (lv > max) max = lv;
        return max;
    }
}