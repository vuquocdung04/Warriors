using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWaveDatabase", menuName = "Game/EnemyWaveDatabase")]
public class EnemyWaveDatabase : ScriptableObject
{
    public TextAsset jsonFile;

    private Dictionary<string, EnemyCivConfig> _map;

    public void Init()
    {
        var configs = JsonConvert.DeserializeObject<List<EnemyCivConfig>>(jsonFile.text);
        _map = new Dictionary<string, EnemyCivConfig>();
        foreach (var c in configs) _map[c.civId] = c;
    }

    public EnemyCivConfig Get(string civId)
    {
        return _map != null && _map.TryGetValue(civId, out var c) ? c : null;
    }
}