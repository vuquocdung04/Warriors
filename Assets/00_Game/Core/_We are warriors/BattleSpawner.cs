using System.Collections.Generic;
using UnityEngine;

public class BattleSpawner : MonoBehaviour
{
    public House allyHouse;
    public House enemyHouse;
    private UnitDatabase _db;

    [Header("Độ trâu nhà địch")]
    public float enemyHouseHpMultiplier = 3f;

    private Dictionary<string, UnitStats> _allyStats;
    private Dictionary<string, UnitStats> _enemyStats;

    public void Init(UnitDatabase db)
    {
        _db = db;

        _allyStats  = BuildStatsDict(UseProfile.CurrentCiv.Value, withEquipment: true);
        _enemyStats = BuildStatsDict(UseProfile.EnemyCiv.Value, withEquipment: false);

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
        var food = FoodManager.Instance;
        if (food != null && !food.CanAfford(data.foodCost))
        {
            Debug.Log($"[Spawn] không đủ food cho '{data.id}' (cần {data.foodCost}, có {food.Food})");
            return;
        }

        Unit u = SpawnUnit(Team.Ally, data);
        if (u != null && food != null) food.Spend(data.foodCost);
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

        var statsDict = team == Team.Ally ? _allyStats : _enemyStats;
        if (!statsDict.TryGetValue(data.id, out var template))
        { Debug.LogError($"[Spawner] thiếu stats '{data.id}'"); return null; }

        Vector2Int anchor = GetSpawnCell(team);
        Vector2Int? cell = FindFreeNear(anchor.x, anchor.y);
        if (cell == null) return null;

        Unit u = Instantiate(prefab);
        u.Init(data, template.Clone(), team, cell.Value);   // clone: máu riêng từng con
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