using UnityEngine;
using DG.Tweening;

public class Stone3Attack : AttackStrategyBase
{
    public Transform weapon;

    private Vector3 _basePos;
    private bool _cached;

    void CacheBase()
    {
        if (_cached) return;
        _basePos = weapon.localPosition; 
        _cached = true;
    }

    public override void Attack(IDamageable target, float duration)
    {
        CacheBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float up = duration * 0.3f;
        float hit = duration * 0.25f;
        float ret = duration * 0.45f;

        seq.Append(weapon.DOLocalMove(new Vector3(-1f, 0.4f, 0f), up));     
        seq.Append(weapon.DOLocalMove(new Vector3(0.14f, 0.22f, 0f), hit));
        seq.AppendCallback(() => Hit(target));
        seq.Append(weapon.DOLocalMove(_basePos, ret));                      

        seq.SetLink(owner.gameObject);
    }
}