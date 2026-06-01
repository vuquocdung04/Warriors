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
            if (i < units.Count)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].Setup(i, units[i], OnCardClicked);
            }
            else cards[i].gameObject.SetActive(false); 
        }
    }

    void OnCardClicked(int index)
    {
        _spawner.SpawnAlly(index);
    }
}