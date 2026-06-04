using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class SkillState
{
    public string id;
    public int level = 1;
    public int card = 0;
    public bool owned = false;
}

public static class SkillSave
{
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