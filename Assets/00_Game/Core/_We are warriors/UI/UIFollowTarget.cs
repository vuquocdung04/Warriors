using UnityEngine;

// Giữ 1 UI bám theo điểm trên rect của target (vd nút nâng cấp nhô ra trên mép thẻ, nằm ngoài RectMask2D của thẻ).
[ExecuteAlways]
public class UIFollowTarget : MonoBehaviour
{
    public RectTransform target;
    [Tooltip("Điểm bám trên rect target (0..1), vd (0.5,1) = giữa mép trên")]
    public Vector2 anchor = new Vector2(0.5f, 1f);
    [Tooltip("Lệch thêm theo pixel local của target")]
    public Vector2 offset;

    void LateUpdate()
    {
        if (target == null) return;
        var r = target.rect;
        var local = new Vector3(r.x + r.width * anchor.x + offset.x, r.y + r.height * anchor.y + offset.y, 0f);
        transform.position = target.TransformPoint(local);
    }
}
