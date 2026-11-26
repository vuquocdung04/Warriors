using UnityEngine;
using UnityEngine.UI;

public class UnitDisplay : MonoBehaviour
{
    public string id;
    public GameObject blindUnit;

    [Header("Bậc unit (thẻ trong trận)")]
    [Tooltip("Các phần art gốc, ẩn đi khi hiện ảnh bậc")]
    public GameObject[] baseParts;
    [Tooltip("Image hiện ảnh của bậc 1, 2")]
    public Image tierPortrait;
    [Tooltip("[0] = bậc 1, [1] = bậc 2")]
    public Sprite[] tierSprites;
    [Tooltip("Ảnh bậc gốc cùng khung với tierSprites (dùng cho màn tiến hóa)")]
    public Sprite basePortrait;

    public Sprite GetPortrait(int tier)
    {
        if (tier <= 0) return basePortrait;
        return tierSprites != null && tier <= tierSprites.Length ? tierSprites[tier - 1] : null;
    }

    public void SetBlind(bool on)
    {
        if (blindUnit != null) blindUnit.SetActive(on);
    }

    public void SetTier(int tier)
    {
        bool useTier = tier > 0 && tierPortrait != null && tierSprites != null && tier <= tierSprites.Length
                       && tierSprites[tier - 1] != null;
        if (baseParts != null)
            foreach (var p in baseParts) if (p != null) p.SetActive(!useTier);
        if (tierPortrait != null)
        {
            tierPortrait.gameObject.SetActive(useTier);
            if (useTier) tierPortrait.sprite = tierSprites[tier - 1];
        }
    }
}
