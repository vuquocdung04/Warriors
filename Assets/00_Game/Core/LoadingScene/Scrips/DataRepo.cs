
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

        UseProfile.EquippedMelee.Value = "2";        // món atk_percent
        EquipmentSave.SetLevel("2", 1);

        var stats = DataRepo.Instance.equipmentDatabase.BuildStats("stone");
        Debug.Log($"[BuildStats] stone melee atk={stats[0].atk}  (base * (1 + percent/100)?)");

        UseProfile.Coin.Value = 100000000;
    }
}
