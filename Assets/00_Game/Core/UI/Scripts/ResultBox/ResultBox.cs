using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Popup kết quả trận (dựng theo PopupWinDrink bên DrinkPacking). Thắng hay thua đều giữ coin/gem nhặt được.
// Nhịp: banner bật -> từng dòng hiện, đếm số lên, icon coin/gem bay xuống dòng Total -> Total chốt
// -> (thắng + mở house mới) thẻ unlock đập xuống
// -> nút bật ra rồi "thở". Mọi thứ diễn ra một nhịp một lúc, chưa xong thì chưa cho bấm.
// Thắng: Home + Replay (chơi lại civ vừa đánh để farm). Thua: chỉ Home.
public class ResultBox : BaseBox<ResultBox>
{
    [System.Serializable]
    public class ResultRow
    {
        public CanvasGroup root;
        public TMP_Text value;
        public Transform icon;
    }

    [FormerlySerializedAs("btnNext")] public Button btnHome;
    public Button btnReplay;
    [Tooltip("Wrapper của các nút: pop + thở ở đây, ButtonScalePress giữ scale riêng của từng nút")]
    public Transform nextRoot;
    [Tooltip("Vị trí Y của nút khi có / không có thẻ unlock (không có thì nút lên sát panel)")]
    public float nextYWithUnlock = -855f;
    public float nextYNoUnlock = -610f;

    [Header("Header")]
    public Image banner;
    public TMP_Text txtTitle;
    public Transform glow;
    public Transform[] decorStars;
    public string titleWin = "VICTORY!";
    public string titleLose = "DEFEAT";
    public Color bannerWin = new Color(1f, 0.78f, 0.22f, 1f);
    public Color bannerLose = new Color(0.55f, 0.58f, 0.68f, 1f);

    [Header("Rows")]
    public ResultRow rowCoin;
    public ResultRow rowGem;
    public ResultRow rowBonus;

    [Header("Unlock")]
    public CanvasGroup unlockCard;
    public Image unlockIcon;
    public TMP_Text unlockName;
    public Transform unlockGlow;
    public float unlockDelay = 0.2f;
    public float slamScale = 2.2f;
    public float slamAngle = -12f;
    public float slamDuration = 0.18f;
    public float joltDistance = 18f;

    [Header("Motion")]
    public float holderDelay = 0.2f;
    public float popDuration = 0.25f;
    public float decorStepDelay = 0.08f;
    public float rowDelay = 0.12f;
    public float countDuration = 0.5f;
    public float hitPunch = 0.15f;
    public float hitDuration = 0.2f;
    public float pulseInterval = 0.06f;
    public float buttonBreath = 1.05f;
    public float starTwinkle = 12f;

    [Header("Total")]
    public CanvasGroup totalRoot;
    public TMP_Text totalCoin;
    public TMP_Text totalGem;
    public Transform totalCoinIcon;
    public Transform totalGemIcon;
    public float totalCountScale = 1.2f;
    public float totalCountDuration = 0.6f;

    [Header("Fly")]
    [Tooltip("Số icon bay tối đa mỗi dòng (ít hơn nếu nhặt ít)")]
    public int flyMax = 6;
    public float flySize = 80f;
    public float flySpread = 30f;
    public float flyDuration = 0.55f;
    public float flyStep = 0.06f;
    public float flyHook = 160f;
    [Tooltip("Dòng sau hiện lên khi icon dòng trước còn đang bay (chồng nhịp cho liền mạch)")]
    public float rowNextDelay = 0.3f;

    [Header("Sound")]
    public string soundCount = "Coins";
    public string soundSlam = "punch";
    public string soundUnlock = "Evolve";

    private readonly System.Collections.Generic.List<GameObject> _flies = new();
    private float _flightEnd;
    private Sequence _sequence;
    private Tween _breathe;
    private Tween _glowSpin;
    private float _lastPulseTime;
    private float _lastTickTime;
    private bool _leaving;
    private bool _win;
    private HouseData _unlocked;

    // % coin/gem cộng thêm sau trận — sau này lấy từ item/trang bị tăng thu nhập
    public static int GetBonusPercent() => 0;

    // Gọi trước Show(): unlocked = house vừa mở nhờ trận thắng này (null nếu không mở gì)
    public void SetResult(bool win, HouseData unlocked)
    {
        _win = win;
        _unlocked = win ? unlocked : null;
    }

    protected override void Init()
    {
        btnHome.OnClicked(() => Leave(SceneName.LOBBY_SCENE));
        // SelectedEnemyCiv vẫn giữ civ vừa đánh -> load lại GamePlay là chơi lại đúng màn đó
        if (btnReplay != null) btnReplay.OnClicked(() => Leave(SceneName.GAME_PLAY));
    }

    protected override void InitState()
    {
        _leaving = false;
        Play();
    }

    void Play()
    {
        Kill();
        ResetState();

        int coin = GameScene.Instance != null ? GameScene.Instance.CoinCollected : 0;
        int gem = GameScene.Instance != null ? GameScene.Instance.GemCollected : 0;
        int bonus = GetBonusPercent();
        int coinTotal = ApplyBonus(coin, bonus);
        int gemTotal = ApplyBonus(gem, bonus);
        GrantBonus(coinTotal - coin, gemTotal - gem);

        _sequence = DOTween.Sequence().SetTarget(this).SetUpdate(true).SetLink(gameObject);
        _sequence.AppendInterval(holderDelay);

        if (banner != null) _sequence.Append(banner.transform.DOScale(1f, popDuration).SetEase(Ease.OutBack, 2f));
        if (txtTitle != null) _sequence.Append(txtTitle.transform.DOPunchScale(Vector3.one * 0.18f, 0.3f, 6, 0.5f));
        if (_win)
        {
            InsertDecor(holderDelay + popDuration * 0.5f);
            _sequence.InsertCallback(holderDelay + popDuration * 0.5f + popDuration + decorStepDelay * Count(decorStars), StartTwinkle);
        }

        // Dòng Total hiện sẵn (0) để có chỗ cho icon bay về
        if (totalRoot != null)
        {
            _sequence.Append(totalRoot.DOFade(1f, popDuration * 0.6f));
            _sequence.Join(totalRoot.transform.DOScale(1f, popDuration).SetEase(Ease.OutBack, 2f));
        }

        // Mỗi dòng đếm xong thì icon bay xuống Total, Total tăng theo từng icon chạm tới
        AppendRow(rowCoin, 0, coin, "", "+");
        AppendFly(rowCoin, totalCoin, totalCoinIcon, coin);
        AppendRow(rowGem, 0, gem, "", "+");
        AppendFly(rowGem, totalGem, totalGemIcon, gem);
        AppendPendingFlight();

        // bonus > 0: Total phồng lên và đếm tiếp lên số đã cộng bonus
        AppendRow(rowBonus, 0, bonus, "%", "+");
        if (coinTotal > coin || gemTotal > gem) AppendTotalCount(coin, coinTotal, gem, gemTotal);
        _sequence.AppendCallback(() => CelebrateTotal(coinTotal, gemTotal));
        _sequence.AppendInterval(0.2f);

        if (_unlocked != null) AppendUnlock();

        _sequence.AppendCallback(ShowNext);
    }

    static int ApplyBonus(int value, int percent) =>
        value <= 0 || percent <= 0 ? value : Mathf.RoundToInt(value * (1f + percent / 100f));

    // coin/gem nhặt trong trận đã cộng vào tài khoản lúc rơi, ở đây chỉ cộng phần bonus
    static void GrantBonus(int coin, int gem)
    {
        if (CurrencyManager.Instance == null) return;
        if (coin > 0) CurrencyManager.Instance.Add(CurrencyType.Coin, coin);
        if (gem > 0) CurrencyManager.Instance.Add(CurrencyType.Gem, gem);
    }

    static int Count(Transform[] list) => list != null ? list.Length : 0;

    void ResetState()
    {
        if (banner != null)
        {
            banner.color = _win ? bannerWin : bannerLose;
            banner.transform.localScale = Vector3.zero;
        }
        if (txtTitle != null)
        {
            txtTitle.text = _win ? titleWin : titleLose;
            txtTitle.transform.localScale = Vector3.one;
        }

        if (decorStars != null)
            foreach (var s in decorStars)
            {
                if (s == null) continue;
                s.DOKill();
                s.localScale = Vector3.zero;
                s.localRotation = Quaternion.identity;
                s.gameObject.SetActive(_win);
            }

        if (glow != null)
        {
            glow.DOKill();
            glow.gameObject.SetActive(_win);
            glow.localScale = Vector3.one;
            if (_win)
                glow.DOScale(1.12f, 1.2f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetLink(glow.gameObject);
        }

        ResetRow(rowCoin, "+0");
        ResetRow(rowGem, "+0");
        ResetRow(rowBonus, "+0%");
        ResetTotal();
        ResetUnlock();
        ClearFlies();
        _flightEnd = 0f;

        SetButtons(false);
        if (btnReplay != null) btnReplay.gameObject.SetActive(_win);
        if (nextRoot != null)
        {
            nextRoot.DOKill();
            nextRoot.localScale = Vector3.zero;
            if (nextRoot is RectTransform rect)
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, _unlocked != null ? nextYWithUnlock : nextYNoUnlock);
        }
    }

    static void ResetRow(ResultRow row, string text)
    {
        if (row == null || row.root == null) return;
        row.root.alpha = 0f;
        row.root.transform.localScale = Vector3.one * 0.8f;
        if (row.value != null)
        {
            row.value.transform.DOKill();
            row.value.text = text;
            row.value.transform.localScale = Vector3.one;
        }
    }

    void ResetTotal()
    {
        if (totalRoot != null)
        {
            totalRoot.DOKill();
            totalRoot.transform.DOKill();
            totalRoot.alpha = 0f;
            totalRoot.transform.localScale = Vector3.one * 0.8f;
        }
        foreach (var t in new[] { totalCoin, totalGem })
        {
            if (t == null) continue;
            t.transform.DOKill();
            t.transform.localScale = Vector3.one;
            t.text = "0";
        }
    }

    // Đếm dòng xong -> bắn icon bay xuống Total. Chỉ chờ rowNextDelay rồi cho dòng sau hiện,
    // mốc icon cuối chạm đích lưu ở _flightEnd, chờ tới đó rồi Total mới "chốt"
    void AppendFly(ResultRow row, TMP_Text total, Transform target, int value)
    {
        if (total == null) return;

        var source = row != null && row.icon != null ? row.icon.GetComponent<Image>() : null;
        int count = source != null && target != null ? Mathf.Clamp(value, 0, flyMax) : 0;
        if (count <= 0)
        {
            _sequence.AppendCallback(() => total.text = value.ToString());
            return;
        }

        float flight = (count - 1) * flyStep + 0.15f + flyDuration;
        _flightEnd = Mathf.Max(_flightEnd, _sequence.Duration(false) + flight);
        _sequence.AppendCallback(() => SpawnFly(source, target, count, arrived =>
        {
            total.text = (value * arrived / count).ToString();
            Hit(total.transform);
            Hit(target);
            Tick();
        }));
        _sequence.AppendInterval(Mathf.Min(flight, rowNextDelay));
    }

    // Chờ icon cuối cùng chạm Total
    void AppendPendingFlight()
    {
        float wait = _flightEnd - _sequence.Duration(false);
        if (wait > 0f) _sequence.AppendInterval(wait);
        _flightEnd = 0f;
    }

    // Icon bật ra quanh dòng rồi bay vòng cung (InQuad: nhanh dần, "hút" vào đích)
    void SpawnFly(Image source, Transform target, int count, System.Action<int> onArrive)
    {
        var layer = mainPanel != null ? (Transform)mainPanel : transform;
        Vector3 from = layer.InverseTransformPoint(source.transform.position);
        Vector3 to = layer.InverseTransformPoint(target.position);
        int arrived = 0;

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("fly", typeof(RectTransform), typeof(Image));
            go.layer = layer.gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(layer, false);
            rt.SetAsLastSibling();
            rt.sizeDelta = Vector2.one * flySize;
            var img = go.GetComponent<Image>();
            img.sprite = source.sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            _flies.Add(go);

            Vector3 start = from + (Vector3)(Random.insideUnitCircle * flySpread);
            Vector3 hook = Vector3.Lerp(start, to, 0.45f) + new Vector3(Random.Range(-1f, 1f) * flyHook, flyHook * 0.4f, 0f);
            rt.localPosition = start;
            rt.localScale = Vector3.one * 0.4f;

            DOTween.Sequence().SetUpdate(true).SetLink(go)
                .AppendInterval(i * flyStep)
                .Append(rt.DOScale(1f, 0.15f).SetEase(Ease.OutBack))
                .Append(rt.DOLocalPath(new[] { hook, to }, flyDuration, PathType.CatmullRom).SetEase(Ease.InQuad))
                .Join(rt.DOScale(0.7f, flyDuration).SetEase(Ease.InQuad))
                .OnComplete(() =>
                {
                    _flies.Remove(go);
                    Destroy(go);
                    onArrive(++arrived);
                });
        }
    }

    // Total phồng lên, đếm tiếp phần bonus, rồi thu về
    void AppendTotalCount(int coinFrom, int coinTo, int gemFrom, int gemTo)
    {
        if (totalRoot == null) return;

        var root = totalRoot.transform;
        float t = 0f;
        _sequence.AppendCallback(() => root.DOKill(true));
        _sequence.Append(root.DOScale(totalCountScale, 0.12f).SetEase(Ease.OutQuad));
        _sequence.Join(DOTween.To(() => t, x =>
        {
            t = x;
            if (totalCoin != null) totalCoin.text = Mathf.RoundToInt(Mathf.Lerp(coinFrom, coinTo, x)).ToString();
            if (totalGem != null) totalGem.text = Mathf.RoundToInt(Mathf.Lerp(gemFrom, gemTo, x)).ToString();
            Tick();
        }, 1f, totalCountDuration).SetEase(Ease.OutCubic));
        _sequence.Append(root.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
    }

    void CelebrateTotal(int coin, int gem)
    {
        if (totalCoin != null) totalCoin.text = coin.ToString();
        if (totalGem != null) totalGem.text = gem.ToString();
        if (coin + gem > 0) PlaySfx(soundCount);
        if (totalRoot == null) return;
        totalRoot.transform.DOKill(true);
        totalRoot.transform.DOPunchScale(Vector3.one * 0.18f, 0.45f, 6, 0.5f).SetUpdate(true).SetLink(totalRoot.gameObject);
    }

    // Tiếng tick khi đếm/khi icon chạm, giới hạn nhịp để không bị rè
    void Tick()
    {
        if (Time.unscaledTime - _lastTickTime < pulseInterval) return;
        _lastTickTime = Time.unscaledTime;
        PlaySfx(soundCount);
    }

    void ClearFlies()
    {
        foreach (var go in _flies)
            if (go != null) Destroy(go);
        _flies.Clear();
    }

    void ResetUnlock()
    {
        if (unlockCard == null) return;

        unlockCard.DOKill();
        unlockCard.transform.DOKill();
        unlockCard.gameObject.SetActive(_unlocked != null);
        unlockCard.alpha = 0f;
        unlockCard.transform.localScale = Vector3.one;
        unlockCard.transform.localRotation = Quaternion.identity;

        if (unlockGlow != null)
        {
            unlockGlow.DOKill();
            unlockGlow.localScale = Vector3.zero;
            unlockGlow.localRotation = Quaternion.identity;
        }

        if (_unlocked == null) return;

        if (unlockName != null) unlockName.text = _unlocked.civName;
        if (unlockIcon != null)
        {
            var sprite = DataRepo.Instance != null ? DataRepo.Instance.unitDatabase.GetCivIcon(_unlocked.civId) : null;
            if (sprite != null) unlockIcon.sprite = sprite;
            unlockIcon.transform.localScale = Vector3.one;
        }
    }

    void InsertDecor(float start)
    {
        if (decorStars == null) return;
        for (int i = 0; i < decorStars.Length; i++)
        {
            var star = decorStars[i];
            if (star == null) continue;
            _sequence.Insert(start + i * decorStepDelay, star.DOScale(1f, popDuration).SetEase(Ease.OutBack, 2f));
        }
    }

    // Sao lắc + phồng lệch nhịp nhau cho màn hình không bị "chết" khi chờ bấm
    void StartTwinkle()
    {
        if (decorStars == null) return;
        for (int i = 0; i < decorStars.Length; i++)
        {
            var star = decorStars[i];
            if (star == null) continue;
            float duration = 0.55f + i * 0.13f;
            float dir = i % 2 == 0 ? 1f : -1f;
            star.localRotation = Quaternion.Euler(0f, 0f, -starTwinkle * dir);
            star.DOLocalRotate(new Vector3(0f, 0f, starTwinkle * dir), duration)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetLink(star.gameObject);
            star.DOScale(1.15f, duration * 0.8f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetLink(star.gameObject);
        }
    }

    // Dòng hiện ra rồi đếm số lên
    void AppendRow(ResultRow row, int from, int to, string suffix, string prefix = "")
    {
        if (row == null || row.root == null) return;

        _sequence.AppendInterval(rowDelay);
        _sequence.Append(row.root.DOFade(1f, popDuration * 0.6f));
        _sequence.Join(row.root.transform.DOScale(1f, popDuration).SetEase(Ease.OutBack, 2f));
        AppendCount(row, from, to, suffix, prefix);
    }

    // Đếm from -> to (icon nảy theo nhịp), xong thì giá trị nảy 1 cái
    void AppendCount(ResultRow row, int from, int to, string suffix, string prefix = "")
    {
        if (row == null || row.value == null) return;

        var text = row.value;
        int shown = from;
        _sequence.Append(DOTween.To(() => shown, x =>
        {
            if (x == shown) return;
            shown = x;
            text.text = prefix + x + suffix;
            Pulse(row.icon);
        }, to, to != from ? countDuration : 0f).SetEase(Ease.OutCubic));
        _sequence.AppendCallback(() =>
        {
            text.text = prefix + to + suffix;
            Hit(text.transform);
            if (to > 0) PlaySfx(soundCount);
        });
    }

    // Thẻ house mới: phóng to + nghiêng từ trên "đập" xuống (InQuad = tăng tốc tới lúc chạm),
    // chạm thì cả popup giật xuống, rung máy, hào quang bung ra sau thẻ, thẻ bẹp-nảy như con dấu
    void AppendUnlock()
    {
        if (unlockCard == null) return;

        var card = unlockCard.transform;
        _sequence.AppendInterval(unlockDelay);
        _sequence.AppendCallback(() =>
        {
            card.localScale = Vector3.one * slamScale;
            card.localRotation = Quaternion.Euler(0f, 0f, slamAngle);
        });
        _sequence.Append(card.DOScale(1f, slamDuration).SetEase(Ease.InQuad));
        _sequence.Join(card.DOLocalRotate(Vector3.zero, slamDuration).SetEase(Ease.InQuad));
        _sequence.Join(unlockCard.DOFade(1f, slamDuration * 0.5f));
        _sequence.AppendCallback(UnlockImpact);
        _sequence.Append(card.DOPunchScale(new Vector3(0.12f, -0.1f, 0f), 0.3f, 6, 0.6f));
        _sequence.AppendInterval(0.15f);
    }

    void UnlockImpact()
    {
        PlaySfx(soundSlam);
        PlaySfx(soundUnlock);
        RDG.Vibration.Vibrate(80);

        if (mainPanel != null)
        {
            mainPanel.DOKill(true);
            mainPanel.DOPunchAnchorPos(new Vector2(0f, -joltDistance), 0.28f, 10, 0.6f).SetUpdate(true).SetLink(mainPanel.gameObject);
        }

        if (unlockIcon != null)
            unlockIcon.transform.DOPunchScale(Vector3.one * 0.3f, 0.45f, 7, 0.6f).SetUpdate(true).SetLink(unlockIcon.gameObject);

        if (unlockGlow != null)
        {
            unlockGlow.DOScale(1f, 0.4f).SetEase(Ease.OutBack, 2f).SetUpdate(true).SetLink(unlockGlow.gameObject);
            _glowSpin = unlockGlow.DOLocalRotate(new Vector3(0f, 0f, -360f), 10f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear).SetLoops(-1).SetUpdate(true).SetLink(unlockGlow.gameObject);
        }
    }

    // Nút bật ra rồi "thở" để mời bấm; lúc này mới cho tương tác
    void ShowNext()
    {
        SetButtons(true);
        if (nextRoot == null) return;

        nextRoot.DOKill();
        nextRoot.DOScale(1f, 0.35f).SetEase(Ease.OutBack, 2f).SetUpdate(true).SetLink(nextRoot.gameObject)
            .OnComplete(() => _breathe = nextRoot.DOScale(buttonBreath, 0.7f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetLink(nextRoot.gameObject));
    }

    void Pulse(Transform icon)
    {
        if (icon == null || Time.unscaledTime - _lastPulseTime < pulseInterval) return;
        _lastPulseTime = Time.unscaledTime;
        Hit(icon);
    }

    void Hit(Transform target)
    {
        if (target == null) return;
        target.DOKill(true);
        target.DOPunchScale(Vector3.one * hitPunch, hitDuration, 1, 0f).SetUpdate(true).SetLink(target.gameObject);
    }

    static void PlaySfx(string key)
    {
        if (!string.IsNullOrEmpty(key) && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(key);
    }

    void SetButtons(bool on)
    {
        btnHome.interactable = on;
        if (btnReplay != null) btnReplay.interactable = on;
    }

    void Leave(string scene)
    {
        if (_leaving) return;
        _leaving = true;
        SetButtons(false);
        FXManager.Instance.LoadScene(scene);
    }

    void Kill()
    {
        _sequence?.Kill();
        _sequence = null;
        _breathe?.Kill();
        _breathe = null;
        _glowSpin?.Kill();
        _glowSpin = null;
        ClearFlies();
    }

    protected override void OnDestroy()
    {
        Kill();
        base.OnDestroy();
    }
}
