using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class EquipmentState
{
    public string id;
    public int level = 1;
    public int card = 0;
}

public static class EquipmentSave
{
    const string KEY = "EQUIPMENT_SAVE";
    private static Dictionary<string, EquipmentState> _cache;

    static void Load()
    {
        if (_cache != null) return;
        _cache = new Dictionary<string, EquipmentState>();

        string json = GamePrefs.Get<string>(KEY, null);
        if (!string.IsNullOrEmpty(json))
        {
            var list = JsonConvert.DeserializeObject<List<EquipmentState>>(json);
            if (list != null)
                foreach (var s in list)
                    if (!string.IsNullOrEmpty(s.id)) _cache[s.id] = s;
        }
    }

    public static EquipmentState Get(string id)
    {
        Load();
        if (!_cache.TryGetValue(id, out var s))
        {
            s = new EquipmentState { id = id, level = 1, card = 0 };
            _cache[id] = s;
        }
        return s;
    }

    public static void Save()
    {
        Load();
        var list = new List<EquipmentState>(_cache.Values);
        GamePrefs.Set(KEY, JsonConvert.SerializeObject(list));
    }

    public static void SetLevel(string id, int level) { Get(id).level = level; Save(); }
    public static void AddCard(string id, int amount) { Get(id).card += amount; Save(); }
}