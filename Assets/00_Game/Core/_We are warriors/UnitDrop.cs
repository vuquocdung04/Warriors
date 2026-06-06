using EventDispatcher;
using UnityEngine;

public class UnitDrop : MonoBehaviour
{
    void OnEnable() => this.RegisterListener(EventID.UNIT_DIED, OnUnitDied);
    void OnDisable() => this.RemoveListener(EventID.UNIT_DIED, OnUnitDied);

    void OnUnitDied(object param)
    {
        if (param is not Unit unit) return;

        if (unit.team == Team.Enemy) DropReward(unit);
    }

    void DropReward(Unit unit)
    {
        Debug.Log($"[UnitDrop] Enemy '{unit.id}' chết tại {unit.transform.position} -> rơi thưởng (chưa làm)");
    }
}
