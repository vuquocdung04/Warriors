#if UNITY_EDITOR
using System.Collections.Generic;

public class StatImporter : ISheetImporter
{
    public string Name => "Stat";

    public object Parse(List<List<string>> rows)
    {
        var list = new List<StatData>();
        if (rows.Count < 2) return list;

        var header = rows[0];
        int Col(string n) => header.FindIndex(h => h.Trim().ToLower() == n);
        int cId = Col("id"), cName = Col("display_name"), cCalc = Col("calc_type");

        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            string Get(int i) => (i >= 0 && i < row.Count) ? row[i].Trim() : "";
            string id = Get(cId);
            if (string.IsNullOrEmpty(id)) continue;

            list.Add(new StatData { id = id, displayName = Get(cName), calcType = Get(cCalc).ToLower() });
        }
        return list;
    }
}
#endif