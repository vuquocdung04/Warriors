using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class GroundCrackFx : MonoBehaviour
{
    public SpriteRenderer sprite;
    public float fadeDuration = 10f;

    private CancellationTokenSource _cts;

    public void Play(Vector3 worldPos)
    {
        transform.position = worldPos;

        var c = sprite.color;
        c.a = 1f;
        sprite.color = c;

        sprite.DOKill();
        sprite.DOFade(0f, fadeDuration).SetEase(Ease.Linear).SetLink(gameObject);

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        DespawnAfter(fadeDuration, _cts.Token).Forget();
    }

    async UniTaskVoid DespawnAfter(float seconds, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(seconds),
                                cancellationToken: token);
            SimplePool2.Despawn(gameObject);
        }
        catch (System.OperationCanceledException) { }
    }

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        sprite.DOKill();
    }
}