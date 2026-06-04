using System.Collections.Generic;

/// <summary>
/// Nơi tập trung công thức tính chỉ số (base + trang bị đang đeo).
/// EquipmentDatabase chỉ giữ data; mọi thay đổi công thức / cân bằng game sửa ở đây.
/// </summary>
public static class EquipmentStatCalculator
{
    // 3 unit của civ -> 3 stat đã cộng equipment đang đeo
    public static List<UnitStats> BuildStats(string civId)
    {
        var db = DataRepo.Instance.equipmentDatabase;
        var units = DataRepo.Instance.unitDatabase.GetCivUnits(civId);
        var result = new List<UnitStats>();

        EquipmentData[] equips =
        {
            db.GetMelee(UseProfile.EquippedMelee.Value),
            db.GetRange(UseProfile.EquippedRange.Value),
            db.GetShield(UseProfile.EquippedShield.Value),
        };
        EquipType[] types = { EquipType.Melee, EquipType.Range, EquipType.Shield };

        for (int i = 0; i < units.Count; i++)
        {
            var stats = new UnitStats(units[i]);
            if (i < equips.Length && equips[i] != null) ApplyEquipment(stats, equips[i], types[i]);
            stats.maxHp = stats.hp;
            result.Add(stats);
        }
        return result;
    }

    public static List<UnitStats> BuildBaseStats(string civId)
    {
        var units = DataRepo.Instance.unitDatabase.GetCivUnits(civId);
        var result = new List<UnitStats>();
        foreach (var u in units)
        {
            var s = new UnitStats(u);
            s.maxHp = s.hp;
            result.Add(s);
        }
        return result;
    }


    static void ApplyEquipment(UnitStats stats, EquipmentData equip, EquipType type)
    {
        var db = DataRepo.Instance.equipmentDatabase;
        int level = EquipmentSave.Get(type, equip.id).level;

        var flat = new Dictionary<string, float>();
        var percent = new Dictionary<string, float>();

        foreach (var s in equip.stats)
        {
            if (level < s.levelUnlock) continue;          // stat chưa mở theo level

            float value = s.value;                         // cố định, không theo level nữa
            StatData def = db.GetStat(s.statType);
            string calc = def != null ? def.calcType : "flat";
            string baseStat = StripSuffix(s.statType);

            switch (calc)
            {
                case "percent":
                    percent[baseStat] = (percent.TryGetValue(baseStat, out var p) ? p : 0f) + value;
                    break;
                case "rate":
                    AddRate(stats, s.statType, value);
                    break;
                default:
                    flat[baseStat] = (flat.TryGetValue(baseStat, out var f) ? f : 0f) + value;
                    break;
            }
        }

        stats.atk = Combine(stats.atk, flat, percent, "atk");
        stats.hp = Combine(stats.hp, flat, percent, "hp");
        stats.moveSpeed = Combine(stats.moveSpeed, flat, percent, "move_speed");
        stats.attackSpeed = Combine(stats.attackSpeed, flat, percent, "attack_speed");
        stats.attackRangeInCells = Combine(stats.attackRangeInCells, flat, percent, "attack_range");
    }

    static float Combine(float baseVal, Dictionary<string, float> flat, Dictionary<string, float> percent, string key)
    {
        float f = flat.TryGetValue(key, out var fv) ? fv : 0f;
        float p = percent.TryGetValue(key, out var pv) ? pv : 0f;
        return (baseVal + f) * (1f + p / 100f);   // percent dạng 50 = +50%
    }

    static void AddRate(UnitStats stats, string statType, float value)
    {
        float v = value / 100f;
        switch (statType)
        {
            case "critical_chance": stats.criticalChance += v; break;
            case "life_steal": stats.lifeSteal += v; break;
            case "push_chance": stats.pushChance += v; break;
            case "poison_chance": stats.poisonChance += v; break;
            case "burn_chance": stats.burnChance += v; break;
            case "freeze_chance": stats.freezeChance += v; break;
        }
    }

    static string StripSuffix(string statType)   // atk_percent -> atk, hp_percent -> hp
    {
        if (statType.EndsWith("_percent")) return statType.Substring(0, statType.Length - "_percent".Length);
        return statType;
    }
}
