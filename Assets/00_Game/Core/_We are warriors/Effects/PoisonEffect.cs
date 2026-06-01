// Trừ máu theo thời gian (độc): mỗi `interval` giây gây `dmgPerTick` sát thương.
public class PoisonEffect : IStatusEffect
{
    readonly float _dmgPerTick;
    readonly float _interval;
    float _remain;
    float _tickTimer;

    public bool IsDone => _remain <= 0f;

    public PoisonEffect(float dmgPerTick, float duration, float interval = 1f)
    {
        _dmgPerTick = dmgPerTick;
        _remain = duration;
        _interval = interval;
        _tickTimer = interval;
    }

    public void OnApply(Unit u) { }

    public void OnTick(Unit u, float dt)
    {
        _remain -= dt;
        _tickTimer -= dt;
        if (_tickTimer <= 0f)
        {
            _tickTimer += _interval;
            u.TakeDamage(_dmgPerTick);
        }
    }

    public void OnRemove(Unit u) { }
}
