
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

        var st = EquipmentSave.Get("1");
        Debug.Log($"[EquipSave] món 1: level={st.level} card={st.card}");   // mong: level=3 card=7

        UseProfile.EquippedMelee.Value = "1";
        Debug.Log($"[EquipSave] đang đeo melee={UseProfile.EquippedMelee.Value}");

        UseProfile.Coin.Value = 100000000;
    }
}
