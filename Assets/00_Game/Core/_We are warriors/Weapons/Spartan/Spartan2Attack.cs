using UnityEngine;
using DG.Tweening;

public class Spartan2Attack : AttackStrategyBase
{
    [Header("Weapon")]
    public Transform weapon;

    [Header("Projectile")]
    public Projectile arrowPrefab;
    public Transform firePoint;
    public float rockSpeed = 8f;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon);
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float draw = duration * 0.4f;
        float recoil = duration * 0.2f;
        float ret = duration * 0.4f;

        // xoay 0 + đẩy tới base + 0.6
        seq.Append(weapon.DOLocalRotate(Vector3.zero, draw));
        seq.Join(weapon.DOLocalMoveX(BasePos(0).x + 0.6f, draw));

        // bắn
        seq.AppendCallback(() => onHit?.Invoke());

        // giật nhẹ: base+0.6 -> base+0.5
        seq.Append(weapon.DOLocalMoveX(BasePos(0).x + 0.45f, recoil));

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

        var rock = Instantiate(arrowPrefab, from, Quaternion.identity);
        rock.Launch(to, rockSpeed, () =>
        {
            if (target != null && target.IsAlive) owner.DealDamage(target);
        });
    }
}