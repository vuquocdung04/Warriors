using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// FX va chạm nhẹ cho đòn đánh theo bậc: toé chấm, vết trên đất, lóe sáng, rung camera (có giới hạn).
// Tự tạo khi cần, pool SpriteRenderer, tween ngắn (<= ~1s) để trận đông vẫn nhẹ.
public class HitFxSpawner : MonoBehaviour
{
    const string FxLayer = "Proejctile";   // trên lính
    const string GroundLayer = "House";     // dưới lính (giống GroundCrack)
    const float ShakeCooldown = 0.5f;       // cả trận tối đa 1 lần rung / 0.5s

    static HitFxSpawner _instance;
    static float _lastShake = -999f;

    readonly Stack<SpriteRenderer> _pool = new();

    static HitFxSpawner Instance
    {
        get
        {
            if (_instance == null) _instance = new GameObject("HitFxSpawner").AddComponent<HitFxSpawner>();
            return _instance;
        }
    }

    void OnDestroy() { if (_instance == this) _instance = null; }

    SpriteRenderer Get(Sprite sp, string layer, int order)
    {
        var sr = _pool.Count > 0 ? _pool.Pop() : new GameObject("fx").AddComponent<SpriteRenderer>();
        if (sr.transform.parent != transform) sr.transform.SetParent(transform, false);
        sr.gameObject.SetActive(true);
        sr.sprite = sp;
        sr.sortingLayerName = layer;
        sr.sortingOrder = order;
        sr.transform.localRotation = Quaternion.identity;
        return sr;
    }

    void Release(SpriteRenderer sr)
    {
        if (sr == null) return;
        sr.DOKill(); sr.transform.DOKill();
        sr.gameObject.SetActive(false);
        _pool.Push(sr);
    }

    // Toé chấm (sơn / mực / tia vàng) bắn ra rồi rơi xuống
    public static void Splash(Vector3 pos, Sprite dot, Color[] colors, int count, float size = 0.14f, float spread = 0.55f)
    {
        if (dot == null || count <= 0) return;
        var s = Instance;
        for (int i = 0; i < count; i++)
        {
            var sr = s.Get(dot, FxLayer, 50);
            sr.color = colors != null && colors.Length > 0 ? colors[Random.Range(0, colors.Length)] : Color.white;
            var tr = sr.transform;
            tr.position = pos;
            float sz = size * Random.Range(0.7f, 1.3f);
            tr.localScale = Vector3.one * sz;
            Vector2 dir = Random.insideUnitCircle.normalized; dir.y = Mathf.Abs(dir.y) * 0.8f + 0.2f;
            Vector3 end = pos + (Vector3)(dir * spread * Random.Range(0.6f, 1f)) + Vector3.down * Random.Range(0.05f, 0.25f);
            float t = Random.Range(0.28f, 0.42f);
            DOTween.Sequence().SetLink(sr.gameObject)
                .Append(tr.DOJump(end, Random.Range(0.12f, 0.3f), 1, t).SetEase(Ease.OutQuad))
                .Join(tr.DOScale(sz * 0.3f, t).SetEase(Ease.InQuad))
                .Join(sr.DOFade(0f, t * 0.5f).SetDelay(t * 0.5f))
                .OnComplete(() => s.Release(sr));
        }
    }

    // Vết trên đất (sơn loang / vết xóa / nứt) mờ dần
    public static void Mark(Vector3 pos, Sprite sp, Color color, float scale, float life = 1f)
    {
        if (sp == null) return;
        var s = Instance;
        var sr = s.Get(sp, GroundLayer, 5);
        sr.color = color;
        var tr = sr.transform;
        tr.position = pos;
        tr.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-20f, 20f));
        tr.localScale = Vector3.one * scale * 0.6f;
        DOTween.Sequence().SetLink(sr.gameObject)
            .Append(tr.DOScale(scale, 0.12f).SetEase(Ease.OutBack))
            .AppendInterval(life * 0.5f)
            .Append(sr.DOFade(0f, life * 0.5f))
            .OnComplete(() => s.Release(sr));
    }

    // Lóe sáng ngắn tại điểm trúng
    public static void Flash(Vector3 pos, Sprite sp, Color color, float scale)
    {
        if (sp == null) return;
        var s = Instance;
        var sr = s.Get(sp, FxLayer, 40);
        sr.color = color;
        var tr = sr.transform;
        tr.position = pos;
        tr.localScale = Vector3.one * scale * 0.4f;
        DOTween.Sequence().SetLink(sr.gameObject)
            .Append(tr.DOScale(scale, 0.18f).SetEase(Ease.OutQuad))
            .Join(sr.DOFade(0f, 0.2f).SetEase(Ease.InQuad))
            .OnComplete(() => s.Release(sr));
    }

    // Rung camera nhẹ, giới hạn tần suất; bỏ qua nếu camera đang có tween khác (vd booster di chuyển camera)
    public static void Shake(float strength, float duration = 0.15f)
    {
        if (strength <= 0f || Time.time - _lastShake < ShakeCooldown) return;
        var cam = GamePlayController.Instance != null ? GamePlayController.Instance.cameraGameplay : Camera.main;
        if (cam == null) return;
        var t = cam.transform;
        if (DOTween.IsTweening(t)) return;
        _lastShake = Time.time;
        t.DOShakePosition(duration, strength, 20, 90f, false, true).SetTarget(t);
    }
}
