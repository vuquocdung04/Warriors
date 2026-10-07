using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FreezeFx : MonoBehaviour
{
    public ParticleSystem burst;    // nổ băng + vòng sóng lúc vừa bị đóng băng
    public ParticleSystem aura;     // bông tuyết bay quanh trong lúc bị đóng băng (loop)
    public ParticleSystem shatter;  // mảnh băng vỡ khi hết đóng băng
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

    // Tách khỏi unit để unit chết/destroy thì hiệu ứng vỡ băng vẫn chạy hết
    public void Release()
    {
        transform.SetParent(null, true);
        aura.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        shatter.Play(true);

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
