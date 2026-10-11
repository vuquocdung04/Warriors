using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EventDispatcher;

public class HouseHpUpgrade : MonoBehaviour
{
    // Mỗi level +10% máu base (cộng dồn tuyến tính, không nhân theo máu hiện tại)
    public const float PercentPerLevel = 0.1f;
    public static float HpMultiplier => 1f + PercentPerLevel * UseProfile.HouseHpLevel.Value;

    [Header("UI")]
    public Button upgradeButton;
    public TMP_Text hpText;        // máu house hiện tại
    public TMP_Text costText;      // giá (coin)
    public Button upgradeX10Button;
    public TMP_Text costX10Text;   // giá nâng 10 lần

    [Header("Config")]
    public int cost = 0;                  // tạm free
    public int bulkCount = 10;

    public void Init()
    {
        upgradeButton.OnClicked(() => OnUpgrade(1));
        if (upgradeX10Button != null) upgradeX10Button.OnClicked(() => OnUpgrade(bulkCount));
        Refresh();
        this.RegisterListener(EventID.ON_FOOD_UI_CHANGED, Refresh);
        this.RegisterListener(EventID.ON_CIV_CHANGED, Refresh);
    }

    void OnDestroy()
    {
        this.RemoveListener(EventID.ON_FOOD_UI_CHANGED, Refresh);
        this.RemoveListener(EventID.ON_CIV_CHANGED, Refresh);
    }

    void Refresh(object obj = null)
    {
        var data = DataRepo.Instance.unitDatabase.GetHouseData(UseProfile.CurrentCiv.Value);
        float baseHp = data != null ? data.houseHp : 0f;
        hpText.text = NumberFormatter.Format(Mathf.RoundToInt(baseHp * HpMultiplier));
        _ = costText.CountToWithIcon(cost, "<sprite=0> ", duration: 0f);
        if (costX10Text != null) _ = costX10Text.CountToWithIcon(cost * bulkCount, "<sprite=0> ", duration: 0f);
    }

    void OnUpgrade(int times)
    {
        if (!CurrencyManager.Instance.TrySpend(CurrencyType.Coin, cost * times)) return;

        UseProfile.HouseHpLevel.Value += times;
        Refresh();
    }
}
