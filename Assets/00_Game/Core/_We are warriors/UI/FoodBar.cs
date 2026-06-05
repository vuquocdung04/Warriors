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

    private float _refillRate;
    private int _food;
    private Tween _fillTween;

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
        fillProgress.fillAmount = 0f;
        _fillTween = DOTween.To(() => fillProgress.fillAmount,
                                v => fillProgress.fillAmount = v,
                                1f, secondsPerFood)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart)
            .OnStepComplete(() => { _food++; RefreshUI(); })
            .SetLink(gameObject);
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
        if (foodText != null) foodText.text = _food.ToString();
        this.PostEvent(EventID.FOOD_CHANGED, _food);
    }
}