using System.Collections.Generic;
using UnityEngine;


public class SpawnModifier
{
    public System.Action<UnitStats> ModifyStats;
    public System.Action<Unit> AfterSpawn;
}
public class BattleSpawner : MonoBehaviour
{
    public House allyHouse;
    public House enemyHouse;
    private UnitDatabase _db;

    public float enemyHouseHpMultiplier = 3f;
    private Dictionary<string, UnitStats> _allyStats;
    private Dictionary<string, UnitStats> _enemyStats;
    private readonly Queue<SpawnModifier> _pendingAllyMods = new();
    private readonly UnitData _championData = new UnitData { id = "champion" };
    private readonly UnitData _zombieData = new UnitData { id = "zombie" };
    public void Init(UnitDatabase db)
    {
        _db = db;

        _allyStats = BuildStatsDict(UseProfile.CurrentCiv.Value, withEquipment: true);
        _enemyStats = BuildStatsDict(UseProfile.SelectedEnemyCiv.Value, withEquipment: false);

        InitHouse(allyHouse, Team.Ally);
        InitHouse(enemyHouse, Team.Enemy);
    }

    Dictionary<string, UnitStats> BuildStatsDict(string civId, bool withEquipment)
    {
        var dict = new Dictionary<string, UnitStats>();
        var statsList = withEquipment
            ? EquipmentStatCalculator.BuildStats(civId)
            : EquipmentStatCalculator.BuildBaseStats(civId);

        var units = _db.GetCivUnits(civId);
        for (int i = 0; i < units.Count && i < statsList.Count; i++)
            dict[units[i].id] = statsList[i];
        return dict;
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

    public void SpawnAlly(int index)
    {
        if (BattleManager.Instance != null && BattleManager.Instance.IsBattleOver) return;
        if (_db == null) return;

        var units = _db.GetCivUnits(UseProfile.CurrentCiv.Value);
        if (index < 0 || index >= units.Count) return;

        UnitData data = units[index];
        var food = BottomBar.Instance != null ? BottomBar.Instance.foodBar : null;
        if (food != null && !food.CanAfford(data.foodCost))
        {
            Debug.Log($"[Spawn] không đủ food cho '{data.id}' (cần {data.foodCost}, có {food.Food})");
            return;
        }

        SpawnModifier mod = _pendingAllyMods.Count > 0 ? _pendingAllyMods.Dequeue() : null;
        Unit u = SpawnUnit(Team.Ally, data, mod);
        if (u != null && food != null) food.Spend(data.foodCost);
    }
    public void EnqueueNextAllyModifier(SpawnModifier mod) => _pendingAllyMods.Enqueue(mod);
    
    Unit SpawnUnit(Team team, UnitData data, SpawnModifier mod = null)
    {
        Unit prefab = _db.GetUnitById(data.id);
        if (prefab == null) { Debug.LogError($"[Spawner] thiếu prefab '{data.id}'"); return null; }

        var statsDict = team == Team.Ally ? _allyStats : _enemyStats;
        if (!statsDict.TryGetValue(data.id, out var template))
        { Debug.LogError($"[Spawner] thiếu stats '{data.id}'"); return null; }

        Vector2Int anchor = GetSpawnCell(team);
        Vector2Int? cell = FindFreeNear(anchor.x, anchor.y);
        if (cell == null) return null;

        var stats = template.Clone();
        mod?.ModifyStats?.Invoke(stats);

        Unit u = Instantiate(prefab);
        u.Init(data, stats, team, cell.Value);
        mod?.AfterSpawn?.Invoke(u);
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

    public Unit SpawnChampion(Unit prefab, UnitStats stats)
    {
        if (prefab == null) { Debug.LogError("[Spawner] thiếu champion prefab"); return null; }

        Vector2Int anchor = GetSpawnCell(Team.Ally);
        Vector2Int? cell = FindFreeNear(anchor.x, anchor.y);
        if (cell == null) return null;

        Unit u = Instantiate(prefab);
        u.Init(_championData, stats, Team.Ally, cell.Value);
        return u;
    }
    public Unit SpawnZombie(Unit prefab, UnitStats stats)
    {
        if (prefab == null) { Debug.LogError("[Spawner] thiếu zombie prefab"); return null; }

        Vector2Int anchor = GetSpawnCell(Team.Ally);
        Vector2Int? cell = FindFreeNear(anchor.x, anchor.y);
        if (cell == null) return null;

        Unit u = Instantiate(prefab);
        u.Init(_zombieData, stats.Clone(), Team.Ally, cell.Value);
        return u;
    }

    public Unit SpawnEnemyById(string unitId)
    {
        if (_db == null) return null;

        var units = _db.GetCivUnits(UseProfile.SelectedEnemyCiv.Value);
        UnitData data = units.Find(u => u.id == unitId);
        if (data == null) { Debug.LogError($"[Spawner] civ enemy không có '{unitId}'"); return null; }

        return SpawnUnit(Team.Enemy, data);
    }
}