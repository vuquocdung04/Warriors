using System.Collections.Generic;
using EventDispatcher;
using UnityEngine;

public class EquipmentBar : MonoBehaviour
{
    public CanvasGroup canvasGroup;     // ẩn/hiện bằng alpha (object vẫn active -> nhận event)
    public List<EquipmentSlot> slots;
    public int showCivOrder = 3;

    public void Init()
    {
        this.RegisterListener(EventID.ON_CIV_CHANGED, OnCivChanged);
        this.RegisterListener(EventID.ON_EQUIPMENT_CHANGED, OnEquipChanged);
        Build();
    }

    void Build()
    {
        int civOrder = DataRepo.Instance.unitDatabase.GetCivOrder(UseProfile.CurrentCiv.Value);
        bool show = civOrder >= showCivOrder;

        canvasGroup.SetCanvasState(show, show ? 1f : 0f);
        if (!show) return;

        for (int i = 0; i < slots.Count; i++)
            slots[i].Init(i, OnSlotClicked);
    }

    void OnCivChanged(object param)
    {
        Build();
    }

    void OnEquipChanged(object param)
    {
        Refresh();
    }
    public void Refresh()
    {
        foreach (var s in slots) s.Refresh();
    }

    void OnSlotClicked(int slotIndex)
    {
        var holder = LobbyController.Instance.topCanvas;
        _ = EquipmentBox.Setup(holder, box => box.ShowAtTab(slotIndex + 1));
    }

    void OnDestroy()
    {
        this.RemoveListener(EventID.ON_CIV_CHANGED, OnCivChanged);
        this.RemoveListener(EventID.ON_EQUIPMENT_CHANGED, OnEquipChanged);
    }
}