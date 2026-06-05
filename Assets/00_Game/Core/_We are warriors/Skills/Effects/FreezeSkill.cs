public class FreezeSkill : ISkillEffect
{
    public void Activate(SkillData data, int level)
    {
        float duration = data.ValueAt(level);
        var enemies = BattleManager.Instance.Enemies;
        for (int i = 0; i < enemies.Count; i++)
            if (enemies[i] != null && enemies[i].IsAlive)
                enemies[i].AddEffect(new FreezeEffect(duration));
    }
}