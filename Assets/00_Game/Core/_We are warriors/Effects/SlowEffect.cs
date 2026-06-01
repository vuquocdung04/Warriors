// Làm chậm di chuyển trong 1 khoảng thời gian.
public class SlowEffect : IStatusEffect
{
    readonly float _mul;     // hệ số moveSpeed, vd 0.5 = chậm 50%
    float _remain;

    public bool IsDone => _remain <= 0f;

    public SlowEffect(float mul, float duration)
    {
        _mul = mul;
        _remain = duration;
    }

    public void OnApply(Unit u)  => u.Effects.MoveMul *= _mul;
    public void OnTick(Unit u, float dt) => _remain -= dt;
    public void OnRemove(Unit u) => u.Effects.MoveMul /= _mul;   // trả lại tốc độ
}
