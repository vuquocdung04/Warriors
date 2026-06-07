using UnityEngine;
using DG.Tweening;

public class Spartan1Attack : AttackStrategyBase
{
    public Transform weapon;
    public Transform shield;
    public Transform head;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon, shield, head);   // 0=weapon, 1=shield, 2=head
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float prep = duration * 0.35f;
        float strike = duration * 0.3f;
        float ret = duration * 0.35f;

        // phase 1: chuẩn bị
        seq.Append(shield.DOLocalMoveX(BasePos(1).x + 0.3f, prep));
        seq.Join(weapon.DOLocalMoveX(BasePos(0).x - 0.2f, prep));
        seq.Join(weapon.DOLocalRotate(new Vector3(0, 0, - 90f), prep));
        seq.Join(head.DOLocalRotate(new Vector3(0, 0, 10f), prep));

        // phase 2: vọt đánh
        seq.Append(shield.DOLocalMoveX(BasePos(1).x - 0.3f, strike));
        seq.Join(weapon.DOLocalMoveX(BasePos(0).x + 0.75f, strike));
        seq.Join(head.DOLocalRotate(new Vector3(0, 0, - 10f), strike));
        seq.AppendCallback(() => onHit?.Invoke());

        // về gốc
        seq.Append(weapon.DOLocalMove(BasePos(0), ret));
        seq.Join(weapon.DOLocalRotate(BaseRot(0), ret));
        seq.Join(shield.DOLocalMove(BasePos(1), ret));
        seq.Join(head.DOLocalRotate(BaseRot(2), ret));

        if (owner != null) seq.SetLink(owner.gameObject);
    }
}