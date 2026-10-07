using UnityEngine;

public class FreezeEffect : IStatusEffect, IControlEffect
{
    private float _timer;
    public FreezeEffect(float duration) => _timer = duration;

    public bool IsDone => _timer <= 0f;

    public void Tick(Unit unit, float dt) => _timer -= dt;
    // Dính đóng băng mới thì lấy thời gian còn lại dài hơn
    public void Refresh(FreezeEffect incoming) => _timer = Mathf.Max(_timer, incoming._timer);
    public void OnApply(Unit unit) => unit.SetFrozen(true);
    public void OnRemove(Unit unit) => unit.SetFrozen(false);
}
