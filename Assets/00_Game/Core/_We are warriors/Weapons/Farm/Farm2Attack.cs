using UnityEngine;
using DG.Tweening;

public class Farm2Attack : AttackStrategyBase
{
    [Header("Tay ném")]
    public Transform weapon;

    [Header("Projectile")]
    public Projectile rockPrefab;
    public Transform firePoint;
    public float rockSpeed = 8f;

    private IDamageable _target;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon);
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float draw = duration * 0.45f;
        float back = duration * 0.55f;

        seq.Append(weapon.DOLocalRotate(Vector3.zero, draw));
        seq.Join(weapon.DOLocalMoveX(0.4f, draw));
        seq.AppendCallback(() =>
         {
             AudioManager.Instance.PlaySfx("whoosh");
             onHit?.Invoke();
         });
        seq.Append(weapon.DOLocalMove(BasePos(0), back));
        seq.Join(weapon.DOLocalRotate(BaseRot(0), back));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }

    protected override void Hit(IDamageable target)
    {
        if (target == null || !target.IsAlive) return;

        Vector3 from = firePoint != null ? firePoint.position : weapon.position;
        Vector3 to = target.AimPoint;

        var p = SimplePool2.Spawn(rockPrefab);   // pool
        p.transform.position = from;
        p.Launch(to, rockSpeed, () =>
        {
            if (target != null && target.IsAlive) owner.DealDamage(target);
        });
    }
}