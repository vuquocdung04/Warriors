using UnityEngine;
using DG.Tweening;

public class ChampionAttack : AttackStrategyBase
{
    public Transform weapon;
    public Transform helmet;
    public Transform shoulderArmor;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon, helmet, shoulderArmor);
        ResetToBase();

        seq?.Kill();
        seq = DOTween.Sequence();

        float p1 = duration * 0.3f;
        float hold = duration * 0.15f;
        float p2 = duration * 0.3f;
        float reset = duration * 0.25f;

        seq.Append(helmet.DOLocalRotate(new Vector3(0, 0, -30f), p1));
        seq.Join(shoulderArmor.DOLocalRotate(new Vector3(0, 0, -65f), p1));
        seq.Join(weapon.DOLocalMove(new Vector3(-1.09f, 0.62f, 0), p1));

        seq.AppendInterval(hold);

        seq.Append(weapon.DOLocalMove(new Vector3(0, 0.17f, 0), p2));
        seq.Join(shoulderArmor.DOLocalRotate(new Vector3(0, 0, 40f), p2));
        seq.Join(shoulderArmor.DOLocalMove(new Vector3(-0.3f, 0.52f, 0), p2));
        seq.Join(helmet.DOLocalRotate(new Vector3(0, 0, 10f), p2));

        seq.AppendCallback(() => onHit?.Invoke());

        seq.Append(weapon.DOLocalMove(BasePos(0), reset));
        seq.Join(shoulderArmor.DOLocalMove(BasePos(2), reset));
        seq.Join(weapon.DOLocalRotate(BaseRot(0), reset));
        seq.Join(helmet.DOLocalRotate(BaseRot(1), reset));
        seq.Join(shoulderArmor.DOLocalRotate(BaseRot(2), reset));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }
}