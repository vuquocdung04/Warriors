using UnityEngine;
using DG.Tweening;

public class Spartan3Attack : AttackStrategyBase
{
    public Transform weapon;
    public Transform head;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon, head);   // 0=weapon, 1=head
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float prep = duration * 0.35f;
        float strike = duration * 0.3f;
        float ret = duration * 0.35f;

        // chuẩn bị: head -10, weapon -110 + lùi base - 0.3
        seq.Append(head.DOLocalRotate(new Vector3(0, 0, -10f), prep));
        seq.Join(weapon.DOLocalRotate(new Vector3(0, 0, -95f), prep));
        seq.Join(weapon.DOLocalMoveX(BasePos(0).x - 0.3f, prep));

        // đánh: head 10, weapon moveX base + 0.5
        seq.Append(head.DOLocalRotate(new Vector3(0, 0, 10f), strike));
        seq.Join(weapon.DOLocalMoveX(BasePos(0).x + 0.9f, strike));
        seq.AppendCallback(() => onHit?.Invoke());

        // về gốc
        seq.Append(weapon.DOLocalMove(BasePos(0), ret));
        seq.Join(weapon.DOLocalRotate(BaseRot(0), ret));
        seq.Join(head.DOLocalRotate(BaseRot(1), ret));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }
}