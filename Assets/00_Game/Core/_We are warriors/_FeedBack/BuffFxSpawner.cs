using UnityEngine;

public enum BuffType { Haste, Frenzy }

// FX bám trên unit trong lúc được buff (dùng chung DotFx: burst lúc nhận + aura loop + Release tắt dần)
public class BuffFxSpawner : StaffSingleton<BuffFxSpawner>
{
    public DotFx hastePrefab;
    public DotFx frenzyPrefab;
    public Vector3 offset = new Vector3(0f, 0.3f, 0f);   // unit pivot ở chân

    public override void Init() { }

    public DotFx Attach(BuffType type, Transform unit, bool flipX)
    {
        var prefab = type == BuffType.Haste ? hastePrefab : frenzyPrefab;
        if (prefab == null) return null;
        var fx = SimplePool2.Spawn(prefab);
        fx.Play(unit, offset);
        // Vệt gió phải bay ngược hướng chạy -> unit địch chạy sang trái thì lật fx
        fx.transform.localScale = new Vector3(flipX ? -1f : 1f, 1f, 1f);
        return fx;
    }
}
