using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentSlot : MonoBehaviour
{
    [Header("State")]
    public GameObject lockObject;
    public TMP_Text lockText;
    public GameObject unlockObject;

    [Header("Item")]
    public Transform itemParent;

    [Header("Button")]
    public Button button;

    [Header("Config")]
    public int unlockCivOrder = 5;

    private int _slotIndex;          // nhận từ Bar (0=melee 1=range 2=shield)
    private EquipmentItem _item;
    private System.Action<int> _onClick;

    public void Init(int slotIndex, System.Action<int> onClick)
    {
        _slotIndex = slotIndex;
        _onClick = onClick;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onClick?.Invoke(_slotIndex));
        Refresh();
    }

    public void Refresh()
    {
        int civOrder = DataRepo.Instance.unitDatabase.GetCivOrder(UseProfile.CurrentCiv.Value);

        if (civOrder < unlockCivOrder)
        {
            lockObject.SetActive(true);
            unlockObject.SetActive(false);
            HideItem();
            if (lockText != null) lockText.text = $"Civ {unlockCivOrder}";
            button.interactable = false;          
            return;
        }

        lockObject.SetActive(false);
        button.interactable = true;              

        string equippedId = GetEquippedId();
        if (string.IsNullOrEmpty(equippedId))
        {
            unlockObject.SetActive(true);
            HideItem();
        }
        else
        {
            unlockObject.SetActive(false);
            ShowItem(equippedId);
        }
    }

    void ShowItem(string equipId)
    {
        var data = GetEquipData(equipId);
        if (data == null) { HideItem(); return; }

        if (_item != null && _item.id != equipId)
        {
            Destroy(_item.gameObject);
            _item = null;
        }

        if (_item == null)
        {
            var prefab = GetItemPrefab(equipId);
            if (prefab == null) { HideItem(); return; }
            _item = Instantiate(prefab, itemParent);
        }

        _item.gameObject.SetActive(true);
        _item.Init(data, null);
        _item.SetNew(false);
        _item.SetEquipped(false);
        _item.SetViewProgress(false);
    }

    void HideItem()
    {
        if (_item != null) _item.gameObject.SetActive(false);
    }

    string GetEquippedId()
    {
        switch (_slotIndex)
        {
            case 0: return UseProfile.EquippedMelee.Value;
            case 1: return UseProfile.EquippedRange.Value;
            default: return UseProfile.EquippedShield.Value;
        }
    }

    EquipmentData GetEquipData(string id)
    {
        var db = DataRepo.Instance.equipmentDatabase;
        switch (_slotIndex)
        {
            case 0: return db.GetMelee(id);
            case 1: return db.GetRange(id);
            default: return db.GetShield(id);
        }
    }

    EquipmentItem GetItemPrefab(string id)
    {
        var db = DataRepo.Instance.equipmentDatabase;
        switch (_slotIndex)
        {
            case 0: return db.GetMeleeItemPrefab(id);
            case 1: return db.GetRangeItemPrefab(id);
            default: return db.GetShieldItemPrefab(id);
        }
    }
}