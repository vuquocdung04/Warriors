using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class EquipmentState
{
    public string key;        // "Melee_1" — duy nhất toàn cục
    public int level = 1;
    public int card = 0;
    public bool owned = false;
}

public static class EquipmentSave
{
    const string SAVE_KEY = "EQUIPMENT_SAVE";
    private static Dictionary<string, EquipmentState> _cache;

    static void Load()
    {
        if (_cache != null) return;
        _cache = new Dictionary<string, EquipmentState>();

        string json = GamePrefs.Get<string>(SAVE_KEY, null);
        if (!string.IsNullOrEmpty(json))
        {
            var list = JsonConvert.DeserializeObject<List<EquipmentState>>(json);
            if (list != null)
                foreach (var s in list)
                    if (!string.IsNullOrEmpty(s.key)) _cache[s.key] = s;
        }
    }

    static string Key(EquipType type, string id) => $"{type}_{id}";

    public static EquipmentState Get(EquipType type, string id)
    {
        Load();
        string k = Key(type, id);
        if (!_cache.TryGetValue(k, out var s))
        {
            s = new EquipmentState { key = k, level = 1, card = 0 };
            _cache[k] = s;
        }
        return s;
    }

    public static void Save()
    {
        Load();
        var list = new List<EquipmentState>(_cache.Values);
        GamePrefs.Set(SAVE_KEY, JsonConvert.SerializeObject(list));
    }
    public static void ClearAll()
    {
        _cache = null;                     
        GamePrefs.DeleteKey(SAVE_KEY);      
    }
    public static void AddCard(EquipType type, string id, int amount) { Get(type, id).card += amount; Save(); }
}