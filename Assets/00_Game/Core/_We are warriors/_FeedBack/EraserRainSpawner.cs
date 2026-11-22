using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

// Skill "Mưa cục tẩy" (civ HNUE thay cho tên lửa):
// đoạn giữa mặt trước nhà ally và nhà enemy chia đều thành N khoảng, mỗi khoảng 1 cục tẩy rơi vào giữa,
// bay chéo từ góc trên-trái xuống, chạm đất thì nổ vụn tẩy + gây % máu tối đa cho enemy trong bán kính.
public class EraserRainSpawner : StaffSingleton<EraserRainSpawner>
{
    public Camera cam;

    [Header("Cục tẩy")]
    public List<Sprite> eraserSprites;
    public float eraserWidth = 2.4f;                          // world unit
    public string sortingLayer = "Unit";
    public int sortingOrder = 30005;

    [Header("Quỹ đạo")]
    public int count = 4;
    public Vector2 startOffset = new Vector2(-9f, 14f);       // điểm xuất phát so với điểm rơi (trên-trái, ngoài màn hình)
    public float flyDuration = 0.55f;
    public float stagger = 0.18f;
    public float spinDegrees = -540f;

    [Header("Bóng báo trước chỗ rơi")]
    public Sprite shadowSprite;
    public Color shadowColor = new Color(0f, 0f, 0f, 0.35f);

    [Header("Nổ")]
    public ParticleSystem impactFxPrefab;
    [Tooltip("Bán kính nổ (world unit); FX tự scale theo")]
    public float radius = 1.9f;
    [Tooltip("Bán kính mà prefab FX được làm sẵn (vòng sóng 2.8 = bán kính 1.4)")]
    public float fxBaseRadius = 1.4f;
    public float shake = 0.18f;
    public float shakeDuration = 0.15f;
    public string launchSound = "Firey Swoosh";
    public string impactSound = "punch";

    public override void Init() { }

    public void Drop(float percent)
    {
        DropRoutine(percent, this.GetCancellationTokenOnDestroy()).Forget();
    }

    async UniTaskVoid DropRoutine(float percent, CancellationToken token)
    {
        try
        {
            var points = GetImpactPoints(count);
            if (!string.IsNullOrEmpty(launchSound)) AudioManager.Instance.PlaySfx(launchSound);
            for (int i = 0; i < points.Count; i++)
            {
                if (BattleManager.Instance != null && BattleManager.Instance.IsBattleOver) return;
                LaunchOne(points[i], i, percent);
                if (i < points.Count - 1 && stagger > 0f)
                    await UniTask.Delay(System.TimeSpan.FromSeconds(stagger), cancellationToken: token);
            }
        }
        catch (System.OperationCanceledException) { }
    }

    void LaunchOne(Vector3 target, int index, float percent)
    {
        // bóng ở chỗ rơi to dần lên khi cục tẩy tới gần
        SpriteRenderer shadow = null;
        if (shadowSprite != null)
        {
            shadow = NewSprite("EraserShadow", shadowSprite, target, sortingOrder - 1);
            shadow.color = shadowColor;
            float sw = shadowSprite.bounds.size.x;
            shadow.transform.localScale = new Vector3(0.2f, 0.08f, 1f) * (radius * 2f / sw);
            shadow.transform.DOScale(new Vector3(1f, 0.4f, 1f) * (radius * 2f / sw), flyDuration).SetEase(Ease.InQuad).SetLink(shadow.gameObject);
        }

        Sprite sp = eraserSprites != null && eraserSprites.Count > 0 ? eraserSprites[index % eraserSprites.Count] : null;
        var eraser = NewSprite("Eraser", sp, target + (Vector3)startOffset, sortingOrder);
        if (sp != null) eraser.transform.localScale = Vector3.one * (eraserWidth / sp.bounds.size.x);
        eraser.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-30f, 30f));

        var t = eraser.transform;
        DOTween.Sequence()
            .Append(t.DOMove(target, flyDuration).SetEase(Ease.InQuad))
            .Join(t.DORotate(new Vector3(0f, 0f, spinDegrees), flyDuration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear))
            .AppendCallback(() =>
            {
                Impact(target, percent);
                if (shadow != null) Destroy(shadow.gameObject);
            })
            // nảy nhẹ rồi mờ đi
            .Append(t.DOPunchScale(Vector3.one * 0.25f * t.localScale.x, 0.18f, 6, 0.6f))
            .Append(eraser.DOFade(0f, 0.25f))
            .OnComplete(() => Destroy(eraser.gameObject))
            .SetLink(eraser.gameObject);
    }

    void Impact(Vector3 point, float percent)
    {
        if (impactFxPrefab != null)
        {
            var fx = Instantiate(impactFxPrefab, point, Quaternion.identity);
            if (fxBaseRadius > 0f) fx.transform.localScale = Vector3.one * (radius / fxBaseRadius);
            fx.Play(true);
            var main = fx.main;
            Destroy(fx.gameObject, main.duration + main.startLifetime.constantMax + 0.5f);
        }
        if (!string.IsNullOrEmpty(impactSound)) AudioManager.Instance.PlaySfx(impactSound);
        ShakeCamera();
        DamageAround(point, radius, percent);
    }

    // trừ % máu tối đa cho enemy trong bán kính; duyệt bản sao vì unit chết sẽ tự rời list
    static void DamageAround(Vector3 point, float r, float percent)
    {
        if (BattleManager.Instance == null) return;
        var list = new List<Unit>(BattleManager.Instance.Enemies);
        float r2 = r * r;
        foreach (var u in list)
        {
            if (u == null || !u.IsAlive) continue;
            Vector2 d = u.transform.position - point;
            if (d.sqrMagnitude > r2) continue;
            u.TakeDamage(u.MaxHp * percent);
        }

        // boss dungeon: cục tẩy rơi trúng thân boss (bức tường từ mép trước trở ra sau)
        var boss = DungeonBoss.Active;
        if (boss != null && boss.IsAlive && boss.SqrDistanceTo(point) <= r2) boss.TakePercentDamage(percent);
    }

    // fallback khi scene chưa có spawner: gây damage ngay tại các điểm, không hiệu ứng
    public static void ApplyWithoutFx(float percent, int count = 4, float r = 1.9f)
    {
        foreach (var p in GetImpactPoints(count)) DamageAround(p, r, percent);
    }

    // chia đều đoạn giữa mặt trước 2 nhà thành count khoảng, lấy tâm mỗi khoảng, y = giữa đường
    static List<Vector3> GetImpactPoints(int count)
    {
        var result = new List<Vector3>();
        var grid = BattleGrid.Instance;
        var spawner = BattleSpawner.Instance;
        if (grid == null || count <= 0) return result;

        float y = grid.transform.position.y + grid.Height * grid.cellSize * 0.5f;
        float left = grid.transform.position.x;
        float right = left + grid.Width * grid.cellSize;
        if (spawner != null && spawner.allyHouse != null && spawner.enemyHouse != null)
        {
            left = spawner.allyHouse.transform.position.x + spawner.allyHouse.wallOffsetX;
            right = spawner.enemyHouse.transform.position.x + spawner.enemyHouse.wallOffsetX;
        }

        float step = (right - left) / count;
        for (int i = 0; i < count; i++)
            result.Add(new Vector3(left + step * (i + 0.5f), y, 0f));
        return result;
    }

    SpriteRenderer NewSprite(string n, Sprite sp, Vector3 pos, int order)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform, true);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp;
        sr.sortingLayerName = sortingLayer;
        sr.sortingOrder = order;
        return sr;
    }

    void ShakeCamera()
    {
        if (cam == null || shake <= 0f) return;
        var t = cam.transform;
        t.DOKill(true);
        t.DOShakePosition(shakeDuration, shake, 20, 90f, false, true).SetTarget(t);
    }

    [Button("Test (Play mode)")]
    void Test()
    {
        if (!Application.isPlaying) { Debug.Log("Phải Play mode"); return; }
        Drop(0.2f);
    }

    void OnDrawGizmosSelected()
    {
        if (BattleGrid.Instance == null) return;
        Gizmos.color = new Color(1f, 0.4f, 0.6f, 0.6f);
        foreach (var p in GetImpactPoints(count)) Gizmos.DrawWireSphere(p, radius);
    }
}
