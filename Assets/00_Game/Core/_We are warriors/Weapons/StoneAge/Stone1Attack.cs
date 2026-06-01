using UnityEngine;
using DG.Tweening;

public class Stone1Attack : AttackStrategyBase
{
    public Transform weapon;

    public override void Attack(IDamageable target)
    {
        seq?.Kill();
        seq = DOTween.Sequence();
        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, 12f), 0.08f));
        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, -100f), 0.06f));
        seq.AppendCallback(() => Hit(target));
        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, -80f), 0.12f));
    }
}