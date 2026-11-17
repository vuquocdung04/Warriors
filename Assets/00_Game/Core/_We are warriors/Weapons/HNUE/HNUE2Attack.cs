using UnityEngine;
using DG.Tweening;

// Máy bay giấy: kéo ra sau lấy đà -> phóng tới, máy bay trên tay ẩn đi và đạn bay ra -> máy bay mới hiện lại
public class HNUE2Attack : AttackStrategyBase
{
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
        var p = SimplePool2.Spawn(planePrefab);
        p.transform.position = from;
        p.LaunchArc(target.AimPoint, planeSpeed, arcHeight, () =>
        {
            if (target != null && target.IsAlive) owner.DealDamage(target);
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
