using UnityEngine;

public class FreezeEffect : IStatusEffect, IControlEffect
{
    private float _timer;
    private float _duration;
    public FreezeEffect(float duration) => _timer = duration;

    public bool IsDone => _timer <= 0f;

    public void Tick(Unit unit, float dt) => _timer -= dt;
    public void Refresh() => _timer = Mathf.Max(_timer, _duration);
    public void OnApply(Unit unit) { }
    public void OnRemove(Unit unit) { }
}