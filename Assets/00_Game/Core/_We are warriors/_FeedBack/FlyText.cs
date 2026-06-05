using DG.Tweening;
using TMPro;
using UnityEngine;

public class FlyText : MonoBehaviour
{
    public TextMeshPro text;        // TMP 3D
    public float riseDistance = 1f;
    public float duration = 0.8f;

    public void Play(string content, Color color, Vector3 worldPos)
    {
        transform.position = worldPos;
        text.text = content;
        text.color = color;
        text.alpha = 1f;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(worldPos.y + riseDistance, duration).SetEase(Ease.OutQuad));
        seq.Join(text.DOFade(0f, duration).SetEase(Ease.InQuad));
        seq.OnComplete(() => SimplePool2.Despawn(gameObject));
        seq.SetLink(gameObject);
    }
}