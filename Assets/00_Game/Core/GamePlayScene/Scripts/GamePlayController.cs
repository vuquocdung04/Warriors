
using Cysharp.Threading.Tasks;
using EventDispatcher;
using UnityEngine;

public class GamePlayController : LeaderSingleton<GamePlayController>
{
    public Camera cameraGameplay;
    public GameScene gameScene;
    public GameFlow gameFlow;

    [Header("We are warriors")]
    public BattleGrid grid;
    public BattleManager battle;
    public BattleSpawner spawner;
    public BottomBar bottomBar;

    public SkillController skillController;
    public FlyTextSpawner flyTextSpawner;

    public EnemyAI enemyAI;
    public EnemyWaveUI enemyWaveUI;
    public DropController dropController;

    protected override void OnAwake()
    {
        base.OnAwake();
        Init().Forget();

    }
    private async UniTaskVoid Init()
    {
        gameFlow.Init();
        var db = DataRepo.Instance.unitDatabase;

        grid.Init();
        battle.Init();
        spawner.Init(db);
        enemyAI.Init(spawner);
        enemyWaveUI.Init();
        var allyUnits = db.GetCivUnits(UseProfile.CurrentCiv.Value);
        int startFood = allyUnits.Count > 0 ? Mathf.Max(0, allyUnits[0].foodCost - 2) : 0;

        bottomBar.Init(spawner, db, startFood);
        skillController.Init();
        dropController.Init();
        gameScene.Init();
        AudioManager.Instance.PlayMusic("GamePlay");

        await UniTask.WaitForEndOfFrame(this);
        await UniTask.Delay(500);
        Time.timeScale = 1;
        FXManager.Instance.isNextSceneReady = true;
        await UniTask.Delay(500);
    }
}
