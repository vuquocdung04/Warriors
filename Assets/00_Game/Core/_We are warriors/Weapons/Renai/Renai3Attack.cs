using UnityEngine;
using DG.Tweening;

public class Renai3Attack : AttackStrategyBase
{
    [Header("Weapon")]
    public Transform weapon;

    [Header("Projectile")]
    public Projectile projectilePrefab;
    public Transform firePoint;
    public float arcDuration = 0.6f;  
    public float arcHeight = 2f;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon);
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float lift = duration * 0.35f;
        float hold = duration * 0.15f;
        float recoil = duration * 0.1f;
        float ret = duration * 0.4f;

        // nâng lên 25
        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, 25f), lift));

        // hold rồi bắn
        seq.AppendInterval(hold);
        seq.AppendCallback(() => onHit?.Invoke());

        // giật lùi nhanh base - 0.2
        seq.Append(weapon.DOLocalMoveX(BasePos(0).x - 0.2f, recoil));

        // về gốc
        seq.Append(weapon.DOLocalMove(BasePos(0), ret));
        seq.Join(weapon.DOLocalRotate(BaseRot(0), ret));

        if (owner != null) seq.SetLink(owner.gameObject);
    }

    protected override void Hit(IDamageable target)
    {
        if (target == null || !target.IsAlive) return;

        Vector3 from = firePoint != null ? firePoint.position : weapon.position;
        Vector3 to = target.Transform.position + Vector3.up;

        var p = Instantiate(projectilePrefab, from, Quaternion.identity);
        p.LaunchArc(to, arcDuration, arcHeight, () =>
        {
            if (target != null && target.IsAlive) owner.DealDamage(target);
        });
    }
}