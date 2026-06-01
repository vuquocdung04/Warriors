
using UnityEngine;

public class DataRepo : MonoBehaviour
{
    public static DataRepo Instance { get; private set; }

    public LocalizationDataBase localizationDataBase;
    public AudioDataBase audioData;
    public AvatarDataBase avatarData;
    public UnitDatabase unitDatabase;
    public void Init()
    {
        Instance = this;
        unitDatabase.Init();
        UseProfile.CurrentCiv.Value = "stone";
    }
}
