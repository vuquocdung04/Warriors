#if UNITY_EDITOR
using System.Collections.Generic;
using static SheetImportUtil;

public class GachaLevelImporter : ISheetImporter
{
    public string Name => "GachaLevel";
    public string AnchorColumn => "level";

    public object Parse(List<List<string>> rows)
    {
        var list = new List<GachaLevelData>();
        if (rows.Count < 2) return list;

        var header = rows[0];
        int Col(string n) => header.FindIndex(h => h.Trim().ToLower() == n);
        int cLv = Col("level"), cSpin = Col("spin_needed");

        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            string Get(int i) => (i >= 0 && i < row.Count) ? row[i].Trim() : "";
            string lv = Get(cLv);
            if (string.IsNullOrEmpty(lv)) continue;

            list.Add(new GachaLevelData
            {
                level = (int)PFloat(lv),
                spinNeeded = (int)PFloat(Get(cSpin))
            });
        }
        return list;
    }
}

public class GachaRateImporter : ISheetImporter
{
    public string Name => "GachaRate";
    public string AnchorColumn => "level";

    public object Parse(List<List<string>> rows)
    {
        var list = new List<GachaRateData>();
        if (rows.Count < 2) return list;

        var header = rows[0];
        int Col(string n) => header.FindIndex(h => h.Trim().ToLower() == n);
        int cLv = Col("level"), cC = Col("common"), cR = Col("rare"), cE = Col("epic"), cL = Col("legend");

        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            string Get(int i) => (i >= 0 && i < row.Count) ? row[i].Trim() : "";
            string lv = Get(cLv);
            if (string.IsNullOrEmpty(lv)) continue;

            list.Add(new GachaRateData
            {
                level = (int)PFloat(lv),
                common = PFloat(Get(cC)),
                rare = PFloat(Get(cR)),
                epic = PFloat(Get(cE)),
                legend = PFloat(Get(cL))
            });
        }
        return list;
    }
}
#endif

