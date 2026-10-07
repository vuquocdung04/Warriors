using UnityEngine;

public class FreezeFxSpawner : StaffSingleton<FreezeFxSpawner>
{
    public FreezeFx prefab;
    public Vector3 offset = new Vector3(0f, 0.6f, 0f);   // unit pivot ở chân, đặt fx giữa thân
    public Color frozenTint = new Color(0.55f, 0.85f, 1f, 1f);

    public override void Init() { }

    public FreezeFx Attach(Transform unit)
    {
        var fx = SimplePool2.Spawn(prefab);
        fx.Play(unit, offset);
        return fx;
    }
}
