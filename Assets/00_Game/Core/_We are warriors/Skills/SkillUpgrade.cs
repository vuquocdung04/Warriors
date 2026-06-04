public static class SkillUpgrade
{
    public static int CardNeeded(int level) => 1 + (level - 1) / 2;

    public static bool CanUpgrade(SkillData skill)
    {
        var state = SkillSave.Get(skill.id);
        if (state.level >= skill.levelMax) return false;
        return state.card >= CardNeeded(state.level);
    }

    public static bool TryUpgrade(SkillData skill)
    {
        if (!CanUpgrade(skill)) return false;
        var state = SkillSave.Get(skill.id);
        state.card -= CardNeeded(state.level);
        state.level += 1;
        SkillSave.Save();
        return true;
    }
}