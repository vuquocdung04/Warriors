using UnityEngine;
using UnityEngine.UI;

public class BoosterBar : MonoBehaviour
{
    public Button btnBooster;

    public void Init()
    {
        btnBooster.onClick.RemoveAllListeners();
        btnBooster.onClick.AddListener(OnBooster);
    }

    void OnBooster()
    {
        Debug.Log("[Booster] clicked");   // TODO
    }
}