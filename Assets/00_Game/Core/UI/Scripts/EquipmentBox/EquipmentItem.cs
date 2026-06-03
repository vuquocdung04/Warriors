using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentItem : MonoBehaviour
{
    public string id;
    [Header("Icon")]
    public Image imageIcon;
    public Image bgCard;
    [Header("State objects (mặc định tắt hết)")]
    public GameObject newObject;
    public GameObject equippedObject;
    public GameObject viewProgress;

    [Header("View progress")]
    public TMP_Text levelText;
    public TMP_Text cardText;
    public Image fillProgress;
    public GameObject arrow;
    [Header("Button")]
    public Button button;

    private EquipmentData _data;
    private System.Action<EquipmentItem> _onClick;

    public EquipmentData Data => _data;
    private EquipType _type;
    public void Init(EquipmentData data, EquipType type, Sprite icon, System.Action<EquipmentItem> onClick)
    {
        _data = data;
        _type = type;
        id = data.id;
        _onClick = onClick;

        imageIcon.sprite = icon;
        bgCard.color = DataRepo.Instance.equipmentDatabase.GetRankColor(data.rank);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onClick?.Invoke(this));
    }
    public void Refresh()
    {
        var state = EquipmentSave.Get(_type, _data.id);

        levelText.text = "Level " + state.level.ToString();

        bool maxed = state.level >= _data.levelMax;
        int need = EquipmentUpgrade.CardNeeded(_data, state.level);

        cardText.text = $"{state.card}/{need}";

        fillProgress.fillAmount = maxed ? 1f
            : (need > 0 ? Mathf.Clamp01((float)state.card / need) : 0f);

        arrow.SetActive(EquipmentUpgrade.CanUpgrade(_type, _data));
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
    public void SetButtonEnabled(bool on)
    {
        button.enabled = on;
    }
}