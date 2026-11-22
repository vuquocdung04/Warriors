using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// "Feeling" lúc dùng skill (pause game trong lúc diễn, tween SetUpdate(true)):
//  1. ĐẬP VÀO : thẻ rơi từ trên xuống (kéo dài dọc) -> chạm thì bẹp ngang rồi nảy đàn hồi, chớp trắng, icon nảy
//  2. GỒNG    : rung tăng dần + to dần + sáng trắng dần (tích năng lượng)
//  3. VỠ      : co lại lấy đà -> bung to, chớp trắng, mảnh vỡ + tia sáng văng ra, thẻ cháy tan nhanh (dissolve OutCubic)
//  4. Nền tối mờ dần -> resume -> lúc này skill mới kích hoạt
public class SkillCastPresenter : MonoBehaviour
{
    public static SkillCastPresenter Instance { get; private set; }
    public static bool IsPlaying => Instance != null && Instance._playing;

    [Header("Refs")]
    public Image dim;              // nền đen, chặn bấm trong lúc diễn
    public RectTransform card;     // thẻ (gốc để scale/xoay)
    public Image[] dissolveImages; // các Image của thẻ dùng chung material dissolve
    public Image icon;
    public TMP_Text txtName;
    public Image flash;            // lớp trắng phủ hình thẻ (cũng nằm trong dissolveImages)

    [Header("Dissolve")]
    public Material dissolveMaterial;   // material gốc (UI/Dissolve), runtime tạo bản sao để chỉnh _Progress

    [Header("1. Đập vào")]
    public float dimIn = 0.1f;
    public float dropHeight = 520f;
    public float dropTime = 0.16f;
    public float settleTime = 0.26f;
    [Range(0f, 1f)] public float dimAlpha = 0.75f;
    public float cardScale = 1.2f;

    [Header("2. Gồng")]
    public float chargeTime = 0.32f;
    public float chargeShake = 7f;        // độ rung tối đa (độ)
    [Range(0f, 1f)] public float chargeGlow = 0.45f;

    [Header("3. Vỡ")]
    public float windupTime = 0.07f;
    public float burstTime = 0.32f;
    public float burstScale = 1.35f;
    public float dimOut = 0.2f;

    [Header("Mảnh vỡ")]
    public Sprite shardSprite;
    public Sprite sparkSprite;
    public int shardCount = 14;
    public int sparkCount = 8;
    public Vector2 shardSize = new Vector2(40f, 90f);
    public Vector2 shardDistance = new Vector2(260f, 560f);
    public float shardTime = 0.55f;
    public Color shardColorA = new Color(0.47f, 0.32f, 0.88f, 1f);
    public Color shardColorB = new Color(0.75f, 0.65f, 1f, 1f);
    public Color sparkColor = new Color(1f, 0.92f, 0.55f, 1f);

    [Header("Sound")]
    public string sfxPop = "whoosh";
    public string sfxLand = "punch";
    public string sfxBreak = "Evolve";

    private Material _mat;
    private Sequence _seq;
    private bool _playing;
    private Action _onDone;
    private Vector2 _cardHome;
    private readonly List<Image> _pieces = new();
    private static readonly int ProgressId = Shader.PropertyToID("_Progress");

    void Awake()
    {
        Instance = this;
        _cardHome = card.anchoredPosition;
        if (dissolveMaterial != null)
        {
            _mat = new Material(dissolveMaterial);
            foreach (var img in dissolveImages)
                if (img != null) img.material = _mat;
        }
        BuildPieces();
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        _seq?.Kill();
        if (_mat != null) Destroy(_mat);
    }

    // onDone gọi sau khi diễn xong + game đã resume -> nơi kích hoạt skill thật
    public void Play(SkillData data, Sprite skillIcon, Action onDone)
    {
        if (_playing) return;
        _playing = true;
        _onDone = onDone;

        GameFlow.Instance.RequestPause();
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        ResetVisual(data, skillIcon);

        _seq?.Kill();
        _seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        float s = cardScale;

        // ---- 1. ĐẬP VÀO ----
        _seq.Append(dim.DOFade(dimAlpha, dimIn));
        _seq.AppendCallback(() => PlaySfx(sfxPop));
        // rơi xuống nhanh dần, kéo dài theo chiều dọc
        _seq.Append(card.DOAnchorPos(_cardHome, dropTime).SetEase(Ease.InQuad));
        _seq.Join(card.DOScale(new Vector3(s * 0.8f, s * 1.25f, 1f), dropTime).SetEase(Ease.InQuad));
        _seq.Join(card.DOLocalRotate(Vector3.zero, dropTime).SetEase(Ease.OutQuad));
        // chạm: bẹp ngang -> nảy đàn hồi về
        _seq.AppendCallback(() => { PlaySfx(sfxLand); SetFlash(0.85f); });
        _seq.Append(card.DOScale(new Vector3(s * 1.25f, s * 0.8f, 1f), 0.06f).SetEase(Ease.OutQuad));
        _seq.Append(card.DOScale(s, settleTime).SetEase(Ease.OutElastic, 1.1f, 0.35f));
        _seq.Join(flash.DOFade(0f, 0.18f));
        _seq.Join(icon.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 7, 0.5f));
        _seq.Join(card.DOPunchAnchorPos(new Vector2(0f, -22f), 0.25f, 8, 0.6f));

        // ---- 2. GỒNG: rung tăng dần + to dần + sáng dần ----
        float shake = 0f;
        _seq.Append(DOTween.To(() => shake, v =>
        {
            shake = v;
            card.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.unscaledTime * 55f) * chargeShake * v);
        }, 1f, chargeTime).SetEase(Ease.InQuad));
        _seq.Join(card.DOScale(s * 1.07f, chargeTime).SetEase(Ease.InQuad));
        _seq.Join(flash.DOFade(chargeGlow, chargeTime).SetEase(Ease.InQuad));

        // ---- 3. VỠ ----
        // co lại lấy đà
        _seq.Append(card.DOScale(s * 0.94f, windupTime).SetEase(Ease.InQuad));
        _seq.Join(card.DOLocalRotate(Vector3.zero, windupTime));
        // bung ra: chớp trắng, văng mảnh, cháy tan nhanh lúc đầu chậm dần về cuối
        _seq.AppendCallback(() =>
        {
            PlaySfx(sfxBreak);
            SetFlash(1f);
            txtName.alpha = 0f;
            BurstPieces();
        });
        _seq.Append(card.DOScale(s * burstScale, burstTime).SetEase(Ease.OutCubic));
        _seq.Join(DOTween.To(() => GetProgress(), SetProgress, 1f, burstTime).SetEase(Ease.OutCubic));
        _seq.Join(flash.DOFade(0.35f, burstTime * 0.5f));

        // ---- 4. nền tối mờ dần rồi trả game (mảnh vỡ vẫn bay nốt) ----
        _seq.Append(dim.DOFade(0f, dimOut));
        _seq.AppendInterval(Mathf.Max(0f, shardTime - burstTime - dimOut));
        _seq.OnComplete(Finish);
    }

    void ResetVisual(SkillData data, Sprite skillIcon)
    {
        SetProgress(0f);

        var c = dim.color; c.a = 0f; dim.color = c;
        dim.raycastTarget = true;

        card.anchoredPosition = _cardHome + Vector2.up * dropHeight;
        card.localScale = new Vector3(cardScale * 0.7f, cardScale * 1.1f, 1f);
        card.localRotation = Quaternion.Euler(0f, 0f, -10f);
        SetFlash(0f);

        if (icon != null)
        {
            icon.sprite = skillIcon;
            icon.enabled = skillIcon != null;
            icon.transform.localScale = Vector3.one;
        }
        if (txtName != null)
        {
            txtName.text = data != null ? data.name : "";
            txtName.alpha = 1f;
        }
        foreach (var p in _pieces) p.gameObject.SetActive(false);
    }

    // ---------- Mảnh vỡ ----------
    void BuildPieces()
    {
        int total = Mathf.Max(0, shardCount) + Mathf.Max(0, sparkCount);
        for (int i = 0; i < total; i++)
        {
            bool spark = i >= shardCount;
            var go = new GameObject(spark ? "spark" : "shard", typeof(RectTransform), typeof(Image));
            go.layer = gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            var img = go.GetComponent<Image>();
            img.sprite = spark ? sparkSprite : shardSprite;
            img.raycastTarget = false;
            img.preserveAspect = spark;
            go.SetActive(false);
            _pieces.Add(img);
        }
    }

    void BurstPieces()
    {
        Vector2 center = _cardHome;
        Vector2 half = card.rect.size * 0.5f * cardScale;
        for (int i = 0; i < _pieces.Count; i++)
        {
            var img = _pieces[i];
            var rt = img.rectTransform;
            bool spark = i >= shardCount;

            // xuất phát rải trên mặt thẻ, bay ra theo hướng từ tâm (lệch ngẫu nhiên)
            Vector2 start = center + new Vector2(UnityEngine.Random.Range(-half.x, half.x), UnityEngine.Random.Range(-half.y, half.y)) * 0.8f;
            Vector2 dir = (start - center).sqrMagnitude > 1f ? (start - center).normalized : UnityEngine.Random.insideUnitCircle.normalized;
            dir = (dir + UnityEngine.Random.insideUnitCircle * 0.35f).normalized;
            float dist = UnityEngine.Random.Range(shardDistance.x, shardDistance.y) * (spark ? 1.2f : 1f);
            Vector2 end = start + dir * dist + Vector2.down * UnityEngine.Random.Range(60f, 200f);   // rơi chút như có trọng lực

            float size = spark ? UnityEngine.Random.Range(shardSize.x, shardSize.y) * 0.9f
                               : UnityEngine.Random.Range(shardSize.x, shardSize.y);
            rt.sizeDelta = spark ? Vector2.one * size : new Vector2(size, size * UnityEngine.Random.Range(0.6f, 1.3f));
            rt.anchoredPosition = start;
            rt.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));
            rt.localScale = Vector3.one;
            img.color = spark ? sparkColor : Color.Lerp(shardColorA, shardColorB, UnityEngine.Random.value);
            img.gameObject.SetActive(true);
            rt.SetAsLastSibling();

            float t = shardTime * UnityEngine.Random.Range(0.75f, 1.1f);
            DOTween.Kill(rt);
            DOTween.Sequence().SetUpdate(true).SetLink(img.gameObject).SetTarget(rt)
                .Append(rt.DOAnchorPos(end, t).SetEase(Ease.OutCubic))
                .Join(rt.DOLocalRotate(new Vector3(0f, 0f, UnityEngine.Random.Range(-540f, 540f)), t, RotateMode.LocalAxisAdd).SetEase(Ease.OutQuad))
                .Join(rt.DOScale(spark ? 0f : 0.4f, t).SetEase(Ease.InQuad))
                .Join(img.DOFade(0f, t * 0.6f).SetDelay(t * 0.4f).SetEase(Ease.InQuad))
                .OnComplete(() => img.gameObject.SetActive(false));
        }
    }

    // ---------- Helpers ----------
    float GetProgress() => _mat != null ? _mat.GetFloat(ProgressId) : 0f;
    void SetProgress(float v) { if (_mat != null) _mat.SetFloat(ProgressId, v); }

    void SetFlash(float a)
    {
        if (flash == null) return;
        var c = flash.color; c.a = a; flash.color = c;
    }

    void Finish()
    {
        foreach (var p in _pieces) p.gameObject.SetActive(false);
        gameObject.SetActive(false);
        _playing = false;
        GameFlow.Instance.RequestResume();

        var done = _onDone;
        _onDone = null;
        done?.Invoke();
    }

    static void PlaySfx(string key)
    {
        if (!string.IsNullOrEmpty(key) && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(key);
    }
}
