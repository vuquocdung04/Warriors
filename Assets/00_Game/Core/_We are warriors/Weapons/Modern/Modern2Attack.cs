using UnityEngine;
using DG.Tweening;
using System;

public class Modern2Attack : AttackStrategyBase
{
    [Header("Weapon")]
    public Transform weapon;

    [Header("Projectile")]
    public Projectile projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 8f;

    protected override void PlayAnim2(IDamageable target, float duration, Action onHit)
    {
        CacheBase(weapon);

        float angle = AimAngleLocal(weapon, target);

        seq?.Kill();
        seq = DOTween.Sequence();

        float aim = duration * 0.35f;
        float hold = duration * 0.15f;
        float recoil = duration * 0.1f;
        float ret = duration * 0.2f;

        // xoay -90
        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, angle - 10f), aim));

        // hold rồi bắn + giật
        seq.AppendInterval(hold);
        seq.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySfx("Reifle Semi");
            onHit?.Invoke();
        });
        seq.Append(weapon.DOLocalMoveX(BasePos(0).x - 0.3f, recoil));   // giật súng
        seq.Append(weapon.DOLocalMoveX(BasePos(0).x, ret));

        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }
    protected override void PlayAnim(float duration, System.Action onHit) { }
    public override void ResetToIdle()
    {
        seq?.Kill();
        OnAnimDone();
        seq = DOTween.Sequence();
        seq.Append(weapon.DOLocalMove(BasePos(0), 0.2f));
        seq.Join(weapon.DOLocalRotate(BaseRot(0), 0.2f));
        if (owner != null) seq.SetLink(owner.gameObject);
    }

    protected override void Hit(IDamageable target)
    {
        if (target == null || !target.IsAlive) return;

        Vector3 from = firePoint != null ? firePoint.position : weapon.position;
        Vector3 to = target.AimPoint;

        var p = SimplePool2.Spawn(projectilePrefab);   // pool
        p.transform.position = from;
        p.Launch(to, projectileSpeed, () =>
        {
            if (target != null && target.IsAlive) owner.DealDamage(target);
        });
    }
    private void OnDrawGizmos()
    {
        if (weapon == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawRay(weapon.position, weapon.up * 2f);
    }
}