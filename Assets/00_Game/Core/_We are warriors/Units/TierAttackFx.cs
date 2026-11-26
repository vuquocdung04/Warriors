using UnityEngine;

// FX đòn đánh theo bậc (gắn trên prefab unit). Bậc 0 = không FX.
// AttackStrategyBase: bật trail lúc ra đòn + gọi OnHit khi trúng. Unit bắn đạn: SetupProjectile cho đạn.
public class TierAttackFx : MonoBehaviour, ITierAware
{
    [System.Serializable]
    public class TierFx
    {
        [Header("Trail (vũ khí cận chiến / đạn)")]
        public bool trail;
        public Gradient trailColor = new Gradient();
        public float trailTime = 0.14f;
        public float trailWidth = 0.12f;

        [Header("Khi trúng")]
        public Color[] splashColors;
        public int splashCount;
        public float splashSize = 0.14f;
        public Sprite groundMark;
        public Color groundMarkColor = Color.white;
        public float groundMarkScale = 0.5f;
        public float groundMarkLife = 1f;
        public bool flash;
        public Color flashColor = new Color(1f, 0.9f, 0.45f, 0.9f);
        public float flashScale = 0.9f;
        [Tooltip("Mục tiêu nảy nhẹ khi trúng")]
        public bool punchTarget;
        [Tooltip("Rung camera (có giới hạn tần suất toàn trận)")]
        public float shake;
        [Tooltip("Khựng anim lúc chạm (giây), script tấn công tự đọc")]
        public float hitStop;
    }

    [Tooltip("Trail ở đầu vũ khí cận chiến (để trống với unit bắn đạn)")]
    public TrailRenderer weaponTrail;
    public Sprite dotSprite;
    public Sprite flashSprite;
    [Tooltip("[0] = bậc 1, [1] = bậc 2")]
    public TierFx[] tiers;

    public TierFx Current { get; private set; }
    public float HitStop => Current != null ? Current.hitStop : 0f;

    public void ApplyTier(int tier)
    {
        Current = tier > 0 && tiers != null && tier <= tiers.Length ? tiers[tier - 1] : null;
        if (weaponTrail == null) return;

        bool on = Current != null && Current.trail;
        weaponTrail.gameObject.SetActive(on);
        if (!on) return;
        ConfigureTrail(weaponTrail, Current);
        // đặt trail ở đầu xa của sprite vũ khí (sprite đã được đổi theo bậc)
        var sr = weaponTrail.transform.parent != null ? weaponTrail.transform.parent.GetComponent<SpriteRenderer>() : null;
        if (sr != null && sr.sprite != null)
        {
            var b = sr.sprite.bounds;
            weaponTrail.transform.localPosition = new Vector3(b.max.x * 0.92f, b.center.y, 0f);
        }
        weaponTrail.emitting = false;
    }

    static void ConfigureTrail(TrailRenderer tr, TierFx fx)
    {
        tr.colorGradient = fx.trailColor;
        tr.time = fx.trailTime;
        tr.widthMultiplier = fx.trailWidth;
    }

    // Trail chỉ chạy lúc đang ra đòn (đi bộ không kéo vệt)
    public void SetTrail(bool on)
    {
        if (weaponTrail == null || !weaponTrail.gameObject.activeSelf) return;
        if (on) weaponTrail.Clear();
        weaponTrail.emitting = on;
    }

    // Đạn dùng chung pool giữa các unit -> luôn cấu hình lại (bậc 0 tắt trail)
    public static void SetupProjectile(GameObject projectile, TierAttackFx fx)
    {
        var tr = projectile.GetComponentInChildren<TrailRenderer>(true);
        if (tr == null) return;
        var cur = fx != null ? fx.Current : null;
        bool on = cur != null && cur.trail;
        tr.Clear();
        tr.emitting = on;
        tr.enabled = on;
        if (on) ConfigureTrail(tr, cur);
    }

    public void OnHit(IDamageable target)
    {
        var fx = Current;
        if (fx == null || target == null || (target is Object o && o == null)) return;

        Vector3 p = target.AimPoint;
        if (fx.flash) HitFxSpawner.Flash(p, flashSprite, fx.flashColor, fx.flashScale);
        if (fx.splashCount > 0) HitFxSpawner.Splash(p, dotSprite, fx.splashColors, fx.splashCount, fx.splashSize);
        if (fx.groundMark != null)
        {
            Vector3 ground = target is Unit u ? u.transform.position : p + Vector3.down * 0.5f;
            HitFxSpawner.Mark(ground, fx.groundMark, fx.groundMarkColor, fx.groundMarkScale, fx.groundMarkLife);
        }
        if (fx.punchTarget && target is Unit tu) tu.PunchHit();
        if (fx.shake > 0f) HitFxSpawner.Shake(fx.shake);
    }
}
