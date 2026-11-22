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
    public long equipOrder = 0; // thứ tự đeo -> quyết định vị trí slot, skill đeo trước đứng trước
}

public static class SkillSave
{
    public const int MaxEquipped = 3;
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
        list.Sort((a, b) => a.equipOrder.CompareTo(b.equipOrder));
        return list;
    }

    // Đeo skill; nếu đã đủ MaxEquipped slot thì tháo skill đeo lâu nhất
    public static void Equip(string id)
    {
        Load();
        var state = Get(id);
        if (state.equipped) return;

        var equipped = GetEquipped();
        while (equipped.Count >= MaxEquipped && equipped.Count > 0)
        {
            equipped[0].equipped = false;
            equipped.RemoveAt(0);
        }

        long nextOrder = equipped.Count > 0 ? equipped[equipped.Count - 1].equipOrder + 1 : 0;
        state.equipped = true;
        state.equipOrder = nextOrder;
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