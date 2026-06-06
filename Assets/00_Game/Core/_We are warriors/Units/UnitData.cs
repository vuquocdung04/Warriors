using System.Collections.Generic;

[System.Serializable]
public class UnitData
{
    public string id;
    public string civId;
    public string displayName;
    public string atkType;          // "Melee" / "Ranged"
    public int frontPriority;
    public float hp;
    public float atk;
    public float attackSpeed;
    public float moveSpeed;
    public float attackRangeInCells;
    public float criticalChance;
    public float lifeSteal;
    public float pushChance;
    public float poisonChance;
    public float burnChance;
    public float freezeChance;
    public int foodCost;
    public int buyPrice;

    public AtkType AtkType =>
        !string.IsNullOrEmpty(atkType) && atkType.Trim().ToLower() == "ranged"
            ? AtkType.Ranged : AtkType.Melee;

    public int FrontPriority =>
        frontPriority > 0 ? frontPriority : (AtkType == AtkType.Melee ? 2 : 1);
}