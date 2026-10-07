using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterBar : MonoBehaviour
{
    public const float COOLDOWN = 10f;

    public Button btnBooster;
    public Transform skillHolder;

    [Header("Cooldown")]
    public Image cooldownFill;      // Image Filled Radial360 phủ lên nút, chạy 1 -> 0 theo thời gian hồi
    public TMP_Text txtCooldown;

    private readonly List<SkillItem> _skills = new();
    private float _cooldownLeft;

    public void Init()
    {
        var equipped = SkillSave.GetEquipped();
        if (equipped.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }
        btnBooster.OnClicked(OnBooster);
        BuildEquippedSkills(equipped);
        SetCooldown(0f);
    }

    void OnBooster()
    {
        if (_cooldownLeft > 0f) return;

        var equipped = SkillSave.GetEquipped();
        if (equipped.Count == 0) return;
        SkillController.Instance.Activate(equipped[0].id);
        SetCooldown(COOLDOWN);
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

        cooldownFill.fillAmount = _cooldownLeft / COOLDOWN;
        txtCooldown.text = Mathf.CeilToInt(_cooldownLeft).ToString();
    }

    void BuildEquippedSkills(List<SkillState> equipped)
    {
        var db = DataRepo.Instance.skillDatabase;

        foreach (var it in _skills) it.gameObject.SetActive(false);

        for (int i = 0; i < equipped.Count; i++)
        {
            var data = db.GetSkill(equipped[i].id);
            if (data == null) continue;

            SkillItem item = i < _skills.Count ? _skills[i] : null;
            if (item == null)
            {
                item = Instantiate(db.itemPrefab, skillHolder);
                _skills.Add(item);
            }
            item.gameObject.SetActive(true);
            item.Init(data, db.GetIcon(data.id), null);
            item.SetNew(false);
            item.SetEquipped(false);
            item.SetViewProgress(false);
            item.SetBg(false);
            item.SetButtonEnabled(false);
        }
    }
}
