
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

        var rankCount = new Dictionary<string, int>();
        for (int i = 0; i < 200; i++)
        {
            var res = GachaService.Spin();
            if (res == null) continue;
            rankCount[res.rank] = rankCount.TryGetValue(res.rank, out var c) ? c + 1 : 1;
        }
        foreach (var kv in rankCount) Debug.Log($"[Gacha] {kv.Key}: {kv.Value}/200");
        Debug.Log($"[Gacha] level={UseProfile.GachaLevel.Value} spin={UseProfile.GachaSpin.Value}");

        // test nâng level món
        var raph = equipmentDatabase.GetMelee("1");
        Debug.Log($"[Up] raph card={EquipmentSave.Get("1").card} cần={EquipmentUpgrade.CardNeeded(raph, EquipmentSave.Get("1").level)} canUp={EquipmentUpgrade.CanUpgrade(raph)}");
        if (EquipmentUpgrade.TryUpgrade(raph))
            Debug.Log($"[Up] raph lên level {EquipmentSave.Get("1").level}");

        UseProfile.Coin.Value = 100000000;
    }
}
