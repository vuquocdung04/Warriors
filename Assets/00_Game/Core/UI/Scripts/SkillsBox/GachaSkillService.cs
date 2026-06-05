using UnityEngine;

public static class GachaSkillService
{
    public class SkillResult
    {
        public SkillData skill;
        public bool isFirstOwn;
    }

    public static SkillResult Spin()
    {
        var db = DataRepo.Instance.skillDatabase;
        var all = db.AllSkills();
        if (all.Count == 0) return null;

        var skill = all[Random.Range(0, all.Count)];   
        var state = SkillSave.Get(skill.id);

        bool firstOwn = !state.owned;
        if (firstOwn) state.owned = true;   
        else state.card += 1;               

        SkillSave.Save();
        return new SkillResult { skill = skill, isFirstOwn = firstOwn };
    }
}