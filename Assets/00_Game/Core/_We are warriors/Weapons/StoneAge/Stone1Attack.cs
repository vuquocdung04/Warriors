using UnityEngine;
using DG.Tweening;

public class Stone1Attack : AttackStrategyBase
{
    public Transform weapon;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon);

        seq?.Kill();
        seq = DOTween.Sequence();
        float up = duration * 0.3f;
        float down = duration * 0.25f;
        float ret = duration * 0.45f;

        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, 12f), up));
        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, -115f), down));
        seq.AppendCallback(() => onHit?.Invoke());
        seq.Append(weapon.DOLocalRotate(BaseRot(0), ret));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }
}