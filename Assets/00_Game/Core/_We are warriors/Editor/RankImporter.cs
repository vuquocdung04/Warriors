#if UNITY_EDITOR
using System.Collections.Generic;
using static SheetImportUtil;

public class RankImporter : ISheetImporter
{
    public string Name => "Rank";
    public string AnchorColumn => "rank";

    public object Parse(List<List<string>> rows)
    {
        var list = new List<RankData>();
        if (rows.Count < 2) return list;

        var header = rows[0];
        int Col(string n) => header.FindIndex(h => h.Trim().ToLower() == n);
        int cRank = Col("rank"), cCount = Col("stat_count"),
            cCard = Col("card_per_level"), cMul = Col("card_multiplier");

        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            string Get(int i) => (i >= 0 && i < row.Count) ? row[i].Trim() : "";
            string rank = Get(cRank);
            if (string.IsNullOrEmpty(rank)) continue;

            list.Add(new RankData
            {
                rank = rank.ToLower(),
                statCount = (int)PFloat(Get(cCount)),
                cardPerLevel = (int)PFloat(Get(cCard)),
                cardMultiplier = PFloat(Get(cMul))
            });
        }
        return list;
    }
}
#endif