public class PushEffect : IStatusEffect, IControlEffect
{
    private float _timer;
    public PushEffect(float duration) => _timer = duration;
    public bool IsDone => _timer <= 0f;
    public void Tick(Unit unit, float dt) => _timer -= dt;
    public void OnApply(Unit unit) { }
    public void OnRemove(Unit unit) { }
}