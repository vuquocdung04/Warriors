using UnityEngine;
using DG.Tweening;

public class Renai1Attack : AttackStrategyBase
{
    public Transform weapon;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon);
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float prep = duration * 0.35f;
        float strike = duration * 0.15f;
        float ret = duration * 0.35f;

        // chuẩn bị: xoay -90 + lùi base - 0.7
        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, -90f), prep));
        seq.Join(weapon.DOLocalMoveX(BasePos(0).x - 1f, prep));

        // đâm về base X
        seq.Append(weapon.DOLocalMoveX(BasePos(0).x, strike));
        seq.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySfx("Heavy Sword");
            onHit?.Invoke();
        });

        // về gốc
        seq.Append(weapon.DOLocalMove(BasePos(0), ret));
        seq.Join(weapon.DOLocalRotate(BaseRot(0), ret));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }
}