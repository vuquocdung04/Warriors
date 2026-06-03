using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CraftRelicsTab : MonoBehaviour
{
    [Header("Spin buttons")]
    public Button btnSpin1;
    public Button btnSpin10;
    public TMP_Text costText1;     // giá x1
    public TMP_Text costText10;    // giá x10

    [Header("Gacha progress")]
    public TMP_Text levelGachaText;     // level gacha hiện tại
    public TMP_Text progressText;       // "5/10"
    public Image fillProgress;

    [Header("Rate")]
    public Button btnShowRate;

    [Header("Cost (tạm 0)")]
    public int costX1 = 0;
    public int costX10 = 0;

    public void Init()
    {
        btnSpin1.OnClicked(() => OnSpin(1));
        btnSpin10.OnClicked(() => OnSpin(10));
        btnShowRate.OnClicked(OnShowRate);

        this.RegisterListener(EventID.ON_EQUIPMENT_CHANGED, OnEquipChanged);
        Refresh();
    }

    void OnEquipChanged(object param) => Refresh();

    void Refresh()
    {
        int level = UseProfile.GachaLevel.Value;
        var cfg = DataRepo.Instance.equipmentDatabase.GetGachaLevel(level);

        if (levelGachaText != null) levelGachaText.text = level.ToString();

        if (cfg != null && cfg.spinNeeded > 0)
        {
            int spin = UseProfile.GachaSpin.Value;
            if (progressText != null) progressText.text = $"{spin}/{cfg.spinNeeded}";
            if (fillProgress != null) fillProgress.fillAmount = Mathf.Clamp01((float)spin / cfg.spinNeeded);
        }
        else   // max level
        {
            if (progressText != null) progressText.text = "MAX";
            if (fillProgress != null) fillProgress.fillAmount = 1f;
        }

        if (costText1 != null) _ = costText1.CountTo(costX1, 0f);
        if (costText10 != null) _ = costText10.CountTo(costX10, 0f);
    }

    void OnSpin(int count)
    {
        var results = new List<GachaService.GachaResult>();
        for (int i = 0; i < count; i++)
        {
            var r = GachaService.Spin();
            if (r != null) results.Add(r);
        }

        Refresh();
        this.PostEvent(EventID.ON_EQUIPMENT_CHANGED);

        var holder = LobbyController.Instance.topCanvas;
        _ = GachaResultBox.Setup(holder, box => box.ShowResult(results));
    }

    void OnShowRate()
    {
        var holder = LobbyController.Instance.topCanvas;
        _ = RateRelicsBox.Setup(holder, box => box.Show());
    }

    void OnDestroy()
    {
        this.RemoveListener(EventID.ON_EQUIPMENT_CHANGED, OnEquipChanged);
    }
}