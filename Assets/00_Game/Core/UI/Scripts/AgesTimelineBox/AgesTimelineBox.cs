using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;

public class AgesTimelineBox : BaseBox<AgesTimelineBox>
{
    [Header("Buttons")]
    public Button btnClose;

    [Header("Items")]
    public Transform itemHolder;
    public AgeTimelineItem itemPrefab;

    [Header("Fill + Scroll")]
    public Image fillProgress;
    public ScrollRect scrollRect;

    [Header("Anim")]
    public float fillDuration = 1f;
    public float scrollDuration = 1f;
    public float startDelay = 0.3f;

    private List<HouseData> _civs;
    private readonly List<AgeTimelineItem> _items = new();
    private int _civCount;

    protected override void Init()
    {
        btnClose.OnClicked(delegate { Close(); });
        BuildItems();
    }

    protected override void InitState() { }

    void BuildItems()
    {
        _civs = DataRepo.Instance.unitDatabase.GetCivsByOrder();
        _civCount = _civs.Count;

        foreach (var it in _items) it.gameObject.SetActive(false);

        for (int i = 0; i < _civs.Count; i++)
        {
            AgeTimelineItem item = i < _items.Count ? _items[i] : null;
            if (item == null)
            {
                item = Instantiate(itemPrefab, itemHolder);
                _items.Add(item);
            }
            item.gameObject.SetActive(true);
            item.Setup(_civs[i]);
        }
    }

    float FillFor(int order) => _civCount > 1 ? (order - 1) / (float)(_civCount - 1) : 0f;

    // mở có animation (gọi khi vừa evolve)
    public void ShowAnimated()
    {
        Show();
        AnimateRoutine().Forget();
        this.PostEvent(EventID.ON_CIV_CHANGED);
        this.PostEvent(EventID.CHANGE_COIN);
    }

    // mở set cứng (các lần sau, mở bằng nút khác)
    public void ShowStatic()
    {
        Show();
        int curOrder = DataRepo.Instance.unitDatabase.GetCivOrder(UseProfile.CurrentCiv.Value);
        fillProgress.fillAmount = FillFor(curOrder);
        ScrollToOrder(curOrder, instant: true);
    }

    async UniTaskVoid AnimateRoutine()
    {
        scrollRect.enabled = false;
        btnClose.enabled = false;
        int curOrder = DataRepo.Instance.unitDatabase.GetCivOrder(UseProfile.CurrentCiv.Value);
        int prevOrder = curOrder - 1;   // evolve luôn lên 1 bậc

        fillProgress.fillAmount = FillFor(prevOrder);
        ScrollToOrder(prevOrder, instant: true);

        await UniTask.Delay(System.TimeSpan.FromSeconds(startDelay),
                            cancellationToken: this.GetCancellationTokenOnDestroy());

        _ = fillProgress.DOFillAmount(FillFor(curOrder), fillDuration).SetEase(Ease.InOutSine).SetLink(gameObject);
        ScrollToOrder(curOrder, instant: false);

        scrollRect.enabled = true;
        btnClose.enabled = true;
    }

    void ScrollToOrder(int order, bool instant)
    {
        var content = scrollRect.content;
        var viewport = scrollRect.viewport;
        var grid = itemHolder.GetComponent<GridLayoutGroup>();

        float scrollable = content.rect.width - viewport.rect.width;
        if (scrollable <= 0f) return;

        int index = order - 1;
        float cw = grid.cellSize.x;
        float sp = grid.spacing.x;
        float padL = grid.padding.left;
        float itemCenterX = padL + index * (cw + sp) + cw * 0.5f;

        float normalized = Mathf.Clamp01((itemCenterX - viewport.rect.width * 0.5f) / scrollable);

        if (instant)
            scrollRect.horizontalNormalizedPosition = normalized;
        else
            DOTween.To(() => scrollRect.horizontalNormalizedPosition,
                       x => scrollRect.horizontalNormalizedPosition = x,
                       normalized, scrollDuration).SetEase(Ease.InOutSine).SetLink(gameObject);
    }

    protected override void OnDestroy() => base.OnDestroy();
}