using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 1 ô skill trong gameplay: nút bấm + icon skill + cooldown riêng
public class BoosterSlot : MonoBehaviour
{
    public Button btnBooster;
    public Transform skillHolder;

    [Header("Cooldown")]
    public Image cooldownFill;      // Image Filled Radial360 phủ lên nút, chạy 1 -> 0 theo thời gian hồi
    public TMP_Text txtCooldown;

    private string _skillId;
    private float _cooldown;
    private float _cooldownLeft;
    private SkillItem _item;

    public void Init(string skillId, float cooldown)
    {
        _skillId = skillId;
        _cooldown = cooldown;

        var db = DataRepo.Instance.skillDatabase;
        var data = db.GetSkill(skillId);
        if (data == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        btnBooster.OnClicked(OnBooster);
        BuildIcon(data, db);
        SetCooldown(0f);
    }

    void BuildIcon(SkillData data, SkillDatabase db)
    {
        if (_item == null) _item = Instantiate(db.itemPrefab, skillHolder);
        _item.gameObject.SetActive(true);
        _item.Init(data, db.GetIcon(data.id), null);
        _item.SetNew(false);
        _item.SetEquipped(false);
        _item.SetViewProgress(false);
        _item.SetBg(false);
        _item.SetButtonEnabled(false);
    }

    void OnBooster()
    {
        if (_cooldownLeft > 0f || string.IsNullOrEmpty(_skillId)) return;
        if (SkillCastPresenter.IsPlaying) return;   // đang diễn thẻ skill khác
        if (BattleManager.Instance != null && (BattleManager.Instance.IsBattleOver || BattleManager.Instance.IsPaused)) return;

        var presenter = SkillCastPresenter.Instance;
        if (presenter == null) { Cast(); return; }

        // thẻ skill hiện -> giữ nhịp -> tan biến -> resume game rồi mới kích hoạt
        var db = DataRepo.Instance.skillDatabase;
        btnBooster.interactable = false;
        presenter.Play(db.GetSkill(_skillId), db.GetIcon(_skillId), Cast);
    }

    void Cast()
    {
        if (this == null) return;
        if (BattleManager.Instance != null && BattleManager.Instance.IsBattleOver) { btnBooster.interactable = true; return; }
        SkillController.Instance.Activate(_skillId);
        SetCooldown(_cooldown);
    }

    // Dùng thời gian game (scaled) để pause game thì cooldown cũng dừng
    void Update()
    {
        if (_cooldownLeft <= 0f) return;

        SetCooldown(_cooldownLeft - Time.deltaTime);
        if (_cooldownLeft <= 0f)
        {
            btnBooster.transform.DOKill(true);
            btnBooster.transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 6, 0.5f).SetLink(gameObject);
        }
    }

    void SetCooldown(float seconds)
    {
        _cooldownLeft = Mathf.Max(0f, seconds);
        bool cooling = _cooldownLeft > 0f;

        btnBooster.interactable = !cooling;
        cooldownFill.gameObject.SetActive(cooling);
        txtCooldown.gameObject.SetActive(cooling);
        if (!cooling) return;

        cooldownFill.fillAmount = _cooldownLeft / _cooldown;
        txtCooldown.text = Mathf.CeilToInt(_cooldownLeft).ToString();
    }
}
