using UnityEngine;
using DG.Tweening;

public class Farm1Attack : AttackStrategyBase
{
    public Transform weapon;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon);
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float windUp = duration * 0.3f, strike = duration * 0.3f, ret = duration * 0.4f;

        seq.Append(weapon.DOLocalRotate(Vector3.zero, windUp));
        seq.Join(weapon.DOLocalMoveX(BasePos(0).x - 0.5f, windUp));
        seq.Append(weapon.DOLocalMoveX(BasePos(0).x + 0.5f, strike));
        seq.AppendCallback(() => onHit?.Invoke());
        seq.Append(weapon.DOLocalMove(BasePos(0), ret));
        seq.Join(weapon.DOLocalRotate(BaseRot(0), ret));

        if (owner != null) seq.SetLink(owner.gameObject);
    }
}