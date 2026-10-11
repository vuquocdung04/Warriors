using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WheelSpinBox : BaseBox<WheelSpinBox>
{
    const string COIN_ICON = "<sprite=0>";

    [Header("Data")]
    public WheelSpinDatabase database;

    [Header("Buttons")]
    public Button btnClose;
    public Button btnCloseByPanel;
    public Button btnSpin;

    [Header("Wheel")]
    public RectTransform mainRotation;
    public RectTransform arrow;
    public GameObject light1;
    public GameObject light2;
    public TextMeshProUGUI txtResult;

    [Header("Reward Items")]
    public WheelRewardItem itemPrefab;
    [Tooltip("Con của mainRotation, item sinh ra quay theo wheel")]
    public RectTransform itemRoot;
    [Tooltip("Khoảng cách từ tâm wheel tới item")]
    public float itemRadius = 320f;

    [Header("Spin")]
    public int minRounds = 4;
    public int maxRounds = 6;
    public float spinDuration = 4f;
    [Tooltip("Góc arrow bị đẩy lệch mỗi lần chạm vạch")]
    public float arrowKickAngle = 28f;
    public float lightBlinkIdle = 0.5f;
    public float lightBlinkSpin = 0.1f;

    private float SegmentAngle => 360f / database.rewards.Count;

    private bool _spinning;
    private int _lastTick;
    private Tween _spinTween;
    private Tween _arrowTween;
    private Tween _lightTween;
    private readonly List<WheelRewardItem> _items = new List<WheelRewardItem>();
    private int[] _amounts;

    protected override void Init()
    {
        btnClose.OnClicked(CloseBox);
        btnCloseByPanel.OnClicked(CloseBox);
        btnSpin.OnClicked(Spin);

        SpawnItems();
    }

    // Ô i nằm ở góc -i*seg tính từ đỉnh, theo chiều kim đồng hồ
    private void SpawnItems()
    {
        foreach (Transform c in itemRoot) Destroy(c.gameObject);
        _items.Clear();

        float seg = SegmentAngle;
        for (int i = 0; i < database.rewards.Count; i++)
        {
            float angle = -i * seg;
            float rad = (angle + 90f) * Mathf.Deg2Rad;

            var item = Instantiate(itemPrefab, itemRoot);
            var rt = (RectTransform)item.transform;
            rt.anchoredPosition = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * itemRadius;
            rt.localEulerAngles = new Vector3(0, 0, angle);
            _items.Add(item);
        }
        _amounts = new int[database.rewards.Count];
    }

    // Civ đổi thì coin thưởng đổi theo -> tính lại mỗi lần mở box
    private void RefreshAmounts()
    {
        float mult = CivCoinMultiplier();
        for (int i = 0; i < database.rewards.Count; i++)
        {
            var reward = database.rewards[i];
            float m = reward.currency == CurrencyType.Coin ? mult : 1f;
            _amounts[i] = (int)System.Math.Min(int.MaxValue, System.Math.Round((double)reward.amount * m));
            _items[i].Setup(reward, _amounts[i]);
        }
    }

    // amount trong database tính theo civ đầu tiên, nhân theo tỉ lệ coinPerKill civ đang sở hữu / civ đầu
    private static float CivCoinMultiplier()
    {
        var civs = DataRepo.Instance.unitDatabase.GetCivsByOrder();
        var economy = DataRepo.Instance.economyDatabase;
        string current = UseProfile.CurrentCiv.Value;

        int baseCoin = 0, currentCoin = 0;
        foreach (var civ in civs)
        {
            var config = economy.Get(civ.civId);
            int coin = config != null ? config.coinPerKill : 0;
            // civ thiếu data (coin = 0) thì giữ hệ số của civ trước đó
            if (coin > 0)
            {
                if (baseCoin == 0) baseCoin = coin;
                currentCoin = coin;
            }
            if (civ.civId == current) break;
        }
        return baseCoin > 0 ? (float)currentCoin / baseCoin : 1f;
    }

    protected override void InitState()
    {
        RefreshAmounts();
        _spinning = false;
        btnSpin.interactable = true;
        arrow.localEulerAngles = Vector3.zero;
        txtResult.text = "SPIN!";
        BlinkLights(lightBlinkIdle);
    }

    private void Spin()
    {
        if (_spinning || database.rewards.Count == 0) return;
        _spinning = true;
        btnSpin.interactable = false;
        txtResult.text = "...";
        BlinkLights(lightBlinkSpin);

        int index = database.RandomIndex();

        // Ô i nằm ở góc -i*seg (theo chiều kim đồng hồ) -> quay wheel +i*seg để đưa nó lên dưới mũi tên
        float seg = SegmentAngle;
        float offset = Random.Range(-seg * 0.35f, seg * 0.35f);
        float current = Mathf.Repeat(mainRotation.localEulerAngles.z, 360f);
        mainRotation.localEulerAngles = new Vector3(0, 0, current);

        int rounds = Random.Range(minRounds, maxRounds + 1);
        float target = index * seg + offset;
        float delta = Mathf.Repeat(target - current, 360f) + rounds * 360f;

        _lastTick = TickOf(current);
        _spinTween?.Kill();
        _spinTween = mainRotation
            .DORotate(new Vector3(0, 0, current + delta), spinDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuart)
            .OnUpdate(OnWheelUpdate)
            .OnComplete(() => OnSpinDone(index))
            .SetLink(gameObject);
    }

    // Mỗi lần wheel quay qua vạch chia ô thì gõ arrow lệch sang trái rồi bật về
    private void OnWheelUpdate()
    {
        int tick = TickOf(mainRotation.localEulerAngles.z);
        if (tick == _lastTick) return;
        _lastTick = tick;

        _arrowTween?.Kill();
        arrow.localEulerAngles = new Vector3(0, 0, -arrowKickAngle);
        _arrowTween = arrow.DOLocalRotate(Vector3.zero, 0.25f)
            .SetEase(Ease.OutBack, 3f)
            .SetLink(gameObject);
    }

    private int TickOf(float z) => Mathf.FloorToInt((Mathf.Repeat(z, 360f) + SegmentAngle * 0.5f) / SegmentAngle);

    private void OnSpinDone(int index)
    {
        _spinning = false;
        btnSpin.interactable = true;
        BlinkLights(lightBlinkIdle);

        var reward = database.rewards[index];
        int amount = _amounts[index];
        CurrencyManager.Instance.Add(reward.currency, amount);
        txtResult.text = "+" + FormatReward(reward.currency, amount);

        var t = _items[index].transform;
        t.DOKill(true);
        t.DOPunchScale(Vector3.one * 0.4f, 0.5f, 6).SetLink(gameObject);
    }

    private void BlinkLights(float interval)
    {
        _lightTween?.Kill();
        light1.SetActive(true);
        light2.SetActive(false);
        _lightTween = DOVirtual.DelayedCall(interval, () =>
            {
                light1.SetActive(!light1.activeSelf);
                light2.SetActive(!light1.activeSelf);
            }, false)
            .SetLoops(-1)
            .SetLink(gameObject);
    }

    private void CloseBox()
    {
        if (_spinning) return;
        _lightTween?.Kill();
        Close();
    }

    private static string FormatReward(CurrencyType currency, int amount)
    {
        string icon = currency == CurrencyType.Coin ? COIN_ICON : currency.ToString();
        return icon + NumberFormatter.Format(amount);
    }
}
