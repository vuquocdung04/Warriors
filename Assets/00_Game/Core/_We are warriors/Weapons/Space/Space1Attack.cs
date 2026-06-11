using UnityEngine;
using DG.Tweening;

public class Space1Attack : AttackStrategyBase
{
    public Transform weapon;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon);
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float lift = duration * 0.3f;
        float strike = duration * 0.15f;   // vụt xuống nhanh
        float hold = duration * 0.15f;
        float ret = duration * 0.4f;

        // giơ lên 30
        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, 30f), lift));

        // vụt xuống 110 thật nhanh + moveX base + 0.1
        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, -110f), strike).SetEase(Ease.Linear));
        seq.Join(weapon.DOLocalMoveX(BasePos(0).x + 0.2f, strike));
        seq.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySfx("Light Sword");
            onHit?.Invoke();
        });

        // hold chút
        seq.AppendInterval(hold);

        // về base
        seq.Append(weapon.DOLocalMove(BasePos(0), ret));
        seq.Join(weapon.DOLocalRotate(BaseRot(0), ret));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }
}