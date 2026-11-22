using DG.Tweening;
using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodBar : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text foodText;
    public Image fillProgress;

    [Header("Continuous (food rate >= 1/s)")]
    [Tooltip("Material dùng shader UI/ScrollPattern: thay cho thanh fill khi hồi >= 1 food/giây")]
    public Material continuousMaterial;
    public const float CONTINUOUS_RATE = 1f;   // food/giây

    private float _refillRate;
    private int _food;
    private Tween _fillTween;
    private float _progress;   // tiến độ 0..1 của 1 food (chạy ngầm khi ở chế độ continuous)

    public int Food => _food;

    public void Init(int startFood)
    {
        _refillRate = UseProfile.FoodRate.Value;
        _food = startFood;
        RefreshUI();
        StartRefill();
        this.RegisterListener(EventID.HOUSE_DESTROYED, OnBattleOver);
    }

    void OnDestroy()
    {
        _fillTween?.Kill();
        this.RemoveListener(EventID.HOUSE_DESTROYED, OnBattleOver);
    }

    void StartRefill()
    {
        _fillTween?.Kill();
        if (fillProgress == null || _refillRate <= 0f) return;

        float secondsPerFood = 1f / _refillRate;

        // Hồi >= 1 food/giây: thanh fill chạy quá nhanh -> thay bằng pattern shine cuộn liên tục, phủ kín ô
        bool continuous = _refillRate >= CONTINUOUS_RATE && continuousMaterial != null;
        fillProgress.material = continuous ? continuousMaterial : null;
        fillProgress.fillAmount = continuous ? 1f : 0f;

        _progress = 0f;
        _fillTween = DOTween.To(() => _progress,
                                v =>
                                {
                                    _progress = v;
                                    if (!continuous) fillProgress.fillAmount = v;
                                },
                                1f, secondsPerFood)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart)
            .OnStepComplete(() => { _food++; RefreshUI(); })
            .SetLink(gameObject);
    }
    public void SetPause(bool paused)
    {
        if (paused) _fillTween.Pause();
        else _fillTween.Play();
    }
    void OnBattleOver(object _) => _fillTween?.Pause();

    public bool CanAfford(int cost) => _food >= cost;

    public void Spend(int cost)
    {
        _food = Mathf.Max(0, _food - cost);
        RefreshUI();
    }

    void RefreshUI()
    {
        _ = foodText.CountToWithIcon(_food, "<sprite=2>", duration: 0f);
        this.PostEvent(EventID.FOOD_CHANGED, _food);
    }
}