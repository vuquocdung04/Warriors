using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Popup thắng (dựng theo PopupWinDrink bên DrinkPacking): banner + sao bật ra,
// từng dòng hiện lần lượt và đếm số lên: coin nhặt, gem nhặt, % bonus.
// Có bonus thì coin/gem đếm tiếp lên số đã cộng bonus và cộng phần thêm vào tài khoản. Xong mới mở nút Next.
public class WinBox : BaseBox<WinBox>
{
    [System.Serializable]
    public class WinRow
    {
        public CanvasGroup root;
        public TMP_Text value;
        public Transform icon;
    }

    public Button btnNext;

    [Header("Header")]
    public Transform banner;
    public Transform glow;
    public Transform[] decorStars;

    [Header("Rows")]
    public WinRow rowCoin;
    public WinRow rowGem;
    public WinRow rowBonus;

    [Header("Motion")]
    public float holderDelay = 0.2f;
    public float popDuration = 0.25f;
    public float decorStepDelay = 0.08f;
    public float rowDelay = 0.12f;
    public float countDuration = 0.5f;
    public float hitPunch = 0.15f;
    public float hitDuration = 0.2f;
    public float pulseInterval = 0.06f;

    private Sequence _sequence;
    private float _lastPulseTime;
    private bool _leaving;

    // % coin/gem cộng thêm sau trận — sau này lấy từ item/trang bị tăng thu nhập
    public static int GetBonusPercent() => 0;

    protected override void Init()
    {
        btnNext.OnClicked(OnClickNext);
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

        if (banner != null) _sequence.Append(banner.DOScale(1f, popDuration).SetEase(Ease.OutBack));
        InsertDecor(holderDelay + popDuration * 0.5f);

        AppendRow(rowCoin, 0, coin, "");
        AppendRow(rowGem, 0, gem, "");
        AppendRow(rowBonus, 0, bonus, "%", "+");

        // bonus > 0: coin + gem đếm tiếp từ số nhặt được lên số đã cộng bonus
        if (coinTotal > coin) AppendCount(rowCoin, coin, coinTotal, "");
        if (gemTotal > gem) AppendCount(rowGem, gem, gemTotal, "");

        _sequence.AppendCallback(() => btnNext.interactable = true);
        _sequence.Append(btnNext.transform.DOScale(1f, popDuration).SetEase(Ease.OutBack));
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

    void ResetState()
    {
        if (banner != null) banner.localScale = Vector3.zero;
        if (decorStars != null)
            foreach (var s in decorStars) if (s != null) { s.DOKill(); s.localScale = Vector3.zero; }

        if (glow != null)
        {
            glow.DOKill();
            glow.localScale = Vector3.one;
            glow.DOScale(1.12f, 1.2f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetLink(glow.gameObject);
        }

        ResetRow(rowCoin, "0");
        ResetRow(rowGem, "0");
        ResetRow(rowBonus, "+0%");

        btnNext.interactable = false;
        btnNext.transform.localScale = Vector3.zero;
    }

    static void ResetRow(WinRow row, string text)
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

    void InsertDecor(float start)
    {
        if (decorStars == null) return;
        for (int i = 0; i < decorStars.Length; i++)
        {
            var star = decorStars[i];
            if (star == null) continue;
            _sequence.Insert(start + i * decorStepDelay, star.DOScale(1f, popDuration).SetEase(Ease.OutBack));
        }
    }

    // Dòng hiện ra rồi đếm số lên
    void AppendRow(WinRow row, int from, int to, string suffix, string prefix = "")
    {
        if (row == null || row.root == null) return;

        _sequence.AppendInterval(rowDelay);
        _sequence.Append(row.root.DOFade(1f, popDuration * 0.6f));
        _sequence.Join(row.root.transform.DOScale(1f, popDuration).SetEase(Ease.OutBack));
        AppendCount(row, from, to, suffix, prefix);
    }

    // Đếm from -> to (icon nảy theo nhịp), xong thì giá trị nảy 1 cái
    void AppendCount(WinRow row, int from, int to, string suffix, string prefix = "")
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
            AudioManager.Instance.PlaySfx("Coins");
        });
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

    void OnClickNext()
    {
        if (_leaving) return;
        _leaving = true;
        btnNext.interactable = false;
        FXManager.Instance.LoadScene(SceneName.LOBBY_SCENE);
    }

    void Kill()
    {
        _sequence?.Kill();
        _sequence = null;
    }

    protected override void OnDestroy()
    {
        Kill();
        base.OnDestroy();
    }
}
