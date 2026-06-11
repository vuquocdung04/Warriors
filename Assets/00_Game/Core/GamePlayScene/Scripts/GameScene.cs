using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : StaffSingleton<GameScene>
{
    public Transform popupHolder;
    public Image darkPanel;

    [Header("Coin View")]
    public TextMeshProUGUI txtCoinCollect;

    [Header("Button")]
    public Button btnSetting;

    private int _coinCollected; 

    public override void Init()
    {
        // btnSetting.OnClicked(delegate
        // {
        //     _ = SettingGameBox.Setup(popupHolder, box => box.Show());
        // });

        _coinCollected = 0;
        txtCoinCollect.text = $"<sprite=0>{_coinCollected}";

        this.RegisterListener(EventID.CHANGE_COIN, OnCoinChanged);
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