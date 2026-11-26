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
                bool upgradable = _spawner.CanUpgradeAlly(i);
                cards[i].Setup(i, units[i], OnCardClicked, upgradable ? OnUpgradeClicked : null, _spawner.GetAllyTier(i));
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

    void OnUpgradeClicked(int index)
    {
        if (!_spawner.TryUpgradeAlly(index)) return;

        var card = cards[index];
        int tier = _spawner.GetAllyTier(index);
        card.SetTier(tier);

        // Màn ăn mừng: unit cũ ▶ unit mới (popup -> trận tự tạm dừng tới khi đóng)
        Sprite oldSp = card.GetPortrait(tier - 1), newSp = card.GetPortrait(tier);
        _ = EvolveBox.Setup(GameScene.GetPopupHolder(), box => box.SetupAndShow(oldSp, newSp, tier));
    }
}