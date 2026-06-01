using UnityEngine;
using UnityEngine.UI;

public class UnitCard : MonoBehaviour
{
    public Button button;
    public int foodCost { get; private set; }   // = foodCost của unit

    private int _index;
    private System.Action<int> _onClick;

    public void Setup(int index, UnitData data, System.Action<int> onClick)
    {
        _index = index;
        foodCost = data.foodCost;
        _onClick = onClick;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onClick?.Invoke(_index));
    }

    public void SetInteractable(bool on)
    {
        if (button != null) button.interactable = on;
    }
}