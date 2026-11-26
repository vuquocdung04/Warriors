using UnityEngine;

// Quy tắc nâng bậc unit trong trận: bậc 0 = gốc, tối đa MaxTier.
public static class UnitTier
{
    public const int MaxTier = 2;

    // Bậc 1: +50%, bậc 2: +100% chỉ số gốc
    public static float StatMultiplier(int tier) => 1f + 0.5f * Mathf.Clamp(tier, 0, MaxTier);

    // Giá lên bậc kế tiếp: bậc 1 = 2x giá triệu hồi, bậc 2 = 2x giá bậc 1
    public static int UpgradeCost(int foodCost, int currentTier) => foodCost * (1 << (currentTier + 1));

    public static void ApplyStats(UnitStats s, int tier)
    {
        if (tier <= 0) return;
        float m = StatMultiplier(tier);
        s.maxHp *= m;
        s.hp *= m;
        s.atk *= m;
    }
}
