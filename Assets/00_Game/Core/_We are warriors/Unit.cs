using DG.Tweening;
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
    public UnitData Data => _data;

    private UnitHpBar _hpBar;
    private UnitData _data;
    private UnitStats _stats;
    private BattleGrid _grid;
    private UnitMovement _movement;
    private UnitState _state = UnitState.Moving;

    private float _attackTimer;
    private IDamageable _attackTarget;
    private IAttackStrategy _attackStrategy;

    [Header("Hit Feedback")]
    public Color hitFlashColor = Color.white;
    public float hitFlashTime = 0.06f;

    private SpriteRenderer[] _sprites;
    private Color[] _spriteBaseColors;
    private UnitEffects _effects;
    private float AttackRangeWorld =>
        (_stats != null ? _stats.attackRangeInCells : 1.5f) * (_grid != null ? _grid.cellSize : 0.5f);
    public Vector2Int CurrentCell => _movement.Cell;
    public void Init(UnitData data, UnitStats stats, Team team, Vector2Int startCell)
    {
        _data = data;
        _stats = stats;              // dùng stat đã cộng equipment
        this.team = team;
        _grid = BattleGrid.Instance;
        _movement = new UnitMovement(this, _grid, startCell, depthScale, maxPassRange);
        _attackStrategy = GetComponentInChildren<IAttackStrategy>();
        _attackStrategy?.Init(this);
        visual.localRotation = Quaternion.Euler(0f, team == Team.Enemy ? 180f : 0f, 0f);
        _state = UnitState.Moving;
        if (BattleManager.Instance != null) BattleManager.Instance.Register(this);
        _hpBar = GetComponent<UnitHpBar>();
        _hpBar?.Set(1f);
        CacheVisualForFeedback();

        InitInternal();
    }
    void InitInternal()
    {
        _effects = new UnitEffects();
        _effects.Init(this);
    }

    public void AddEffect(IStatusEffect e) => _effects.Add(e);


    void CacheVisualForFeedback()
    {
        _sprites = visual != null ? visual.GetComponentsInChildren<SpriteRenderer>(true)
                                  : System.Array.Empty<SpriteRenderer>();
        _spriteBaseColors = new Color[_sprites.Length];
        for (int i = 0; i < _sprites.Length; i++) _spriteBaseColors[i] = _sprites[i].color;
    }

    public float SqrDistanceTo(Vector3 p) => (transform.position - p).sqrMagnitude;

    public void Tick(float dt)
    {
        if (_state == UnitState.Dead) return;

        _effects.Tick(dt);
        if (_effects.IsControlled()) return;

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
        if (_movement.IsMoving)
        {
            float speed = _stats.moveSpeed * _effects.MoveMul();
            _movement.Step(dt, speed);
            return;
        }

        _attackTarget = BattleManager.Instance.FindNearestEnemyInRange(this, AttackRangeWorld);
        if (IsValidTarget(_attackTarget)) { _state = UnitState.Attacking; return; }
        _attackTarget = null;
        _movement.TryAdvance();
    }
    public void PushBack(int cells, float slideSpeed)
    {
        int dir = team == Team.Ally ? -1 : +1;   
        var tween = _movement.PushBy(new Vector2Int(dir * cells, 0), slideSpeed);

        _attackTarget = null;                   
        _state = UnitState.Moving;

        float dur = tween != null ? tween.Duration() : 0f;
        if (dur > 0f) AddEffect(new PushEffect(dur)); 
    }
    // ---- ATTACKING ----
    void UpdateAttacking(float dt)
    {
        // đã engage thì giữ đánh tới khi địch ra NGOÀI tầm + đệm 15% (chống rung khi địch đứng đúng mép tầm)
        if (!IsValidTarget(_attackTarget) || !InRange(_attackTarget, 1.15f))
        { _attackTarget = null; _state = UnitState.Moving; return; }
        TryAttack();
    }

    static bool IsValidTarget(IDamageable t)
    {
        if (t == null) return false;
        if (t is Object o && o == null) return false;
        return t.IsAlive;
    }

    bool InRange(IDamageable t, float mult = 1f)
    {
        float r = AttackRangeWorld * mult;
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
        if (_stats.hp <= 0f) { Die(); return; }
        PlayHitFeedback();
    }

    // Phản hồi khi trúng đòn: nháy sáng sprite rồi trả về màu gốc (DOTween tự huỷ khi destroy nhờ SetLink).
    void PlayHitFeedback()
    {
        if (_sprites.Length == 0) return;
        for (int i = 0; i < _sprites.Length; i++)
            if (_sprites[i] != null) _sprites[i].color = hitFlashColor;
        DOVirtual.DelayedCall(hitFlashTime, RestoreSpriteColors).SetLink(gameObject);
    }

    void RestoreSpriteColors()
    {
        for (int i = 0; i < _sprites.Length; i++)
            if (_sprites[i] != null) _sprites[i].color = _spriteBaseColors[i];
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
