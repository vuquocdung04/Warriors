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

    public Color Tint(DotType type) => type == DotType.Burn ? burnTint : poisonTint;
}
