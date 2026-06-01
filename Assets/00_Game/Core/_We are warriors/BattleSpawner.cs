using UnityEngine;

public class BattleSpawner : MonoBehaviour
{
    public House allyHouse;
    public House enemyHouse;
    private UnitDatabase _db;

    [Header("Độ trâu nhà địch")]
    public float enemyHouseHpMultiplier = 3f;

    public void Init(UnitDatabase db)
    {
        _db = db;
        InitHouse(allyHouse, Team.Ally);
        InitHouse(enemyHouse, Team.Enemy);
    }

    void InitHouse(House house, Team team)
    {
        if (house == null) return;
        string civ = team == Team.Ally ? UseProfile.CurrentCiv.Value : UseProfile.EnemyCiv.Value;
        HouseData data = _db.GetHouseData(civ);
        if (data == null) { Debug.LogError($"[Spawner] thiếu house data civ '{civ}'"); return; }

        float mult = team == Team.Enemy ? enemyHouseHpMultiplier : 1f;
        house.Init(data, team, mult);
    }

    // 3 nút UI gọi: SpawnAlly(0/1/2)
    public void SpawnAlly(int index)
    {
        if (BattleManager.Instance != null && BattleManager.Instance.IsBattleOver) return;
        if (_db == null) return;

        var units = _db.GetCivUnits(UseProfile.CurrentCiv.Value);
        if (index < 0 || index >= units.Count) return;

        UnitData data = units[index];
        var food = FoodManager.Instance;
        if (food != null && !food.CanAfford(data.foodCost))
        {
            Debug.Log($"[Spawn] không đủ food cho '{data.id}' (cần {data.foodCost}, có {food.Food})");
            return;
        }

        Unit u = SpawnUnit(Team.Ally, data);
        if (u != null && food != null) food.Spend(data.foodCost);     // ra lính rồi mới trừ
    }

    public Unit Spawn(Team team, string civId, int index)
    {
        if (_db == null) return null;

        var units = _db.GetCivUnits(civId);
        if (index < 0 || index >= units.Count)
        { Debug.LogError($"[Spawner] civ '{civId}' không có lính index {index}"); return null; }

        return SpawnUnit(team, units[index]);
    }

    Unit SpawnUnit(Team team, UnitData data)
    {
        Unit prefab = _db.GetUnitById(data.id);
        if (prefab == null) { Debug.LogError($"[Spawner] thiếu prefab '{data.id}'"); return null; }

        Vector2Int anchor = GetSpawnCell(team);
        Vector2Int? cell = FindFreeNear(anchor.x, anchor.y);
        if (cell == null) return null;   // hết ô -> không spawn (food chưa bị trừ)

        Unit u = Instantiate(prefab);
        u.Init(data, team, cell.Value);
        return u;
    }

    Vector2Int GetSpawnCell(Team team)
    {
        var grid = BattleGrid.Instance;
        House house = team == Team.Ally ? allyHouse : enemyHouse;
        if (house != null) return grid.WorldToCell(house.GetSpawnPosition());
        int x = team == Team.Ally ? 0 : grid.Width - 1;
        return new Vector2Int(x, grid.Height / 2);
    }

    Vector2Int? FindFreeNear(int x, int y)
    {
        var grid = BattleGrid.Instance;
        if (grid.IsFree(x, y)) return new Vector2Int(x, y);
        for (int d = 1; d < grid.Height; d++)
        {
            if (grid.IsFree(x, y + d)) return new Vector2Int(x, y + d);
            if (grid.IsFree(x, y - d)) return new Vector2Int(x, y - d);
        }
        return null;
    }
}