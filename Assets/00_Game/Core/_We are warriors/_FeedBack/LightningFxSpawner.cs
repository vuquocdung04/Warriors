using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

// Sét đánh (port từ LightningDrink + FXDrink bên DrinkPacking):
// tia sét kéo từ mép trên màn hình xuống unit, các tia đánh lệch nhau 1 chút, trúng là rung camera + giết luôn
public class LightningFxSpawner : StaffSingleton<LightningFxSpawner>
{
    public LightningFx prefab;
    public Camera cam;

    [Header("Bolt")]
    public float scale = 1f;
    public Vector3 offset = new Vector3(0f, 0.3f, 0f);   // điểm sét chạm trên thân unit (pivot ở chân)
    public float topPadding = 2.5f;                      // tia sét bắt đầu cao hơn mép trên màn hình
    public float hitDelay = 0.05f;                       // tia chạm xuống rồi mới giết

    [Header("Nhịp đánh nhiều tia")]
    public float strikeDelay = 0.25f;
    public float strikeStagger = 0.12f;
    public float strikeWindow = 0.5f;                    // nhiều mục tiêu thì dồn hết vào khoảng này

    [Header("Feedback")]
    public float shake = 0.15f;
    public float shakeDuration = 0.12f;
    public string sound = "lightningSound";

    public override void Init() { }

    public void StrikeUnits(List<Unit> targets)
    {
        if (targets == null || targets.Count == 0) return;
        StrikeRoutine(targets, this.GetCancellationTokenOnDestroy()).Forget();
    }

    async UniTaskVoid StrikeRoutine(List<Unit> targets, CancellationToken token)
    {
        try
        {
            if (strikeDelay > 0f) await UniTask.Delay(System.TimeSpan.FromSeconds(strikeDelay), cancellationToken: token);

            float stagger = GetStagger(targets.Count);
            for (int i = 0; i < targets.Count; i++)
            {
                if (BattleManager.Instance != null && BattleManager.Instance.IsBattleOver) return;

                var u = targets[i];
                if (u != null && u.IsAlive) Strike(u, token);

                if (i < targets.Count - 1 && stagger > 0f)
                    await UniTask.Delay(System.TimeSpan.FromSeconds(stagger), cancellationToken: token);
            }
        }
        catch (System.OperationCanceledException) { }
    }

    float GetStagger(int count)
    {
        if (strikeWindow <= 0f || count <= 2) return strikeStagger;
        return Mathf.Min(strikeStagger, strikeWindow / (count - 1));
    }

    void Strike(Unit target, CancellationToken token)
    {
        if (prefab != null)
        {
            var fx = SimplePool2.Spawn(prefab, target.transform.position + offset * scale, Quaternion.identity);
            fx.transform.localScale = Vector3.one * scale;
            PlaceBolt(fx, GetScreenTop() - fx.transform.position.y + topPadding);
            fx.PlayPooled();
        }

        if (!string.IsNullOrEmpty(sound)) AudioManager.Instance.PlaySfx(sound);
        HitAfterDelay(target, token).Forget();
    }

    async UniTaskVoid HitAfterDelay(Unit target, CancellationToken token)
    {
        try
        {
            if (hitDelay > 0f) await UniTask.Delay(System.TimeSpan.FromSeconds(hitDelay), cancellationToken: token);
            ShakeCamera();
            if (target != null && target.IsAlive) target.Kill(true);   // sét đánh là chết, vẫn rơi thưởng
        }
        catch (System.OperationCanceledException) { }
    }

    void ShakeCamera()
    {
        if (cam == null || shake <= 0f) return;
        var t = cam.transform;
        t.DOKill(true);
        t.DOShakePosition(shakeDuration, shake, 20, 90f, false, true).SetTarget(t);
    }

    float GetScreenTop()
    {
        if (cam == null) return transform.position.y;
        return cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f)).y;
    }

    // Particle có size 3D (tia sét) thì kéo dài đúng tới mép trên màn hình
    static void PlaceBolt(LightningFx item, float height)
    {
        for (int i = 0; i < item.ListParticle.Count; i++)
        {
            var particle = item.ListParticle[i];
            if (particle == null) continue;

            var main = particle.main;
            if (!main.startSize3D) continue;

            float s = particle.transform.lossyScale.y;
            main.startSizeY = s > 0f ? height / s : height;
            particle.transform.position = item.transform.position + Vector3.up * height;
        }
    }
}
