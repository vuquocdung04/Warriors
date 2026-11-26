using UnityEngine;
using DG.Tweening;

// Máy bay giấy: kéo ra sau lấy đà -> phóng tới, máy bay trên tay ẩn đi và đạn bay ra -> máy bay mới hiện lại
public class HNUE2Attack : AttackStrategyBase, ITierAware
{
    [System.Serializable]
    public class TierShot
    {
        [Tooltip("Sprite đạn của bậc này (null = giữ sprite gốc của planePrefab)")]
        public Sprite sprite;
        [Tooltip("Số máy bay bắn ra mỗi lần: chiếc giữa vào mục tiêu chính, các chiếc khác vào địch gần nhất khác")]
        public int count = 1;
        [Tooltip("Sát thương của máy bay bắn vào mục tiêu phụ (so với 1 phát thường)")]
        [Range(0f, 1f)] public float sideDamage = 0.5f;
        [Tooltip("Thiếu mục tiêu phụ: máy bay thừa đáp lệch trên/dưới mục tiêu chính bao nhiêu (không gây dame)")]
        public float scatter = 0.45f;
        [Tooltip("Chênh độ cao vòng cung giữa các máy bay")]
        public float arcSpread = 0.6f;
        [Tooltip("Bắn lệch nhau theo thời gian (giây)")]
        public float stagger = 0.05f;
    }

    [Header("Đạn theo bậc ([0] = bậc 1, [1] = bậc 2)")]
    public TierShot[] tierShots;

    private TierShot _shot;
    private Sprite _baseSprite;
    private bool _baseCached;

    public void ApplyTier(int tier) =>
        _shot = tier > 0 && tierShots != null && tier <= tierShots.Length ? tierShots[tier - 1] : null;

    [Header("Tay ném")]
    public Transform weapon;
    [Tooltip("Sprite máy bay đang cầm, ẩn lúc ném")]
    public GameObject weaponVisual;
    [Tooltip("Lấy đà: kéo về sau (X âm) và vòng lên (Y) theo cung tròn")]
    public float windUpBack = 0.25f;
    public float windUpUp = 0.3f;
    public float windUpAngle = 35f;
    [Tooltip("Ném: vòng qua đỉnh rồi vung tới trước")]
    public float throwForward = 0.3f;
    public float releaseAngle = -15f;

    [Header("Projectile")]
    public Projectile planePrefab;
    public Transform firePoint;
    public float planeSpeed = 6f;
    [Tooltip("Độ cao vòng cung của máy bay")]
    public float arcHeight = 1.2f;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon);
        ResetToBase();
        SetVisual(true);

        seq?.Kill();
        seq = DOTween.Sequence();

        float windUp = duration * 0.35f, toss = duration * 0.12f, hidden = duration * 0.33f, reappear = duration * 0.2f;
        Vector3 basePos = BasePos(0), baseRot = BaseRot(0);

        // lấy đà theo cung: lùi về sau rồi vòng lên trên đầu, mũi hếch lên
        Vector3 back = basePos + new Vector3(-windUpBack, windUpUp, 0);
        var windPath = new[]
        {
            basePos + new Vector3(-windUpBack * 0.8f, windUpUp * 0.35f, 0),
            back
        };
        seq.Append(weapon.DOLocalPath(windPath, windUp, PathType.CatmullRom).SetEase(Ease.OutQuad));
        seq.Join(weapon.DOLocalRotate(baseRot + new Vector3(0, 0, windUpAngle), windUp).SetEase(Ease.OutQuad));
        // ném: vòng qua đỉnh rồi vung tới trước, mũi chúi dần xuống
        var throwPath = new[]
        {
            basePos + new Vector3(0f, windUpUp * 1.15f, 0),
            basePos + new Vector3(throwForward, windUpUp * 0.35f, 0)
        };
        seq.Append(weapon.DOLocalPath(throwPath, toss, PathType.CatmullRom).SetEase(Ease.InQuad));
        seq.Join(weapon.DOLocalRotate(baseRot + new Vector3(0, 0, releaseAngle), toss).SetEase(Ease.InQuad));
        seq.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySfx("whoosh");
            onHit?.Invoke();       // Hit() bắn đạn từ firePoint (firePoint vẫn active vì chỉ ẩn sprite)
            SetVisual(false);
        });
        seq.AppendInterval(hidden);
        // máy bay mới hiện lại ở tay
        seq.AppendCallback(() =>
        {
            weapon.localPosition = basePos;
            weapon.localEulerAngles = baseRot;
            SetVisual(true);
            if (weaponVisual != null) weaponVisual.transform.localScale = _visualScale * 0.2f;
        });
        if (weaponVisual != null)
            seq.Append(weaponVisual.transform.DOScale(_visualScale, reappear).SetEase(Ease.OutBack));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }

    private Vector3 _visualScale = Vector3.one;
    private bool _scaleCached;

    void SetVisual(bool on)
    {
        if (weaponVisual == null) return;
        if (!_scaleCached) { _visualScale = weaponVisual.transform.localScale; _scaleCached = true; }
        weaponVisual.SetActive(on);
    }

    protected override void Hit(IDamageable target)
    {
        target = ResolveTarget(target);
        if (target == null) return;

        Vector3 from = firePoint != null ? firePoint.position : weapon.position;
        int count = _shot != null ? Mathf.Max(1, _shot.count) : 1;
        if (count == 1) { LaunchPlane(from, target, Vector3.zero, arcHeight, 1f); return; }

        // mục tiêu phụ: địch gần nhất khác trong tầm
        _sideTargets.Clear();
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.FindTargetsInRadius(owner.team, owner.transform.position, owner.AttackRange * 1.15f, _nearby);
            Vector3 me = owner.transform.position;
            _nearby.Remove(target);
            _nearby.Sort((a, b) => a.SqrDistanceTo(me).CompareTo(b.SqrDistanceTo(me)));
            for (int k = 0; k < _nearby.Count && _sideTargets.Count < count - 1; k++) _sideTargets.Add(_nearby[k]);
        }

        int mid = count / 2, side = 0, scatterIdx = 0;
        for (int i = 0; i < count; i++)
        {
            float arc = arcHeight + (i - mid) * _shot.arcSpread;
            if (i == mid) { LaunchPlane(from, target, Vector3.zero, arc, 1f); continue; }

            IDamageable t; Vector3 offset; float mul;
            if (side < _sideTargets.Count) { t = _sideTargets[side++]; offset = Vector3.zero; mul = _shot.sideDamage; }
            else   // thiếu địch -> đáp rải quanh mục tiêu chính, chỉ là hiệu ứng
            {
                t = target; mul = 0f;
                offset = Vector3.up * (scatterIdx++ % 2 == 0 ? _shot.scatter : -_shot.scatter);
            }
            float delay = Mathf.Abs(i - mid) * _shot.stagger;
            DG.Tweening.DOVirtual.DelayedCall(delay, () =>
            {
                if (t != null && !(t is Object o && o == null) && t.IsAlive) LaunchPlane(from, t, offset, arc, mul);
            }).SetLink(gameObject);
        }
    }

    private readonly System.Collections.Generic.List<IDamageable> _nearby = new();
    private readonly System.Collections.Generic.List<IDamageable> _sideTargets = new();

    // damageMul = 0 -> chỉ hiệu ứng, không gây sát thương
    void LaunchPlane(Vector3 from, IDamageable target, Vector3 offset, float arc, float damageMul)
    {
        var p = SimplePool2.Spawn(planePrefab);
        // đạn dùng chung pool giữa các unit -> luôn gán lại sprite (bậc 0 trả về sprite gốc)
        var sr = p.GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null)
        {
            if (!_baseCached)
            {
                var prefSr = planePrefab.GetComponentInChildren<SpriteRenderer>(true);
                _baseSprite = prefSr != null ? prefSr.sprite : sr.sprite;
                _baseCached = true;
            }
            sr.sprite = _shot != null && _shot.sprite != null ? _shot.sprite : _baseSprite;
        }
        p.transform.position = from;
        TierAttackFx.SetupProjectile(p.gameObject, fx);   // trail theo bậc (bậc 0 tắt)
        p.LaunchArc(target.AimPoint + offset, planeSpeed, arc, () =>
        {
            if (damageMul > 0f && target != null && !(target is Object o && o == null) && target.IsAlive)
            {
                owner.DealDamage(target, damageMul);
                fx?.OnHit(target);
            }
        }, true);
    }

    public override void ResetToIdle()
    {
        seq?.Kill();
        ResetToBase();
        SetVisual(true);
        if (weaponVisual != null && _scaleCached) weaponVisual.transform.localScale = _visualScale;
        OnAnimDone();
    }
}
