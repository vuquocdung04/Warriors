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
    [Header("Title")]
    public TextMeshProUGUI txtBattleTitle;
    [Header("Button")]
    public Button btnSetting;

    private int _coinCollected;

    public override void Init()
    {
        btnSetting.OnClicked(delegate
        {
            _ = SettingGameBox.Setup(popupHolder, box => box.Show());
        });

        _coinCollected = 0;
        txtCoinCollect.text = $"<sprite=0>{_coinCollected}";

        this.RegisterListener(EventID.CHANGE_COIN, OnCoinChanged);

        SetupCivMap();
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
        _ = txtCoinCollect.CountToWithIcon(_coinCollected, "<sprite=0> ", duration: 0.3f);
    }

    protected override void OnDestroy()
    {
        this.RemoveListener(EventID.CHANGE_COIN, OnCoinChanged);
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