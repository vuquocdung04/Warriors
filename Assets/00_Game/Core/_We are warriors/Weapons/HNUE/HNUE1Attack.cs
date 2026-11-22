using UnityEngine;
using DG.Tweening;

// Bút chì: rút về sau lấy đà rồi đâm thẳng tới
public class HNUE1Attack : AttackStrategyBase
{
    public Transform weapon;
    public float pullBack = 0.35f;
    public float thrust = 0.55f;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon);
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float windUp = duration * 0.35f, strike = duration * 0.15f, hold = duration * 0.1f, ret = duration * 0.4f;
        float baseX = BasePos(0).x;

        seq.Append(weapon.DOLocalMoveX(baseX - pullBack, windUp).SetEase(Ease.OutQuad));
        seq.Append(weapon.DOLocalMoveX(baseX + thrust, strike).SetEase(Ease.InQuad));
        seq.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySfx("punch");
            onHit?.Invoke();
        });
        seq.AppendInterval(hold);
        seq.Append(weapon.DOLocalMove(BasePos(0), ret).SetEase(Ease.OutQuad));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }

    public override void ResetToIdle()
    {
        seq?.Kill();
        ResetToBase();
        OnAnimDone();
    }
}
