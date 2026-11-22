using System.Collections.Generic;
using DG.Tweening;
using EventDispatcher;
using Sirenix.OdinInspector;
using UnityEngine;

// Boss khủng long trong Dungeon: đi chậm từ phải sang nhà người chơi, gặp lính/nhà thì dừng lại
// Cắn (AOE quanh chỗ miệng cắm xuống đất) và Gầm (AOE sóng xung kích) xen kẽ.
// Với lính, boss là 1 "bức tường di động" ở mép trước (giống House) -> lính ở mọi hàng đều đánh tới được.
// Chết -> bắn HOUSE_DESTROYED(Team.Enemy) để dùng lại luồng thắng trận sẵn có.
[RequireComponent(typeof(DinoAnimator))]
public class DungeonBoss : MonoBehaviour, IDamageable
{
    [Title("Info")]
    public string bossName = "Dino";

    [Title("Stats (nhân từ unit thứ 3 của civ người chơi)")]
    public float hpMultiplier = 100f;
    [Tooltip("ATK boss = atk unit thứ 3 x hệ số này (AOE nên để thấp hơn HP)")]
    public float atkMultiplier = 3f;
    [ReadOnly, ShowInInspector] private float _maxHp;
    [ReadOnly, ShowInInspector] private float _hp;
    [ReadOnly, ShowInInspector] private float _atk;

    [Title("Movement / Combat")]
    [Tooltip("Tốc độ đi (world unit / giây)")]
    public float moveSpeed = 0.245f;
    [Tooltip("Mép trước cách mục tiêu bao nhiêu thì dừng lại đánh")]
    public float attackRange = 1.4f;
    [Tooltip("Nghỉ giữa 2 đòn (giây, tính sau khi animation đòn trước xong)")]
    public float attackCooldown = 1.2f;
    [Tooltip("Cứ bao nhiêu đòn thì có 1 đòn Gầm (còn lại là Cắn)")]
    public int roarEvery = 3;
    [Tooltip("Bán kính AOE của Cắn quanh chỗ miệng cắm xuống")]
    public float biteRadius = 2.4f;
    public float biteDamageMul = 1f;
    public float roarDamageMul = 1f;
    [Tooltip("Tâm sóng gầm cao tối đa bao nhiêu so với tâm làn (x nửa chiều cao làn). 0 = đúng tâm làn")]
    [Range(0f, 1f)] public float roarCenterMaxInLane = 0.2f;
    [Tooltip("Tâm sóng gầm theo X: 0 = ngay chóp miệng, 1 = mép trước (ngực, chỗ lính dừng đánh)")]
    [Range(0f, 1f)] public float roarCenterTowardFront = 0.5f;

    [Title("Body")]
    [Tooltip("Mép trước (chóp miệng) tính từ root theo hướng mặt, world unit")]
    public float frontOffset = 5.2f;
    [Tooltip("Điểm ngắm cho đạn/chữ damage: lùi vào trong mép trước + nâng lên")]
    public Vector2 aimOffset = new Vector2(1.2f, 1.2f);

    [Title("Hit Feedback")]
    public Color hitFlashColor = new Color(1f, 0.75f, 0.75f, 1f);
    public float hitFlashTime = 0.06f;

    [Title("Skill lên boss")]
    [Tooltip("Skill trừ theo % máu tối đa (Burn/Poison/Eraser) chỉ ăn bấy nhiêu phần lên boss, tránh boss tan quá nhanh")]
    [Range(0f, 1f)] public float percentSkillScale = 0.25f;
    [Tooltip("Thời gian đóng băng lên boss = thời gian skill x hệ số này")]
    [Range(0f, 1f)] public float freezeScale = 0.6f;
    [Tooltip("Mỗi tia sét gây bấy nhiêu % máu tối đa (trước percentSkillScale)")]
    public float lightningPercentPerStrike = 0.03f;

    [Title("Particle độc / lửa trên thân boss")]
    [Tooltip("Các điểm gắn particle, toạ độ local của rig (chưa lật/scale): đuôi, lưng, ngực, cổ")]
    public Vector3[] dotFxPoints =
    {
        new Vector3(1.4f, 0.4f, 0f),
        new Vector3(2.6f, 0.9f, 0f),
        new Vector3(3.9f, 0.1f, 0f),
        new Vector3(4.7f, 2.3f, 0f),
    };
    [Tooltip("Phóng to particle so với cỡ dùng cho lính")]
    public float dotFxScale = 3f;
    [Tooltip("Push skill đẩy boss lùi bao nhiêu (world unit)")]
    public float pushDistance = 0.8f;

    // Boss đang có trong trận (skill dùng để tác động lên boss)
    public static DungeonBoss Active { get; private set; }

    public bool IsAlive => _hp > 0f && !_dead;
    public Team Team => Team.Enemy;
    public Transform Transform => transform;
    public float MaxHp => _maxHp;
    public float Hp => _hp;

    // Mặt quay sang trái (đi về nhà người chơi bên trái) -> hướng mặt = -1
    float Facing => transform.lossyScale.x < 0f ? -1f : 1f;
    float FrontX => transform.position.x + Facing * frontOffset;
    float LaneY => _laneY;

    public Vector3 AimPoint => new Vector3(FrontX - Facing * aimOffset.x, LaneY + aimOffset.y, 0f);

    public event System.Action<DungeonBoss> OnHpChanged;

    private DinoAnimator _anim;
    private float _laneY, _laneHalf;
    private float _cooldown;
    private int _attackCount;
    private bool _dead;
    private bool _paused;
    private SpriteRenderer[] _sprites;
    private Color[] _baseColors;
    private readonly List<IDamageable> _hits = new();

    public void Init(UnitData baseUnit, float laneY, float laneHalfHeight)
    {
        _anim = GetComponent<DinoAnimator>();
        _laneY = laneY;
        _laneHalf = laneHalfHeight;

        _maxHp = _hp = Mathf.Max(1f, baseUnit != null ? baseUnit.hp * hpMultiplier : 1000f);
        _atk = baseUnit != null ? baseUnit.atk * atkMultiplier : 10f;

        _sprites = GetComponentsInChildren<SpriteRenderer>(true);
        _baseColors = new Color[_sprites.Length];
        for (int i = 0; i < _sprites.Length; i++) _baseColors[i] = _sprites[i].color;

        _anim.OnBiteHit += OnBiteHit;
        _anim.OnShockwave += OnShockwave;
        _anim.shockwaveCenterResolver = ResolveRoarCenter;

        BattleManager.Instance.RegisterTarget(this);
        _anim.SetMove(true);
        Active = this;
        OnHpChanged?.Invoke(this);
    }

    void OnDestroy()
    {
        if (Active == this) Active = null;
        if (_anim == null) return;
        _anim.OnBiteHit -= OnBiteHit;
        _anim.OnShockwave -= OnShockwave;
    }

    void Update()
    {
        if (_dead || _anim == null) return;
        var bm = BattleManager.Instance;
        if (bm == null || bm.IsBattleOver) { StopAll(); return; }

        // đồng bộ pause của game (popup) + đóng băng với animation DOTween
        bool hold = bm.IsPaused || _freezeLeft > 0f;
        if (hold != _paused)
        {
            _paused = hold;
            if (_paused) _anim.Pause(); else _anim.Resume();
        }
        if (bm.IsPaused) return;

        TickStatus(Time.deltaTime);
        if (_dead || _freezeLeft > 0f) return;

        if (_anim.IsAttacking) return;
        if (_cooldown > 0f) _cooldown -= Time.deltaTime;

        if (HasTargetInRange())
        {
            if (_anim.IsMoving) _anim.SetMove(false);
            if (_cooldown <= 0f) Attack();
        }
        else
        {
            if (!_anim.IsMoving) _anim.SetMove(true);
            transform.position += Vector3.right * Facing * moveSpeed * Time.deltaTime;
        }
    }

    // ---- TARGETING ----
    // Lính: chỉ xét khoảng cách theo X tới mép trước (boss chắn cả làn). Nhà: dùng tường của nhà.
    bool HasTargetInRange()
    {
        var targets = BattleManager.Instance.TargetsFor(Team.Enemy);
        Vector3 front = new Vector3(FrontX, LaneY, 0f);
        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            if (t == null || (t is Object o && o == null) || !t.IsAlive) continue;
            float d = t is Unit u ? Mathf.Max(0f, (u.transform.position.x - FrontX) * Facing)
                                  : Mathf.Sqrt(t.SqrDistanceTo(front));
            if (d <= attackRange) return true;
        }
        return false;
    }

    void Attack()
    {
        _attackCount++;
        bool roar = roarEvery > 0 && _attackCount % roarEvery == 0;
        if (roar) _anim.PlayRoar(); else _anim.PlayBite();
        _cooldown = attackCooldown;
    }

    void OnBiteHit()
    {
        if (_anim.mouthPoint == null) return;
        Vector3 c = _anim.mouthPoint.position;
        DealAoe(new Vector3(c.x, Mathf.Clamp(c.y, LaneY - _laneHalf, LaneY + _laneHalf), 0f), biteRadius, _atk * biteDamageMul);
    }

    void OnShockwave(Vector3 center, float radius) => DealAoe(center, radius, _atk * roarDamageMul);

    // Lúc gầm đầu hất cao + chìa xa ra trước, còn lính đứng dưới cổ (trước ngực) -> tâm trước miệng trượt hết.
    // Đặt tâm: X ở giữa chóp miệng và mép trước (ngực), Y kéo vào trong làn -> sóng toả ngay chỗ lính đứng.
    Vector3 ResolveRoarCenter(Vector3 mouthCenter)
    {
        float mouthX = _anim.mouthPoint != null ? _anim.mouthPoint.position.x : mouthCenter.x;
        float x = Mathf.Lerp(mouthX, FrontX, roarCenterTowardFront);
        float y = Mathf.Clamp(mouthCenter.y, LaneY - _laneHalf * 0.5f, LaneY + _laneHalf * roarCenterMaxInLane);
        return new Vector3(x, y, mouthCenter.z);
    }

    void DealAoe(Vector3 center, float radius, float dmg)
    {
        if (!IsAlive || BattleManager.Instance.IsBattleOver) return;
        BattleManager.Instance.FindTargetsInRadius(Team.Enemy, center, radius, _hits);
        foreach (var t in _hits)
        {
            if (t == null || !t.IsAlive) continue;
            t.TakeDamage(dmg);
            if (FlyTextSpawner.Instance != null) FlyTextSpawner.Instance.Damage(dmg, t.AimPoint, false);
        }
        _hits.Clear();
    }

    // ---- IDamageable ----
    // Bức tường dọc tại mép trước, phủ hết chiều cao làn
    public float SqrDistanceTo(Vector3 p)
    {
        float dx = Mathf.Max(0f, (FrontX - p.x) * -Facing);   // chỉ tính khi đứng phía trước mặt boss
        float dy = Mathf.Max(0f, Mathf.Abs(p.y - LaneY) - _laneHalf);
        return dx * dx + dy * dy;
    }

    public void TakeDamage(float dmg)
    {
        if (!IsAlive) return;
        _hp = Mathf.Max(0f, _hp - dmg);
        OnHpChanged?.Invoke(this);
        if (_hp <= 0f) { Die(); return; }
        Flash();
    }

    void Flash()
    {
        for (int i = 0; i < _sprites.Length; i++)
            if (_sprites[i] != null) _sprites[i].color = _baseColors[i] * hitFlashColor;
        DOVirtual.DelayedCall(hitFlashTime, RestoreTint).SetLink(gameObject);
    }

    // Trả về màu theo hiệu ứng đang dính (ưu tiên băng > cháy > độc), không có thì màu gốc
    void RestoreTint()
    {
        if (_sprites == null || _dead) return;   // đang chết (fade) thì không ghi đè màu
        Color tint = Color.white;
        if (_freezeLeft > 0f && FreezeFxSpawner.Instance != null) tint = FreezeFxSpawner.Instance.frozenTint;
        else if (_burn.left > 0f && DotFxSpawner.Instance != null) tint = DotFxSpawner.Instance.Tint(DotType.Burn);
        else if (_poison.left > 0f && DotFxSpawner.Instance != null) tint = DotFxSpawner.Instance.Tint(DotType.Poison);
        for (int i = 0; i < _sprites.Length; i++)
            if (_sprites[i] != null) _sprites[i].color = _baseColors[i] * tint;
    }

    // ================= SKILL LÊN BOSS =================
    struct Dot { public float left, percent, tick; }
    private float _freezeLeft;
    private Dot _burn, _poison;

    public void Freeze(float duration)
    {
        if (!IsAlive) return;
        _freezeLeft = Mathf.Max(_freezeLeft, duration * freezeScale);
        RestoreTint();
    }

    // percentPerSec: % máu tối đa mỗi giây (như lính), boss chỉ ăn percentSkillScale phần
    public void AddDot(DotType type, float percentPerSec, float duration)
    {
        if (!IsAlive) return;
        var d = new Dot { left = duration, percent = percentPerSec, tick = 1f };
        if (type == DotType.Burn) _burn = d; else _poison = d;
        AttachDotFx(type);
        RestoreTint();
    }

    private readonly List<DotFx> _burnFx = new();
    private readonly List<DotFx> _poisonFx = new();

    // Rải nhiều cụm particle dọc thân, mỗi cụm bám theo bone gần nhất để đi theo animation
    void AttachDotFx(DotType type)
    {
        var list = type == DotType.Burn ? _burnFx : _poisonFx;
        var spawner = DotFxSpawner.Instance;
        if (list.Count > 0 || spawner == null || dotFxPoints == null) return;

        for (int i = 0; i < dotFxPoints.Length; i++)
        {
            Vector3 world = transform.TransformPoint(dotFxPoints[i]);
            Transform bone = BoneFor(i);
            var fx = spawner.AttachAt(type, bone, bone.InverseTransformPoint(world), dotFxScale);
            if (fx != null) list.Add(fx);
        }
    }

    Transform BoneFor(int pointIndex)
    {
        if (_anim == null) return transform;
        if (pointIndex == 0 && _anim.tail != null) return _anim.tail;
        if (pointIndex >= dotFxPoints.Length - 1 && _anim.neck != null) return _anim.neck;
        return _anim.body != null ? _anim.body : transform;
    }

    void ReleaseDotFx(DotType type)
    {
        var list = type == DotType.Burn ? _burnFx : _poisonFx;
        foreach (var fx in list)
            if (fx != null) fx.Release();
        list.Clear();
    }

    // Damage theo % máu tối đa (Eraser, Lightning...)
    public void TakePercentDamage(float percent)
    {
        if (!IsAlive || percent <= 0f) return;
        float dmg = _maxHp * percent * percentSkillScale;
        TakeDamage(dmg);
        if (FlyTextSpawner.Instance != null) FlyTextSpawner.Instance.Damage(dmg, AimPoint, false);
    }

    public void StrikeLightning(int strikes)
    {
        if (!IsAlive || strikes <= 0) return;
        var fx = LightningFxSpawner.Instance;
        for (int i = 0; i < strikes; i++)
        {
            Vector3 p = AimPoint + new Vector3(-Facing * (0.6f + i * 0.9f), Random.Range(-0.3f, 0.6f), 0f);
            if (fx != null) fx.StrikePoint(p, i, () => TakePercentDamage(lightningPercentPerStrike));
            else TakePercentDamage(lightningPercentPerStrike);
        }
    }

    public void PushBack(float extraDamage)
    {
        if (!IsAlive) return;
        // đẩy lùi về phía sau lưng boss
        transform.DOKill(false);
        transform.DOMoveX(transform.position.x - Facing * pushDistance, 0.25f).SetEase(Ease.OutQuad).SetLink(gameObject);
        if (extraDamage > 0f)
        {
            TakeDamage(extraDamage);
            if (FlyTextSpawner.Instance != null) FlyTextSpawner.Instance.Damage(extraDamage, AimPoint, false);
        }
    }

    void TickStatus(float dt)
    {
        if (_freezeLeft > 0f)
        {
            _freezeLeft -= dt;
            if (_freezeLeft <= 0f) RestoreTint();
        }
        TickDot(ref _burn, DotType.Burn, dt);
        TickDot(ref _poison, DotType.Poison, dt);
    }

    void TickDot(ref Dot d, DotType type, float dt)
    {
        if (d.left <= 0f || _dead) return;
        d.left -= dt;
        d.tick -= dt;
        if (d.tick <= 0f)
        {
            d.tick += 1f;
            TakePercentDamage(d.percent);
        }
        if (d.left <= 0f)
        {
            ReleaseDotFx(type);
            RestoreTint();
        }
    }

    void Die()
    {
        _dead = true;
        StopAll();
        ReleaseDotFx(DotType.Burn);
        ReleaseDotFx(DotType.Poison);
        if (DeathFxSpawner.Instance != null) DeathFxSpawner.Instance.Play(AimPoint);

        // ngã xuống + mờ dần
        var seq = DOTween.Sequence().SetLink(gameObject);
        seq.Append(transform.DORotate(new Vector3(0f, 0f, 12f * Facing), 0.5f).SetEase(Ease.InQuad));
        seq.Join(transform.DOMoveY(transform.position.y - 0.4f, 0.5f).SetEase(Ease.InQuad));
        foreach (var sr in _sprites)
            if (sr != null) seq.Join(sr.DOFade(0f, 0.8f).SetDelay(0.3f));

        this.PostEvent(EventID.HOUSE_DESTROYED, Team.Enemy);   // -> BattleManager kết thúc trận, thắng
    }

    void StopAll()
    {
        if (_anim == null) return;
        if (_anim.IsMoving) _anim.SetMove(false);
    }

    void OnDrawGizmosSelected()
    {
        float y = Application.isPlaying ? LaneY : transform.position.y;
        float half = Application.isPlaying ? _laneHalf : 2f;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(FrontX, y - half), new Vector3(FrontX, y + half));
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawLine(new Vector3(FrontX - Facing * attackRange, y - half), new Vector3(FrontX - Facing * attackRange, y + half));
    }
}
