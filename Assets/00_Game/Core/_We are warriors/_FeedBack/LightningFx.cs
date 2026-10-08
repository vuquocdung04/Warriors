using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 1 tia sét (port từ FXDrinkItem bên DrinkPacking): chạy các particle, hết thì tự trả về pool
public class LightningFx : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> listParticle = new List<ParticleSystem>();
    [SerializeField] private float despawnTimeout = 3f;

    public IReadOnlyList<ParticleSystem> ListParticle => listParticle;

    private CancellationTokenSource _cts;

    bool IsPlaying
    {
        get
        {
            for (int i = 0; i < listParticle.Count; i++)
                if (listParticle[i] != null && listParticle[i].IsAlive(true)) return true;
            return false;
        }
    }

    public void PlayPooled()
    {
        for (int i = 0; i < listParticle.Count; i++)
        {
            if (listParticle[i] == null) continue;
            listParticle[i].Clear(true);
            listParticle[i].Play(true);
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        DespawnWhenDone(_cts.Token).Forget();
    }

    async UniTaskVoid DespawnWhenDone(CancellationToken token)
    {
        try
        {
            float elapsed = 0f;
            await UniTask.Yield(token);
            while (IsPlaying && elapsed < despawnTimeout)
            {
                elapsed += Time.deltaTime;
                await UniTask.Yield(token);
            }
            SimplePool2.Despawn(gameObject);
        }
        catch (System.OperationCanceledException) { }
    }

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}
