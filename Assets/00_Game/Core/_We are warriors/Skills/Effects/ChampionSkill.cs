using System.Collections.Generic;

public class ChampionSkill : ISkillEffect
{
    const float MOVE_SPEED = 1f;
    const float ATTACK_SPEED = 0.75f;
    const float ATTACK_RANGE = 6f;
    const int FRONT_PRIORITY = 100;

    public void Activate(SkillData data, int level)
    {
        var allies = new List<Unit>(BattleManager.Instance.Allies);   // copy: sắp destroy
        if (allies.Count == 0) return;

        float sumAtk = 0f, sumHp = 0f;
        foreach (var u in allies)
        {
            if (u == null || !u.IsAlive) continue;
            sumAtk += u.Atk;
            sumHp += u.MaxHp;
        }
        if (sumHp <= 0f) return;

        float mul = data.ValueAt(level) / 100f;   // 110 -> 1.1
        var stats = new UnitStats
        {
            atk = sumAtk * mul,
            maxHp = sumHp * mul,
            hp = sumHp * mul,
            moveSpeed = MOVE_SPEED,
            attackSpeed = ATTACK_SPEED,
            attackRangeInCells = ATTACK_RANGE,
            frontPriority = FRONT_PRIORITY,
        };

        foreach (var u in allies)
            if (u != null && u.IsAlive) u.Kill(false);   // không rơi reward

        var prefab = DataRepo.Instance.skillDatabase.championPrefab;
        GamePlayController.Instance.spawner.SpawnChampion(prefab, stats);
    }
}