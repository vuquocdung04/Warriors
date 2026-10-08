using System.Collections.Generic;
using UnityEngine;

// Sét đánh chết ngay {value} enemy gần nhà mình nhất (mỗi cấp +1 mục tiêu)
public class LightningSkill : ISkillEffect
{
    public void Activate(SkillData data, int level)
    {
        int count = Mathf.RoundToInt(data.ValueAt(level));
        if (count <= 0) return;

        var targets = new List<Unit>();
        foreach (var u in BattleManager.Instance.Enemies)
            if (u != null && u.IsAlive) targets.Add(u);

        // enemy đi từ phải sang trái -> x nhỏ nhất là con sắp chạm nhà mình
        targets.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        if (targets.Count > count) targets.RemoveRange(count, targets.Count - count);

        var fx = LightningFxSpawner.Instance;
        if (fx != null) fx.StrikeUnits(targets);
        else foreach (var u in targets) u.Kill(true);
    }
}
