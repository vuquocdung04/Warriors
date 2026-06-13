#if UNITY_EDITOR
using System.Collections.Generic;
using static SheetImportUtil;

public class EconomyImporter : ISheetImporter
{
    public string Name => "Economy";
    public string AnchorColumn => "civ_id";

    public object Parse(List<List<string>> rows)
    {
        var list = new List<EconomyConfig>();
        if (rows.Count < 2) return list;

        var header = rows[0];
        int Col(string n) => header.FindIndex(h => h.Trim().ToLower() == n);
        int cCiv = Col("civ_id"), cCoin = Col("coin_per_kill"), cCoinRate = Col("coin_drop_rate"),
            cGem = Col("gem_per_kill"), cGemRate = Col("gem_drop_rate");

        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            string Get(int i) => (i >= 0 && i < row.Count) ? row[i].Trim() : "";

            string civId = Get(cCiv);
            if (string.IsNullOrEmpty(civId)) continue;

            list.Add(new EconomyConfig
            {
                civId = civId.ToLower(),
                coinPerKill = PMoney(Get(cCoin)),       // 1.6m -> 1600000
                coinDropRate = PFloat(Get(cCoinRate)),  // 0.7
                gemPerKill = PMoney(Get(cGem)),
                gemDropRate = PFloat(Get(cGemRate)),    // 0.3
            });
        }
        return list;
    }
}
#endif