#if UNITY_EDITOR
using System.Collections.Generic;
using static SheetImportUtil;

public class SkillImporter : ISheetImporter
{
    public string Name => "Skill";
    public string AnchorColumn => "id";

    public object Parse(List<List<string>> rows)
    {
        var list = new List<SkillData>();
        if (rows.Count < 2) return list;

        var header = rows[0];
        int Col(string n) => header.FindIndex(h => h.Trim().ToLower() == n);
        int cId = Col("id"), cName = Col("name"), cMax = Col("level_max"),
            cBase = Col("base_value"), cPer = Col("value_per_level"), cDesc = Col("description");

        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            string Get(int i) => (i >= 0 && i < row.Count) ? row[i].Trim() : "";

            string id = Get(cId);
            if (string.IsNullOrEmpty(id)) continue;

            list.Add(new SkillData
            {
                id = id,
                name = Get(cName),
                levelMax = (int)PFloat(Get(cMax)),
                baseValue = PFloat(Get(cBase)),
                valuePerLevel = PFloat(Get(cPer)),
                description = Get(cDesc)
            });
        }
        return list;
    }
}
#endif