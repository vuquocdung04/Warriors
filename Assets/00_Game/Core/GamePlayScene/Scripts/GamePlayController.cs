
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
    public BattleLayout layout;
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
        FXManager.Instance.HoldReveal();
        gameFlow.Init();
        var db = DataRepo.Instance.unitDatabase;

        if (layout != null) layout.Apply();   // căn house theo mép màn hình trước khi tạo grid
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
        FXManager.Instance.NotifySceneReady();

        // Load ngầm các popup trong trận để lúc mở/thắng/thua không bị delay
        UniTask.WhenAll(
            SettingGameBox.Preload(),
            QuitLevelBox.Preload(),
            WinBox.Preload(),
            LoseBox.Preload()).Forget();
        await UniTask.Delay(500);
    }
}
