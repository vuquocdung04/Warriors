using UnityEngine;

public class BattleSpawner : MonoBehaviour
{
    public House allyHouse;
    public House enemyHouse;

    private BattleGrid _grid;
    private UnitDatabase _db;
    private BattleManager _battle;

    public void Init(BattleGrid grid, UnitDatabase db, BattleManager battle)
    {
        _grid = grid;
        _db = db;
        _battle = battle;
        InitHouse(allyHouse, Team.Ally);
        InitHouse(enemyHouse, Team.Enemy);
    }
    [Header("Độ trâu nhà địch")]
    public float enemyHouseHpMultiplier = 3f;

    void InitHouse(House house, Team team)
    {
        if (house == null) return;
        string civ = team == Team.Ally ? UseProfile.CurrentCiv.Value : UseProfile.EnemyCiv.Value;
        HouseData data = _db.GetHouseData(civ);
        if (data == null) { Debug.LogError($"[Spawner] thiếu house data civ '{civ}'"); return; }

        float mult = team == Team.Enemy ? enemyHouseHpMultiplier : 1f;
        house.Init(data, team, _grid, OnHouseDestroyed, mult);
    }
    void OnHouseDestroyed(Team team)
    {
        _battle.EndBattle();

        bool win = team == Team.Enemy;
        Debug.Log($"[Battle] House {team} sập -> {(win ? "THẮNG" : "THUA")}");
        // TODO: gọi GameFlow.ShowWin()/ShowLose()
    }

    // 3 nút UI gọi: SpawnAlly(0/1/2)
    public void SpawnAlly(int index)
    {
        if (_battle != null && _battle.IsBattleOver) return;
        Spawn(Team.Ally, UseProfile.CurrentCiv.Value, index);
    }

    public void Spawn(Team team, string civId, int index)
    {
        if (_db == null) return;

        var units = _db.GetCivUnits(civId);
        if (index < 0 || index >= units.Count)
        { Debug.LogError($"[Spawner] civ '{civId}' không có lính index {index}"); return; }

        UnitData data = units[index];
        Unit prefab = _db.GetUnitById(data.id);
        if (prefab == null) { Debug.LogError($"[Spawner] thiếu prefab '{data.id}'"); return; }

        Vector2Int anchor = GetSpawnCell(team);
        Vector2Int? cell = FindFreeNear(anchor.x, anchor.y);
        if (cell == null) return;

        Unit u = Instantiate(prefab);
        u.Init(data, team, _grid, cell.Value);
    }

    Vector2Int GetSpawnCell(Team team)
    {
        House house = team == Team.Ally ? allyHouse : enemyHouse;
        if (house != null) return _grid.WorldToCell(house.GetSpawnPosition());
        int x = team == Team.Ally ? 0 : _grid.Width - 1;
        return new Vector2Int(x, _grid.Height / 2);
    }

    Vector2Int? FindFreeNear(int x, int y)
    {
        if (_grid.IsFree(x, y)) return new Vector2Int(x, y);
        for (int d = 1; d < _grid.Height; d++)
        {
            if (_grid.IsFree(x, y + d)) return new Vector2Int(x, y + d);
            if (_grid.IsFree(x, y - d)) return new Vector2Int(x, y - d);
        }
        return null;
    }
}