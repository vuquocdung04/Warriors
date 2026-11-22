using System.Collections.Generic;
using UnityEngine;

// Quản lý các ô skill trong gameplay (tối đa SkillSave.MaxEquipped), mỗi ô hồi chiêu riêng
public class BoosterBar : MonoBehaviour
{
    public const float COOLDOWN = 10f;

    public List<BoosterSlot> slots; // slot[0] = ô dưới cùng

    public void Init()
    {
        var equipped = SkillSave.GetEquipped();
        if (equipped.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < equipped.Count) slots[i].Init(equipped[i].id, COOLDOWN);
            else slots[i].gameObject.SetActive(false);   // slot trống -> ẩn
        }
    }
}
