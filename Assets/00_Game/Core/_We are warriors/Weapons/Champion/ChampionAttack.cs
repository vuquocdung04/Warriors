using UnityEngine;
using DG.Tweening;

public class ChampionAttack : AttackStrategyBase
{
    public Transform weapon;
    public Transform helmet;
    public Transform shoulderArmor;

    private Vector3 _weaponPos, _shoulderPos;
    private Vector3 _weaponRot, _helmetRot, _shoulderRot;
    private bool _cached;

    void CacheBase()
    {
        if (_cached) return;
        _weaponPos = weapon.localPosition;
        _shoulderPos = shoulderArmor.localPosition;
        _weaponRot = weapon.localEulerAngles;
        _helmetRot = helmet.localEulerAngles;
        _shoulderRot = shoulderArmor.localEulerAngles;
        _cached = true;
    }

    public override void Attack(IDamageable target, float duration)
    {
        CacheBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        // chia duration theo tỉ lệ: phase1 0.3, hold 0.15, phase2 0.3, reset 0.25
        float p1 = duration * 0.3f;
        float hold = duration * 0.15f;
        float p2 = duration * 0.3f;
        float reset = duration * 0.25f;

        // phase 1: vung lên
        seq.Append(helmet.DOLocalRotate(new Vector3(0, 0, -30f), p1));
        seq.Join(shoulderArmor.DOLocalRotate(new Vector3(0, 0, -65f), p1));
        seq.Join(weapon.DOLocalMove(new Vector3(-1.09f, 0.62f, 0), p1));

        // giữ cuối phase 1
        seq.AppendInterval(hold);

        // phase 2: chém xuống
        seq.Append(weapon.DOLocalMove(new Vector3(0, 0.17f, 0), p2));
        seq.Join(shoulderArmor.DOLocalRotate(new Vector3(0, 0, 40f), p2));
        seq.Join(shoulderArmor.DOLocalMove(new Vector3(-0.3f, 0.52f, 0), p2));
        seq.Join(helmet.DOLocalRotate(new Vector3(0, 0, 10f), p2));

        // chém trúng -> gây damage
        seq.AppendCallback(() => owner.DealDamage(target));

        // reset về ban đầu
        seq.Append(weapon.DOLocalMove(_weaponPos, reset));
        seq.Join(shoulderArmor.DOLocalMove(_shoulderPos, reset));
        seq.Join(weapon.DOLocalRotate(_weaponRot, reset));
        seq.Join(helmet.DOLocalRotate(_helmetRot, reset));
        seq.Join(shoulderArmor.DOLocalRotate(_shoulderRot, reset));

        seq.SetLink(owner.gameObject);
    }
}