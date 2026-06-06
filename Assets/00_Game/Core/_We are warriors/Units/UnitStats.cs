public class UnitStats
{
    public AtkType atkType;
    public int frontPriority;
    public float maxHp, hp;
    public float atk, attackSpeed, moveSpeed, attackRangeInCells;
    public float criticalChance, lifeSteal;
    public float pushChance, poisonChance, burnChance, freezeChance;   // thêm
    public UnitStats() { }
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
        pushChance = d.pushChance;
        poisonChance = d.poisonChance;
        burnChance = d.burnChance;
        freezeChance = d.freezeChance;
    }

    public UnitStats Clone() => (UnitStats)MemberwiseClone();   // thêm: mỗi unit 1 bản riêng
}