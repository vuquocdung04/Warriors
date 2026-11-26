using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Màn tiến hóa sau khi nâng bậc unit trong trận, cùng phong cách thẻ dùng skill (SkillCastPresenter):
// nền tối, thẻ cũ ▶ thẻ mới; thẻ mới rơi xuống -> bẹp -> nảy + chớp trắng. Mở popup -> GameFlow tự tạm dừng trận.
public class EvolveBox : BaseBox<EvolveBox>
{
    [System.Serializable]
    public class CardView
    {
        public RectTransform root;
        public Image icon;
        public Image flash;
        public TMP_Text txtName;
    }

    public Button btnClose;            // nền tối, chạm để đóng
    public CardView oldCard;
    public CardView newCard;
    public RectTransform arrow;
    public TMP_Text txtStats;
    public TMP_Text txtTap;

    [Header("Anim (giống thẻ skill)")]
    public float newCardScale = 1f;
    public float dropHeight = 520f;
    public float dropTime = 0.16f;
    public float settleTime = 0.26f;
    [Tooltip("Chặn chạm đóng ngay lập tức (tránh cú chạm nâng cấp đóng luôn popup)")]
    public float closeLockTime = 0.6f;

    [Header("Sound")]
    public string sfxPop = "whoosh";
    public string sfxLand = "Evolve";

    private float _openedAt;
    private Sequence _seq;
    private Vector2 _newHome;
    private Vector3 _oldScale;
    private bool _cached;

    protected override void Init()
    {
        btnClose.OnClicked(() =>
        {
            if (Time.unscaledTime - _openedAt < closeLockTime) return;
            Close();
        });
    }

    protected override void InitState() { }

    public void SetupAndShow(Sprite oldSprite, Sprite newSprite, int newTier)
    {
        if (!_cached) { _newHome = newCard.root.anchoredPosition; _oldScale = oldCard.root.localScale; _cached = true; }

        SetCard(oldCard, oldSprite, TierName(newTier - 1));
        SetCard(newCard, newSprite, TierName(newTier));
        string m = UnitTier.StatMultiplier(newTier).ToString("0.0");
        txtStats.text = $"HP x{m}   ATK x{m}";
        _openedAt = Time.unscaledTime;
        Show();
        PlayReveal();
    }

    static void SetCard(CardView c, Sprite sp, string name)
    {
        c.icon.sprite = sp;
        c.icon.enabled = sp != null;
        c.txtName.text = name;
    }

    static string TierName(int tier) => tier <= 0 ? "Base" : $"Tier {tier}";

    void PlayReveal()
    {
        _seq?.Kill();
        var oldT = oldCard.root; var newT = newCard.root;
        foreach (var t in new Transform[] { oldT, newT, arrow, newCard.icon.transform }) t.DOKill();
        txtStats.DOKill(); txtTap.DOKill();

        float s = newCardScale;
        oldT.localScale = _oldScale * 0.6f;
        arrow.localScale = Vector3.zero;
        newT.anchoredPosition = _newHome + Vector2.up * dropHeight;
        newT.localScale = new Vector3(s * 0.7f, s * 1.1f, 1f);
        newT.localRotation = Quaternion.Euler(0f, 0f, -10f);
        newT.gameObject.SetActive(false);
        SetAlpha(newCard.flash, 0f);
        txtStats.alpha = 0f; txtTap.alpha = 0f;

        _seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        // thẻ cũ hiện ra + tam giác
        _seq.Append(oldT.DOScale(_oldScale, 0.18f).SetEase(Ease.OutBack));
        _seq.Append(arrow.DOScale(1f, 0.16f).SetEase(Ease.OutBack));
        // thẻ mới rơi xuống, kéo dài dọc
        _seq.AppendCallback(() => { newT.gameObject.SetActive(true); PlaySfx(sfxPop); });
        _seq.Append(newT.DOAnchorPos(_newHome, dropTime).SetEase(Ease.InQuad));
        _seq.Join(newT.DOScale(new Vector3(s * 0.8f, s * 1.25f, 1f), dropTime).SetEase(Ease.InQuad));
        _seq.Join(newT.DOLocalRotate(Vector3.zero, dropTime).SetEase(Ease.OutQuad));
        // chạm: bẹp ngang -> nảy đàn hồi, chớp trắng, icon nảy
        _seq.AppendCallback(() => { PlaySfx(sfxLand); SetAlpha(newCard.flash, 0.85f); });
        _seq.Append(newT.DOScale(new Vector3(s * 1.25f, s * 0.8f, 1f), 0.06f).SetEase(Ease.OutQuad));
        _seq.Append(newT.DOScale(s, settleTime).SetEase(Ease.OutElastic, 1.1f, 0.35f));
        _seq.Join(newCard.flash.DOFade(0f, 0.18f));
        _seq.Join(newCard.icon.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 7, 0.5f));
        _seq.Join(newT.DOPunchAnchorPos(new Vector2(0f, -22f), 0.25f, 8, 0.6f));
        // chỉ số + gợi ý chạm
        _seq.Append(txtStats.DOFade(1f, 0.18f));
        _seq.Append(txtTap.DOFade(0.75f, 0.18f));
        _seq.OnComplete(() =>
            arrow.DOScale(1.1f, 0.45f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true).SetLink(gameObject));
    }

    static void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        var c = g.color; c.a = a; g.color = c;
    }

    static void PlaySfx(string key)
    {
        if (!string.IsNullOrEmpty(key) && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(key);
    }

    protected override void OnDestroy()
    {
        _seq?.Kill();
        base.OnDestroy();
    }
}
