using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// Hiệu ứng bám trên unit khi dính DoT (cháy / độc)
public class DotFx : MonoBehaviour
{
    public ParticleSystem burst;    // bùng lên lúc vừa dính
    public ParticleSystem aura;     // lửa / bong bóng độc bốc lên trong lúc dính (loop)
    public float despawnDelay = 1f;

    private CancellationTokenSource _cts;

    public void Play(Transform target, Vector3 localOffset)
    {
        CancelDespawn();
        transform.SetParent(target, false);
        transform.localPosition = localOffset;

        aura.Clear(true);
        burst.Play(true);
        aura.Play(true);
    }

    // Tách khỏi unit để unit chết/destroy thì particle còn lại vẫn tắt dần
    public void Release()
    {
        transform.SetParent(null, true);
        aura.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        CancelDespawn();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        DespawnAfter(despawnDelay, _cts.Token).Forget();
    }

    async UniTaskVoid DespawnAfter(float seconds, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(seconds), cancellationToken: token);
            SimplePool2.Despawn(gameObject);
        }
        catch (System.OperationCanceledException) { }
    }

    void CancelDespawn()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    void OnDestroy() => CancelDespawn();
}
