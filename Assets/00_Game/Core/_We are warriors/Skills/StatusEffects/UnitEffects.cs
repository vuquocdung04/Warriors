using System.Collections.Generic;


public interface IControlEffect { }
public interface IMoveModifier { float MoveMultiplier { get; } }


public class UnitEffects
{
    private readonly List<IStatusEffect> _effects = new();
    private Unit _unit;

    public void Init(Unit unit) { _unit = unit; _effects.Clear(); }

    public void Add(IStatusEffect e)
    {
        _effects.Add(e);
        e.OnApply(_unit);
    }

    public void Tick(float dt)
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            var e = _effects[i];
            e.Tick(_unit, dt);
            if (e.IsDone)
            {
                e.OnRemove(_unit);
                _effects.RemoveAt(i);
            }
        }
    }

    // cờ tổng hợp Unit cần biết — gom từ mọi effect, Unit KHÔNG biết loại cụ thể
    public bool IsControlled()   // bị khống chế (freeze/stun) -> đứng im
    {
        foreach (var e in _effects)
            if (e is IControlEffect) return true;
        return false;
    }

    public float MoveMul()       // slow nhân tốc độ
    {
        float m = 1f;
        foreach (var e in _effects)
            if (e is IMoveModifier mod) m *= mod.MoveMultiplier;
        return m;
    }
}