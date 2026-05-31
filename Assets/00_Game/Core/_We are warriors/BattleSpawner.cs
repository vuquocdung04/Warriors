using UnityEngine;
using UnityEngine.InputSystem;

public class BattleSpawner : MonoBehaviour
{
    public string[] unitIds;       // [0]=archer, [1]=spartan... khớp key 1-4

    private BattleGrid _grid;
    private UnitDatabase _db;
    private BattleManager _battle;
    private int _selected;

    public void Init(BattleGrid grid, UnitDatabase db, BattleManager battle)
    {
        _grid = grid;
        _db = db;
        _battle = battle;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.digit1Key.wasPressedThisFrame) _selected = 0;
            if (kb.digit2Key.wasPressedThisFrame) _selected = 1;
            if (kb.digit3Key.wasPressedThisFrame) _selected = 2;
            if (kb.digit4Key.wasPressedThisFrame) _selected = 3;
        }

        var pointer = Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
        {
            float x = pointer.position.ReadValue().x;
            Team team = x < Screen.width * 0.5f ? Team.Ally : Team.Enemy;
            Spawn(team);
        }
    }

    public void Spawn(Team team)
    {
        if (_db == null || unitIds == null || _selected >= unitIds.Length) return;
        string id = unitIds[_selected];

        UnitData data = _db.GetDataById(id);
        Unit prefab = _db.GetUnitById(id);
        if (data == null || prefab == null)
        {
            Debug.LogError($"[Spawner] id '{id}' -> data:{(data == null ? "NULL" : "ok")} prefab:{(prefab == null ? "NULL" : "ok")}");
            return;
        }

        Vector2Int anchor = GetSpawnCell(team);
        Vector2Int? cell = FindFreeNear(anchor.x, anchor.y);
        if (cell == null) return;

        Unit u = Instantiate(prefab);
        u.Init(data, team, _grid, cell.Value);
    }

    Vector2Int GetSpawnCell(Team team)
    {
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