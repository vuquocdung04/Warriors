using UnityEngine;
using UnityEngine.UI;

public class UpgradesBox : BaseBox<UpgradesBox>
{
    [Header("Tab buttons")]
    public Button btnEvolve;
    public Button btnUpgrade;

    [Header("Tab content")]
    public CanvasGroup tabEvolutionBar;
    public CanvasGroup tabUpgradeBar;
    [Header("Bars")]
    public EvolutionBar evolutionBar;
    public UpgradeBar upgradeBar;

    [Header("Button parents")]
    public GameObject evolveButtonParent;
    public GameObject upgradeButtonParent;

    protected override void Init()
    {
        btnEvolve.OnClicked(delegate { ShowEvolve(); });
        btnUpgrade.OnClicked(delegate { ShowUpgrade(); });

        evolutionBar.Init();
        upgradeBar.Init(UseProfile.CurrentCiv.Value);
        ShowUpgrade();
    }

    protected override void InitState()
    {
    }

    void ShowEvolve()
    {
        tabEvolutionBar.SetCanvasState(true, 1f);
        tabUpgradeBar.SetCanvasState(false, 0f);

        evolveButtonParent.SetActive(false);
        upgradeButtonParent.SetActive(true);
    }
    void ShowUpgrade()
    {
        tabEvolutionBar.SetCanvasState(false, 0f);
        tabUpgradeBar.SetCanvasState(true, 1f);

        evolveButtonParent.SetActive(true);
        upgradeButtonParent.SetActive(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}