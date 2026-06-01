using UnityEngine;
using DG.Tweening;

public class Stone2Attack : AttackStrategyBase
{
    [Header("Tay ném")]
    public Transform weapon;
    public Vector3 windUpPos = new Vector3(-0.5f, 0.6f, 0f);
    public float windUp = 0.08f;

    [Header("Đá bay")]
    public Projectile rockPrefab;
    public Transform firePoint;
    public float rockSpeed = 8f;

    private Vector3 _weaponHome;
    private bool _captured;

    public override void Attack(IDamageable target)
    {
        if (!_captured) { _weaponHome = weapon.localPosition; _captured = true; }
        if (target == null || !target.IsAlive) return;

        seq?.Kill();
        seq = DOTween.Sequence();
        seq.Append(weapon.DOLocalMove(windUpPos, windUp));
        seq.AppendCallback(() => Throw(target));
        seq.Append(weapon.DOLocalMove(_weaponHome, windUp));
        seq.AppendCallback(() => weapon.gameObject.SetActive(true));
    }

    void Throw(IDamageable target)
    {
        weapon.gameObject.SetActive(false);

        Vector3 from = firePoint != null ? firePoint.position : weapon.position;
        Vector3 to = target.Transform.position + Vector3.up;   // .Transform (interface), không phải .transform

        var rock = Instantiate(rockPrefab, from, Quaternion.identity);
        rock.Launch(to, rockSpeed, () =>
        {
            if (target != null && target.IsAlive) owner.DealDamage(target);
        });
    }
}