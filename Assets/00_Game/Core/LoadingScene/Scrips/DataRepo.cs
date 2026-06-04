
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
        var db = skillDatabase;
        foreach (var s in db.AllSkills())
        {
            Debug.Log($"[SkillDB] {s.id} {s.name} lvMax={s.levelMax} base={s.baseValue} per={s.valuePerLevel}");
            Debug.Log($"  lv1: {s.DescAt(1)}");
            Debug.Log($"  lv2: {s.DescAt(2)}");
        }
        UseProfile.Coin.Value = 100000000;
    }


}
