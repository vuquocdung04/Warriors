using System.Collections.Generic;

public class SkillController : StaffSingleton<SkillController>
{
    private Dictionary<string, ISkillEffect> _effects;

    public override void Init()
    {
        _effects = new Dictionary<string, ISkillEffect>
        {
            { "1", new FreezeSkill() },
            { "2", new BuffSkill() },
            { "3", new PushSkill() },
            { "4", new ChampionSkill() },
            { "5", new NecromancySkill() },
        };
    }

    public void Activate(string skillId)
    {
        var data = DataRepo.Instance.skillDatabase.GetSkill(skillId);
        if (data == null) return;
        if (!_effects.TryGetValue(skillId, out var effect)) return;

        int level = SkillSave.Get(skillId).level;
        effect.Activate(data, level);
    }
}