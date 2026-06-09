using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

public abstract class AttackStrategyBase : MonoBehaviour, IAttackStrategy
{
    protected Unit owner;
    protected Sequence seq;
    private bool _playing;

    public virtual void Init(Unit owner) => this.owner = owner;

    public void Attack(IDamageable target, float duration)
    {
        if (_playing) return;
        _playing = true;
        PlayAnim2(target, duration, () => Hit(target));
    }

    // loại theo target override cái này
    protected virtual void PlayAnim2(IDamageable target, float duration, System.Action onHit)
    {
        PlayAnim(duration, onHit);
    }

    // loại thường (melee/súng cố định) implement cái này
    protected abstract void PlayAnim(float duration, System.Action onHit);

    public virtual void ResetToIdle() { }

    protected void OnAnimDone() => _playing = false;

    protected virtual void Hit(IDamageable target)
    {
        if (target != null && target.IsAlive) owner.DealDamage(target);
    }

    // ngắm: nòng +Y, xử flip qua InverseTransformPoint
    protected float AimAngleLocal(Transform weapon, IDamageable target)
    {
        if (target == null || weapon.parent == null) return weapon.localEulerAngles.z;
        Vector3 worldTarget = target.AimPoint + Vector3.up;
        Vector3 localTarget = weapon.parent.InverseTransformPoint(worldTarget);
        Vector2 dir = (Vector2)(localTarget - weapon.localPosition);
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
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

    [Button("Test Animation")]
    void TestAnimation()
    {
        if (!Application.isPlaying) { Debug.Log("Phải Play mode"); return; }
        PlayAnim2(null, 1f, null);
    }
}