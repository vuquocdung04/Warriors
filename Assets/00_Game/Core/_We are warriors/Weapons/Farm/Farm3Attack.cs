using UnityEngine;
using DG.Tweening;

public class Farm3Attack : AttackStrategyBase
{
    public Transform weapon;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon);
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float strike = duration * 0.3f, hold = duration * 0.1f, ret = duration * 0.6f;
        float baseZ = BaseRot(0).z;

        float z = baseZ;
        void SetZ(float v) => weapon.localRotation = Quaternion.Euler(0, 0, v);

        seq.Append(DOTween.To(() => z, v => { z = v; SetZ(v); }, -130f, strike).SetEase(Ease.Linear));
        seq.AppendCallback(() => onHit?.Invoke());
        seq.AppendInterval(hold);
        seq.Append(DOTween.To(() => z, v => { z = v; SetZ(v); }, baseZ - 360f, ret).SetEase(Ease.Linear));
        seq.AppendCallback(() => weapon.localEulerAngles = BaseRot(0));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }
}