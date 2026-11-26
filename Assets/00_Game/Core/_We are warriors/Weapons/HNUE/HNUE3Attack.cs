using UnityEngine;
using DG.Tweening;

// Bút xóa: nhấc lên (Y) giơ cao -> giữ nhịp -> nện xuống kèm lao tới X như cột đập -> rút về
public class HNUE3Attack : AttackStrategyBase
{
    public Transform weapon;
    [Tooltip("Nhấc vũ khí lên bao nhiêu (local Y)")]
    public float liftY = 0.25f;
    [Tooltip("Giơ ngả ra sau thêm bao nhiêu độ khi nhấc")]
    public float liftAngle = 25f;
    [Tooltip("Góc local Z lúc nện xuống")]
    public float smashAngle = -130f;
    [Tooltip("Lao vũ khí tới bao nhiêu (local X) lúc nện")]
    public float smashX = 0.25f;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon);
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float lift = duration * 0.25f, hold = duration * 0.15f, smash = duration * 0.1f, impact = duration * 0.15f, ret = duration * 0.35f;
        Vector3 basePos = BasePos(0);
        float baseZ = NormalizeAngle(BaseRot(0).z);

        float z = baseZ;
        DG.Tweening.Core.DOGetter<float> getZ = () => z;
        DG.Tweening.Core.DOSetter<float> setZ = v => { z = v; weapon.localRotation = Quaternion.Euler(0, 0, v); };

        // nhấc lên + giơ cao
        seq.Append(weapon.DOLocalMoveY(basePos.y + liftY, lift).SetEase(Ease.OutQuad));
        seq.Join(DOTween.To(getZ, setZ, baseZ + liftAngle, lift).SetEase(Ease.OutQuad));
        // giữ nhịp
        seq.AppendInterval(hold);
        // nện xuống + lao tới
        seq.Append(DOTween.To(getZ, setZ, smashAngle, smash).SetEase(Ease.InQuad));
        seq.Join(weapon.DOLocalMove(new Vector3(basePos.x + smashX, basePos.y, basePos.z), smash).SetEase(Ease.InQuad));
        seq.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySfx("punch");
            onHit?.Invoke();
        });
        // hit-stop (bậc cao): khựng đúng lúc chạm, vũ khí rung nhẹ -> cảm giác nặng
        float hitStop = fx != null ? fx.HitStop : 0f;
        if (hitStop > 0f)
            seq.Append(weapon.DOShakePosition(hitStop, 0.05f, 30, 90f, false, false));
        seq.AppendInterval(impact);
        // rút về
        seq.Append(DOTween.To(getZ, setZ, baseZ, ret).SetEase(Ease.OutQuad));
        seq.Join(weapon.DOLocalMove(basePos, ret).SetEase(Ease.OutQuad));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }

    static float NormalizeAngle(float a) => a > 180f ? a - 360f : a;

    public override void ResetToIdle()
    {
        seq?.Kill();
        ResetToBase();
        OnAnimDone();
    }
}
