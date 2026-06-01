using EventDispatcher;
using UnityEngine;

public class Unit : MonoBehaviour, IDamageable
{
    public string id;
    public Team team;
    public Team Team => team;
    public Transform Transform => transform;
    public Transform visual;

    [Header("Sorting / Movement")]
    public float depthScale = 100f;
    public int maxPassRange = 4;

    public bool IsAlive => _state != UnitState.Dead;
    public int FrontPriority => _stats != null ? _stats.frontPriority : 0;
    public UnitData Data => _data;          // cho UnitDrop biết loại lính khi chết

    private UnitHpBar _hpBar;
    private UnitData _data;
    private UnitStats _stats;
    private BattleGrid _grid;
    private UnitMovement _movement;
    private UnitState _state = UnitState.Moving;

    private float _attackTimer;
    private IDamageable _attackTarget;
    private IAttackStrategy _attackStrategy;

    private float AttackRangeWorld =>
        (_stats != null ? _stats.attackRangeInCells : 1.5f) * (_grid != null ? _grid.cellSize : 0.5f);

    public void Init(UnitData data, Team team, Vector2Int startCell)
    {
        _data = data;
        _stats = new UnitStats(data);
        this.team = team;
        _grid = BattleGrid.Instance;

        _movement = new UnitMovement(this, _grid, startCell, depthScale, maxPassRange);

        _attackStrategy = GetComponentInChildren<IAttackStrategy>();
        _attackStrategy?.Init(this);

        visual.localRotation = Quaternion.Euler(0f, team == Team.Enemy ? 180f : 0f, 0f);
        _state = UnitState.Moving;

        if (BattleManager.Instance != null) BattleManager.Instance.Register(this);
        else Debug.LogError("[Unit] Thiếu BattleManager trong scene!");

        _hpBar = GetComponent<UnitHpBar>();
        _hpBar?.Set(1f);
    }

    public float SqrDistanceTo(Vector3 p) => (transform.position - p).sqrMagnitude;

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
        if (_movement.IsMoving) { _movement.Step(dt, _stats.moveSpeed); return; }

        _attackTarget = BattleManager.Instance.FindNearestEnemyInRange(this, AttackRangeWorld);
        if (IsValidTarget(_attackTarget)) { _state = UnitState.Attacking; return; }
        _attackTarget = null;
        _movement.TryAdvance();
    }

    // ---- ATTACKING ----
    void UpdateAttacking(float dt)
    {
        if (!IsValidTarget(_attackTarget) || !InRange(_attackTarget))
        { _attackTarget = null; _state = UnitState.Moving; return; }
        TryAttack();
    }

    static bool IsValidTarget(IDamageable t)
    {
        if (t == null) return false;
        if (t is Object o && o == null) return false;
        return t.IsAlive;
    }

    bool InRange(IDamageable t)
    {
        float r = AttackRangeWorld;
        return t.SqrDistanceTo(transform.position) <= r * r;
    }

    void TryAttack()
    {
        if (_attackTimer > 0f) return;
        _attackTimer = 1f / Mathf.Max(0.01f, _stats.attackSpeed);
        _attackStrategy?.Attack(_attackTarget);
    }

    public void DealDamage(IDamageable target)
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

    // ---- DAMAGE / DEATH ----
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
        _movement.Release();
        this.PostEvent(EventID.UNIT_DIED, this);   // UnitDrop sẽ nghe để rơi phần thưởng
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
