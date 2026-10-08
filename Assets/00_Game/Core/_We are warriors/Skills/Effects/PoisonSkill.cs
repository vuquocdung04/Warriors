// Gây độc toàn bộ enemy: mất {value}% maxHp mỗi giây trong DURATION giây (mỗi cấp tăng % sát thương)
public class PoisonSkill : ISkillEffect
{
    const float DURATION = 5f;

    public void Activate(SkillData data, int level)
    {
        float percent = data.ValueAt(level) / 100f;
        var enemies = BattleManager.Instance.Enemies;
        for (int i = 0; i < enemies.Count; i++)
            if (enemies[i] != null && enemies[i].IsAlive)
                enemies[i].AddEffect(new PoisonEffect(percent, DURATION));
    }
}
