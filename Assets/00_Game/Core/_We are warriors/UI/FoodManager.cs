using DG.Tweening;
using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodManager : StaffSingleton<FoodManager>
{
    [Header("UI")]
    public TMP_Text foodText;          // số food hiện tại
    public Image fillProgress;         // fill tiến trình hồi miếng tiếp theo

    [Header("Hồi food")]
    public float refillRate = 0.2f;    // food/giây (0.2 = 5s/miếng)

    private int _food;
    private int _startFood;            // = foodCost lính đầu - 2 (GamePlayController truyền vào)
    private Tween _fillTween;

    public int Food => _food;

    // startFood do GamePlayController tính theo lính đầu tiên rồi truyền vào.
    public void Init(int startFood)
    {
        _startFood = startFood;
        Init();
    }

    public override void Init()
    {
        _food = _startFood;
        RefreshUI();
        StartRefill();
        this.RegisterListener(EventID.HOUSE_DESTROYED, OnBattleOver);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _fillTween?.Kill();
        this.RemoveListener(EventID.HOUSE_DESTROYED, OnBattleOver);
    }

    // Fill 0->1 lặp vô hạn = tiến trình hồi 1 food; mỗi vòng xong +1 food (thay cho Update).
    void StartRefill()
    {
        _fillTween?.Kill();
        if (fillProgress == null || refillRate <= 0f) return;

        float secondsPerFood = 1f / refillRate;
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
        RefreshUI();   // chỉ đổi số đếm; fill (tiến trình miếng kế) do tween tự chạy, không reset
    }

    void RefreshUI()
    {
        if (foodText != null) foodText.text = _food.ToString();
        this.PostEvent(EventID.FOOD_CHANGED, _food);   // card tự nghe để đổi màu
    }
}
