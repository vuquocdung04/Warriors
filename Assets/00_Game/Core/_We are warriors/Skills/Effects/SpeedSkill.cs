// Tăng {value}% tốc độ di chuyển cho toàn bộ lính mình đang trên sân trong DURATION giây
public class SpeedSkill : ISkillEffect
{
    const float DURATION = 10f;

    public void Activate(SkillData data, int level)
    {
        float bonus = data.ValueAt(level) / 100f;
        var allies = BattleManager.Instance.Allies;
        for (int i = 0; i < allies.Count; i++)
            if (allies[i] != null && allies[i].IsAlive)
                allies[i].AddEffect(new SpeedBuffEffect(bonus, DURATION));
    }
}
