using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NavButton : MonoBehaviour
{
    public ENavType navType;
    [Space(5)] [SerializeField] private Button btnMain;

    [SerializeField] RectTransform icon;

    [SerializeField] private Sprite sprUnSelected;
    [SerializeField] private Image imgMain;
    [SerializeField] private RectTransform rectMain;

    public void Init()
    {
        UIUtils.FitToTargetHeight(icon.GetComponent<Image>(), 130f);
    }

    public void HandleSelected(bool isSelected, Sprite sprSelected, Vector2 size)
    {
        rectMain.sizeDelta = size; // mọi nav cùng size
        imgMain.SetSprite(isSelected ? sprSelected : sprUnSelected);

        if (isSelected)
        {
            icon.DOScale(Vector3.one * 1.3f, 0.15f);
            icon.DOAnchorPosY(100f, 0.15f);
        }
        else
        {
            icon.DOScale(Vector3.one * 0.7f, 0.15f);
            icon.DOAnchorPosY(0f, 0.15f);
        }
    }

    public void SetupClick(System.Action callback)
    {
        btnMain.OnClicked(() => callback?.Invoke());
    }

    public void InitSetup()
    {
        rectMain = GetComponent<RectTransform>();
        btnMain = GetComponent<Button>();
        icon = transform.GetComponentInChildren<RectTransform>();
        imgMain = GetComponent<Image>();
    }
}

public enum ENavType
{
    nav0 = 0,
    nav1 = 1,
    nav2 = 2,
    nav3 = 3,
    nav4 = 4,
}