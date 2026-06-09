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

    [Header("Ally house theo civ sở hữu (index 0..5)")]
    public List<GameObject> allyHouseObjects;

    private List<HouseData> _civs;
    private int _viewIndex;

    protected override void Init()
    {
        var holder = LobbyController.Instance.topCanvas;
        btnSetting.OnClicked(delegate { _ = SettingLobbyBox.Setup(holder, box => box.Show()); });
        btnPlay.OnClicked(delegate
        {
            string civId = _civs[_viewIndex].civId;          
            UseProfile.SelectedEnemyCiv.Value = civId;     
            FXManager.Instance.LoadSceneWithIrisWipe(SceneName.GAME_PLAY);
        });

        btnNext.OnClicked(delegate { ShowCiv(_viewIndex + 1); });
        btnPrev.OnClicked(delegate { ShowCiv(_viewIndex - 1); });

        _civs = DataRepo.Instance.unitDatabase.GetCivsByOrder();

        RefreshForCurrentCiv();

        this.RegisterListener(EventID.ON_CIV_CHANGED, OnCivChanged);
    }

    protected override void InitState()
    {
    }


    void OnCivChanged(object param)
    {
        RefreshForCurrentCiv();
    }
    void RefreshForCurrentCiv()
    {
        _viewIndex = IndexOfCiv(UseProfile.EnemyCiv.Value);
        RefreshAllyHouse();
        ShowCiv(_viewIndex);
    }
    void ShowCiv(int index)
    {
        int maxIndex = UnlockedCount() - 1;
        index = Mathf.Clamp(index, 0, maxIndex);
        _viewIndex = index;

        for (int i = 0; i < civObjects.Count; i++)
            civObjects[i].SetActive(i == index);

        RefreshNavButtons();
    }
    void RefreshAllyHouse()
    {
        int ownIndex = IndexOfCiv(UseProfile.CurrentCiv.Value);
        for (int i = 0; i < allyHouseObjects.Count; i++)
            allyHouseObjects[i].SetActive(i == ownIndex);
    }

    void RefreshNavButtons()
    {
        int unlocked = UnlockedCount();
        int maxIndex = unlocked - 1;

        bool showNav = unlocked > 1;
        btnNext.gameObject.SetActive(showNav && _viewIndex < maxIndex);
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
        return DataRepo.Instance.unitDatabase.GetCivOrder(UseProfile.EnemyCiv.Value);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        this.RemoveListener(EventID.ON_CIV_CHANGED, OnCivChanged);
    }
}