
using System.Collections.Generic;
using UnityEngine;

public class DataRepo : MonoBehaviour
{
    public static DataRepo Instance { get; private set; }

    public LocalizationDataBase localizationDataBase;
    public AudioDataBase audioData;
    public AvatarDataBase avatarData;
    public UnitDatabase unitDatabase;
    public EquipmentDatabase equipmentDatabase;
    public void Init()
    {
        Instance = this;
        unitDatabase.Init();
        equipmentDatabase.Init();

        SetupTestData();

        UseProfile.Coin.Value = 100000000;
    }
    void SetupTestData()
    {
        // --- ĐEO THỬ ---
        EquipmentSave.Get(EquipType.Melee, "1").level = 2;     // melee common -> level 2
        UseProfile.EquippedMelee.Value = "1";

        UseProfile.EquippedRange.Value = "5";                   // range rare
        UseProfile.EquippedShield.Value = "13";                 // shield legend

        // --- MỞ KHÓA (sở hữu: card > 0) ---
        Own(EquipType.Melee, "1", "2", "3", "4");               // unit1: 4 món melee
        Own(EquipType.Range, "5");                              // unit2: 1 món range
        Own(EquipType.Shield, "1", "2", "3", "4", "5", "6", "7", "8", "9");  // unit3: 9 món shield

        EquipmentSave.Save();
    }

    void Own(EquipType type, params string[] ids)
    {
        foreach (var id in ids)
            EquipmentSave.Get(type, id).card = 1;
    }

}
