using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WheelReward
{
    public CurrencyType currency = CurrencyType.Coin;
    [Tooltip("Coin tính theo civ đầu tiên, runtime tự nhân theo civ đang sở hữu")]
    public int amount = 100;
    public Sprite icon;
    [Tooltip("Trọng số random, càng lớn càng dễ trúng")]
    public int weight = 10;
}

[CreateAssetMenu(fileName = "WheelSpinDatabase", menuName = "Game/WheelSpinDatabase")]
public class WheelSpinDatabase : ScriptableObject
{
    [Tooltip("8 ô theo chiều kim đồng hồ, ô 0 nằm ngay dưới mũi tên")]
    public List<WheelReward> rewards = new List<WheelReward>();

    public int RandomIndex()
    {
        int total = 0;
        foreach (var r in rewards) total += Mathf.Max(0, r.weight);
        if (total <= 0) return Random.Range(0, rewards.Count);

        int roll = Random.Range(0, total);
        for (int i = 0; i < rewards.Count; i++)
        {
            roll -= Mathf.Max(0, rewards[i].weight);
            if (roll < 0) return i;
        }
        return rewards.Count - 1;
    }
}
