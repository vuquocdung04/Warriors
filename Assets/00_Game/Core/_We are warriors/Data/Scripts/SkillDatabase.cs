using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class SkillIcon
{
    public string id;
    [PreviewField(50, ObjectFieldAlignment.Left)]
    public Sprite sprite;
}

[CreateAssetMenu(menuName = "Game/Skill Database")]
public class SkillDatabase : ScriptableObject
{
    public Unit zombiePrefab;
    public Unit championPrefab;
    public SkillItem itemPrefab;
    public TextAsset skillJson;

    [TableList] public List<SkillIcon> icons;

    private Dictionary<string, SkillData> _skills;
    private Dictionary<string, Sprite> _icons;

    public void Init()
    {
        _skills = new Dictionary<string, SkillData>();
        if (skillJson != null)
        {
            var list = JsonConvert.DeserializeObject<List<SkillData>>(skillJson.text);
            if (list != null)
                foreach (var s in list)
                    if (!string.IsNullOrEmpty(s.id)) _skills[s.id] = s;
        }
        else Debug.LogError("[SkillDB] thiếu json");

        _icons = new Dictionary<string, Sprite>();
        foreach (var e in icons)
            if (e != null && !string.IsNullOrEmpty(e.id)) _icons[e.id] = e.sprite;
    }

    public SkillData GetSkill(string id)
        => _skills != null && _skills.TryGetValue(id, out var s) ? s : null;

    public List<SkillData> AllSkills() => new List<SkillData>(_skills.Values);

    public Sprite GetIcon(string id)
        => _icons != null && _icons.TryGetValue(id, out var s) ? s : null;
}