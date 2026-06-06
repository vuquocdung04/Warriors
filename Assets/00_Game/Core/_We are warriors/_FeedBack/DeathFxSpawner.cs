using UnityEngine;

public class DeathFxSpawner : StaffSingleton<DeathFxSpawner>
{
    public DeathFx prefab;

    public override void Init()
    {
        throw new System.NotImplementedException();
    }

    public void Play(Vector3 worldPos)
    {
        var fx = SimplePool2.Spawn(prefab);
        fx.Play(worldPos);
    }
}