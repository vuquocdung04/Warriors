using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitCard : MonoBehaviour
{
    public Button button;
    public TMP_Text foodCostText;
    public Transform displayParent;   // sinh UnitDisplay vào đây

    [Header("Màu chữ")]
    public Color enoughColor = Color.white;
    public Color notEnoughColor = Color.red;

    [Header("Màu nút theo food")]
    public Image buttonImage;
    public Color btnEnoughColor = new Color(1f / 255f, 163f / 255f, 255f / 255f);   // #01A3FF
    public Color btnNotEnoughColor = new Color(60f / 255f, 66f / 255f, 90f / 255f); // #3C425A

    public int foodCost { get; private set; }

    private int _index;
    private UnitDisplay _display;
    private System.Action<int> _onClick;

    public void Setup(int index, UnitData data, System.Action<int> onClick)
    {
        _index = index;
        foodCost = data.foodCost;
        _onClick = onClick;

        foodCostText.text = foodCost.ToString();

        // sinh UnitDisplay làm con
        if (_display != null) Destroy(_display.gameObject);
        var prefab = DataRepo.Instance.unitDatabase.GetDisplayById(data.id);
        if (prefab != null)
        {
            _display = Instantiate(prefab, displayParent);
            var rt = _display.transform as RectTransform;
            if (rt != null) rt.anchoredPosition = Vector2.zero;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onClick?.Invoke(_index));
        this.RemoveListener(EventID.FOOD_CHANGED, OnFoodChanged);
        this.RegisterListener(EventID.FOOD_CHANGED, OnFoodChanged);

        int food = BottomBar.Instance != null && BottomBar.Instance.foodBar != null
            ? BottomBar.Instance.foodBar.Food : 0;
        RefreshAffordable(food);
    }

    void OnDestroy() => this.RemoveListener(EventID.FOOD_CHANGED, OnFoodChanged);

    void OnFoodChanged(object param) => RefreshAffordable((int)param);

    void RefreshAffordable(int currentFood)
    {
        bool enough = currentFood >= foodCost;
        if (foodCostText != null) foodCostText.color = enough ? enoughColor : notEnoughColor;
        if (buttonImage != null) buttonImage.color = enough ? btnEnoughColor : btnNotEnoughColor;
    }
}