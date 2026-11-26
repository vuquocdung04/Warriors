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

    [Header("Nâng bậc trong trận")]
    public Button upgradeButton;
    public Image upgradeButtonImage;
    public Image upgradeIcon;
    public TMP_Text upgradeCostText;
    public GameObject maxBadge;
    public Image[] tierPips;            // 3 chấm: gốc, bậc 1, bậc 2
    public Color upEnoughColor = new Color(80f / 255f, 210f / 255f, 90f / 255f);
    public Color upNotEnoughColor = new Color(150f / 255f, 155f / 255f, 170f / 255f);
    public Color pipOnColor = new Color(1f, 205f / 255f, 60f / 255f);
    public Color pipOffColor = new Color(25f / 255f, 70f / 255f, 120f / 255f);

    public int foodCost { get; private set; }
    public int Tier { get; private set; }

    private int _index;
    private UnitDisplay _display;
    private System.Action<int> _onClick;
    private System.Action<int> _onUpgrade;
    private int _food;

    int UpgradeCost => UnitTier.UpgradeCost(foodCost, Tier);
    bool IsMax => Tier >= UnitTier.MaxTier;
    bool Upgradable => _onUpgrade != null;   // unit không có art tiến hóa -> thẻ như cũ

    public void Setup(int index, UnitData data, System.Action<int> onClick, System.Action<int> onUpgrade = null, int tier = 0)
    {
        _index = index;
        foodCost = data.foodCost;
        _onClick = onClick;
        _onUpgrade = onUpgrade;

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
        // nút nâng bậc + chấm luôn nằm trên art unit
        if (upgradeButton != null) upgradeButton.transform.SetAsLastSibling();
        if (tierPips != null && tierPips.Length > 0 && tierPips[0] != null) tierPips[0].transform.parent.SetAsLastSibling();

        button.onClick.RemoveAllListeners();
        button.OnClicked(() => _onClick?.Invoke(_index));
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.OnClicked(() => _onUpgrade?.Invoke(_index));
        }
        this.RemoveListener(EventID.FOOD_CHANGED, OnFoodChanged);
        this.RegisterListener(EventID.FOOD_CHANGED, OnFoodChanged);

        _food = BottomBar.Instance != null && BottomBar.Instance.foodBar != null
            ? BottomBar.Instance.foodBar.Food : 0;
        SetTier(tier);
    }

    public Sprite GetPortrait(int tier) => _display != null ? _display.GetPortrait(tier) : null;

    public void SetTier(int tier)
    {
        Tier = Mathf.Clamp(tier, 0, UnitTier.MaxTier);
        if (_display != null) _display.SetTier(Tier);

        if (tierPips != null)
            for (int i = 0; i < tierPips.Length; i++)
                if (tierPips[i] != null) tierPips[i].color = i <= Tier ? pipOnColor : pipOffColor;
        if (tierPips != null && tierPips.Length > 0 && tierPips[0] != null)
            tierPips[0].transform.parent.gameObject.SetActive(Upgradable);

        if (upgradeButton != null) upgradeButton.gameObject.SetActive(Upgradable && !IsMax);
        if (maxBadge != null) maxBadge.SetActive(Upgradable && IsMax);
        RefreshAffordable(_food);
    }

    void OnDestroy() => this.RemoveListener(EventID.FOOD_CHANGED, OnFoodChanged);

    // Nút nâng cấp / MAX nằm ngoài thẻ (để nhô ra trên mép) -> thẻ ẩn thì ẩn theo
    void OnDisable()
    {
        if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
        if (maxBadge != null) maxBadge.SetActive(false);
    }

    void OnFoodChanged(object param) => RefreshAffordable((int)param);

    void RefreshAffordable(int currentFood)
    {
        _food = currentFood;
        bool enough = currentFood >= foodCost;
        if (foodCostText != null) foodCostText.color = enough ? enoughColor : notEnoughColor;
        if (buttonImage != null) buttonImage.color = enough ? btnEnoughColor : btnNotEnoughColor;

        if (!Upgradable || IsMax) return;
        bool canUp = currentFood >= UpgradeCost;
        if (upgradeCostText != null)
        {
            upgradeCostText.text = UpgradeCost.ToString();
            upgradeCostText.color = canUp ? enoughColor : notEnoughColor;
        }
        if (upgradeButtonImage != null) upgradeButtonImage.color = canUp ? upEnoughColor : upNotEnoughColor;
        if (upgradeIcon != null) upgradeIcon.color = canUp ? Color.white : new Color(0.75f, 0.75f, 0.75f, 1f);
    }
}
