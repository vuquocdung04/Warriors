using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Unit Database")]
public class UnitDatabase : ScriptableObject
{
    public TextAsset json;
    public List<Unit> prefabs;
    public TextAsset houseJson;
    private Dictionary<string, HouseData> _houses;
    private Dictionary<string, UnitData> _data;
    private Dictionary<string, Unit> _prefab;

    public void Init()
    {
        _data = new Dictionary<string, UnitData>();
        if (json != null)
        {
            var list = JsonConvert.DeserializeObject<List<UnitData>>(json.text);
            if (list != null)
                foreach (var u in list)
                    if (!string.IsNullOrEmpty(u.id)) _data[u.id] = u;
        }
        else Debug.LogError("[UnitDatabase] Chưa gán json");

        _prefab = new Dictionary<string, Unit>();
        foreach (var p in prefabs)
        {
            if (p == null) { Debug.LogWarning("[DB] prefab null trong list"); continue; }
            Debug.Log($"[DB] prefab id = '{p.id}'");
            if (!string.IsNullOrEmpty(p.id)) _prefab[p.id] = p;
        }

        _houses = new Dictionary<string, HouseData>();
        if (houseJson != null)
        {
            var list = JsonConvert.DeserializeObject<List<HouseData>>(houseJson.text);
            if (list != null)
                foreach (var h in list)
                    if (!string.IsNullOrEmpty(h.civId)) _houses[h.civId] = h;
        }
    }
    public List<UnitData> GetCivUnits(string civId)
    {
        var result = new List<UnitData>();
        foreach (var kv in _data)
            if (kv.Value.civId == civId) result.Add(kv.Value);

        result.Sort((a, b) => string.Compare(a.id, b.id));
        return result;
    }
    public Unit GetUnitById(string id) => _prefab != null && _prefab.TryGetValue(id, out var p) ? p : null;
    public HouseData GetHouseData(string civId)
        => _houses != null && _houses.TryGetValue(civId, out var h) ? h : null;

    public List<HouseData> GetCivsByOrder()
    {
        var list = new List<HouseData>(_houses.Values);
        list.Sort((a, b) => a.order.CompareTo(b.order));
        return list;
    }
    public int GetCivOrder(string civId)
        => _houses != null && _houses.TryGetValue(civId, out var h) ? h.order : 1;
}