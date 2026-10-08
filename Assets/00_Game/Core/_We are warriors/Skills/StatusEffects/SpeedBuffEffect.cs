using UnityEngine;

// Tăng tốc độ di chuyển trong 1 khoảng thời gian (UnitEffects.MoveMul nhân vào moveSpeed)
public class SpeedBuffEffect : IStatusEffect, IMoveModifier
{
    private float _timer;
    private float _bonus;   // 0.2 = +20%

    public SpeedBuffEffect(float bonus, float duration)
    {
        _bonus = bonus;
        _timer = duration;
    }

    public float MoveMultiplier => 1f + _bonus;
    public bool IsDone => _timer <= 0f;

    public void Tick(Unit unit, float dt) => _timer -= dt;
    // Dùng skill lần nữa khi đang buff: giữ % cao hơn, gia hạn theo thời gian dài hơn
    public void Refresh(SpeedBuffEffect incoming)
    {
        _bonus = Mathf.Max(_bonus, incoming._bonus);
        _timer = Mathf.Max(_timer, incoming._timer);
    }

    public void OnApply(Unit unit) { }
    public void OnRemove(Unit unit) { }
}
