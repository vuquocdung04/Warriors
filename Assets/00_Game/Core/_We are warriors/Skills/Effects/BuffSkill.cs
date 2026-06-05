using UnityEngine;

public class BuffSkill : ISkillEffect
{
    public void Activate(SkillData data, int level)
    {
        float mul = 1f + data.ValueAt(level) / 100f;
        GamePlayController.Instance.spawner.EnqueueNextAllyModifier(new SpawnModifier
        {
            ModifyStats = s => { s.atk *= mul; s.maxHp *= mul; s.hp = s.maxHp; },
            AfterSpawn = u => u.transform.localScale = Vector3.one * 1.5f,
        });
    }
}