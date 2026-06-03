using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatItem : MonoBehaviour
{
    [Header("Unlock")]
    public GameObject iconUnlock;      // tab icon thật (lấy theo stat_type)
    public Image iconImage;            // image trong iconUnlock

    [Header("Lock")]
    public GameObject iconLock;        // icon khóa (set sẵn)
    public TMP_Text levelUnlockText;   // "level cần để mở"

    [Header("Mô tả (luôn hiện)")]
    public TMP_Text textDes;           // "+10 Health"

    public void Setup(EquipmentStat stat, int currentLevel)
    {
        var db = DataRepo.Instance.equipmentDatabase;
        StatData def = db.GetStat(stat.statType);

        bool unlocked = currentLevel >= stat.levelUnlock;

        if (iconUnlock != null) iconUnlock.SetActive(unlocked);
        if (iconLock != null) iconLock.SetActive(!unlocked);

        if (unlocked)
        {
            if (iconImage != null) iconImage.sprite = db.GetStatIcon(stat.statType);
        }
        else
        {
            if (levelUnlockText != null) levelUnlockText.text = stat.levelUnlock.ToString();
        }

        // textDes luôn hiện: +{value}{%} {displayName}
        if (textDes != null)
        {
            string calc = def != null ? def.calcType : "flat";
            string suffix = (calc == "percent" || calc == "rate") ? "%" : "";
            string name = def != null ? def.displayName : stat.statType;
            textDes.text = $"+{FormatValue(stat.value)}{suffix} {name}";
        }
    }

    string FormatValue(float v)
    {
        // bỏ .0 thừa: 10 -> "10", 10.5 -> "10.5"
        return v % 1 == 0 ? ((int)v).ToString() : v.ToString("0.#");
    }
}