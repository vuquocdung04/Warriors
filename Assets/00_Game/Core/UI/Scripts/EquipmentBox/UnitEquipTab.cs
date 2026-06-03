using System.Collections.Generic;
using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitEquipTab : MonoBehaviour
{
    public int slotIndex;

    [Header("TOP - Unit display")]
    public Transform cardUnitHolder;

    [Header("TOP - trạng thái trang bị")]
    public GameObject emptyEquipment;
    public GameObject notEmptyEquipment;

    [Header("TOP - NotEmpty content")]
    public Transform slotEquip;
    public TMP_Text txtName;
    public TMP_Text txtRank;
    public Transform statsHolder;
    public StatItem statItemPrefab;

    [Header("BOTTOM - list món sở hữu")]
    public Transform ownedHolder;

    private UnitDisplay _display;
    private EquipmentItem _equipItem;
    private readonly List<StatItem> _stats = new();
    private readonly List<EquipmentItem> _owned = new();

    EquipType SlotType =>
        slotIndex == 0 ? EquipType.Melee :
        slotIndex == 1 ? EquipType.Range : EquipType.Shield;

    public void Init(int index)
    {
        slotIndex = index;
        this.RegisterListener(EventID.ON_EQUIPMENT_CHANGED, OnEquipChanged);
        BuildUnitDisplay();
        RefreshEquip();
        BuildOwned();
    }

    void OnEquipChanged(object param)
    {
        RefreshEquip();
        BuildOwned();
    }

    void BuildUnitDisplay()
    {
        var units = DataRepo.Instance.unitDatabase.GetCivUnits(UseProfile.CurrentCiv.Value);
        if (slotIndex >= units.Count) return;

        string unitId = units[slotIndex].id;
        var prefab = DataRepo.Instance.unitDatabase.GetDisplayById(unitId);
        if (prefab == null) return;

        if (_display != null) Destroy(_display.gameObject);
        _display = Instantiate(prefab, cardUnitHolder);
        _display.transform.localPosition = Vector3.zero;
    }

    public void RefreshEquip()
    {
        string equipId = GetEquippedId();
        bool hasEquip = !string.IsNullOrEmpty(equipId);

        emptyEquipment.SetActive(!hasEquip);
        notEmptyEquipment.SetActive(hasEquip);
        if (!hasEquip) return;

        EquipmentData data = GetEquipData(equipId);
        if (data == null) { emptyEquipment.SetActive(true); notEmptyEquipment.SetActive(false); return; }

        ShowEquipItem(data, equipId);
        ShowEquipInfo(data);
        ShowStats(data);
    }

    void ShowEquipItem(EquipmentData data, string equipId)
    {
        if (_equipItem == null)
        {
            var prefab = GetItemPrefab();
            if (prefab == null) return;
            _equipItem = Instantiate(prefab, slotEquip);
        }
        _equipItem.gameObject.SetActive(true);
        _equipItem.Init(data, SlotType, GetIcon(equipId), OnEquippedClicked);   // top: bấm được -> mở detail (Remove)
        _equipItem.SetNew(false);
        _equipItem.SetEquipped(false);
        _equipItem.SetViewProgress(true);
        _equipItem.Refresh();
    }

    void ShowEquipInfo(EquipmentData data)
    {
        var db = DataRepo.Instance.equipmentDatabase;
        if (txtName != null) txtName.text = data.name;
        if (txtRank != null) { txtRank.text = data.rank; txtRank.color = db.GetRankColor(data.rank); }
    }

    void ShowStats(EquipmentData data)
    {
        int level = EquipmentSave.Get(SlotType, data.id).level;
        foreach (var s in _stats) s.gameObject.SetActive(false);

        for (int i = 0; i < data.stats.Count; i++)
        {
            StatItem item = i < _stats.Count ? _stats[i] : null;
            if (item == null)
            {
                item = Instantiate(statItemPrefab, statsHolder);
                _stats.Add(item);
            }
            item.gameObject.SetActive(true);
            item.Setup(data.stats[i], level);
        }
    }

    void BuildOwned()
    {
        var all = GetAllOfType();
        string equippedId = GetEquippedId();

        foreach (var it in _owned) it.gameObject.SetActive(false);

        int idx = 0;
        foreach (var data in all)
        {
            if (EquipmentSave.Get(SlotType, data.id).card <= 0) continue;

            EquipmentItem item = idx < _owned.Count ? _owned[idx] : null;
            if (item == null)
            {
                item = Instantiate(GetItemPrefab(), ownedHolder);
                _owned.Add(item);
            }

            item.gameObject.SetActive(true);
            item.Init(data, SlotType, GetIcon(data.id), OnOwnedClicked);   // bottom: bấm -> mở detail (Equip)
            item.SetNew(false);
            item.SetViewProgress(true);
            item.SetEquipped(data.id == equippedId);
            item.Refresh();
            idx++;
        }
    }

    // bấm món bottom -> detail với Equip
    void OnOwnedClicked(EquipmentItem clicked)
    {
        var holder = LobbyController.Instance.topCanvas;
        _ = DetailEquipBox.Setup(holder, box => { box.Show(); box.SetData(clicked.Data, SlotType, false); });
    }

    // bấm món top (đang đeo) -> detail với Remove
    void OnEquippedClicked(EquipmentItem clicked)
    {
        var holder = LobbyController.Instance.topCanvas;
        _ = DetailEquipBox.Setup(holder, box => { box.Show(); box.SetData(clicked.Data, SlotType, true); });
    }

    string GetEquippedId()
    {
        switch (slotIndex)
        {
            case 0: return UseProfile.EquippedMelee.Value;
            case 1: return UseProfile.EquippedRange.Value;
            default: return UseProfile.EquippedShield.Value;
        }
    }

    EquipmentData GetEquipData(string id)
    {
        var db = DataRepo.Instance.equipmentDatabase;
        switch (slotIndex)
        {
            case 0: return db.GetMelee(id);
            case 1: return db.GetRange(id);
            default: return db.GetShield(id);
        }
    }

    List<EquipmentData> GetAllOfType()
    {
        var db = DataRepo.Instance.equipmentDatabase;
        switch (slotIndex)
        {
            case 0: return db.AllMelee();
            case 1: return db.AllRange();
            default: return db.AllShield();
        }
    }

    EquipmentItem GetItemPrefab()
    {
        var db = DataRepo.Instance.equipmentDatabase;
        switch (slotIndex)
        {
            case 0: return db.GetMeleeItemPrefab();
            case 1: return db.GetRangeItemPrefab();
            default: return db.GetShieldItemPrefab();
        }
    }

    Sprite GetIcon(string id)
    {
        var db = DataRepo.Instance.equipmentDatabase;
        switch (slotIndex)
        {
            case 0: return db.GetMeleeIcon(id);
            case 1: return db.GetRangeIcon(id);
            default: return db.GetShieldIcon(id);
        }
    }

    void OnDestroy()
    {
        this.RemoveListener(EventID.ON_EQUIPMENT_CHANGED, OnEquipChanged);
    }
}