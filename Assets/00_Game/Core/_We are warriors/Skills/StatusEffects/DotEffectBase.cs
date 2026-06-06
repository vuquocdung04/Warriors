using UnityEngine;

// DoT trừ % maxHp mỗi giây, refresh khi gắn lại
public abstract class DotEffectBase : IStatusEffect
{
    protected abstract float PercentPerTick { get; }   // 0.01 = 1% maxHp
    protected abstract float TotalDuration { get; }     // 5s
    protected abstract void ShowText(float dmg, Vector3 pos);

    private float _remain;
    private float _tickTimer;

    protected DotEffectBase() => Refresh();

    public void Refresh()   // gắn lại -> gia hạn
    {
        _remain = TotalDuration;
        _tickTimer = 1f;     // tick đầu sau 1 giây
    }

    public bool IsDone => _remain <= 0f;

    public void Tick(Unit unit, float dt)
    {
        _remain -= dt;
        _tickTimer -= dt;
        if (_tickTimer <= 0f && _remain > -0.001f)
        {
            _tickTimer += 1f;
            float dmg = unit.MaxHp * PercentPerTick;
            unit.TakeDamage(dmg);
            ShowText(dmg, unit.Transform.position + Vector3.up * 0.5f);
        }
    }

    public void OnApply(Unit unit) { }
    public void OnRemove(Unit unit) { }
}

public class PoisonEffect : DotEffectBase
{
    protected override float PercentPerTick => 0.01f;   // 1% maxHp
    protected override float TotalDuration => 5f;
    protected override void ShowText(float dmg, Vector3 pos) => FlyTextSpawner.Instance.Poison(dmg, pos);
}

public class BurnEffect : DotEffectBase
{
    protected override float PercentPerTick => 0.01f;
    protected override float TotalDuration => 5f;
    protected override void ShowText(float dmg, Vector3 pos) => FlyTextSpawner.Instance.Burn(dmg, pos);
}