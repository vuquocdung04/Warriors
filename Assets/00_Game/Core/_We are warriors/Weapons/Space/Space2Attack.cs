using UnityEngine;
using DG.Tweening;

public class Space2Attack : AttackStrategyBase
{
    [Header("Weapon")]
    public Transform weapon;

    [Header("Projectile")]
    public Projectile projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 8f;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon);
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float aim = duration * 0.45f;
        float ret = duration * 0.55f;

        // xoay -90 + nâng Y base + 0.2, bắn luôn
        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, -90f), aim));
        seq.Join(weapon.DOLocalMoveY(BasePos(0).y + 0.3f, aim));
        seq.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySfx("LaserGun");
            onHit?.Invoke();
        });

        // về base
        seq.Append(weapon.DOLocalMove(BasePos(0), ret));
        seq.Join(weapon.DOLocalRotate(BaseRot(0), ret));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }

    protected override void Hit(IDamageable target)
    {
        if (target == null || !target.IsAlive) return;

        Vector3 from = firePoint != null ? firePoint.position : weapon.position;
        Vector3 to = target.AimPoint;

        var p = Instantiate(projectilePrefab, from, Quaternion.identity);
        p.Launch(to, projectileSpeed, () =>
        {
            if (target != null && target.IsAlive) owner.DealDamage(target);
        });
    }
}