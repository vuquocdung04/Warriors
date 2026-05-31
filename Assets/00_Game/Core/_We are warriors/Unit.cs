using UnityEngine;
using UnityEngine.Rendering;

public class Unit : MonoBehaviour, IDamageable
{
    public string id;
    public Team team;

    public Transform visual;

    [Header("Sorting / Movement")]
    public float depthScale = 100f;
    public int maxPassRange = 4;

    public bool IsAlive => _state != UnitState.Dead;
    public int FrontPriority => _stats != null ? _stats.frontPriority : 0;

    private UnitHpBar _hpBar;
    private UnitData _data;
    private UnitStats _stats;
    private BattleGrid _grid;
    private SortingGroup _sortingGroup;
    private UnitState _state = UnitState.Moving;

    private Vector2Int _cell, _targetCell;
    private bool _moving;
    private int _preferVertical = 1;
    private float _attackTimer;
    private Unit _attackTarget;
    private IAttackStrategy _attackStrategy;
    private int ForwardX => team == Team.Ally ? +1 : -1;
    private float AttackRangeWorld =>
        (_stats != null ? _stats.attackRangeInCells : 1.5f) * (_grid != null ? _grid.cellSize : 0.5f);

    public void Init(UnitData data, Team team, BattleGrid grid, Vector2Int startCell)
    {
        _data = data;
        _stats = new UnitStats(data);
        this.team = team;
        _grid = grid;
        _cell = startCell;
        _grid.Occupy(_cell, this);

        _sortingGroup = GetComponentInChildren<SortingGroup>();

        _attackStrategy = GetComponentInChildren<IAttackStrategy>();
        _attackStrategy?.Init(this);

        transform.position = _grid.CellToWorld(_cell);
        visual.localRotation = Quaternion.Euler(0f, team == Team.Enemy ? 180f : 0f, 0f);
        UpdateDepth();
        _preferVertical = Random.value < 0.5f ? 1 : -1;
        _state = UnitState.Moving;

        if (BattleManager.Instance != null) BattleManager.Instance.Register(this);
        else Debug.LogError("[Unit] Thiếu BattleManager trong scene!");

        _hpBar = GetComponent<UnitHpBar>();
        _hpBar?.Set(1f);
    }

    public void Tick(float dt)
    {
        if (_state == UnitState.Dead) return;
        if (_attackTimer > 0f) _attackTimer -= dt;

        switch (_state)
        {
            case UnitState.Moving: UpdateMoving(dt); break;
            case UnitState.Attacking: UpdateAttacking(dt); break;
        }
    }

    // ---- MOVING ----
    void UpdateMoving(float dt)
    {
        if (_moving) { MoveStep(dt); return; }

        _attackTarget = BattleManager.Instance.FindNearestEnemyInRange(this, AttackRangeWorld);
        if (_attackTarget != null) { _state = UnitState.Attacking; return; }

        // luôn dồn về hàng của địch gần nhất
        var nearest = BattleManager.Instance.FindNearestEnemyInRange(this, float.MaxValue);
        if (nearest != null)
        {
            int targetY = _grid.WorldToCell(nearest.transform.position).y;
            if (targetY > _cell.y) _preferVertical = 1;
            else if (targetY < _cell.y) _preferVertical = -1;
        }

        TryAdvance();
    }
    void MoveStep(float dt)
    {
        Vector3 dest = _grid.CellToWorld(_targetCell);
        transform.position = Vector3.MoveTowards(transform.position, dest, _stats.moveSpeed * dt);
        UpdateDepth();
        if ((transform.position - dest).sqrMagnitude < 0.0004f)
        {
            transform.position = dest;
            _grid.Free(_cell);
            _cell = _targetCell;
            _moving = false;
        }
    }

    void TryAdvance()
    {
        int fx = ForwardX;
        Vector2Int fwd = new Vector2Int(_cell.x + fx, _cell.y);

        if (_grid.IsFree(fwd)) { BeginMove(fwd); return; }

        Vector2Int? landing = FindFreeAhead(fx);             // xuyên qua đồng minh priority thấp hơn
        if (landing != null) { BeginMove(landing.Value); return; }

        if (TryDiagonal(fx, _preferVertical)) return;
        if (TryDiagonal(fx, -_preferVertical)) return;
        // kẹt hết -> đứng im
    }

    bool TryDiagonal(int fx, int dy)
    {
        Vector2Int c = new Vector2Int(_cell.x + fx, _cell.y + dy);
        if (!_grid.IsFree(c)) return false;
        _preferVertical = dy > 0 ? 1 : -1;
        BeginMove(c);
        return true;
    }

    Vector2Int? FindFreeAhead(int fx)
    {
        for (int step = 1; step <= maxPassRange; step++)
        {
            Vector2Int c = new Vector2Int(_cell.x + fx * step, _cell.y);
            if (!_grid.IsInside(c)) return null;
            if (_grid.IsFree(c)) return c;

            Unit u = _grid.GetOccupant(c);
            if (u == null) return c;
            if (u.team != team) return null;                  // địch -> chặn
            if (u.FrontPriority >= FrontPriority) return null; // ngang/cao (melee) -> không xuyên
            // đồng minh thấp hơn (ranged) -> cho qua
        }
        return null;
    }

    void BeginMove(Vector2Int target)
    {
        _grid.Occupy(target, this);
        _targetCell = target;
        _moving = true;
    }

    void UpdateDepth()
    {
        if (_sortingGroup != null)
            _sortingGroup.sortingOrder = Mathf.RoundToInt(-transform.position.y * depthScale);
    }

    // ---- ATTACKING ----
    void UpdateAttacking(float dt)
    {
        if (_attackTarget == null || !_attackTarget.IsAlive || !InRange(_attackTarget))
        { _state = UnitState.Moving; return; }
        TryAttack();
    }

    bool InRange(Unit t)
    {
        float r = AttackRangeWorld;
        return (t.transform.position - transform.position).sqrMagnitude <= r * r;
    }

    void TryAttack()
    {
        if (_attackTimer > 0f) return;
        _attackTimer = 1f / Mathf.Max(0.01f, _stats.attackSpeed);
        _attackStrategy?.Attack(_attackTarget);
    }
    public void DealDamage(Unit target)
    {
        if (target == null || !target.IsAlive) return;
        float dmg = _stats.atk;
        if (Random.value < _stats.criticalChance) dmg *= 2f;
        target.TakeDamage(dmg);
        if (_stats.lifeSteal > 0f) Heal(dmg * _stats.lifeSteal);
    }
    void Heal(float amount)
    {
        _stats.hp = Mathf.Min(_stats.maxHp, _stats.hp + amount);
        _hpBar?.Set(_stats.hp / _stats.maxHp);
    }

    // ---- DAMAGE / D
    // EATH ----
    public void TakeDamage(float dmg)
    {
        if (_state == UnitState.Dead) return;
        _stats.hp -= dmg;
        _hpBar?.Set(_stats.hp / _stats.maxHp);
        if (_stats.hp <= 0f) Die();
    }

    void Die()
    {
        _state = UnitState.Dead;
        _grid.Free(_cell);
        if (_moving) _grid.Free(_targetCell);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        float cs = _grid != null ? _grid.cellSize : 0.5f;
        float cells = _stats != null ? _stats.attackRangeInCells
                     : (_data != null ? _data.attackRangeInCells : 1.5f);

        Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.9f);        // đỏ: tầm đánh
        Gizmos.DrawWireSphere(transform.position, cells * cs);
    }
}