using System.Collections.Generic;
using UnityEngine;

public class UnitCardBar : MonoBehaviour
{
    [Header("3 card đặt sẵn trong UI (trái -> phải)")]
    public List<UnitCard> cards;   // [0],[1],[2]

    private BattleSpawner _spawner;
    private UnitDatabase _db;

    public void Init(BattleSpawner spawner, UnitDatabase db)
    {
        _spawner = spawner;
        _db = db;
        Build();
    }

    public void Build()
    {
        var units = _db.GetCivUnits(UseProfile.CurrentCiv.Value);

        for (int i = 0; i < cards.Count; i++)
        {
            if (i < units.Count && IsUnitUnlocked(i))   // thêm check unlock
            {
                cards[i].gameObject.SetActive(true);
                cards[i].Setup(i, units[i], OnCardClicked);
            }
            else cards[i].gameObject.SetActive(false);
        }
    }

    bool IsUnitUnlocked(int index)
    {
        if (index == 0) return true;                      
        if (index == 1) return UseProfile.Unit2Unlock.Value;
        if (index == 2) return UseProfile.Unit3Unlock.Value; 
        return false;
    }
    void OnCardClicked(int index)
    {
        _spawner.SpawnAlly(index);
    }
}