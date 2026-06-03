using UnityEngine;

public static class EquipmentUpgrade
{
    public static int CardNeeded(EquipmentData equip, int level)
    {
        var db = DataRepo.Instance.equipmentDatabase;
        var rank = db.GetRank(equip.rank);
        if (rank == null) return int.MaxValue;
        return Mathf.RoundToInt(rank.cardPerLevel * Mathf.Pow(rank.cardMultiplier, level - 1));
    }

    public static bool CanUpgrade(EquipType type, EquipmentData equip)
    {
        var state = EquipmentSave.Get(type, equip.id);
        if (state.level >= equip.levelMax) return false;
        return state.card >= CardNeeded(equip, state.level);
    }

    public static bool TryUpgrade(EquipType type, EquipmentData equip)
    {
        if (!CanUpgrade(type, equip)) return false;
        var state = EquipmentSave.Get(type, equip.id);
        state.card -= CardNeeded(equip, state.level);
        state.level += 1;
        EquipmentSave.Save();
        return true;
    }
}