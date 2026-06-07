using UnityEngine;
using DG.Tweening;

public class Farm1Attack : AttackStrategyBase
{
    public Transform weapon;


    private Vector3 _basePos;
    private Vector3 _baseRot;
    private bool _cached;

    void CacheBase()
    {
        if (_cached) return;
        _basePos = weapon.localPosition;
        _baseRot = weapon.localEulerAngles;
        _cached = true;
    }

    public override void Attack(IDamageable target, float duration)
    {
        CacheBase();
        weapon.localPosition = _basePos;
        weapon.localEulerAngles = _baseRot;

        seq?.Kill();
        seq = DOTween.Sequence();

        float prep = duration * 0.4f;
        float ret = duration * 0.6f;

        seq.Append(weapon.DOLocalRotate(Vector3.zero, prep));
        seq.Join(weapon.DOLocalMoveX(0.5f, prep));
        seq.AppendCallback(() => Hit(target));
        seq.Append(weapon.DOLocalMove(_basePos, ret));
        seq.Join(weapon.DOLocalRotate(_baseRot, ret));

        seq.SetLink(owner.gameObject);
    }

}