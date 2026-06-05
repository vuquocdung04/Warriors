using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class SkillState
{
    public string id;
    public int level = 1;
    public int card = 0;
    public bool owned = false;
    public bool equipped = false;
}

public static class SkillSave
{
    public const int MaxEquipped = 1;
    const string SAVE_KEY = "SKILL_SAVE";
    private static Dictionary<string, SkillState> _cache;

    static void Load()
    {
        if (_cache != null) return;
        _cache = new Dictionary<string, SkillState>();

        string json = GamePrefs.Get<string>(SAVE_KEY, null);
        if (!string.IsNullOrEmpty(json))
        {
            var list = JsonConvert.DeserializeObject<List<SkillState>>(json);
            if (list != null)
                foreach (var s in list)
                    if (!string.IsNullOrEmpty(s.id)) _cache[s.id] = s;
        }
    }
    public static List<SkillState> GetEquipped()
    {
        Load();
        var list = new List<SkillState>();
        foreach (var s in _cache.Values)
            if (s.equipped) list.Add(s);
        return list;
    }
    public static void Equip(string id)
    {
        Load();
        var equipped = GetEquipped();

        if (Get(id).equipped) return;

        while (equipped.Count >= MaxEquipped && equipped.Count > 0)
        {
            equipped[0].equipped = false;
            equipped.RemoveAt(0);
        }

        Get(id).equipped = true;
        Save();
    }
    public static void Unequip(string id)
    {
        Get(id).equipped = false;
        Save();
    }
    public static SkillState Get(string id)
    {
        Load();
        if (!_cache.TryGetValue(id, out var s))
        {
            s = new SkillState { id = id, level = 1, card = 0, owned = false };
            _cache[id] = s;
        }
        return s;
    }

    public static void Save()
    {
        Load();
        var list = new List<SkillState>(_cache.Values);
        GamePrefs.Set(SAVE_KEY, JsonConvert.SerializeObject(list));
    }

    public static void ClearAll()
    {
        _cache = null;
        GamePrefs.DeleteKey(SAVE_KEY);
    }
}