#if UNITY_EDITOR
using System.Collections.Generic;
using static SheetImportUtil;

public class UnitImporter : ISheetImporter
{
    public string Name => "Unit";

    public object Parse(List<List<string>> rows)
    {
        var list = new List<UnitData>();
        if (rows.Count < 2) return list;

        var header = rows[0];
        int Col(string n) => header.FindIndex(h => h.Trim().ToLower() == n);
        int cId = Col("id"), cCiv = Col("civ_id"), cName = Col("name"), cType = Col("atk_type"),
            cHp = Col("hp"), cAtk = Col("atk"), cAs = Col("attack_speed"), cMs = Col("move_speed"),
            cCrit = Col("critical_chance"), cLs = Col("life_steal"), cFood = Col("food_cost"),
            cBuy = Col("buy_price"), cRange = Col("attack_range");

        string lastCiv = "";
        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            string Get(int i) => (i >= 0 && i < row.Count) ? row[i].Trim() : "";
            string id = Get(cId);
            if (string.IsNullOrEmpty(id)) continue;

            string civ = Get(cCiv);
            if (!string.IsNullOrEmpty(civ)) lastCiv = civ; else civ = lastCiv;
            string type = Get(cType);
            bool isRanged = type.Trim().ToLower().StartsWith("rang");

            list.Add(new UnitData
            {
                id = id, civId = civ, displayName = Get(cName), atkType = type,
                frontPriority = isRanged ? 1 : 2,
                hp = PFloat(Get(cHp)), atk = PFloat(Get(cAtk)),
                attackSpeed = PFloat(Get(cAs)), moveSpeed = PFloat(Get(cMs)),
                attackRangeInCells = PFloat(Get(cRange)),
                criticalChance = PFloat(Get(cCrit)), lifeSteal = PFloat(Get(cLs)),
                foodCost = (int)PFloat(Get(cFood)), buyPrice = PMoney(Get(cBuy)),
            });
        }
        return list;
    }
}
#endif