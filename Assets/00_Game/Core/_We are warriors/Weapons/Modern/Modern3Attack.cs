using UnityEngine;
using DG.Tweening;

public class Modern3Attack : AttackStrategyBase
{
    [Header("Weapon")]
    public Transform weapon;
    public Transform body;

    [Header("Projectile")]
    public Projectile projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 8f;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon, body);   // 0=weapon, 1=body
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float hold = duration * 0.4f;
        float recoil = duration * 0.12f;
        float shake1 = duration * 0.12f;
        float shake2 = duration * 0.12f;
        float ret = duration * 0.24f;

        // hold lâu (nạp)
        seq.AppendInterval(hold);

        // bắn + giật nòng + body lắc 7
        seq.AppendCallback(() => onHit?.Invoke());
        seq.Append(weapon.DOLocalMoveX(BasePos(0).x - 0.4f, recoil));
        seq.Append(body.DOLocalRotate(new Vector3(0, 0, 7f), shake1));

        // body lắc về -6
        seq.Append(body.DOLocalRotate(new Vector3(0, 0, -6f), shake2));

        // về base (nòng + body)
        seq.Append(weapon.DOLocalMoveX(BasePos(0).x, ret));
        seq.Join(body.DOLocalRotate(BaseRot(1), ret));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }

    protected override void Hit(IDamageable target)
    {
        if (target == null || !target.IsAlive) return;

        Vector3 from = firePoint != null ? firePoint.position : weapon.position;
        Vector3 to = target.Transform.position + Vector3.up;

        var p = Instantiate(projectilePrefab, from, Quaternion.identity);
        p.Launch(to, projectileSpeed, () =>
        {
            if (target != null && target.IsAlive) owner.DealDamage(target);
        });
    }
}