using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// Triệu hồi hiệp sĩ: gộp sức mạnh toàn bộ lính phe mình thành 1 champion.
// Diễn: pause trận (không ai chết) -> lính trượt tụm lại 1 điểm, thu nhỏ dần -> sét đánh xuống điểm đó
// -> đúng lúc sét chạm lính "hợp thể" biến mất -> hiệp sĩ bật ra tại chỗ -> resume.
public class ChampionSkill : ISkillEffect
{
    const float MOVE_SPEED = 1f;
    const float ATTACK_SPEED = 0.75f;
    const float ATTACK_RANGE = 6f;
    const int FRONT_PRIORITY = 100;

    // nhịp diễn (giây, không phụ thuộc pause)
    const float GATHER_TIME = 0.38f;
    const float GATHER_SPREAD = 0.35f;   // lính tụm quanh điểm trong bán kính này (world unit)
    const float GATHER_SHRINK = 0.75f;
    const float POP_TIME = 0.35f;
    const float AFTER_POP = 0.15f;

    public void Activate(SkillData data, int level)
    {
        var allies = new List<Unit>();
        foreach (var u in BattleManager.Instance.Allies)
            if (u != null && u.IsAlive) allies.Add(u);
        if (allies.Count == 0) return;

        float sumAtk = 0f, sumHp = 0f;
        foreach (var u in allies)
        {
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

        PlaySummon(allies, stats);
    }

    static void PlaySummon(List<Unit> allies, UnitStats stats)
    {
        // pause cả trận: lính 2 phe đứng im, không ai chết trong lúc diễn
        GameFlow.Instance.RequestPause();

        Vector3 point = GatherPoint(allies);
        var seq = DOTween.Sequence().SetUpdate(true);

        // 1. lính trượt tụm lại, thu nhỏ dần (lệch nhịp nhẹ cho tự nhiên)
        for (int i = 0; i < allies.Count; i++)
        {
            var u = allies[i];
            Vector3 target = point + (Vector3)(Random.insideUnitCircle * GATHER_SPREAD);
            float delay = Random.Range(0f, 0.08f);
            seq.Insert(delay, u.transform.DOMove(target, GATHER_TIME).SetEase(Ease.InBack, 1.3f).SetLink(u.gameObject));
            seq.Insert(delay, u.transform.DOScale(u.transform.localScale * GATHER_SHRINK, GATHER_TIME).SetEase(Ease.InQuad).SetLink(u.gameObject));
        }

        // 2. sét đánh xuống điểm tụm -> chạm thì hợp thể + hiệp sĩ xuất hiện
        seq.AppendCallback(() =>
        {
            var fx = LightningFxSpawner.Instance;
            if (fx != null) fx.StrikePoint(point, 0, () => Merge(allies, stats, point));
            else Merge(allies, stats, point);
        });
    }

    static void Merge(List<Unit> allies, UnitStats stats, Vector3 point)
    {
        foreach (var u in allies)
            if (u != null && u.IsAlive) u.Kill(false);   // không rơi reward

        var prefab = DataRepo.Instance.skillDatabase.championPrefab;
        var champ = GamePlayController.Instance.spawner.SpawnChampionAt(prefab, stats, point);
        if (champ == null)
        {
            GameFlow.Instance.RequestResume();
            return;
        }

        // 3. hiệp sĩ bật ra từ nhỏ -> vượt cỡ -> về, rồi mới resume
        var t = champ.transform;
        Vector3 baseScale = t.localScale;
        t.localScale = Vector3.zero;
        DOTween.Sequence().SetUpdate(true).SetLink(champ.gameObject)
            .Append(t.DOScale(baseScale, POP_TIME).SetEase(Ease.OutBack, 2.5f))
            .Join(t.DOPunchPosition(Vector3.up * 0.25f, POP_TIME, 6, 0.5f))
            .AppendInterval(AFTER_POP)
            .OnKill(() => GameFlow.Instance.RequestResume());   // OnKill: kể cả khi champion bị huỷ giữa chừng vẫn resume
    }

    // Giữa đội hình theo X (trung bình), giữa làn theo Y
    static Vector3 GatherPoint(List<Unit> allies)
    {
        float x = 0f;
        foreach (var u in allies) x += u.transform.position.x;
        x /= allies.Count;

        var grid = BattleGrid.Instance;
        float y = grid.transform.position.y + grid.Height * grid.cellSize * 0.5f;
        return new Vector3(x, y, 0f);
    }
}
