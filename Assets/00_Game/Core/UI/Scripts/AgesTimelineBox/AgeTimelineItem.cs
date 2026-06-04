using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AgeTimelineItem : MonoBehaviour
{
    public Image icon;
    public TMP_Text txtCivName;
    public TMP_Text txtEraYear;
    public TMP_Text txtEraDesc;

    public void Setup(HouseData data)
    {
        icon.sprite = DataRepo.Instance.unitDatabase.GetCivIcon(data.civId);
        txtCivName.text = data.civName;
        txtEraYear.text = data.eraYears;
        txtEraDesc.text = data.eraDesc;
    }
}