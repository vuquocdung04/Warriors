using UnityEngine;
using DG.Tweening;

public class ChampionAttack : AttackStrategyBase
{
    public Transform weapon;
    public Transform helmet;
    public Transform shoulderArmor;

    // vị trí / góc gốc để reset
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

    public override void Attack(IDamageable target)
    {
        CacheBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        // phase 1: vung lên
        seq.Append(helmet.DOLocalRotate(new Vector3(0, 0, -30f), 0.2f));
        seq.Join(shoulderArmor.DOLocalRotate(new Vector3(0, 0, -65f), 0.2f));
        seq.Join(weapon.DOLocalMove(new Vector3(-1.09f, 0.62f, 0), 0.2f));

        // giữ 0.1f cuối phase 1
        seq.AppendInterval(0.1f);

        // phase 2: chém xuống
        seq.Append(weapon.DOLocalMove(new Vector3(0, 0.17f, 0), 0.2f));
        seq.Join(shoulderArmor.DOLocalRotate(new Vector3(0, 0, 40f), 0.2f));
        seq.Join(shoulderArmor.DOLocalMove(new Vector3(-0.3f, 0.52f, 0), 0.2f));
        seq.Join(helmet.DOLocalRotate(new Vector3(0, 0, 10f), 0.2f));

        // chém trúng -> gây damage
        seq.AppendCallback(() => owner.DealDamage(target));

        // reset về vị trí ban đầu
        seq.Append(weapon.DOLocalMove(_weaponPos, 0.15f));
        seq.Join(shoulderArmor.DOLocalMove(_shoulderPos, 0.15f));
        seq.Join(weapon.DOLocalRotate(_weaponRot, 0.15f));
        seq.Join(helmet.DOLocalRotate(_helmetRot, 0.15f));
        seq.Join(shoulderArmor.DOLocalRotate(_shoulderRot, 0.15f));

        seq.SetLink(owner.gameObject);
    }
}