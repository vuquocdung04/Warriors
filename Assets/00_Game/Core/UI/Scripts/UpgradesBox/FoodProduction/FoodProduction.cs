using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EventDispatcher;

public class FoodProduction : MonoBehaviour
{
    [Header("UI")]
    public Button upgradeButton;
    public TMP_Text rateText;      // "0.18/s"
    public TMP_Text costText;      // giá (coin)
    public Button upgradeX10Button;
    public TMP_Text costX10Text;   // giá nâng 10 lần

    [Header("Config")]
    public float ratePerLevel = 0.02f;   // tăng mỗi lần upgrade
    public int cost = 0;                  // tạm free
    public int bulkCount = 10;

    public void Init()
    {
        upgradeButton.OnClicked(() => OnUpgrade(1));
        if (upgradeX10Button != null) upgradeX10Button.OnClicked(() => OnUpgrade(bulkCount));
        Refresh();
        this.RegisterListener(EventID.ON_FOOD_UI_CHANGED, Refresh);

    }

    void OnDestroy()
    {
        this.RemoveListener(EventID.ON_FOOD_UI_CHANGED, Refresh);
    }


    void Refresh(object obj = null)
    {
        float rate = UseProfile.FoodRate.Value;
        rateText.text = $"{rate:0.##}/s";
        _ = costText.CountToWithIcon(cost, "<sprite=0> ", duration: 0f);
        if (costX10Text != null) _ = costX10Text.CountToWithIcon(cost * bulkCount, "<sprite=0> ", duration: 0f);
    }

    void OnUpgrade(int times)
    {
        int total = cost * times;
        if (UseProfile.Coin.Value < total)
        {
            Debug.Log("[FoodProduction] không đủ coin");
            return;
        }

        UseProfile.Coin.Value -= total;
        UseProfile.FoodRate.Value += ratePerLevel * times;

        Refresh();
    }
}