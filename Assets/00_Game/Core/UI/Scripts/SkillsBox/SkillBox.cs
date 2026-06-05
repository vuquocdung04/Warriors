using System.Collections.Generic;
using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillBox : BaseBox<SkillBox>
{
    [Header("Gacha")]
    public Button btnX1;
    public Button btnX10;
    public TMP_Text costText1;
    public TMP_Text costText10;
    public int costX1 = 0;
    public int costX10 = 0;

    [Header("Holders")]
    public Transform skillHolder;          // list skill đã sở hữu
    public List<Transform> selectedHolders; // ô skill đang đeo (tạm 1, list cho mở rộng)

    private readonly List<SkillItem> _owned = new();
    private readonly List<SkillItem> _selected = new();

    protected override void Init()
    {
        btnX1.OnClicked(() => OnSpin(1));
        btnX10.OnClicked(() => OnSpin(10));

        this.RegisterListener(EventID.ON_SKILL_CHANGED, OnSkillChanged);

        BuildOwned();
        BuildSelected();
        RefreshCost();
    }

    protected override void InitState() { }

    void OnSkillChanged(object param)
    {
        BuildOwned();
        BuildSelected();
    }

    void RefreshCost()
    {
        if (costText1 != null) _ = costText1.CountToWithIcon(costX1, "<sprite=19> ", duration: 0f);
        if (costText10 != null) _ = costText10.CountToWithIcon(costX10, "<sprite=19> ", duration: 0f);
    }

    void BuildOwned()
    {
        var db = DataRepo.Instance.skillDatabase;
        var all = db.AllSkills();

        foreach (var it in _owned) it.gameObject.SetActive(false);

        int idx = 0;
        foreach (var data in all)
        {
            var state = SkillSave.Get(data.id);
            if (!state.owned) continue;

            SkillItem item = idx < _owned.Count ? _owned[idx] : null;
            if (item == null)
            {
                item = Instantiate(db.itemPrefab, skillHolder);
                _owned.Add(item);
            }
            item.gameObject.SetActive(true);
            item.Init(data, db.GetIcon(data.id), OnOwnedClicked);
            item.SetNew(false);
            item.SetViewProgress(true);
            item.SetEquipped(state.equipped);
            item.Refresh();
            idx++;
        }
    }

    void BuildSelected()
    {
        var db = DataRepo.Instance.skillDatabase;
        var equipped = SkillSave.GetEquipped();   // list skill đang đeo

        foreach (var it in _selected) it.gameObject.SetActive(false);

        for (int i = 0; i < selectedHolders.Count; i++)
        {
            if (i >= equipped.Count) continue;   // slot này chưa có skill đeo

            var data = db.GetSkill(equipped[i].id);
            if (data == null) continue;

            SkillItem item = i < _selected.Count ? _selected[i] : null;
            if (item == null)
            {
                item = Instantiate(db.itemPrefab, selectedHolders[i]);
                _selected.Add(item);
            }
            else
            {
                item.transform.SetParent(selectedHolders[i], false);
            }
            item.gameObject.SetActive(true);
            item.Init(data, db.GetIcon(data.id), OnSelectedClicked);
            item.SetNew(false);
            item.SetEquipped(false);
            item.SetViewProgress(false);   // ô đeo: ẩn progress/new/equipped
            item.SetBg(false);

            item.Refresh();
        }
    }

    void OnOwnedClicked(SkillItem clicked)
    {
        var holder = LobbyController.Instance.topCanvas;
        _ = DetailSkillBox.Setup(holder, box => { box.Show(); box.SetData(clicked.Data, false); });
    }

    void OnSelectedClicked(SkillItem clicked)
    {
        var holder = LobbyController.Instance.topCanvas;
        _ = DetailSkillBox.Setup(holder, box => { box.Show(); box.SetData(clicked.Data, true); });
    }

    void OnSpin(int count)
    {
        var entries = new List<IGachaResultEntry>();
        for (int i = 0; i < count; i++)
        {
            var r = GachaSkillService.Spin();
            if (r != null) entries.Add(new SkillGachaEntry(r));
        }
        // KHÔNG post ở đây

        var holder = LobbyController.Instance.topCanvas;
        _ = GachaResultBox.Setup(holder, box => box.ShowResult(entries, EventID.ON_SKILL_CHANGED));
    }
    protected override void OnDestroy()
    {
        this.RemoveListener(EventID.ON_SKILL_CHANGED, OnSkillChanged);
        base.OnDestroy();
    }
}