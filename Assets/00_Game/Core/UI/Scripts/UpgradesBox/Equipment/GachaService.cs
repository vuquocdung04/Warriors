using System.Collections.Generic;
using UnityEngine;

public static class GachaService
{
    public class GachaResult
    {
        public EquipmentData equip;
        public string rank;
        public bool leveledUpGacha;
    }

    public static GachaResult Spin()
    {
        var db = DataRepo.Instance.equipmentDatabase;
        int gLevel = UseProfile.GachaLevel.Value;

        // 1. roll rank theo tỉ lệ của level gacha hiện tại
        string rank = RollRank(db.GetGachaRate(gLevel));

        // 2. random 1 món trong rank đó (gộp cả 3 loại)
        EquipmentData equip = RandomEquipOfRank(db, rank);
        if (equip == null) return null;

        // 3. +1 card cho món
        EquipmentSave.AddCard(equip.id, 1);

        // 4. tăng spin, đủ ngưỡng -> lên level gacha (dừng ở max)
        bool leveled = AdvanceGacha(db);

        return new GachaResult { equip = equip, rank = rank, leveledUpGacha = leveled };
    }

    static string RollRank(GachaRateData rate)
    {
        if (rate == null) return "common";
        float r = Random.value;
        if (r < rate.legend) return "legend";
        r -= rate.legend;
        if (r < rate.epic) return "epic";
        r -= rate.epic;
        if (r < rate.rare) return "rare";
        return "common";
    }

    static EquipmentData RandomEquipOfRank(EquipmentDatabase db, string rank)
    {
        var pool = new List<EquipmentData>();
        foreach (var e in db.AllMelee())  if (e.rank == rank) pool.Add(e);
        foreach (var e in db.AllRange())  if (e.rank == rank) pool.Add(e);
        foreach (var e in db.AllShield()) if (e.rank == rank) pool.Add(e);

        if (pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    static bool AdvanceGacha(EquipmentDatabase db)
    {
        int level = UseProfile.GachaLevel.Value;
        var cfg = db.GetGachaLevel(level);
        if (cfg == null || cfg.spinNeeded <= 0) return false;   // max level -> ngừng đếm

        int spin = UseProfile.GachaSpin.Value + 1;
        if (spin >= cfg.spinNeeded)
        {
            UseProfile.GachaLevel.Value = level + 1;
            UseProfile.GachaSpin.Value = 0;
            return true;
        }
        UseProfile.GachaSpin.Value = spin;
        return false;
    }
}