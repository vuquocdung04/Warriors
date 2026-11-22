// Mưa cục tẩy: 4 cục tẩy khổng lồ bay chéo từ trên-trái xuống, chia đều đoạn giữa 2 nhà,
// mỗi cục nổ gây {value}% máu tối đa cho mọi lính địch trong bán kính (mặc định 20%, +5% mỗi cấp)
public class EraserRainSkill : ISkillEffect
{
    public void Activate(SkillData data, int level)
    {
        float percent = data.ValueAt(level) / 100f;
        if (percent <= 0f) return;

        var spawner = EraserRainSpawner.Instance;
        if (spawner != null) spawner.Drop(percent);
        else EraserRainSpawner.ApplyWithoutFx(percent);   // scene thiếu spawner: vẫn gây damage, không có hiệu ứng
    }
}
