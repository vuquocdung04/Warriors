using UnityEngine;
using DG.Tweening;
using System;

public class Stone2Attack : AttackStrategyBase
{
    [Header("Tay ném")]
    public Transform weapon;
    public Vector3 windUpPos = new Vector3(-0.5f, 0.6f, 0f);

    [Header("Đá bay")]
    public Projectile rockPrefab;
    public Transform firePoint;
    public float rockSpeed = 8f;

    protected override void PlayAnim2(IDamageable target,float duration, System.Action onHit)
    {
        CacheBase(weapon);

        seq?.Kill();
        seq = DOTween.Sequence();

        float windUp = duration * 0.4f;
        float back = duration * 0.6f;

        seq.Append(weapon.DOLocalMove(windUpPos, windUp));
        seq.AppendCallback(() => onHit?.Invoke());
        seq.Append(weapon.DOLocalMove(BasePos(0), back));
        seq.AppendCallback(() => weapon.gameObject.SetActive(true));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }

    protected override void Hit(IDamageable target)
    {
        weapon.gameObject.SetActive(false);
        if (target == null || !target.IsAlive) return;

        Vector3 from = firePoint != null ? firePoint.position : weapon.position;
        Vector3 to = target.AimPoint;

        var rock = Instantiate(rockPrefab, from, Quaternion.identity);
        rock.Launch(to, rockSpeed, () =>
        {
            if (target != null && target.IsAlive) owner.DealDamage(target);
        });
    }

    protected override void PlayAnim(float duration, Action onHit)
    {
        
    }
}