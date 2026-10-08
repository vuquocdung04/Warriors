using System.Collections.Generic;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : StaffSingleton<GameScene>
{
    public Transform popupHolder;
    public Image darkPanel;

    [Header("Map theo civ enemy (index 0..5 theo order)")]
    public List<GameObject> civMaps;
    [Header("Coin View")]
    public TextMeshProUGUI txtCoinCollect;
    public TextMeshProUGUI txtGemCollect;
    [Header("Title")]
    public TextMeshProUGUI txtBattleTitle;
    [Header("Button")]
    public Button btnSetting;
    [Space(5)]
    public GameObject blockRaycast;
    private int _coinCollected;
    public int CoinCollected => _coinCollected;
    private int _gemCollected;
    public int GemCollected => _gemCollected;

    const string COIN_ICON = "<sprite=0> ";
    const string GEM_ICON = "<sprite=52> ";

    public override void Init()
    {
        btnSetting.OnClicked(delegate
        {
            _ = SettingGameBox.Setup(popupHolder, box =>
            {
                box.Show();
                box.PostEvent(EventID.POPUP_OPENED);
            });
        });

        _coinCollected = 0;
        txtCoinCollect.text = COIN_ICON + _coinCollected;
        _gemCollected = 0;
        if (txtGemCollect != null) txtGemCollect.text = GEM_ICON + _gemCollected;

        this.RegisterListener(EventID.CHANGE_COIN, OnCoinChanged);
        this.RegisterListener(EventID.CHANGE_GEM, OnGemChanged);

        SetupCivMap();
    }
    public static void SetBlockRaycast(bool on)
    {
        if (Instance != null && Instance.blockRaycast != null)
            Instance.blockRaycast.SetActive(on);
    }
    void SetupCivMap()
    {
        string civId = UseProfile.SelectedEnemyCiv.Value;
        int order = DataRepo.Instance.unitDatabase.GetCivOrder(civId);   // 1-based
        int index = order - 1;

        for (int i = 0; i < civMaps.Count; i++)
            civMaps[i].SetActive(i == index);

        if (txtBattleTitle != null)
            txtBattleTitle.text = $"Battle {order}";
    }
    void OnCoinChanged(object param)
    {
        int amount = param is int a ? a : 0;
        _coinCollected += amount;
        _ = txtCoinCollect.CountToWithIcon(_coinCollected, COIN_ICON, duration: 0.3f);
    }

    // chỉ cộng khi event mang số lượng nhặt được (drop trong trận); event không kèm số thì bỏ qua
    void OnGemChanged(object param)
    {
        if (!(param is int amount) || txtGemCollect == null) return;
        _gemCollected += amount;
        _ = txtGemCollect.CountToWithIcon(_gemCollected, GEM_ICON, duration: 0.3f);
    }

    protected override void OnDestroy()
    {
        this.RemoveListener(EventID.CHANGE_COIN, OnCoinChanged);
        this.RemoveListener(EventID.CHANGE_GEM, OnGemChanged);
    }

    public static void EnableDarkPanel(bool state)
    {
        Instance.darkPanel.DOKill();
        if (state)
        {
            Instance.darkPanel.gameObject.SetActive(true);
            Color color = Instance.darkPanel.color;
            color.a = 0f;
            Instance.darkPanel.color = color;
            Instance.darkPanel.DOFade(0.9f, 0.15f).SetUpdate(true);
        }
        else Instance.darkPanel.gameObject.SetActive(false);
    }

    public static Transform GetPopupHolder() => Instance.popupHolder;
}