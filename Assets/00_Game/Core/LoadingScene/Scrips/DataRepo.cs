
using System.Collections.Generic;
using UnityEngine;

public class DataRepo : MonoBehaviour
{
    public static DataRepo Instance { get; private set; }

    public LocalizationDataBase localizationDataBase;
    public AvatarDataBase avatarData;
    public List<AudioDataBase> audioDataList;
    [Space(5)]
    public UnitDatabase unitDatabase;
    public EquipmentDatabase equipmentDatabase;
    public GachaDatabase gachaDatabase;
    public SkillDatabase skillDatabase;
    public EnemyWaveDatabase enemyWaveDatabase;
    public EconomyDatabase economyDatabase;
    public void Init()
    {
        Instance = this;
        unitDatabase.Init();
        equipmentDatabase.Init();
        gachaDatabase.Init();
        skillDatabase.Init();
        enemyWaveDatabase.Init();
        economyDatabase.Init();
        UseProfile.EnemyCiv.Value = "renai";
        UseProfile.Coin.Value = 100000000;
    }


}
