using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentNavButton : MonoBehaviour
{
    public TMP_Text txtName;

    private Button _button;
    private Image _image;
    private RectTransform _rect;

    public void Init()
    {
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();
        _rect = transform as RectTransform;
    }

    public void SetupClick(System.Action onClick)
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => onClick?.Invoke());
    }

    public void SetSelected(bool selected, Color selectedColor, Color normalColor,
                            float selectedHeight, float normalHeight)
    {
        _image.color = selected ? selectedColor : normalColor;

        var size = _rect.sizeDelta;
        size.y = selected ? selectedHeight : normalHeight;
        _rect.sizeDelta = size;
    }
}