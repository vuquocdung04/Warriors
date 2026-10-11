using UnityEngine;

// Tăng tốc độ đánh trong 1 khoảng thời gian (UnitEffects.AttackSpeedMul nhân vào attackSpeed)
public class AttackSpeedBuffEffect : IStatusEffect, IAttackSpeedModifier
{
    private float _timer;
    private float _bonus;   // 0.1 = +10%

    public AttackSpeedBuffEffect(float bonus, float duration)
    {
        _bonus = bonus;
        _timer = duration;
    }

    public float AttackSpeedMultiplier => 1f + _bonus;
    public bool IsDone => _timer <= 0f;

    public void Tick(Unit unit, float dt) => _timer -= dt;
    // Dùng skill lần nữa khi đang buff: giữ % cao hơn, gia hạn theo thời gian dài hơn
    public void Refresh(AttackSpeedBuffEffect incoming)
    {
        _bonus = Mathf.Max(_bonus, incoming._bonus);
        _timer = Mathf.Max(_timer, incoming._timer);
    }

    public void OnApply(Unit unit) => unit.SetBuffFx(BuffType.Frenzy, true);
    public void OnRemove(Unit unit) => unit.SetBuffFx(BuffType.Frenzy, false);
}
