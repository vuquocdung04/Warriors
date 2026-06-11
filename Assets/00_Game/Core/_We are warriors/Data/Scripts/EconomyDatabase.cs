using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "EconomyDatabase", menuName = "Game/EconomyDatabase")]
public class EconomyDatabase : ScriptableObject
{
    public TextAsset jsonFile;

    private Dictionary<string, EconomyConfig> _map;

    public void Init()
    {
        var configs = JsonConvert.DeserializeObject<List<EconomyConfig>>(jsonFile.text);
        _map = new Dictionary<string, EconomyConfig>();
        foreach (var c in configs) _map[c.civId] = c;
    }

    public EconomyConfig Get(string civId)
    {
        return _map != null && _map.TryGetValue(civId, out var c) ? c : null;
    }
}