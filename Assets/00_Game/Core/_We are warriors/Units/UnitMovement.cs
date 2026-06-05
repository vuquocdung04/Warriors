using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class UnitMovement
{
    readonly Unit _owner;
    readonly BattleGrid _grid;
    readonly SortingGroup _sortingGroup;
    readonly float _depthScale;
    readonly int _maxPassRange;
    readonly int _forwardX;

    Vector2Int _cell, _targetCell;
    bool _moving;
    int _preferVertical;

    public Vector2Int Cell => _cell;
    public bool IsMoving => _moving;

    public UnitMovement(Unit owner, BattleGrid grid, Vector2Int startCell, float depthScale, int maxPassRange)
    {
        _owner = owner;
        _grid = grid;
        _depthScale = depthScale;
        _maxPassRange = maxPassRange;
        _forwardX = owner.team == Team.Ally ? +1 : -1;
        _sortingGroup = owner.GetComponentInChildren<SortingGroup>();

        _cell = startCell;
        _grid.Occupy(_cell, owner);
        owner.transform.position = _grid.CellToWorld(_cell);
        _preferVertical = Random.value < 0.5f ? 1 : -1;
        UpdateDepth();
    }

    public void Step(float dt, float moveSpeed)
    {
        Vector3 dest = _grid.CellToWorld(_targetCell);
        Transform t = _owner.transform;
        t.position = Vector3.MoveTowards(t.position, dest, moveSpeed * dt);
        UpdateDepth();
        if ((t.position - dest).sqrMagnitude < 0.0004f)
        {
            t.position = dest;
            _grid.Free(_cell);
            _cell = _targetCell;
            _moving = false;
        }
    }

    public void TryAdvance()
    {
        Vector2Int fwd = new Vector2Int(_cell.x + _forwardX, _cell.y);
        if (_grid.IsFree(fwd)) { Begin(fwd); return; }

        Vector2Int? landing = FindFreeAhead();          // xuyên qua đồng minh priority thấp hơn
        if (landing != null) { Begin(landing.Value); return; }

        if (TryDiagonal(_preferVertical)) return;
        if (TryDiagonal(-_preferVertical)) return;
        // kẹt hết -> đứng im
    }

    bool TryDiagonal(int dy)
    {
        Vector2Int c = new Vector2Int(_cell.x + _forwardX, _cell.y + dy);
        if (!_grid.IsFree(c)) return false;
        _preferVertical = dy > 0 ? 1 : -1;
        Begin(c);
        return true;
    }

    Vector2Int? FindFreeAhead()
    {
        for (int step = 1; step <= _maxPassRange; step++)
        {
            Vector2Int c = new Vector2Int(_cell.x + _forwardX * step, _cell.y);
            if (!_grid.IsInside(c)) return null;
            if (_grid.IsFree(c)) return c;

            Unit u = _grid.GetOccupant(c);
            if (u == null) return c;
            if (u.team != _owner.team) return null;                        // địch -> chặn
            if (u.FrontPriority > _owner.FrontPriority) return null;        // chỉ chặn đồng minh ưu tiên CAO hơn
            // đồng minh ngang/thấp hơn -> cho lách qua tìm ô trống
        }
        return null;
    }

    void Begin(Vector2Int target)
    {
        _grid.Occupy(target, _owner);
        _targetCell = target;
        _moving = true;
    }

    void UpdateDepth()
    {
        if (_sortingGroup != null)
            _sortingGroup.sortingOrder = Mathf.RoundToInt(-_owner.transform.position.y * _depthScale);
    }

    public Tween PushBy(Vector2Int delta, float slideSpeed)
    {
        if (_moving)
        {
            _grid.Free(_targetCell);
            _moving = false;
        }

        Vector2Int target = FindPushTarget(_cell + delta);
        if (target == _cell) return null;

        _grid.Free(_cell);
        _cell = target;
        _grid.Occupy(_cell, _owner);

        Vector3 dest = _grid.CellToWorld(_cell);
        float dur = slideSpeed > 0f ? Vector3.Distance(_owner.transform.position, dest) / slideSpeed : 0f;
        return _owner.transform.DOMove(dest, dur).SetEase(Ease.OutQuad)
            .OnUpdate(UpdateDepth).SetLink(_owner.gameObject);
    }
    Vector2Int FindPushTarget(Vector2Int wanted)
    {
        int dirX = wanted.x > _cell.x ? 1 : (wanted.x < _cell.x ? -1 : 0);
        Vector2Int best = _cell;

        Vector2Int c = _cell;
        while (c != wanted)
        {
            Vector2Int next = new Vector2Int(c.x + dirX, c.y);
            if (!_grid.IsInside(next)) break;       
            if (!_grid.IsFree(next)) break;          
            best = next;
            c = next;
        }
        return best;
    }
    public void Release()
    {
        _grid.Free(_cell);
        if (_moving) _grid.Free(_targetCell);
    }
}
