using UnityEngine;

public class FlyTextSpawner : StaffSingleton<FlyTextSpawner>
{
    public FlyText prefab;   // field instance -> kéo Inspector

    public override void Init()
    {
        throw new System.NotImplementedException();
    }

    public void Show(string content, Color color, Vector3 worldPos)
    {
        var ft = SimplePool2.Spawn(prefab);
        ft.Play(content, color, worldPos);
    }
}