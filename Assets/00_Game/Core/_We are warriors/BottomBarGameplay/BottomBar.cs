using UnityEngine;

public class BottomBar : StaffSingleton<BottomBar>
{
    public FoodBar foodBar;
    public UnitCardBar unitCardBar;
    public BoosterBar boosterBar;

    public void Init(BattleSpawner spawner, UnitDatabase db, int startFood)
    {
        foodBar.Init(startFood);
        unitCardBar.Init(spawner, db);
        boosterBar.Init();
    }

    public override void Init()
    {
        throw new System.NotImplementedException();
    }
}