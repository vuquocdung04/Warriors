using System.Collections.Generic;
using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetailEquipBox : BaseBox<DetailEquipBox>
{
    [Header("Buttons")]
    public Button btnClose;
    public Button btnUpgrade;
    public Button btnEquip;
    public Button btnRemove;

    [Header("Item / Info")]
    public Transform itemHolder;
    public Transform statsHolder;
    public StatItem statItemPrefab;
    public TMP_Text txtName;
    public TMP_Text txtRank;

    private EquipmentData _data;
    private EquipType _type;
    private bool _fromEquipped;     // mở từ top (đang đeo) = true -> hiện Remove
    private EquipmentItem _item;
    private readonly List<StatItem> _stats = new();

    protected override void Init()
    {
        btnClose.OnClicked(delegate { Close(); });
        btnUpgrade.OnClicked(OnUpgrade);
        btnEquip.OnClicked(OnEquip);
        btnRemove.OnClicked(OnRemove);
    }

    protected override void InitState() { }

    // fromEquipped: true nếu bấm ở top (món đang đeo) -> hiện Remove; false nếu bấm bottom -> hiện Equip
    public void SetData(EquipmentData data, EquipType type, bool fromEquipped)
    {
        _data = data;
        _type = type;
        _fromEquipped = fromEquipped;
        Refresh();
    }

    void Refresh()
    {
        var db = DataRepo.Instance.equipmentDatabase;

        if (_item == null)
            _item = Instantiate(GetItemPrefab(), itemHolder);
        _item.gameObject.SetActive(true);
        _item.Init(_data, _type, GetIcon(_data.id), null);
        _item.SetNew(false);
        _item.SetEquipped(false);
        _item.SetViewProgress(true);
        _item.SetButtonEnabled(false);
        _item.Refresh();

        if (txtName != null) txtName.text = _data.name;
        if (txtRank != null) { txtRank.text = _data.rank; txtRank.color = db.GetRankColor(_data.rank); }

        ShowStats();

        // Equip/Remove theo chỗ mở
        btnEquip.gameObject.SetActive(!_fromEquipped);
        btnRemove.gameObject.SetActive(_fromEquipped);

        // Upgrade: khả dụng thì bật
        btnUpgrade.interactable = EquipmentUpgrade.CanUpgrade(_type, _data);
    }

    void ShowStats()
    {
        int level = EquipmentSave.Get(_type, _data.id).level;
        foreach (var s in _stats) s.gameObject.SetActive(false);

        for (int i = 0; i < _data.stats.Count; i++)
        {
            StatItem item = i < _stats.Count ? _stats[i] : null;
            if (item == null)
            {
                item = Instantiate(statItemPrefab, statsHolder);
                _stats.Add(item);
            }
            item.gameObject.SetActive(true);
            item.Setup(_data.stats[i], level);
        }
    }

    void OnUpgrade()
    {
        if (!EquipmentUpgrade.CanUpgrade(_type, _data))
        {
            Debug.Log($"[Detail] không nâng được {_data.id} (thiếu card hoặc max)");   // xử lý sau
            return;
        }
        EquipmentUpgrade.TryUpgrade(_type, _data);
        Refresh();
        this.PostEvent(EventID.ON_EQUIPMENT_CHANGED);
    }

    void OnEquip()
    {
        SetEquippedId(_data.id);
        EquipmentSave.Save();
        this.PostEvent(EventID.ON_EQUIPMENT_CHANGED);
        Close();
    }

    void OnRemove()
    {
        SetEquippedId("");
        EquipmentSave.Save();
        this.PostEvent(EventID.ON_EQUIPMENT_CHANGED);
        Close();
    }

    string GetEquippedId()
    {
        switch (_type)
        {
            case EquipType.Melee: return UseProfile.EquippedMelee.Value;
            case EquipType.Range: return UseProfile.EquippedRange.Value;
            default: return UseProfile.EquippedShield.Value;
        }
    }

    void SetEquippedId(string id)
    {
        switch (_type)
        {
            case EquipType.Melee: UseProfile.EquippedMelee.Value = id; break;
            case EquipType.Range: UseProfile.EquippedRange.Value = id; break;
            default: UseProfile.EquippedShield.Value = id; break;
        }
    }

    EquipmentItem GetItemPrefab()
    {
        var db = DataRepo.Instance.equipmentDatabase;
        switch (_type)
        {
            case EquipType.Melee: return db.GetMeleeItemPrefab();
            case EquipType.Range: return db.GetRangeItemPrefab();
            default: return db.GetShieldItemPrefab();
        }
    }

    Sprite GetIcon(string id)
    {
        var db = DataRepo.Instance.equipmentDatabase;
        switch (_type)
        {
            case EquipType.Melee: return db.GetMeleeIcon(id);
            case EquipType.Range: return db.GetRangeIcon(id);
            default: return db.GetShieldIcon(id);
        }
    }

    protected override void OnDestroy() => base.OnDestroy();
}