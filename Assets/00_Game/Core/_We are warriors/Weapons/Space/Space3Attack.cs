using UnityEngine;
using DG.Tweening;

public class Space3Attack : AttackStrategyBase
{
    [Header("Body")]
    public Transform body;

    [Header("Projectile")]
    public Projectile projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 8f;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(body);
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float kick = duration * 0.15f;
        float sway1 = duration * 0.15f;
        float sway2 = duration * 0.15f;
        float ret = duration * 0.55f;

        // bắn ngay
        seq.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySfx("LaserGun");
            onHit?.Invoke();
        });

        // giật lùi + nghiêng -15
        seq.Append(body.DOLocalMoveX(BasePos(0).x - 0.3f, kick));
        seq.Join(body.DOLocalRotate(new Vector3(0, 0, -15f), kick));

        // nghiêng ngả: lắc về 10
        seq.Append(body.DOLocalRotate(new Vector3(0, 0, 10f), sway1));
        // lắc về -6
        seq.Append(body.DOLocalRotate(new Vector3(0, 0, -6f), sway2));

        // về base (pos + rot)
        seq.Append(body.DOLocalMove(BasePos(0), ret));
        seq.Join(body.DOLocalRotate(BaseRot(0), ret));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }

    protected override void Hit(IDamageable target)
    {
        if (target == null || !target.IsAlive) return;

        Vector3 from = firePoint != null ? firePoint.position : body.position;
        Vector3 to = target.AimPoint;

        var p = SimplePool2.Spawn(projectilePrefab);
        p.transform.position = from;
        p.Launch(to, projectileSpeed, () =>
        {
            if (target != null && target.IsAlive) owner.DealDamage(target);
        });
    }
}