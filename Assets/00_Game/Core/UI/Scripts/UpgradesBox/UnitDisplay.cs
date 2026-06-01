using UnityEngine;

public class UnitDisplay : MonoBehaviour
{
    public string id; 
    public GameObject blindUnit;

    public void SetBlind(bool on)
    {
        if (blindUnit != null) blindUnit.SetActive(on);
    }
}