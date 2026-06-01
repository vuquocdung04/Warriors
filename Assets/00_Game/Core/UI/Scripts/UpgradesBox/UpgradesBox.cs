using UnityEngine;
using UnityEngine.UI;

public class UpgradesBox : BaseBox<UpgradesBox>
{
    [Header("Tab buttons")]
    public Button btnEvolve;
    public Button btnUpgrade;

    [Header("Tab content")]
    public GameObject tabEvolutionBar;
    public GameObject tabUpgradeBar;

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
        tabEvolutionBar.SetActive(true);
        tabUpgradeBar.SetActive(false);
        evolveButtonParent.SetActive(false);
        upgradeButtonParent.SetActive(true);
    }

    void ShowUpgrade()
    {
        tabEvolutionBar.SetActive(false);
        tabUpgradeBar.SetActive(true);
        evolveButtonParent.SetActive(true);
        upgradeButtonParent.SetActive(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}