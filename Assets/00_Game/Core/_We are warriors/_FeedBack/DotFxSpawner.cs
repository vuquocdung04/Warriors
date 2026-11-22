using UnityEngine;

public enum DotType { Burn, Poison }

public class DotFxSpawner : StaffSingleton<DotFxSpawner>
{
    public DotFx burnPrefab;
    public DotFx poisonPrefab;
    public Vector3 offset = new Vector3(0f, 0.3f, 0f);   // unit pivot ở chân
    public Color burnTint = new Color(1f, 0.72f, 0.55f, 1f);
    public Color poisonTint = new Color(0.7f, 1f, 0.6f, 1f);

    public override void Init() { }

    public DotFx Attach(DotType type, Transform unit)
    {
        var prefab = type == DotType.Burn ? burnPrefab : poisonPrefab;
        if (prefab == null) return null;
        var fx = SimplePool2.Spawn(prefab);
        fx.Play(unit, offset);
        return fx;
    }

    // Gắn tại điểm tuỳ ý + phóng to (boss to hơn lính nhiều)
    public DotFx AttachAt(DotType type, Transform parent, Vector3 localOffset, float scale)
    {
        var prefab = type == DotType.Burn ? burnPrefab : poisonPrefab;
        if (prefab == null) return null;
        var fx = SimplePool2.Spawn(prefab);
        fx.Play(parent, localOffset);
        // cha bị lật bằng scale X âm (boss quay mặt sang trái) -> particle bị lật mặt và không hiện; lật ngược lại để bù
        float flip = parent != null && parent.lossyScale.x < 0f ? -1f : 1f;
        fx.transform.localScale = new Vector3(scale * flip, scale, scale);
        return fx;
    }

    public Color Tint(DotType type) => type == DotType.Burn ? burnTint : poisonTint;
}
