using UnityEngine;
using DG.Tweening;

public class Stone3Attack : AttackStrategyBase
{
    public Transform weapon;

    protected override void PlayAnim(float duration, System.Action onHit)
    {
        CacheBase(weapon);

        seq?.Kill();
        seq = DOTween.Sequence();

        float up = duration * 0.3f;
        float hit = duration * 0.25f;
        float ret = duration * 0.45f;

        seq.Append(weapon.DOLocalMove(new Vector3(-1f, 0.4f, 0f), up));
        seq.Append(weapon.DOLocalMove(new Vector3(0.14f, 0.22f, 0f), hit));
        seq.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySfx("whoosh");
            onHit?.Invoke();
        });
        seq.Append(weapon.DOLocalMove(BasePos(0), ret));
        seq.OnComplete(() => OnAnimDone());

        if (owner != null) seq.SetLink(owner.gameObject);
    }
}