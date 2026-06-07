using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

public abstract class AttackStrategyBase : MonoBehaviour, IAttackStrategy
{
    protected Unit owner;
    protected Sequence seq;

    public virtual void Init(Unit owner) => this.owner = owner;

    public void Attack(IDamageable target, float duration) => PlayAnim(duration, () => Hit(target));

    protected abstract void PlayAnim(float duration, System.Action onHit);

    protected virtual void Hit(IDamageable target)
    {
        if (target != null && target.IsAlive) owner.DealDamage(target);
    }

    private Transform[] _targets;
    private Vector3[] _pos;
    private Vector3[] _rot;
    private bool _cached;

    protected void CacheBase(params Transform[] targets)
    {
        if (_cached) return;
        _targets = targets;
        _pos = new Vector3[targets.Length];
        _rot = new Vector3[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            _pos[i] = targets[i].localPosition;
            _rot[i] = targets[i].localEulerAngles;
        }
        _cached = true;
    }

    protected Vector3 BasePos(int i) => _pos[i];
    protected Vector3 BaseRot(int i) => _rot[i];

    protected void ResetToBase()
    {
        if (_targets == null) return;
        for (int i = 0; i < _targets.Length; i++)
        {
            _targets[i].localPosition = _pos[i];
            _targets[i].localEulerAngles = _rot[i];
        }
    }

    [Button("Test Animation", ButtonSizes.Large)]
    void TestAnimation()
    {
        if (!Application.isPlaying) { Debug.Log("Phải Play mode (DOTween)"); return; }
        PlayAnim(1f, null);
    }
}