using EventDispatcher;
using UnityEngine;

public class DropController : MonoBehaviour
{
    public CurrencyDrop coinPrefab;   // prefab sprite coin
    public CurrencyDrop gemPrefab;    // prefab sprite gem

    private EconomyConfig _config;
    public int edgeOffset = 2;
    public void Init()
    {
        _config = DataRepo.Instance.economyDatabase.Get(UseProfile.SelectedEnemyCiv.Value);
        this.RegisterListener(EventID.UNIT_DIED, OnUnitDied);
    }

    void OnUnitDied(object param)
    {
        var unit = param as Unit;
        if (unit == null || unit.Team != Team.Enemy || _config == null) return;

        Vector3 edge = GetEdgePosition(unit.CurrentCell);

        if (Random.value < _config.coinDropRate)
            SimplePool2.Spawn(coinPrefab).Play(DropType.Coin, _config.coinPerKill, edge);

        if (Random.value < _config.gemDropRate)
            SimplePool2.Spawn(gemPrefab).Play(DropType.Gem, _config.gemPerKill, edge);
    }

    Vector3 GetEdgePosition(Vector2Int cell)
    {
        var grid = BattleGrid.Instance;
        int mid = grid.Height / 2;
        int edgeY = cell.y >= mid ? grid.Height - 1 + edgeOffset : -edgeOffset;
        return grid.CellToWorld(new Vector2Int(cell.x, edgeY));
    }

    void OnDestroy() => this.RemoveListener(EventID.UNIT_DIED, OnUnitDied);
}