using EventDispatcher;
using TMPro;
using UnityEngine;

public class GemView : MonoBehaviour
{
    public TextMeshProUGUI gemText;

    void Start()
    {
        this.RegisterListener(EventID.CHANGE_GEM, OnGemChanged);
        Refresh();
    }

    void OnGemChanged(object param) => Refresh();

    void Refresh()
    {
        int gem = CurrencyManager.Instance.Get(CurrencyType.Gem);
        if (gemText != null)
            _ = gemText.CountToWithIcon(gem, "<sprite=52> ", duration: 0.3f);
    }

    void OnDestroy()
    {
        this.RemoveListener(EventID.CHANGE_GEM, OnGemChanged);
    }
}