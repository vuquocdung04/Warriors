using System.Collections.Generic;
using UnityEngine;

public static class GachaService
{
    public class GachaResult
    {
        public EquipmentData equip;
        public EquipType type;
        public string rank;
        public bool isFirstOwn;
        public bool leveledUpGacha;
    }

    public static GachaResult Spin()
    {
        var db = DataRepo.Instance.equipmentDatabase;
        int gLevel = UseProfile.GachaLevel.Value;

        string rank = RollRank(db.GetGachaRate(gLevel));

        var (equip, type) = RandomEquipOfRank(db, rank);
        if (equip == null) return null;

        var state = EquipmentSave.Get(type, equip.id);
        bool firstOwn = !state.owned;

        if (firstOwn)
            state.owned = true;          // lần đầu: chỉ mở khóa, KHÔNG +card
        else
            state.card += 1;             // lần sau: +1 card để nâng

        EquipmentSave.Save();

        bool leveled = AdvanceGacha(db);
        return new GachaResult { equip = equip, type = type, rank = rank, isFirstOwn = firstOwn, leveledUpGacha = leveled };
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

    static (EquipmentData, EquipType) RandomEquipOfRank(EquipmentDatabase db, string rank)
    {
        var pool = new List<(EquipmentData, EquipType)>();
        foreach (var e in db.AllMelee()) if (e.rank == rank) pool.Add((e, EquipType.Melee));
        foreach (var e in db.AllRange()) if (e.rank == rank) pool.Add((e, EquipType.Range));
        foreach (var e in db.AllShield()) if (e.rank == rank) pool.Add((e, EquipType.Shield));

        if (pool.Count == 0) return (null, EquipType.Melee);
        return pool[Random.Range(0, pool.Count)];
    }

    static bool AdvanceGacha(EquipmentDatabase db)
    {
        int level = UseProfile.GachaLevel.Value;
        var cfg = db.GetGachaLevel(level);
        if (cfg == null || cfg.spinNeeded <= 0) return false;

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