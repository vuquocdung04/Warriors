using UnityEngine;
using DG.Tweening;

public class Modern1Attack : AttackStrategyBase
{
    public Transform weapon;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon);
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float lift = duration * 0.3f;
        float stab = duration * 0.1f;
        float ret = duration * 0.45f;

        // giơ dao: xoay -90 trước
        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, -90f), lift));

        // đâm: moveX base + 0.3
        seq.Append(weapon.DOLocalMoveX(BasePos(0).x + 0.5f, stab));
        seq.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySfx("Light Sword");
            onHit?.Invoke();
        });

        // về base
        seq.Append(weapon.DOLocalMove(BasePos(0), ret));
        seq.Join(weapon.DOLocalRotate(BaseRot(0), ret));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }
}