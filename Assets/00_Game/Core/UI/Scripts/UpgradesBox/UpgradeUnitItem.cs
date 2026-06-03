using EventDispatcher;
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
    private bool _listenerReady;

    public UnitData Data => _data;
    public int Index { get; private set; }

    public void Setup(UnitData data, bool unlocked, System.Action<UpgradeUnitItem> onBuy, int index)
    {
        _data = data;
        _onBuy = onBuy;
        Index = index;

        if (!_listenerReady)   // gắn 1 lần
        {
            buyButton.OnClicked(() => _onBuy?.Invoke(this));
            this.RegisterListener(EventID.ON_EQUIPMENT_CHANGED, OnEquipChanged);
            _listenerReady = true;
        }

        if (_display != null) Destroy(_display.gameObject);
        var prefab = DataRepo.Instance.unitDatabase.GetDisplayById(data.id);
        if (prefab != null)
        {
            _display = Instantiate(prefab, displayParent);
            var rt = _display.transform as RectTransform;
            rt.anchoredPosition = Vector2.zero;
        }

        nameText.text = data.displayName;
        _ = buyPriceText.CountTo(data.buyPrice, 0f);

        RefreshStats();
        SetUnlocked(unlocked);
    }

    void OnEquipChanged(object param) => RefreshStats();

    void RefreshStats()
    {
        var statsList = DataRepo.Instance.equipmentDatabase.BuildStats(UseProfile.CurrentCiv.Value);
        if (Index >= 0 && Index < statsList.Count)
        {
            var s = statsList[Index];
            _ = attackText.CountTo(s.atk, 0f);
            _ = hpText.CountTo(s.maxHp, 0f);
        }
    }

    public void SetUnlocked(bool unlocked)
    {
        unlockObject.SetActive(unlocked);
        lockObject.SetActive(!unlocked);
        _display.SetBlind(!unlocked);
    }

    void OnDestroy()
    {
        this.RemoveListener(EventID.ON_EQUIPMENT_CHANGED, OnEquipChanged);
    }
}