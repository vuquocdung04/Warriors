#if UNITY_EDITOR
using System.Collections.Generic;
using static SheetImportUtil;

public class HouseImporter : ISheetImporter
{
    public string Name => "House";

    public object Parse(List<List<string>> rows)
    {
        var list = new List<HouseData>();
        if (rows.Count < 2) return list;

        var header = rows[0];
        int Col(string n) => header.FindIndex(h => h.Trim().ToLower() == n);
        int cCiv = Col("civ_id"), cName = Col("civ_name"), cOrder = Col("order"),
            cCost = Col("unlock_cost"), cHp = Col("house_hp"), cYears = Col("era_years"), cDesc = Col("era_desc");

        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            string Get(int i) => (i >= 0 && i < row.Count) ? row[i].Trim() : "";
            string civ = Get(cCiv);
            if (string.IsNullOrEmpty(civ)) continue;

            list.Add(new HouseData
            {
                civId = civ,
                civName = Get(cName),
                order = (int)PFloat(Get(cOrder)),
                unlockCost = PMoney(Get(cCost)),
                houseHp = PFloat(Get(cHp)),
                eraYears = Get(cYears),
                eraDesc = Get(cDesc),
            });
        }
        return list;
    }
}
#endif