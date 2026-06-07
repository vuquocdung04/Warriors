using UnityEngine;
using DG.Tweening;

public class Stone1Attack : AttackStrategyBase
{
    public Transform weapon;

    private Vector3 _baseRot;
    private bool _cached;

    void CacheBase()
    {
        if (_cached) return;
        _baseRot = weapon.localEulerAngles;
        _cached = true;
    }

    public override void Attack(IDamageable target, float duration)
    {
        CacheBase();

        seq?.Kill();
        seq = DOTween.Sequence();
        float up = duration * 0.3f;
        float down = duration * 0.25f;
        float ret = duration * 0.45f;

        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, 12f), up));      
        seq.Append(weapon.DOLocalRotate(new Vector3(0, 0, -100f), down)); 
        seq.AppendCallback(() => Hit(target));
        seq.Append(weapon.DOLocalRotate(_baseRot, ret));               
    }
}