using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillItem : MonoBehaviour
{
    public string id;

    [Header("Icon")]
    public Image imageIcon;
    public Image bg;

    [Header("State objects (mặc định tắt hết)")]
    public GameObject newObject;
    public GameObject equippedObject;
    public GameObject viewProgress;

    [Header("View progress")]
    public TMP_Text levelText;
    public TMP_Text cardText;
    public Image fillProgress;
    public GameObject arrow;          // đủ card nâng -> bật

    [Header("Button")]
    public Button button;

    private SkillData _data;
    private System.Action<SkillItem> _onClick;

    public SkillData Data => _data;

    public void Init(SkillData data, Sprite icon, System.Action<SkillItem> onClick)
    {
        _data = data;
        id = data.id;
        _onClick = onClick;

        if (imageIcon != null) imageIcon.sprite = icon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onClick?.Invoke(this));
    }

    public void Refresh()
    {
        var state = SkillSave.Get(_data.id);

        if (levelText != null) levelText.text = "Level " + state.level.ToString();

        bool maxed = state.level >= _data.levelMax;
        int need = SkillUpgrade.CardNeeded(state.level);

        if (cardText != null) cardText.text = $"{state.card}/{need}";

        if (fillProgress != null)
            fillProgress.fillAmount = maxed ? 1f
                : (need > 0 ? Mathf.Clamp01((float)state.card / need) : 0f);

        if (arrow != null) arrow.SetActive(SkillUpgrade.CanUpgrade(_data));
    }

    public void SetNew(bool on) { if (newObject != null) newObject.SetActive(on); }
    public void SetEquipped(bool on) { if (equippedObject != null) equippedObject.SetActive(on); }
    public void SetViewProgress(bool on) { if (viewProgress != null) viewProgress.SetActive(on); }
    public void SetButtonEnabled(bool on) { if (button != null) button.enabled = on; }
    public void SetBg(bool on) { if (bg != null) bg.gameObject.SetActive(on);}
}