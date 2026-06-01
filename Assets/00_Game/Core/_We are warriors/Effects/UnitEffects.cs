using System.Collections.Generic;

// "1 script control": mỗi Unit giữ 1 cái. Lưu + tick danh sách effect.
// Effect tự cộng/trừ các hệ số bên dưới khi OnApply/OnRemove; Unit chỉ đọc.
public class UnitEffects
{
    readonly List<IStatusEffect> _list = new();

    public int ControlCount;        // freeze/stun: > 0 nghĩa là đang bị khoá hành động
    public float MoveMul = 1f;      // slow / +tốc chạy
    public float AtkMul = 1f;       // +sát thương
    public float AtkSpeedMul = 1f;  // +tốc bắn

    public bool IsControlled => ControlCount > 0;

    public void Add(Unit u, IStatusEffect effect)
    {
        if (effect == null) return;
        _list.Add(effect);
        effect.OnApply(u);
    }

    public void Tick(Unit u, float dt)
    {
        for (int i = _list.Count - 1; i >= 0; i--)
        {
            _list[i].OnTick(u, dt);
            if (!u.IsAlive) return;          // bị độc giết -> dừng
            if (_list[i].IsDone) { _list[i].OnRemove(u); _list.RemoveAt(i); }
        }
    }
}
