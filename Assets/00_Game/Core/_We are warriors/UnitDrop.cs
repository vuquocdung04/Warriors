using EventDispatcher;
using UnityEngine;

// Nghe UNIT_DIED -> rơi phần thưởng tại chỗ lính chết.
// Là subscriber thuần (không deps, không ai gọi Instance) nên tự đăng ký trong OnEnable/OnDisable,
// khỏi cần GamePlayController.Init wiring. Nội dung thưởng chưa chốt -> tạm Debug.Log.
public class UnitDrop : MonoBehaviour
{
    void OnEnable() => this.RegisterListener(EventID.UNIT_DIED, OnUnitDied);
    void OnDisable() => this.RemoveListener(EventID.UNIT_DIED, OnUnitDied);

    void OnUnitDied(object param)
    {
        // PostEvent chạy trước Destroy nên unit vẫn còn -> đọc được vị trí/dữ liệu
        if (param is not Unit unit) return;

        if (unit.team == Team.Enemy) DropReward(unit);   // địch chết -> người chơi nhận thưởng
    }

    // TODO: rơi coin / exp / vật phẩm thật
    void DropReward(Unit unit)
    {
        Debug.Log($"[UnitDrop] Enemy '{unit.id}' chết tại {unit.transform.position} -> rơi thưởng (chưa làm)");
    }
}
