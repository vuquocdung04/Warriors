using RDG;
using UnityEngine;
using UnityEngine.UI;

public static partial class UIUtils
{
    public static void OnClicked(this Button btn, System.Action callback, AudioClip key = null)
    {
        btn.onClick.AddListener(delegate
        {
            if (key == null)
                AudioManager.Instance.PlaySfx("Click");
            else
                AudioManager.Instance.PlaySfx(key);
            Vibration.Vibrate(50);
            callback.Invoke();
        });
    }

    public static void SetActive(this Button btn, bool isActive)
    {
        btn.gameObject.SetActive(isActive);
    }
}