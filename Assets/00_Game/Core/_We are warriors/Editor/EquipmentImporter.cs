#if UNITY_EDITOR
using System.Collections.Generic;
using static SheetImportUtil;

public class EquipmentImporter : ISheetImporter
{
    private readonly string _name;
    public EquipmentImporter(string name) => _name = name;   // "MeleeEquipment"...
    public string Name => _name;

    public object Parse(List<List<string>> rows)
    {
        var list = new List<EquipmentData>();
        if (rows.Count < 2) return list;

        var header = rows[0];
        int Col(string n) => header.FindIndex(h => h.Trim().ToLower() == n);
        int cId = Col("id"), cName = Col("name"), cRank = Col("rank"), cMax = Col("level_max"),
            cSlot = Col("stat_slot"), cType = Col("stat_type"), cUnlock = Col("level_unlock"),
            cL1 = Col("lv1"), cL2 = Col("lv2"), cL3 = Col("lv3"), cL4 = Col("lv4"), cL5 = Col("lv5");

        EquipmentData current = null;
        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            string Get(int i) => (i >= 0 && i < row.Count) ? row[i].Trim() : "";

            string id = Get(cId);
            if (!string.IsNullOrEmpty(id))   // dòng mở 1 món mới
            {
                current = new EquipmentData
                {
                    id = id,
                    name = Get(cName),
                    rank = Get(cRank).ToLower(),
                    levelMax = (int)PFloat(Get(cMax)),
                    stats = new List<EquipmentStat>()
                };
                list.Add(current);
            }
            if (current == null) continue;   // dòng stat phải thuộc 1 món

            string statType = Get(cType);
            if (string.IsNullOrEmpty(statType)) continue;   // dòng trống thật -> bỏ

            current.stats.Add(new EquipmentStat
            {
                slot = (int)PFloat(Get(cSlot)),
                statType = statType,
                levelUnlock = (int)PFloat(Get(cUnlock)),
                levels = new List<float>
                {
                    PFloat(Get(cL1)), PFloat(Get(cL2)), PFloat(Get(cL3)),
                    PFloat(Get(cL4)), PFloat(Get(cL5))
                }
            });
        }
        return list;
    }
}
#endif