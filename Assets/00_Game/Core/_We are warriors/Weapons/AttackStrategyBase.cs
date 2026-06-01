using UnityEngine;
using DG.Tweening;

public abstract class AttackStrategyBase : MonoBehaviour, IAttackStrategy
{
    protected Unit owner;
    protected Sequence seq;

    public void Init(Unit o) => owner = o;

    public abstract void Attack(IDamageable target);

    protected void Hit(IDamageable target)
    {
        if (target != null && target.IsAlive) owner.DealDamage(target);
    }

    protected virtual void OnDestroy() => seq?.Kill();
}