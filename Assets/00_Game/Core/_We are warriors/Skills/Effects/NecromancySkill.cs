using UnityEngine;

public class NecromancySkill : ISkillEffect
{
    const float MOVE_SPEED = 1.25f;
    const float ATTACK_SPEED = 1f;
    const float ATTACK_RANGE = 3f;
    const int FRONT_PRIORITY = 0;
    const float HP_PERCENT = 0.5f; 

    public void Activate(SkillData data, int level)
    {
        int count = Mathf.RoundToInt(data.ValueAt(level)); 
        if (count <= 0) return;

        var unit1 = EquipmentStatCalculator.BuildStats(UseProfile.CurrentCiv.Value);
        if (unit1.Count == 0) return;

        float atk = unit1[0].atk;               
        float hp = unit1[0].maxHp * HP_PERCENT;   

        var template = new UnitStats
        {
            atk = atk,
            maxHp = hp,
            hp = hp,
            moveSpeed = MOVE_SPEED,
            attackSpeed = ATTACK_SPEED,
            attackRangeInCells = ATTACK_RANGE,
            frontPriority = FRONT_PRIORITY,
        };

        var prefab = DataRepo.Instance.skillDatabase.zombiePrefab;
        var spawner = GamePlayController.Instance.spawner;

        for (int i = 0; i < count; i++)
            spawner.SpawnZombie(prefab, template); 
    }
}