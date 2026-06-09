using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EventDispatcher;
using UnityEngine;
using TMPro;

public class EnemyWaveUI : MonoBehaviour
{
    public CanvasGroup group;       
    public TMP_Text waveText;

    public float holdTime = 1f;    
    public float fadeTime = 0.4f;

    private CancellationTokenSource _cts;

    public void Init()
    {
        this.RegisterListener(EventID.ON_ENEMY_WAVE_CHANGED, OnWaveChanged);
        if (group != null) group.alpha = 0f;
    }

    void OnWaveChanged(object param)
    {
        var info = param as WaveInfo;
        if (info == null) return;

        if (waveText != null) waveText.text = $"Wave {info.current}/{info.total}";
        Show().Forget();
    }

    async UniTaskVoid Show()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        var token = _cts.Token;

        try
        {
            group.DOKill();
            group.alpha = 1f;   // hiện ngay

            await UniTask.Delay(System.TimeSpan.FromSeconds(holdTime), cancellationToken: token);

            _ = group.DOFade(0f, fadeTime).SetLink(group.gameObject);
        }
        catch (System.OperationCanceledException) { }
    }

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        if (group != null) group.DOKill();
    }
}