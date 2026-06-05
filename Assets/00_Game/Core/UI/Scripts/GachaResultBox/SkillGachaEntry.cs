using UnityEngine;

public class SkillGachaEntry : IGachaResultEntry
{
    private readonly GachaSkillService.SkillResult _res;
    public SkillGachaEntry(GachaSkillService.SkillResult res) => _res = res;

    public bool IsNew => _res.isFirstOwn;

    public GameObject Spawn(Transform holder)
    {
        var db = DataRepo.Instance.skillDatabase;
        var item = Object.Instantiate(db.itemPrefab, holder);
        item.Init(_res.skill, db.GetIcon(_res.skill.id), null);
        item.SetEquipped(false);
        item.SetViewProgress(false);
        item.SetButtonEnabled(false);
        item.SetNew(_res.isFirstOwn);
        return item.gameObject;
    }
}