using UnityEngine;

// DoT trừ % maxHp mỗi giây, refresh khi gắn lại
public abstract class DotEffectBase : IStatusEffect
{
    protected abstract DotType Type { get; }
    protected abstract void ShowText(float dmg, Vector3 pos);

    private float _percentPerTick;   // 0.01 = 1% maxHp
    private float _duration;         // tổng thời gian
    private float _remain;
    private float _tickTimer;

    protected DotEffectBase(float percentPerTick, float duration)
    {
        _percentPerTick = percentPerTick;
        _duration = duration;
        Refresh();
    }

    public void Refresh()   // gắn lại -> gia hạn
    {
        _remain = _duration;
        _tickTimer = 1f;     // tick đầu sau 1 giây
    }

    // Dính thêm 1 lần nữa (đánh thường / skill): giữ cái mạnh hơn, gia hạn theo cái dài hơn
    public void Refresh(DotEffectBase incoming)
    {
        _percentPerTick = Mathf.Max(_percentPerTick, incoming._percentPerTick);
        _remain = Mathf.Max(_remain, incoming._duration);
    }

    public bool IsDone => _remain <= 0f;

    public void Tick(Unit unit, float dt)
    {
        _remain -= dt;
        _tickTimer -= dt;
        if (_tickTimer <= 0f && _remain > -0.001f)
        {
            _tickTimer += 1f;
            float dmg = Mathf.Max(1f, Mathf.Round(unit.MaxHp * _percentPerTick));   // tối thiểu 1 máu, tránh % ra 0
            unit.TakeDamage(dmg);
            ShowText(dmg, unit.Transform.position + Vector3.up * 0.5f);
        }
    }

    public void OnApply(Unit unit) => unit.SetDotFx(Type, true);
    public void OnRemove(Unit unit) => unit.SetDotFx(Type, false);
}

public class PoisonEffect : DotEffectBase
{
    // mặc định (đánh thường có tỉ lệ gây độc): 1% maxHp/s trong 5s
    public PoisonEffect(float percentPerTick = 0.01f, float duration = 5f) : base(percentPerTick, duration) { }
    protected override DotType Type => DotType.Poison;
    protected override void ShowText(float dmg, Vector3 pos) => FlyTextSpawner.Instance.Poison(dmg, pos);
}

public class BurnEffect : DotEffectBase
{
    public BurnEffect(float percentPerTick = 0.01f, float duration = 5f) : base(percentPerTick, duration) { }
    protected override DotType Type => DotType.Burn;
    protected override void ShowText(float dmg, Vector3 pos) => FlyTextSpawner.Instance.Burn(dmg, pos);
}
