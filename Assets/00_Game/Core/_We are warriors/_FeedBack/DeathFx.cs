using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DeathFx : MonoBehaviour
{
    public ParticleSystem ps;

    private CancellationTokenSource _cts;

    public void Play(Vector3 worldPos)
    {
        transform.position = worldPos;
        ps.Clear();
        ps.Play();

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        DespawnAfter(ps.main.startLifetime.constantMax, _cts.Token).Forget();
    }

    async UniTaskVoid DespawnAfter(float seconds, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(seconds),
                                ignoreTimeScale: true, cancellationToken: token);
            SimplePool2.Despawn(gameObject);
        }
        catch (System.OperationCanceledException) { }
    }

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}