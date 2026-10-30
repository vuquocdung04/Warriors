using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Nhấn: co nhanh (OutQuad), thả: bật lại có vượt (OutBack) — theo UIPressScale bên Base.
// Lúc nút co lại thì nới raycastPadding cho bằng kích thước gốc, ngón tay trượt nhẹ vẫn không bị "tuột" khỏi nút.
// Âm thanh + rung đã có trong UIUtils.OnClicked nên không phát lại ở đây.
[RequireComponent(typeof(Button))]
public class ButtonScalePress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private static readonly List<Graphic> GraphicBuffer = new();

    [SerializeField] private float pressScale = 0.8f;
    [SerializeField] private float pressDuration = 0.08f;
    [SerializeField] private float releaseDuration = 0.15f;
    [SerializeField] private Ease releaseEase = Ease.OutBack;

    private readonly List<Graphic> _raycastGraphics = new();
    private readonly List<Vector4> _defaultPaddings = new();

    private Button _button;
    private Vector3 _originalScale;
    private Tween _tween;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _originalScale = transform.localScale;
        CacheRaycastGraphics();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.IsInteractable()) return;

        ExpandRaycastArea();
        _tween?.Kill();
        _tween = transform.DOScale(_originalScale * pressScale, pressDuration).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _tween?.Kill();
        _tween = transform.DOScale(_originalScale, releaseDuration).SetEase(releaseEase).SetUpdate(true)
            .OnComplete(RestoreRaycastArea);
    }

    private void OnDisable()
    {
        _tween?.Kill();
        transform.localScale = _originalScale;
        RestoreRaycastArea();
    }

    private void CacheRaycastGraphics()
    {
        _raycastGraphics.Clear();
        _defaultPaddings.Clear();

        GetComponentsInChildren(true, GraphicBuffer);
        foreach (var graphic in GraphicBuffer)
        {
            if (!graphic.raycastTarget) continue;
            _raycastGraphics.Add(graphic);
            _defaultPaddings.Add(graphic.raycastPadding);
        }
        GraphicBuffer.Clear();
    }

    private void ExpandRaycastArea()
    {
        float factor = 1f / Mathf.Max(pressScale, 0.01f) - 1f;
        if (factor <= 0f) return;

        for (int i = 0; i < _raycastGraphics.Count; i++)
        {
            var graphic = _raycastGraphics[i];
            if (graphic == null) continue;

            Rect rect = graphic.rectTransform.rect;
            Vector3 offset = transform.InverseTransformPoint(graphic.rectTransform.position);
            Vector4 padding = _defaultPaddings[i];

            graphic.raycastPadding = new Vector4(
                padding.x + (rect.xMin + offset.x) * factor,
                padding.y + (rect.yMin + offset.y) * factor,
                padding.z - (rect.xMax + offset.x) * factor,
                padding.w - (rect.yMax + offset.y) * factor);
        }
    }

    private void RestoreRaycastArea()
    {
        for (int i = 0; i < _raycastGraphics.Count; i++)
            if (_raycastGraphics[i] != null)
                _raycastGraphics[i].raycastPadding = _defaultPaddings[i];
    }
}
