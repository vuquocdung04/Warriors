using EventDispatcher;
using UnityEngine;

// Test slow: gắn vào 1 nút UI, kéo Click() vào Button.onClick -> làm chậm toàn bộ địch.
public class SlowEnemiesButton : MonoBehaviour
{
    public float moveMul = 0.5f;   // 0.5 = chậm còn 50%
    public float duration = 3f;

    public void Click()
    {
        this.PostEvent(EventID.APPLY_EFFECT_ALL_ENEMIES,
            (System.Func<IStatusEffect>)(() => new SlowEffect(moveMul, duration)));
    }
}
