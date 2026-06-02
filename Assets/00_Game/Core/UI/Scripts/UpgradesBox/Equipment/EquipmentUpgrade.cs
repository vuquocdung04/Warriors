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

    public static bool CanUpgrade(EquipmentData equip)
    {
        var state = EquipmentSave.Get(equip.id);
        if (state.level >= equip.levelMax) return false;       // đã max
        return state.card >= CardNeeded(equip, state.level);
    }

    public static bool TryUpgrade(EquipmentData equip)
    {
        if (!CanUpgrade(equip)) return false;
        var state = EquipmentSave.Get(equip.id);
        int need = CardNeeded(equip, state.level);

        state.card -= need;
        state.level += 1;
        EquipmentSave.Save();
        return true;
    }
}