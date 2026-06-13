#if UNITY_EDITOR
using System.Collections.Generic;
using static SheetImportUtil;

public class GachaImporter : ISheetImporter
{
    public string Name => "Gacha";
    public string AnchorColumn => "level";

    public object Parse(List<List<string>> rows)
    {
        var list = new List<GachaLevelData>();
        if (rows.Count < 2) return list;

        var header = rows[0];
        int Col(string n) => header.FindIndex(h => h.Trim().ToLower() == n);
        int cLevel = Col("level"), cSpin = Col("spin_needed"),
            cCommon = Col("common"), cRare = Col("rare"), cEpic = Col("epic"), cLegend = Col("legend");

        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            string Get(int i) => (i >= 0 && i < row.Count) ? row[i].Trim() : "";
            if (string.IsNullOrEmpty(Get(cLevel))) continue;

            list.Add(new GachaLevelData
            {
                level = (int)PFloat(Get(cLevel)),
                spinNeeded = (int)PFloat(Get(cSpin)),
                common = PFloat(Get(cCommon)),
                rare = PFloat(Get(cRare)),
                epic = PFloat(Get(cEpic)),
                legend = PFloat(Get(cLegend)),
            });
        }
        return list;
    }
}
#endif

