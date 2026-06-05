using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoosterBar : MonoBehaviour
{
    public Button btnBooster;
    public Transform skillHolder;
    private readonly List<SkillItem> _skills = new();
    public void Init()
    {
        btnBooster.OnClicked(OnBooster);
        BuildEquippedSkills();
    }

    void OnBooster()
    {
        Debug.Log("[Booster] clicked");   // TODO
        btnBooster.enabled = false;
    }

    void BuildEquippedSkills()
    {
        var db = DataRepo.Instance.skillDatabase;
        var equipped = SkillSave.GetEquipped();

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