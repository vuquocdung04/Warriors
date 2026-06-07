using Sirenix.OdinInspector;
using UnityEngine;

public class DeathFxSpawner : StaffSingleton<DeathFxSpawner>
{
    public DeathFx prefab;
    public GroundCrackFx crackPrefab;
    public override void Init()
    {
        throw new System.NotImplementedException();
    }

    public void Play(Vector3 worldPos)
    {
        var fx = SimplePool2.Spawn(prefab);
        fx.Play(worldPos + Vector3.up * 0.5f); /// because unit pivot bottom center

        var crack = SimplePool2.Spawn(crackPrefab);
        crack.Play(worldPos);
    }


    [Button("Test FX")]
    private void Test()
    {
        Play(Vector3.zero);
    }
}