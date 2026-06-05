using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultBox : BaseBox<GachaResultBox>
{
    [Header("Tabs")]
    public CanvasGroup tabX1;
    public CanvasGroup tabX10;

    [Header("Holders")]
    public Transform holderX1;
    public Transform holderX10;

    [Header("Close")]
    public Button btnClose;

    [Header("Config")]
    public float spawnDelay = 0.1f;
    public float popDuration = 0.2f;
    private EventID? _postOnClose;
    protected override void Init()
    {
        btnClose.OnClicked(delegate { OnClose(); });
    }

    protected override void InitState() { }

    // dùng chung cho mọi loại gacha (equipment/skill/treasure...)
    public void ShowResult(List<IGachaResultEntry> entries, EventID? postOnClose = null)
    {
        _postOnClose = postOnClose;
        Show();

        ClearHolder(holderX1);
        ClearHolder(holderX10);

        bool isX10 = entries.Count > 1;
        tabX1.SetCanvasState(!isX10, !isX10 ? 1f : 0f);
        tabX10.SetCanvasState(isX10, isX10 ? 1f : 0f);

        btnClose.enabled = false;

        if (isX10) PlayX10(entries).Forget();
        else
        {
            if (entries.Count > 0) SpawnEntry(entries[0], holderX1);
            btnClose.enabled = true;
        }
    }

    async UniTaskVoid PlayX10(List<IGachaResultEntry> entries)
    {
        var token = this.GetCancellationTokenOnDestroy();
        foreach (var e in entries)
        {
            SpawnEntry(e, holderX10);
            await UniTask.Delay(System.TimeSpan.FromSeconds(spawnDelay), cancellationToken: token);
        }
        btnClose.enabled = true;
    }

    void SpawnEntry(IGachaResultEntry entry, Transform holder)
    {
        var go = entry.Spawn(holder);
        if (entry.IsNew)
        {
            var t = go.transform;
            t.localScale = Vector3.one * 1.1f;
            t.DOScale(1f, popDuration).SetEase(Ease.OutBack).SetLink(go);
        }
    }
    void OnClose()
    {
        Close();
        if (_postOnClose.HasValue) this.PostEvent(_postOnClose.Value);
        _postOnClose = null;
    }
    void ClearHolder(Transform holder)
    {
        for (int i = holder.childCount - 1; i >= 0; i--)
            Destroy(holder.GetChild(i).gameObject);
    }

    protected override void OnDestroy() => base.OnDestroy();
}