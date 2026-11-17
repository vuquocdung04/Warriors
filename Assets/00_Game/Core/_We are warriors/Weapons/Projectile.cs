using System;
using UnityEngine;
using DG.Tweening;

public class Projectile : MonoBehaviour
{
    [Header("FX khi trúng (dùng sau)")]
    public AudioClip hitSound;
    public ParticleSystem hitEffect;

    public void Launch(Vector3 targetPos, float speed, Action onArrive)
    {
        transform.DOKill();
        float dist = Vector3.Distance(transform.position, targetPos);
        float duration = dist / Mathf.Max(0.01f, speed);

        Vector3 dir = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        transform.DOMove(targetPos, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                onArrive?.Invoke();
                PlayHitFx();
                SimplePool2.Despawn(gameObject);
            });
    }

    // faceDirection: xoay đạn theo tiếp tuyến đường cong (mũi +X), lật Y khi bay sang trái để sprite không bị úp ngược
    public void LaunchArc(Vector3 target, float speed, float arcHeight, System.Action onHit, bool faceDirection = false)
    {
        transform.DOKill();

        Vector3 start = transform.position;
        float distance = Vector3.Distance(start, target);
        float duration = distance / Mathf.Max(0.01f, speed);
        float t = 0f;
        Vector3 delta = target - start;
        if (faceDirection)
        {
            var s = transform.localScale;
            transform.localScale = new Vector3(s.x, delta.x < 0f ? -Mathf.Abs(s.y) : Mathf.Abs(s.y), s.z);
        }
        DOTween.To(() => t, v =>
        {
            t = v;
            Vector3 pos = Vector3.Lerp(start, target, t);
            pos.y += arcHeight * 4f * t * (1f - t);
            transform.position = pos;
            if (faceDirection)
            {
                float dy = delta.y + arcHeight * 4f * (1f - 2f * t);
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dy, delta.x) * Mathf.Rad2Deg);
            }
        }, 1f, duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            onHit?.Invoke();
            SimplePool2.Despawn(gameObject);
        }).SetLink(gameObject);
    }
    void PlayHitFx()
    {
        if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, transform.position);
        if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);
    }
}