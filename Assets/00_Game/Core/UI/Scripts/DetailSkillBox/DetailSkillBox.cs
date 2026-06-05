using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetailSkillBox : BaseBox<DetailSkillBox>
{
    [Header("Buttons")]
    public Button btnClose;
    public Button btnUpgrade;
    public Button btnEquip;
    public Button btnRemove;

    [Header("Item / Info")]
    public Transform itemHolder;
    public TMP_Text txtName;
    public TMP_Text txtDes;

    private SkillData _data;
    private bool _fromEquipped;
    private SkillItem _item;

    protected override void Init()
    {
        btnClose.OnClicked(delegate { Close(); });
        btnUpgrade.OnClicked(OnUpgrade);
        btnEquip.OnClicked(OnEquip);
        btnRemove.OnClicked(OnRemove);
    }

    protected override void InitState() { }

    // fromEquipped: true nếu mở từ skill đang đeo -> hiện Remove; false -> hiện Equip
    public void SetData(SkillData data, bool fromEquipped)
    {
        _data = data;
        _fromEquipped = fromEquipped;
        Refresh();
    }

    void Refresh()
    {
        var db = DataRepo.Instance.skillDatabase;

        if (_item == null)
            _item = Instantiate(db.itemPrefab, itemHolder);   // 1 prefab skill item
        _item.gameObject.SetActive(true);
        _item.Init(_data, db.GetIcon(_data.id), null);
        _item.SetNew(false);
        _item.SetEquipped(false);
        _item.SetViewProgress(true);
        _item.SetButtonEnabled(false);
        _item.Refresh();

        var state = SkillSave.Get(_data.id);
        bool canUp = SkillUpgrade.CanUpgrade(_data);
        if (txtName != null) txtName.text = _data.name;
        if (txtDes != null) txtDes.text = _data.DescPreview(state.level, canUp);

        btnEquip.gameObject.SetActive(!_fromEquipped);
        btnRemove.gameObject.SetActive(_fromEquipped);
        btnUpgrade.interactable = SkillUpgrade.CanUpgrade(_data);
    }

    void OnUpgrade()
    {
        if (!SkillUpgrade.CanUpgrade(_data))
        {
            Debug.Log($"[DetailSkill] không nâng được {_data.id}");
            return;
        }
        SkillUpgrade.TryUpgrade(_data);
        Refresh();
        this.PostEvent(EventID.ON_SKILL_CHANGED);
    }

    void OnEquip()
    {
        SkillSave.Equip(_data.id);
        this.PostEvent(EventID.ON_SKILL_CHANGED);
        Close();
    }

    void OnRemove()
    {
        SkillSave.Unequip(_data.id);
        this.PostEvent(EventID.ON_SKILL_CHANGED);
        Close();
    }

    protected override void OnDestroy() => base.OnDestroy();
}