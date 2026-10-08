using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// Hiệu ứng hút máu one-shot: hạt máu hút vào thân -> loé đỏ -> tim bay lên
// Không gắn làm con của unit để unit chết/destroy không kéo theo object trong pool
public class LifeStealFx : MonoBehaviour
{
    public ParticleSystem root;
    public float despawnDelay = 1f;

    private CancellationTokenSource _cts;

    public void Play(Vector3 worldPos)
    {
        transform.position = worldPos;
        root.Clear(true);
        root.Play(true);

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
