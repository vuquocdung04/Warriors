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

    [Header("Config")]
    public float ratePerLevel = 0.02f;   // tăng mỗi lần upgrade
    public int cost = 0;                  // tạm free

    public void Init()
    {
        upgradeButton.OnClicked(OnUpgrade);
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
    }

    void OnUpgrade()
    {
        if (UseProfile.Coin.Value < cost)
        {
            Debug.Log("[FoodProduction] không đủ coin");
            return;
        }

        UseProfile.Coin.Value -= cost;
        UseProfile.FoodRate.Value += ratePerLevel;

        Refresh();
    }
}