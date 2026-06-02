public class UnitCombatStats
{
    public float hp, atk, attackSpeed, moveSpeed;
    public float criticalChance, lifeSteal, attackRangeInCells;
    public float pushChance, poisonChance, burnChance, freezeChance;
    public int frontPriority;
    public AtkType atkType;

    public UnitCombatStats(UnitData d)   // khởi từ base
    {
        hp = d.hp; atk = d.atk;
        attackSpeed = d.attackSpeed; moveSpeed = d.moveSpeed;
        criticalChance = d.criticalChance; lifeSteal = d.lifeSteal;
        attackRangeInCells = d.attackRangeInCells;
        frontPriority = d.frontPriority;
        atkType = d.AtkType;
        // pushChance... mặc định 0 (base unit chưa có)
    }
}