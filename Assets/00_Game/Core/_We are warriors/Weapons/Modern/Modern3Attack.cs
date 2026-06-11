using UnityEngine;
using DG.Tweening;
using System;

public class Modern3Attack : AttackStrategyBase
{
    [Header("Weapon")]
    public Transform weapon;
    public Transform body;

    [Header("Projectile")]
    public Projectile projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 8f;

    protected override void PlayAnim2(IDamageable target, float duration, Action onHit)
    {
        CacheBase(weapon, body);
        float angle = AimAngleTank(weapon, target);

        seq?.Kill();
        seq = DOTween.Sequence();

        float hold = duration * 0.2f;
        float aim = 0.2f;
        float recoil = duration * 0.12f;
        float shake1 = duration * 0.12f;
        float shake2 = duration * 0.12f;
        float ret = duration * 0.24f;

        // hold lâu (nạp)
        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, angle), aim));
        seq.AppendInterval(hold);

        // bắn + giật nòng + body lắc 7
        seq.AppendCallback(() =>
       {
           AudioManager.Instance.PlaySfx("Tank");
           onHit?.Invoke();
       });
        seq.Append(weapon.DOLocalMoveX(BasePos(0).x - 0.4f, recoil));
        seq.Append(body.DOLocalRotate(new Vector3(0, 0, 7f), shake1));

        // body lắc về -6
        seq.Append(body.DOLocalRotate(new Vector3(0, 0, -6f), shake2));


        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }
    float AimAngleTank(Transform weapon, IDamageable target)
    {
        if (target == null || weapon.parent == null) return weapon.localEulerAngles.z;
        Vector3 localTarget = weapon.parent.InverseTransformPoint(target.AimPoint);
        Vector2 dir = (Vector2)(localTarget - weapon.localPosition);
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
    public override void ResetToIdle()
    {
        seq?.Kill();
        OnAnimDone();
        seq = DOTween.Sequence();
        seq.Append(weapon.DOLocalMove(BasePos(0), 0.2f));
        seq.Join(body.DOLocalRotate(BaseRot(1), 0.2f));
        seq.Join(weapon.DOLocalRotate(BaseRot(0), 0.2f));
        if (owner != null) seq.SetLink(owner.gameObject);
    }
    protected override void Hit(IDamageable target)
    {
        if (target == null || !target.IsAlive) return;

        Vector3 from = firePoint != null ? firePoint.position : weapon.position;
        Vector3 to = target.AimPoint;
        var p = SimplePool2.Spawn(projectilePrefab);
        p.transform.position = from;
        p.Launch(to, projectileSpeed, () =>
        {
            if (target != null && target.IsAlive) owner.DealDamage(target);
        });
    }

    protected override void PlayAnim(float duration, Action onHit)
    {

    }
}