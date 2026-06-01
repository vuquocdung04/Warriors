using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodUpgrade : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text rateText;
    public TMP_Text costText;
    public Button upgradeButton;

    [Header("Config")]
    public float ratePerLevel = 0.02f;
    public int cost = 0;

    void Start()
    {
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgrade);
        RefreshUI();
    }

    void OnUpgrade()
    {
        if (UseProfile.Coin.Value < cost) return;

        UseProfile.Coin.Value -= cost;
        UseProfile.FoodRate.Value += ratePerLevel;
        RefreshUI();
    }

    void RefreshUI()
    {
        rateText.text = $"{UseProfile.FoodRate.Value:0.00}/s";
        _ = costText.CountTo(cost, 0f);
    }
}