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
        float dist = Vector3.Distance(transform.position, targetPos);
        float duration = dist / Mathf.Max(0.01f, speed);

        transform.DOMove(targetPos, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                onArrive?.Invoke();
                PlayHitFx();
                Destroy(gameObject);
            });
    }

    public void LaunchArc(Vector3 target, float duration, float arcHeight, System.Action onHit)
    {
        Vector3 start = transform.position;
        float t = 0f;

        DOTween.To(() => t, v =>
        {
            t = v;
            Vector3 pos = Vector3.Lerp(start, target, t);
            pos.y += arcHeight * 4f * t * (1f - t);
            transform.position = pos;
        }, 1f, duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            onHit?.Invoke();
            Destroy(gameObject);
        }).SetLink(gameObject);
    }
    void PlayHitFx()
    {
        if (hitSound != null) AudioSource.PlayClipAtPoint(hitSound, transform.position);
        if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);
    }
}