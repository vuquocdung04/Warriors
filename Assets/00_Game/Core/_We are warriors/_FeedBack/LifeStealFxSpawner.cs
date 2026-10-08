using UnityEngine;

public class LifeStealFxSpawner : StaffSingleton<LifeStealFxSpawner>
{
    public LifeStealFx prefab;
    public Vector3 offset = new Vector3(0f, 0.3f, 0f);   // unit pivot ở chân
    public float minInterval = 0.35f;                     // unit đánh nhanh thì không spam hiệu ứng

    public override void Init() { }

    public void Play(Vector3 unitPos)
    {
        if (prefab == null) return;
        var fx = SimplePool2.Spawn(prefab);
        fx.Play(unitPos + offset);
    }
}
