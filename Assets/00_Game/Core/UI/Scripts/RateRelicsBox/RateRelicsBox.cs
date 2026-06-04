using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RateRelicsBox : BaseBox<RateRelicsBox>
{
    [Header("Buttons")]
    public Button btnClose;
    public Button btnNext;
    public Button btnPrev;

    [Header("Info")]
    public TMP_Text txtLevel;
    public TMP_Text txtCommon;
    public TMP_Text txtRare;
    public TMP_Text txtEpic;
    public TMP_Text txtLegend;

    private int _viewLevel;
    private int _maxLevel;

    protected override void Init()
    {
        btnClose.OnClicked(delegate { Close(); });
        btnNext.OnClicked(delegate { ShowLevel(_viewLevel + 1); });
        btnPrev.OnClicked(delegate { ShowLevel(_viewLevel - 1); });

        _maxLevel = DataRepo.Instance.gachaDatabase.GetMaxGachaLevel();
        ShowLevel(UseProfile.GachaLevel.Value);
    }

    protected override void InitState() { }

    void ShowLevel(int level)
    {
        _viewLevel = Mathf.Clamp(level, 1, _maxLevel);

        var rate = DataRepo.Instance.gachaDatabase.GetGachaRate(_viewLevel);
        txtLevel.text = "Level " + _viewLevel.ToString();
        if (rate != null)
        {
            txtCommon.text = Percent(rate.common);
            txtRare.text = Percent(rate.rare);
            txtEpic.text = Percent(rate.epic);
            txtLegend.text = Percent(rate.legend);
        }

        btnPrev.gameObject.SetActive(_viewLevel > 1);
        btnNext.gameObject.SetActive(_viewLevel < _maxLevel);
    }

    string Percent(float v) => $"{Mathf.RoundToInt(v * 100f)}%";   // 0.53 -> "53%"

    protected override void OnDestroy() => base.OnDestroy();
}