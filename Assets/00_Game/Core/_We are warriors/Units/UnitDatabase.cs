using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Unit Database")]
public class UnitDatabase : ScriptableObject
{
    public TextAsset json;
    public List<Unit> prefabs;

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
            Debug.Log($"[DB] prefab id = '{p.id}'");   // xem có ra 'stone_1' không
            if (!string.IsNullOrEmpty(p.id)) _prefab[p.id] = p;
        }
    }

    public UnitData GetDataById(string id) => _data != null && _data.TryGetValue(id, out var d) ? d : null;
    public Unit GetUnitById(string id) => _prefab != null && _prefab.TryGetValue(id, out var p) ? p : null;
}