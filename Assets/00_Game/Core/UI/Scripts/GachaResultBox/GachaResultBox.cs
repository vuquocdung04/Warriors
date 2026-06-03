using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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

    protected override void Init()
    {
        btnClose.OnClicked(delegate { Close(); });
    }

    protected override void InitState() { }

    public void ShowResult(List<GachaService.GachaResult> results)
    {
        Show();

        ClearHolder(holderX1);
        ClearHolder(holderX10);

        bool isX10 = results.Count > 1;
        tabX1.SetCanvasState(!isX10, !isX10 ? 1f : 0f);
        tabX10.SetCanvasState(isX10, isX10 ? 1f : 0f);

        btnClose.enabled = false;

        if (isX10) PlayX10(results).Forget();
        else
        {
            if (results.Count > 0) SpawnItem(results[0], holderX1);
            btnClose.enabled = true;
        }
    }

    async UniTaskVoid PlayX10(List<GachaService.GachaResult> results)
    {
        var token = this.GetCancellationTokenOnDestroy();
        foreach (var res in results)
        {
            SpawnItem(res, holderX10);
            await UniTask.Delay(System.TimeSpan.FromSeconds(spawnDelay), cancellationToken: token);
        }
        btnClose.enabled = true;
    }

    void SpawnItem(GachaService.GachaResult res, Transform holder)
    {
        var item = Instantiate(GetItemPrefab(res.type), holder);

        item.Init(res.equip, res.type, GetIcon(res.type, res.equip.id), null);
        item.SetEquipped(false);
        item.SetViewProgress(false);
        item.SetButtonEnabled(false);
        item.SetNew(res.isFirstOwn);

        if (res.isFirstOwn)
        {
            var t = item.transform;
            t.localScale = Vector3.one * 1.1f;
            t.DOScale(1f, popDuration).SetEase(Ease.OutBack).SetLink(item.gameObject);
        }
    }

    void ClearHolder(Transform holder)
    {
        for (int i = holder.childCount - 1; i >= 0; i--)
            Destroy(holder.GetChild(i).gameObject);
    }

    EquipmentItem GetItemPrefab(EquipType type) => type switch
    {
        EquipType.Melee => DataRepo.Instance.equipmentDatabase.GetMeleeItemPrefab(),
        EquipType.Range => DataRepo.Instance.equipmentDatabase.GetRangeItemPrefab(),
        _ => DataRepo.Instance.equipmentDatabase.GetShieldItemPrefab(),
    };

    Sprite GetIcon(EquipType type, string id) => type switch
    {
        EquipType.Melee => DataRepo.Instance.equipmentDatabase.GetMeleeIcon(id),
        EquipType.Range => DataRepo.Instance.equipmentDatabase.GetRangeIcon(id),
        _ => DataRepo.Instance.equipmentDatabase.GetShieldIcon(id),
    };

    protected override void OnDestroy() => base.OnDestroy();
}