// Đốt toàn bộ enemy trong {value} giây, mỗi giây mất PERCENT maxHp (mỗi cấp tăng thời gian cháy)
public class BurnSkill : ISkillEffect
{
    const float PERCENT = 0.03f;

    public void Activate(SkillData data, int level)
    {
        float duration = data.ValueAt(level);
        var enemies = BattleManager.Instance.Enemies;
        for (int i = 0; i < enemies.Count; i++)
            if (enemies[i] != null && enemies[i].IsAlive)
                enemies[i].AddEffect(new BurnEffect(PERCENT, duration));
    }
}
