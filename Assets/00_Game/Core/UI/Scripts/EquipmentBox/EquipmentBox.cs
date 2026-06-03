using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentBox : BaseBox<EquipmentBox>
{
    public Button btnClose;
    [Header("Nav buttons (Craft, Unit1, Unit2, Unit3)")]
    public List<EquipmentNavButton> navButtons;

    [Header("Tab content (cùng thứ tự nav)")]
    public List<CanvasGroup> tabs;

    [Header("Màu nav (chung 4 nút)")]
    public Color selectedColor = Color.white;
    public Color normalColor = Color.gray;

    [Header("Height nav")]
    public float selectedHeight = 150f;
    public float normalHeight = 120f;

    private int _current = -1;

    protected override void Init()
    {
        var units = DataRepo.Instance.unitDatabase.GetCivUnits(UseProfile.CurrentCiv.Value);

        for (int i = 0; i < navButtons.Count; i++)
        {
            int index = i;
            navButtons[i].Init();
            navButtons[i].SetupClick(() => SelectTab(index));

            if (navButtons[i].txtName != null)
            {
                int unitIdx = i - 1;
                navButtons[i].txtName.text =
                    (unitIdx >= 0 && unitIdx < units.Count) ? units[unitIdx].displayName : "";
            }
        }

        btnClose.OnClicked(Close);
    }
    protected override void InitState()
    {
    }

    void SelectTab(int index)
    {
        if (index == _current) return;
        _current = index;

        for (int i = 0; i < navButtons.Count; i++)
            navButtons[i].SetSelected(i == index, selectedColor, normalColor, selectedHeight, normalHeight);

        for (int i = 0; i < tabs.Count; i++)
            tabs[i].SetCanvasState(i == index, i == index ? 1f : 0f);
    }
    public void ShowAtTab(int tabIndex)
    {
        Show();             
        SelectTab(tabIndex);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}