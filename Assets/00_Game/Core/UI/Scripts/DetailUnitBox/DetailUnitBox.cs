using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Bảng chỉ số unit (đã cộng trang bị đang đeo) — mở khi bấm vào card ở tab Upgrades
public class DetailUnitBox : BaseBox<DetailUnitBox>
{
    [Header("Buttons")]
    public Button btnClose;

    [Header("Info")]
    public Transform displayHolder;
    public TMP_Text txtName;
    public TMP_Text txtType;          // Melee / Ranged
    public TMP_Text txtMainStats;     // atk, hp, tốc đánh, tốc chạy, tầm
    public TMP_Text txtSpecialLeft;   // chí mạng, hút máu, đẩy lùi
    public TMP_Text txtSpecialRight;  // độc, cháy, đóng băng

    const string LABEL = "#C9CEE6";
    const float VALUE_POS = 55f;      // % chiều rộng dòng nơi bắt đầu cột giá trị

    private UnitData _data;
    private int _index;
    private UnitDisplay _display;

    protected override void Init()
    {
        btnClose.OnClicked(delegate { Close(); });
        this.RegisterListener(EventID.ON_EQUIPMENT_CHANGED, OnEquipChanged);
    }

    protected override void InitState() { }

    // index: vị trí unit trong civ hiện tại (0..2), dùng để lấy stat đã cộng trang bị
    public void SetData(UnitData data, int index)
    {
        _data = data;
        _index = index;
        BuildDisplay();
        Refresh();
    }

    void OnEquipChanged(object param) { if (_data != null) Refresh(); }

    void BuildDisplay()
    {
        if (_display != null) Destroy(_display.gameObject);
        var prefab = DataRepo.Instance.unitDatabase.GetDisplayById(_data.id);
        if (prefab == null) return;

        _display = Instantiate(prefab, displayHolder);
        ((RectTransform)_display.transform).anchoredPosition = Vector2.zero;
        _display.SetBlind(false);
    }

    void Refresh()
    {
        var list = EquipmentStatCalculator.BuildStats(UseProfile.CurrentCiv.Value);
        var s = _index >= 0 && _index < list.Count ? list[_index] : new UnitStats(_data);

        txtName.text = _data.displayName;
        if (txtType != null) txtType.text = s.atkType == AtkType.Melee ? "Melee" : "Ranged";

        txtMainStats.text =
            Line("Attack", Num(s.atk)) +
            Line("Health", Num(s.maxHp)) +
            Line("Attack Speed", Num(s.attackSpeed) + "/s") +
            Line("Move Speed", Num(s.moveSpeed)) +
            Line("Range", Num(s.attackRangeInCells), last: true);

        txtSpecialLeft.text =
            Line("Critical", Pct(s.criticalChance)) +
            Line("Life Steal", Pct(s.lifeSteal)) +
            Line("Push", Pct(s.pushChance), last: true);

        txtSpecialRight.text =
            Line("Poison", Pct(s.poisonChance)) +
            Line("Burn", Pct(s.burnChance)) +
            Line("Freeze", Pct(s.freezeChance), last: true);
    }

    static string Line(string label, string value, bool last = false) =>
        $"<color={LABEL}>{label}</color><pos={VALUE_POS}%>{value}" + (last ? "" : "\n");

    // bỏ .0 thừa: 10 -> "10", 1.25 -> "1.25"
    static string Num(float v) => v % 1 == 0 ? ((int)v).ToString() : v.ToString("0.##");
    static string Pct(float rate) => Num(rate * 100f) + "%";

    protected override void OnDestroy()
    {
        this.RemoveListener(EventID.ON_EQUIPMENT_CHANGED, OnEquipChanged);
        base.OnDestroy();
    }
}
