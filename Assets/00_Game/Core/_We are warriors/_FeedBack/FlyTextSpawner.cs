using UnityEngine;

public class FlyTextSpawner : StaffSingleton<FlyTextSpawner>
{
    public FlyText prefab;
    static readonly Color NORMAL = Color.white;
    static readonly Color CRIT = new Color(1f, 0.25f, 0.2f);    // đỏ cam
    static readonly Color POISON = new Color(0.4f, 0.85f, 0.3f);  // xanh lá
    static readonly Color BURN = new Color(1f, 0.55f, 0.1f);    // cam
    public override void Init()
    {
        throw new System.NotImplementedException();
    }
    public void Damage(float dmg, Vector3 pos, bool crit)
    {
        Show(((int)dmg).ToString(), crit ? CRIT : NORMAL, pos, crit ? 1.3f : 1f);
    }

    void Show(string content, Color color, Vector3 pos, float scale)
    {
        var ft = SimplePool2.Spawn(prefab);
        ft.Play(content, color, pos, scale);
    }
     public void Poison(float dmg, Vector3 pos) => Show(((int)dmg).ToString(), POISON, pos, 0.8f);
    public void Burn(float dmg, Vector3 pos)   => Show(((int)dmg).ToString(), BURN, pos, 0.8f);

}