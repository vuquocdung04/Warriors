using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EventDispatcher;

public class EvolutionBar : MonoBehaviour
{
    public AudioClip evolveSFX;

    [Header("Tabs")]
    public GameObject inTimelineTab;
    public GameObject maxTimelineTab;

    [Header("InTimeline - images")]
    public Image currentCivImage;
    public Image nextCivImage;
    public List<Sprite> civSprites;

    [Header("InTimeline - buttons")]
    public Button btnEvolveFree;
    public Button btnEvolve;
    public Image btnEvolveImage;
    public TMP_Text costText;
    public TMP_Text desText;

    [Header("InTimeline - tên age")]
    public TMP_Text currentAgeText;
    public TMP_Text nextAgeText;

    [Header("Màu nút Evolve")]
    public Color enoughColor = Color.white;
    public Color notEnoughColor = Color.gray;
    [Header("Xem timeline")]
    public Button btnTimeline;

    [Header("MaxTimeline")]
    public Button btnTravel;
    public GameObject winBattleObject;
    public TMP_Text winBattleText;
    public GameObject travelObject;
    private List<HouseData> _civs;

    public void Init()
    {
        _civs = DataRepo.Instance.unitDatabase.GetCivsByOrder();

        btnEvolveFree.OnClicked(OnEvolveFree,evolveSFX);
        btnEvolve.OnClicked(OnEvolve,evolveSFX);
        btnTravel.OnClicked(OnTravel);
        btnTimeline.OnClicked(OnShowTimeline);

        this.RegisterListener(EventID.ON_CIV_CHANGED, OnCivChanged);
        Refresh();
    }
    void OnShowTimeline()
    {
        var holder = LobbyController.Instance.topCanvas;
        _ = AgesTimelineBox.Setup(holder, box => box.ShowStatic());
    }
    void OnCivChanged(object param)
    {
        Refresh();
    }
    void Refresh()
    {
        int curOrder = DataRepo.Instance.unitDatabase.GetCivOrder(UseProfile.CurrentCiv.Value);
        int maxOrder = _civs.Count;

        bool isMax = curOrder >= maxOrder;
        inTimelineTab.SetActive(!isMax);
        maxTimelineTab.SetActive(isMax);

        if (isMax) RefreshMax(maxOrder);
        else RefreshInTimeline(curOrder);
    }

    void RefreshInTimeline(int curOrder)
    {
        int curIndex = curOrder - 1;
        int nextIndex = curIndex + 1;

        HouseData current = _civs[curIndex];
        HouseData next = _civs[nextIndex];

        if (curIndex < civSprites.Count)
            currentCivImage.sprite = civSprites[curIndex];
        if (nextIndex < civSprites.Count)
            nextCivImage.sprite = civSprites[nextIndex];

        currentAgeText.text = current.civName;
        nextAgeText.text = next.civName;

        int cost = next.unlockCost;

        int enemyOrder = DataRepo.Instance.unitDatabase.GetCivOrder(UseProfile.EnemyCiv.Value);
        bool canFree = next.unlockCost == 0 || enemyOrder > next.order;

        btnEvolveFree.gameObject.SetActive(canFree);
        btnEvolve.gameObject.SetActive(!canFree);

        _ = costText.CountToWithIcon(cost, "<sprite=0> ", duration: 0f);
        desText.text = $"Or win battle {next.civName}";
        desText.gameObject.SetActive(!canFree);

        bool enough = UseProfile.Coin.Value >= cost;
        btnEvolveImage.color = enough ? enoughColor : notEnoughColor;
        costText.color = enough ? Color.white : Color.red;
        btnEvolve.interactable = enough;
    }
    void RefreshMax(int maxOrder)
    {
        bool won = UseProfile.WonFinalCiv.Value;

        winBattleText.text = $"Win Battle {maxOrder}";
        winBattleObject.SetActive(!won);
        travelObject.SetActive(won);
        btnTravel.interactable = won;
    }

    void OnEvolveFree() => EvolveTo(NextCiv());

    void OnEvolve()
    {
        var next = NextCiv();
        if (next == null) return;
        if (UseProfile.Coin.Value < next.unlockCost) return;

        UseProfile.Coin.Value -= next.unlockCost;
        EvolveTo(next);
    }

    void EvolveTo(HouseData next)
    {
        if (next == null) return;

        UseProfile.CurrentCiv.Value = next.civId;
        UseProfile.Unit2Unlock.Value = false;
        UseProfile.Unit3Unlock.Value = false;
        UseProfile.Coin.Value = 0;
        UseProfile.FoodRate.Value = 0.18f;

        var holder = LobbyController.Instance.topCanvas;
        _ = AgesTimelineBox.Setup(holder, box => box.ShowAnimated());
    }
    void OnTravel() => Debug.Log("[Evolution] Travel clicked");

    HouseData NextCiv()
    {
        int curOrder = DataRepo.Instance.unitDatabase.GetCivOrder(UseProfile.CurrentCiv.Value);
        return GetCivByOrder(curOrder + 1);
    }

    HouseData GetCivByOrder(int order)
    {
        foreach (var c in _civs)
            if (c.order == order) return c;
        return null;
    }

    void OnDestroy()
    {
        this.RemoveListener(EventID.ON_CIV_CHANGED, OnCivChanged);
    }
}