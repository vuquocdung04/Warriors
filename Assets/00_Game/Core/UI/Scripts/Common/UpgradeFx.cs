using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Hiệu ứng "Level Up" dùng chung cho popup nâng cấp skill / trang bị. Gọi Play() ngay sau khi nâng thành công.
//  1. Tích lực : thanh tiến độ vụt đầy, thẻ co lại + rung
//  2. Bung     : thẻ nảy to, chớp trắng phủ đúng hình thẻ, vòng sáng lan ra, sao + mảnh sáng văng ra (màu theo độ hiếm)
//  3. Kết quả  : chữ "LEVEL UP!" bật lên rồi bay lên mờ dần, số level nảy, thanh tiến độ chạy lại, dòng chỉ số nháy xanh
// Mọi object hiệu ứng tự tạo lúc chạy dưới "layer" (nằm trên nội dung popup).
public class UpgradeFx : MonoBehaviour
{
    [Header("Layer chứa hiệu ứng (để trống = chính object này)")]
    public RectTransform layer;

    [Header("Sprites")]
    public Sprite ringSprite;
    public Sprite glowSprite;
    public Sprite sparkSprite;
    public Sprite shardSprite;

    [Header("Chữ LEVEL UP")]
    public TMP_FontAsset font;
    public Material fontMaterial;
    public string levelUpText = "LEVEL UP!";
    public float levelUpSize = 70f;
    public Color levelUpColor = new Color(1f, 0.93f, 0.4f, 1f);

    [Header("Timing")]
    public float chargeTime = 0.18f;
    public float burstTime = 0.3f;
    public float textTime = 0.8f;

    [Header("Mảnh")]
    public int sparkCount = 10;
    public int shardCount = 6;
    public Vector2 pieceDistance = new Vector2(150f, 330f);
    public Vector2 pieceSize = new Vector2(28f, 60f);

    [Header("Sound")]
    public string sfx = "Evolve";

    public static readonly Color StatFlashColor = new Color(0.45f, 1f, 0.45f, 1f);

    private Image _ring, _glow;
    private TextMeshProUGUI _txt;
    private readonly List<Image> _pieces = new();
    private readonly Dictionary<Transform, Image> _flashes = new();
    private Sequence _seq;

    RectTransform Layer => layer != null ? layer : (RectTransform)transform;

    // card: gốc thẻ để scale/rung; cardShape: Image tạo hình thẻ (để chớp trắng đúng hình)
    // fill/levelText: của thẻ (có thể null); newFill: giá trị tiến độ sau khi nâng; stats: các dòng chỉ số để nháy
    public void Play(RectTransform card, Image cardShape, Color color, Image fill, float newFill,
                     TMP_Text levelText, IList<Transform> stats = null)
    {
        if (card == null) return;
        EnsureObjects();
        KillAll(card, fill, levelText);

        Vector3 baseScale = Vector3.one;
        card.localScale = baseScale;
        card.localRotation = Quaternion.identity;
        Vector2 center = ToLayer(card.position);
        Vector2 top = ToLayer(card.TransformPoint(new Vector3(0f, card.rect.yMax, 0f)));
        var flash = GetFlash(card, cardShape);

        _seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

        // ---- 1. TÍCH LỰC ----
        _seq.Append(card.DOScale(baseScale * 0.9f, chargeTime).SetEase(Ease.OutQuad));
        _seq.Join(card.DOShakeRotation(chargeTime, new Vector3(0f, 0f, 9f), 30, 0f, false));
        if (fill != null) _seq.Join(fill.DOFillAmount(1f, chargeTime * 0.8f).SetEase(Ease.InQuad));
        if (flash != null) _seq.Join(flash.DOFade(0.35f, chargeTime).SetEase(Ease.InQuad));

        // ---- 2. BUNG ----
        _seq.AppendCallback(() =>
        {
            PlaySfx(sfx);
            card.localRotation = Quaternion.identity;
            if (flash != null) SetAlpha(flash, 1f);
            BurstRing(center, color);
            BurstPieces(center, color);
        });
        _seq.Append(card.DOScale(baseScale * 1.2f, burstTime * 0.3f).SetEase(Ease.OutQuad));
        _seq.Append(card.DOScale(baseScale, burstTime * 0.7f).SetEase(Ease.OutBack, 2.5f));
        if (flash != null) _seq.Join(flash.DOFade(0f, burstTime * 0.8f));

        // ---- 3. KẾT QUẢ (bắt đầu ngay khi thẻ vừa bung, chồng nhịp cho liền mạch) ----
        float t3 = chargeTime + burstTime * 0.3f;
        _seq.InsertCallback(t3, () =>
        {
            ShowLevelUp(top);
            if (stats != null)
                for (int i = 0; i < stats.Count; i++)
                    if (stats[i] != null && stats[i].gameObject.activeInHierarchy)
                        FlashStat(stats[i], i * 0.05f);
        });
        if (levelText != null)
        {
            Color baseCol = levelText.color;
            _seq.Insert(t3, levelText.transform.DOPunchScale(Vector3.one * 0.45f, 0.4f, 6, 0.5f));
            _seq.Insert(t3, DOTween.To(() => 0f, v => levelText.color = Color.Lerp(StatFlashColor, baseCol, v), 1f, 0.6f));
        }
        if (fill != null)
        {
            _seq.InsertCallback(t3, () => fill.fillAmount = 0f);
            _seq.Insert(t3, fill.DOFillAmount(Mathf.Clamp01(newFill), 0.4f).SetEase(Ease.OutCubic));
        }
    }

    // ---------------- parts ----------------
    void EnsureObjects()
    {
        if (_ring != null) return;
        _glow = NewImage("upgGlow", glowSprite);
        _ring = NewImage("upgRing", ringSprite);
        for (int i = 0; i < sparkCount + shardCount; i++)
        {
            bool spark = i < sparkCount;
            var img = NewImage(spark ? "upgSpark" : "upgShard", spark ? sparkSprite : shardSprite);
            img.preserveAspect = spark;
            _pieces.Add(img);
        }
        var go = new GameObject("upgText", typeof(RectTransform));
        go.layer = Layer.gameObject.layer;
        go.transform.SetParent(Layer, false);
        _txt = go.AddComponent<TextMeshProUGUI>();
        if (font != null) _txt.font = font;
        if (fontMaterial != null) _txt.fontSharedMaterial = fontMaterial;
        _txt.text = levelUpText;
        _txt.fontSize = levelUpSize;
        _txt.color = levelUpColor;
        _txt.alignment = TextAlignmentOptions.Center;
        _txt.enableWordWrapping = false;
        _txt.raycastTarget = false;
        _txt.rectTransform.sizeDelta = new Vector2(600f, 120f);
        go.SetActive(false);
    }

    Image NewImage(string name, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.layer = Layer.gameObject.layer;
        go.transform.SetParent(Layer, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        go.SetActive(false);
        return img;
    }

    // Lớp trắng phủ đúng hình thẻ (copy sprite/kiểu của Image tạo hình), đặt trên cùng của thẻ
    Image GetFlash(RectTransform card, Image shape)
    {
        if (shape == null) return null;
        if (!_flashes.TryGetValue(card, out var flash) || flash == null)
        {
            var go = new GameObject("upgFlash", typeof(RectTransform), typeof(Image));
            go.layer = card.gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(shape.transform.parent, false);
            flash = go.GetComponent<Image>();
            flash.raycastTarget = false;
            _flashes[card] = flash;
        }
        var srt = shape.rectTransform; var frt = flash.rectTransform;
        frt.anchorMin = srt.anchorMin; frt.anchorMax = srt.anchorMax; frt.pivot = srt.pivot;
        frt.anchoredPosition = srt.anchoredPosition; frt.sizeDelta = srt.sizeDelta;
        frt.localScale = srt.localScale; frt.localRotation = srt.localRotation;
        flash.sprite = shape.sprite; flash.type = shape.type; flash.preserveAspect = shape.preserveAspect;
        flash.pixelsPerUnitMultiplier = shape.pixelsPerUnitMultiplier;
        flash.color = new Color(1f, 1f, 1f, 0f);
        flash.transform.SetAsLastSibling();
        flash.gameObject.SetActive(true);
        return flash;
    }

    void BurstRing(Vector2 center, Color color)
    {
        if (_glow != null && _glow.sprite != null)
        {
            var g = _glow.rectTransform;
            g.SetAsLastSibling();
            g.anchoredPosition = center; g.sizeDelta = new Vector2(420f, 420f); g.localScale = Vector3.one * 0.4f;
            _glow.color = new Color(color.r, color.g, color.b, 0.75f);
            _glow.gameObject.SetActive(true);
            DOTween.Sequence().SetUpdate(true).SetLink(_glow.gameObject)
                .Append(g.DOScale(1.5f, 0.45f).SetEase(Ease.OutCubic))
                .Join(_glow.DOFade(0f, 0.45f).SetEase(Ease.InQuad))
                .OnComplete(() => _glow.gameObject.SetActive(false));
        }
        if (_ring != null && _ring.sprite != null)
        {
            var r = _ring.rectTransform;
            r.SetAsLastSibling();
            r.anchoredPosition = center; r.sizeDelta = new Vector2(300f, 300f); r.localScale = Vector3.one * 0.3f;
            _ring.color = Color.Lerp(color, Color.white, 0.35f);
            _ring.gameObject.SetActive(true);
            DOTween.Sequence().SetUpdate(true).SetLink(_ring.gameObject)
                .Append(r.DOScale(2.1f, 0.5f).SetEase(Ease.OutCubic))
                .Join(_ring.DOFade(0f, 0.5f).SetEase(Ease.InQuad))
                .OnComplete(() => _ring.gameObject.SetActive(false));
        }
    }

    void BurstPieces(Vector2 center, Color color)
    {
        Color light = Color.Lerp(color, Color.white, 0.5f);
        for (int i = 0; i < _pieces.Count; i++)
        {
            var img = _pieces[i];
            if (img.sprite == null) continue;
            bool spark = i < sparkCount;
            var rt = img.rectTransform;

            Vector2 dir = Random.insideUnitCircle.normalized;
            float dist = Random.Range(pieceDistance.x, pieceDistance.y);
            Vector2 start = center + dir * 40f;
            Vector2 end = center + dir * dist + Vector2.down * Random.Range(20f, 90f);
            float size = Random.Range(pieceSize.x, pieceSize.y) * (spark ? 1.2f : 1f);

            rt.SetAsLastSibling();
            rt.anchoredPosition = start;
            rt.sizeDelta = spark ? Vector2.one * size : new Vector2(size, size * Random.Range(0.6f, 1.2f));
            rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            rt.localScale = Vector3.one;
            img.color = spark ? (Random.value < 0.5f ? Color.white : light) : Color.Lerp(color, light, Random.value);
            img.gameObject.SetActive(true);

            float t = Random.Range(0.45f, 0.7f);
            DOTween.Kill(rt);
            DOTween.Sequence().SetUpdate(true).SetLink(img.gameObject).SetTarget(rt)
                .Append(rt.DOAnchorPos(end, t).SetEase(Ease.OutCubic))
                .Join(rt.DOLocalRotate(new Vector3(0f, 0f, Random.Range(-360f, 360f)), t, RotateMode.LocalAxisAdd))
                .Join(rt.DOScale(0f, t).SetEase(Ease.InQuad))
                .OnComplete(() => img.gameObject.SetActive(false));
        }
    }

    void ShowLevelUp(Vector2 top)
    {
        if (_txt == null) return;
        var rt = _txt.rectTransform;
        rt.SetAsLastSibling();
        rt.anchoredPosition = top + Vector2.up * 30f;
        rt.localScale = Vector3.zero;
        _txt.alpha = 1f;
        _txt.gameObject.SetActive(true);
        DOTween.Kill(rt);
        DOTween.Sequence().SetUpdate(true).SetLink(_txt.gameObject).SetTarget(rt)
            .Append(rt.DOScale(1f, 0.3f).SetEase(Ease.OutBack, 3f))
            .Join(rt.DOAnchorPosY(top.y + 30f + 110f, textTime).SetEase(Ease.OutCubic))
            .Insert(textTime * 0.55f, _txt.DOFade(0f, textTime * 0.45f))
            .OnComplete(() => _txt.gameObject.SetActive(false));
    }

    void FlashStat(Transform stat, float delay)
    {
        stat.DOKill(true);
        stat.DOPunchScale(Vector3.one * 0.12f, 0.35f, 6, 0.5f).SetDelay(0.05f + delay).SetUpdate(true).SetLink(stat.gameObject);
        foreach (var t in stat.GetComponentsInChildren<TMP_Text>())
        {
            var text = t;
            DOTween.Kill(text);            // OnKill của lần trước trả màu gốc -> rồi mới lưu
            Color baseCol = text.color;
            DOTween.To(() => 0f, v => text.color = Color.Lerp(StatFlashColor, baseCol, v), 1f, 0.7f)
                .SetDelay(delay).SetUpdate(true).SetTarget(text).SetLink(text.gameObject)
                .OnKill(() => { if (text != null) text.color = baseCol; });
        }
    }

    // ---------------- helpers ----------------
    Vector2 ToLayer(Vector3 world)
    {
        Vector3 local = Layer.InverseTransformPoint(world);
        // đổi sang anchoredPosition khi anchor ở giữa layer
        return new Vector2(local.x, local.y) - Layer.rect.center;
    }

    void KillAll(RectTransform card, Image fill, TMP_Text levelText)
    {
        _seq?.Kill(true);
        card.DOKill(true);
        if (fill != null) fill.DOKill(true);
        if (levelText != null) levelText.transform.DOKill(true);
    }

    static void SetAlpha(Image img, float a)
    {
        var c = img.color; c.a = a; img.color = c;
    }

    static void PlaySfx(string key)
    {
        if (!string.IsNullOrEmpty(key) && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(key);
    }
}
