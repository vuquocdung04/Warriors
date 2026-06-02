
using Cysharp.Threading.Tasks;
using EventDispatcher;
using UnityEngine;

public class GamePlayController : LeaderSingleton<GamePlayController>
{
    public Camera cameraUI;
    public Camera cameraGameplay;
    public GameScene gameScene;
    public BoosterController boosterController;
    public HandAnimation handAnimation;
    public GameFlow gameFlow;
    public InputController inputController;

    [Header("We are warriors")]
    public BattleGrid grid;
    public BattleManager battle;
    public BattleSpawner spawner;
    public EnemyAI enemyAI;
    public FoodManager foodManager;
    public UnitCardBar cardBar;

    protected override void OnAwake()
    {
        base.OnAwake();

        var db = DataRepo.Instance.unitDatabase;

        grid.Init();
        battle.Init();
        spawner.Init(db);
        enemyAI.Init(spawner);
        cardBar.Init(spawner, db);

        var allyUnits = db.GetCivUnits(UseProfile.CurrentCiv.Value);
        int startFood = allyUnits.Count > 0 ? Mathf.Max(0, allyUnits[0].foodCost - 2) : 0;
        foodManager.Init(startFood);

        FXManager.Instance.isNextSceneReady = true;

    }
    private async UniTaskVoid Init()
    {
        gameScene.Init();
        handAnimation.Init();
        boosterController.Init();
        inputController.Init();
        gameFlow.Init();
        gameFlow.RequestPause();

        AudioManager.Instance.PlayMusic("Normal Level Music (Cover) 1");

        await UniTask.WaitForEndOfFrame(this);
        await UniTask.Delay(500);
        FXManager.Instance.isNextSceneReady = true;
        await UniTask.Delay(500);
        gameFlow.RequestResume();
    }
}
