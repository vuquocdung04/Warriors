using System.Collections.Generic;
using EventDispatcher;
using UnityEngine;

public class UpgradeBar : MonoBehaviour
{
    [Header("3 item (trái -> phải)")]
    public List<UpgradeUnitItem> items;

    public void Init(string civId)
    {
        Build(civId);
        this.RegisterListener(EventID.ON_CIV_CHANGED, OnCivChanged);
    }
    bool IsUnlocked(int index)
    {
        if (index == 0) return true;
        if (index == 1) return UseProfile.Unit2Unlock.Value;
        return UseProfile.Unit3Unlock.Value;
    }

    void OnBuy(UpgradeUnitItem item)
    {
        int price = item.Data.buyPrice;
        if (UseProfile.Coin.Value < price) return;

        UseProfile.Coin.Value -= price;
        if (item.Index == 1) UseProfile.Unit2Unlock.Value = true;
        else if (item.Index == 2) UseProfile.Unit3Unlock.Value = true;
        item.SetUnlocked(true);

        this.PostEvent(EventID.ON_EQUIPMENT_CHANGED);
    }

    void OnCivChanged(object param)
    {
        Build(UseProfile.CurrentCiv.Value);
    }
    void Build(string civId)
    {
        var units = DataRepo.Instance.unitDatabase.GetCivUnits(civId);
        for (int i = 0; i < items.Count; i++)
        {
            if (i < units.Count)
            {
                items[i].gameObject.SetActive(true);
                items[i].Setup(units[i], IsUnlocked(i), OnBuy, i);
            }
            else items[i].gameObject.SetActive(false);
        }
    }
    void OnDestroy()
    {
        this.RemoveListener(EventID.ON_CIV_CHANGED, OnCivChanged);
    }
}