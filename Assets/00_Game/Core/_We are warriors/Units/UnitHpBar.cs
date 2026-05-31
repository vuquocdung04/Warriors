using UnityEngine;
using UnityEngine.UI;

public class UnitHpBar : MonoBehaviour
{
    public Image fill;
    public GameObject barRoot;  
    public bool hideWhenFull = true;

    public void Set(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        if (fill != null) fill.fillAmount = ratio;

        if (hideWhenFull)
        {
            bool show = ratio < 0.999f;
            if (barRoot != null) barRoot.SetActive(show);   
            else if (fill != null) fill.enabled = show;     
        }
    }
}