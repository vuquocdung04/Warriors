public interface IAttackStrategy
{
    void Init(Unit owner);
    void Attack(Unit target);
}