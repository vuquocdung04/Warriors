using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class NavController : MonoBehaviour
{
    public static NavController Instance { get; private set; }
    [SerializeField] private Sprite sprSelected;
    public List<NavButton> navButtons;
    private Vector2 navSize;
    private NavButton currentNavSelected;

    public void Init()
    {
        Instance = this;

        foreach (var nav in navButtons)
        {
            nav.Init();
            nav.SetupClick(delegate
            {
                if (nav != currentNavSelected)
                {
                    UpdateNavButtonState(nav);
                }
            });
        }
        InitAfterLayoutAsync().Forget();
    }

    private async UniTaskVoid InitAfterLayoutAsync()
    {
        await UniTask.WaitForEndOfFrame(this);
        InitNavButtonStateWith(ENavType.nav2);
    }

    private void InitSize()
    {
        int countNavBar = navButtons.Count;
        if (countNavBar == 0) return;

        float totalWidth = GetComponent<RectTransform>().rect.width;
        float height = 250;
        float width = totalWidth / countNavBar;
        navSize = new Vector2(width, height);
    }

    public void NavigateTo(ENavType type)
    {
        var target = navButtons.Find(n => n.navType == type);
        if (target == null || target == currentNavSelected) return;
        UpdateNavButtonState(target);
    }

    private void InitNavButtonStateWith(ENavType type)
    {
        InitSize();
        foreach (var t in navButtons)
        {
            bool isSelected = t.navType == type;
            if (isSelected) currentNavSelected = t;
            t.HandleSelected(isSelected, sprSelected, navSize);
        }
    }

    private void UpdateNavButtonState(NavButton navButton)
    {
        HandleScreenSliding(navButton); // tính hướng dựa trên currentNavSelected cũ

        foreach (var t in navButtons)
        {
            t.HandleSelected(false, sprSelected, navSize);
        }

        currentNavSelected = navButton;
        navButton.HandleSelected(true, sprSelected, navSize);
    }

    private void HandleScreenSliding(NavButton clicked)
    {
        bool clickedIsRight = clicked.transform.localPosition.x > currentNavSelected.transform.localPosition.x;

        var outAnim = clickedIsRight ? BoxAnimationFactory.SlideToLeft : BoxAnimationFactory.SlideToRight;
        var inAnim = clickedIsRight ? BoxAnimationFactory.SlideFromRight : BoxAnimationFactory.SlideFromLeft;

        ClosePrevBox(currentNavSelected.navType, outAnim);
        OpenCurrentBox(clicked.navType, inAnim);
    }

    private void OpenCurrentBox(ENavType type, IShowAnimation anim)
    {
        switch (type)
        {
            case ENavType.nav0:
                RankBox.Instance.Show(anim);
                // dungeon
                break;
            case ENavType.nav1:
                UpgradesBox.Instance.Show(anim);
                break;
            case ENavType.nav2:
                LobbyBox.Instance.Show(anim);
                break;
            case ENavType.nav3:
                // card icon
                break;
            case ENavType.nav4:
                ShopBox.Instance.Show(anim);
                break;
        }
    }

    private void ClosePrevBox(ENavType type, IShowAnimation anim)
    {
        switch (type)
        {
            case ENavType.nav0:
                if (RankBox.Instance != null) RankBox.Instance.Close(anim);

                break;
            case ENavType.nav1:
                if (UpgradesBox.Instance != null) UpgradesBox.Instance.Close(anim);
                break;
            case ENavType.nav2:
                if (LobbyBox.Instance != null) LobbyBox.Instance.Close(anim);
                break;
            case ENavType.nav3:
                // TODO: gắn box cho nav3
                break;
            case ENavType.nav4:
                if (ShopBox.Instance != null) ShopBox.Instance.Close(anim);
                // TODO: gắn box cho nav4
                break;
        }
    }

    [ContextMenu("Setup Nav button")]
    private void Setup()
    {
        navButtons.Clear();
        navButtons = GetComponentsInChildren<NavButton>().ToList();

        foreach (var t in navButtons)
        {
            t.InitSetup();
        }
    }
}