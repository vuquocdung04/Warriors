using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EventDispatcher;
using UnityEngine;

public enum DropType { Coin, Gem }

public class CurrencyDrop : MonoBehaviour
{
    public SpriteRenderer icon;

    private CancellationTokenSource _cts;

    public void Play(DropType type, int amount, Vector3 pos)
    {
        if (type == DropType.Coin)
        {
            UseProfile.Coin.Value += amount;
            this.PostEvent(EventID.CHANGE_COIN, amount);
        }
        else
        {
            UseProfile.Gem.Value += amount;
        }

        pos.x += Random.Range(-0.3f, 0.3f);
        pos.y += Random.Range(-0.3f, 0.3f);
        transform.position = pos;
        transform.localScale = Vector3.one;

        var c = icon.color; c.a = 1f; icon.color = c;
        icon.DOKill();
        transform.DOKill();

        transform.DOJump(pos, jumpPower: 0.5f, numJumps: 1, duration: 0.4f).SetLink(gameObject);

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        Run(0.8f, 0.6f, _cts.Token).Forget();
    }

    async UniTaskVoid Run(float hold, float fade, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(hold), cancellationToken: token);
            _ = icon.DOFade(0f, fade).SetLink(gameObject);
            await UniTask.Delay(System.TimeSpan.FromSeconds(fade), cancellationToken: token);
            SimplePool2.Despawn(gameObject);
        }
        catch (System.OperationCanceledException) { }
    }

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        icon.DOKill();
        transform.DOKill();
    }
}