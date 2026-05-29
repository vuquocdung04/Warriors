using System.Collections.Generic;
using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;

public class LobbyBox : BaseBox<LobbyBox>
{

    [Header("Sprites")]
    [SerializeField] private Sprite mainHardSprite;
    [SerializeField] private Sprite lightHardSprite;

    [Header("Buttons")]
    public Button btnSetting;
    public Button btnPlay;


    protected override void Init()
    {
        var holder = LobbyController.Instance.topCanvas;
        btnSetting.OnClicked(delegate { _ = SettingLobbyBox.Setup(holder, box => box.Show()); });
        btnPlay.OnClicked(delegate { FXManager.Instance.LoadSceneWithIrisWipe(SceneName.GAME_PLAY); });
    }

    protected override void InitState()
    {
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}