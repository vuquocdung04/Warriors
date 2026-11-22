using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Thanh máu boss trên đầu màn hình (Dungeon): tên boss + fill đỏ, vệt máu vừa mất (delay) chạy theo sau.
// Fill dùng Image Sliced (bo góc không bị méo) -> co chiều rộng bằng anchorMax.x thay vì fillAmount.
public class BossHpBar : MonoBehaviour
{
    public RectTransform fill;        // Image Sliced, anchor trái
    public RectTransform fillDelay;   // vệt máu vừa mất, chạy theo sau fill
    public TMP_Text txtName;
    public TMP_Text txtHp;
    public float delayTime = 0.35f;
    [Tooltip("Tỉ lệ tối thiểu để phần bo góc 2 đầu không bị dính vào nhau khi gần hết máu")]
    public float minVisibleRatio = 0.02f;

    private DungeonBoss _boss;
    private Tween _delayTween;
    private float _delayRatio = 1f;

    public void Bind(DungeonBoss boss)
    {
        _boss = boss;
        gameObject.SetActive(true);
        if (txtName != null) txtName.text = boss.bossName;
        _delayRatio = 1f;
        SetRatio(fill, 1f);
        SetRatio(fillDelay, 1f);
        boss.OnHpChanged += Refresh;
        Refresh(boss);
    }

    void Refresh(DungeonBoss boss)
    {
        float ratio = boss.MaxHp > 0f ? Mathf.Clamp01(boss.Hp / boss.MaxHp) : 0f;
        SetRatio(fill, ratio);

        if (fillDelay != null)
        {
            _delayTween?.Kill();
            _delayTween = DOTween.To(() => _delayRatio, v => { _delayRatio = v; SetRatio(fillDelay, v); }, ratio, delayTime)
                .SetDelay(0.15f).SetEase(Ease.OutQuad).SetLink(gameObject);
        }
        if (txtHp != null) txtHp.text = NumberFormatter.Format(Mathf.CeilToInt(boss.Hp));
    }

    void SetRatio(RectTransform rt, float ratio)
    {
        if (rt == null) return;
        rt.gameObject.SetActive(ratio > 0f);
        var max = rt.anchorMax;
        max.x = Mathf.Max(minVisibleRatio, ratio);
        rt.anchorMax = max;
    }

    void OnDestroy()
    {
        if (_boss != null) _boss.OnHpChanged -= Refresh;
    }
}
