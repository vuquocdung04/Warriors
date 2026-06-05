using System.Collections.Generic;
using UnityEngine;

public class PushSkill : ISkillEffect
{
    public float slideSpeed = 4f;   
    public int pushCells = 2;

    public void Activate(SkillData data, int level)
    {
        float percent = data.ValueAt(level) / 100f;     
        float unit1Atk = GetUnit1Atk();
        float dmg = unit1Atk * percent;

        var enemies = BattleManager.Instance.Enemies;

        var list = new List<Unit>(enemies);
        list.Sort((a, b) => b.CurrentCell.x.CompareTo(a.CurrentCell.x));

        foreach (var u in list)
        {
            if (u == null || !u.IsAlive) continue;
            u.PushBack(pushCells, slideSpeed);
            u.TakeDamage(dmg);
        }
    }

    float GetUnit1Atk()
    {
        var stats = EquipmentStatCalculator.BuildStats(UseProfile.CurrentCiv.Value);
        return stats.Count > 0 ? stats[0].atk : 0f;  
    }
}