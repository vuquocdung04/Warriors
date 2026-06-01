using System.Collections.Generic;
using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;

public class LobbyBox : BaseBox<LobbyBox>
{
    [Header("Buttons")]
    public Button btnSetting;
    public Button btnPlay;
    public Button btnNext;
    public Button btnPrev;

    [Header("6 GameObject civ (index 0..5 theo order)")]
    public List<GameObject> civObjects;

    [Header("House images")]
    public Image enemyHouseImage;
    public Image allyHouseImage;
    public List<Sprite> houseSprites;

    private List<HouseData> _civs;
    private int _viewIndex;

    protected override void Init()
    {
        var holder = LobbyController.Instance.topCanvas;
        btnSetting.OnClicked(delegate { _ = SettingLobbyBox.Setup(holder, box => box.Show()); });
        btnPlay.OnClicked(delegate { FXManager.Instance.LoadSceneWithIrisWipe(SceneName.GAME_PLAY); });

        btnNext.OnClicked(delegate { ShowCiv(_viewIndex + 1); });
        btnPrev.OnClicked(delegate { ShowCiv(_viewIndex - 1); });

        _civs = DataRepo.Instance.unitDatabase.GetCivsByOrder();

        _viewIndex = IndexOfCiv(UseProfile.CurrentCiv.Value);
        RefreshAllyHouse();
        ShowCiv(_viewIndex);
    }

    protected override void InitState()
    {
    }

    void ShowCiv(int index)
    {
        index = Mathf.Clamp(index, 0, civObjects.Count - 1);
        _viewIndex = index;

        for (int i = 0; i < civObjects.Count; i++)
            civObjects[i].SetActive(i == index);

        if (enemyHouseImage != null && index < houseSprites.Count)
            enemyHouseImage.sprite = houseSprites[index];

        enemyHouseImage.FitToTargetWidth(130);

        RefreshNavButtons();
    }

    void RefreshAllyHouse()
    {
        int ownIndex = IndexOfCiv(UseProfile.CurrentCiv.Value);
        if (allyHouseImage != null && ownIndex < houseSprites.Count)
            allyHouseImage.sprite = houseSprites[ownIndex];

        allyHouseImage.FitToTargetWidth(130);
    }

    void RefreshNavButtons()
    {
        bool showNav = UnlockedCount() > 1;
        btnNext.gameObject.SetActive(showNav && _viewIndex < civObjects.Count - 1);
        btnPrev.gameObject.SetActive(showNav && _viewIndex > 0);
    }

    int IndexOfCiv(string civId)
    {
        for (int i = 0; i < _civs.Count; i++)
            if (_civs[i].civId == civId) return i;
        return 0;
    }

    int UnlockedCount()
    {
        return DataRepo.Instance.unitDatabase.GetCivOrder(UseProfile.CurrentCiv.Value);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}