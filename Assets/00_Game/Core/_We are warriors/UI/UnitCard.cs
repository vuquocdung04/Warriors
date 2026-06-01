using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitCard : MonoBehaviour
{
    public Button button;
    public TMP_Text foodCostText;

    [Header("Màu chữ")]
    public Color enoughColor = Color.white;
    public Color notEnoughColor = Color.red;

    public int foodCost { get; private set; }

    private int _index;
    private System.Action<int> _onClick;

    public void Setup(int index, UnitData data, System.Action<int> onClick)
    {
        _index = index;
        foodCost = data.foodCost;
        _onClick = onClick;

        foodCostText.text = foodCost.ToString();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onClick?.Invoke(_index));
        this.RemoveListener(EventID.FOOD_CHANGED, OnFoodChanged);
        this.RegisterListener(EventID.FOOD_CHANGED, OnFoodChanged);
        RefreshAffordable(FoodManager.Instance != null ? FoodManager.Instance.Food : 0);
    }

    void OnDestroy() => this.RemoveListener(EventID.FOOD_CHANGED, OnFoodChanged);

    void OnFoodChanged(object param) => RefreshAffordable((int)param);

    void RefreshAffordable(int currentFood)
    {
        bool enough = currentFood >= foodCost;
        if (foodCostText != null) foodCostText.color = enough ? enoughColor : notEnoughColor;
        if (button != null) button.interactable = enough;
    }
}
