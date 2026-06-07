public interface IAttackStrategy
{
    void Init(Unit owner);
    public abstract void Attack(IDamageable target, float duration);
}