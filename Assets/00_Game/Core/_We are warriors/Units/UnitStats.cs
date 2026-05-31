public class UnitStats
{
    public AtkType atkType;
    public int frontPriority;
    public float maxHp, hp;
    public float atk, attackSpeed, moveSpeed, attackRangeInCells;
    public float criticalChance, lifeSteal;

    public UnitStats(UnitData d)
    {
        atkType = d.AtkType;
        frontPriority = d.FrontPriority;
        maxHp = hp = d.hp;
        atk = d.atk;
        attackSpeed = d.attackSpeed;
        moveSpeed = d.moveSpeed;
        attackRangeInCells = d.attackRangeInCells;
        criticalChance = d.criticalChance;
        lifeSteal = d.lifeSteal;
    }
}