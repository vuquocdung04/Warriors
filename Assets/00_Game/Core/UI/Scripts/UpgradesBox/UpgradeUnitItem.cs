using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUnitItem : MonoBehaviour
{
    [Header("State")]
    public GameObject unlockObject;
    public GameObject lockObject;

    [Header("Unlock - info")]
    public TMP_Text nameText;
    public TMP_Text attackText;
    public TMP_Text hpText;

    [Header("Lock - mua")]
    public TMP_Text buyPriceText;
    public Button buyButton;

    [Header("Hiển thị")]
    public Transform displayParent;

    private UnitData _data;
    private UnitDisplay _display;
    private System.Action<UpgradeUnitItem> _onBuy;

    public UnitData Data => _data;
    public int Index { get; private set; }

    public void Setup(UnitData data, bool unlocked, System.Action<UpgradeUnitItem> onBuy, int index)
    {
        _data = data;
        _onBuy = onBuy;
        Index = index;

        if (_display != null) Destroy(_display.gameObject);
        var prefab = DataRepo.Instance.unitDatabase.GetDisplayById(data.id);
        if (prefab != null)
        {
            _display = Instantiate(prefab, displayParent);
            
            var rt = _display.transform as RectTransform;
            rt.anchoredPosition = Vector2.zero; 
        }

        nameText.text = data.displayName;
        _ = attackText.CountTo(data.atk, 0f);
        _ = hpText.CountTo(data.hp, 0f);
        _ = buyPriceText.CountTo(data.buyPrice, 0f);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => _onBuy?.Invoke(this));

        SetUnlocked(unlocked);
    }

    public void SetUnlocked(bool unlocked)
    {
        unlockObject.SetActive(unlocked);
        lockObject.SetActive(!unlocked);
        _display.SetBlind(!unlocked);
    }
}