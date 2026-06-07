#if UNITY_EDITOR
using System.Collections.Generic;
using static SheetImportUtil;

public class EnemyWaveImporter : ISheetImporter
{
    public string Name => "EnemyWave";

    public object Parse(List<List<string>> rows)
    {
        var list = new List<EnemyCivConfig>();
        if (rows.Count < 2) return list;

        var header = rows[0];
        int Col(string n) => header.FindIndex(h => h.Trim().ToLower() == n);
        int cCiv = Col("civ_id"), cHp = Col("house_hp"), cWave = Col("wave"),
            cWaveDelay = Col("delay_between_wave"), cSpawnDelay = Col("spawn_delay"), cUnits = Col("units");

        EnemyCivConfig curCiv = null;
        EnemyWave curWave = null;
        int curWaveNo = -1;

        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            string Get(int i) => (i >= 0 && i < row.Count) ? row[i].Trim() : "";

            string civId = Get(cCiv);
            if (!string.IsNullOrEmpty(civId))
            {
                curCiv = new EnemyCivConfig
                {
                    civId = civId.ToLower(),
                    houseHp = PMoney(Get(cHp)),
                };
                list.Add(curCiv);
                curWave = null;
                curWaveNo = -1;
            }
            if (curCiv == null) continue;

            string units = Get(cUnits);
            if (string.IsNullOrEmpty(units)) continue;

            string waveStr = Get(cWave);
            if (!string.IsNullOrEmpty(waveStr))
            {
                int waveNo = (int)PFloat(waveStr);
                if (waveNo != curWaveNo)
                {
                    curWave = new EnemyWave
                    {
                        wave = waveNo,
                        delayBetweenWave = PFloat(Get(cWaveDelay)),
                    };
                    curCiv.waves.Add(curWave);
                    curWaveNo = waveNo;
                }
            }
            if (curWave == null) continue;

            curWave.spawns.Add(new SpawnEntry
            {
                spawnDelay = PFloat(Get(cSpawnDelay)),
                units = units,
            });
        }
        return list;
    }
}
#endif