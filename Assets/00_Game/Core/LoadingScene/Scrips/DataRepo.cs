
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
    public GachaDatabase gachaDatabase;
    public SkillDatabase skillDatabase;
    public void Init()
    {
        Instance = this;
        unitDatabase.Init();
        equipmentDatabase.Init();
        gachaDatabase.Init();
        skillDatabase.Init();

        UseProfile.FoodRate.Value = 1f;
        UseProfile.Coin.Value = 100000000;
    }


}
