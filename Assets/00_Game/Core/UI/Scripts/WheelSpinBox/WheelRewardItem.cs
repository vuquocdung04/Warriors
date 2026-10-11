using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WheelRewardItem : MonoBehaviour
{
    public Image imgIcon;
    public TextMeshProUGUI txtAmount;

    public void Setup(WheelReward reward, int amount)
    {
        imgIcon.sprite = reward.icon;
        imgIcon.enabled = reward.icon != null;
        txtAmount.text = NumberFormatter.Format(amount);
    }
}
