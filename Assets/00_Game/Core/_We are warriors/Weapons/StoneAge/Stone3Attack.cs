using UnityEngine;
using DG.Tweening;

public class Stone3Attack : AttackStrategyBase
{
    public Transform weapon;

    public override void Attack(IDamageable target)
    {
        seq?.Kill();
        seq = DOTween.Sequence();
        seq.Append(weapon.DOLocalMove(new Vector3(-1f, 0.4f,0f), 0.08f));
        seq.Append(weapon.DOLocalMove(new Vector3(0.14f, 0.22f, 0f), 0.06f));
        seq.AppendCallback(() => Hit(target));
        seq.Append(weapon.DOLocalMove(new Vector3(-0.6f, 0.34f, 0f), 0.12f));
    }
}