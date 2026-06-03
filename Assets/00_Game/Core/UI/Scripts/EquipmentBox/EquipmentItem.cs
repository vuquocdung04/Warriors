using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentItem : MonoBehaviour
{
    public string id;

    [Header("State objects (mặc định tắt hết)")]
    public GameObject newObject;
    public GameObject equippedObject;
    public GameObject viewProgress;

    [Header("View progress")]
    public TMP_Text levelText;
    public TMP_Text cardText;
    public Image fillProgress;

    [Header("Button")]
    public Button button;

    private EquipmentData _data;
    private System.Action<EquipmentItem> _onClick;

    public EquipmentData Data => _data;

    public void Init(EquipmentData data, System.Action<EquipmentItem> onClick)
    {
        _data = data;
        id = data.id;
        _onClick = onClick;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onClick?.Invoke(this));
    }

    public void Refresh()
    {
        var state = EquipmentSave.Get(_data.id);

        if (levelText != null) levelText.text = state.level.ToString();

        bool maxed = state.level >= _data.levelMax;
        int need = EquipmentUpgrade.CardNeeded(_data, state.level);

        cardText.text = $"{state.card}/{need}";

        fillProgress.fillAmount = maxed ? 1f
            : (need > 0 ? Mathf.Clamp01((float)state.card / need) : 0f);
    }

    public void SetNew(bool on)
    {
        newObject.SetActive(on);
    }

    public void SetEquipped(bool on)
    {
        equippedObject.SetActive(on);
    }

    public void SetViewProgress(bool on)
    {
        viewProgress.SetActive(on);
    }
}