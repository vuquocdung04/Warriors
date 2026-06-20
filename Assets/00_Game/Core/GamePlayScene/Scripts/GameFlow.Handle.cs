using Cysharp.Threading.Tasks;
using UnityEngine;

public partial class GameFlow
{
    private void HandleStateEntered(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:

                break;
            case GameState.Paused:
                SetGameplayPaused(true);
                break;
            case GameState.Win:
                HandleWin();
                GameScene.SetBlockRaycast(true);
                AudioManager.Instance.PlaySfx("sfx-Win");
                ShowBoxDelayed(true).Forget();
                break;
            case GameState.Lose:
                GameScene.SetBlockRaycast(true);
                AudioManager.Instance.PlaySfx("sfx-Lose");
                ShowBoxDelayed(false).Forget();
                break;
            case GameState.BoosterActive:
                
                break;
            case GameState.Tutorial:
                
                break;
        }
    }

    private void HandleStateExited(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                break;
            case GameState.Paused:
                SetGameplayPaused(false);
                break;
            case GameState.BoosterActive:
                // close booster UI
                break;
            case GameState.Tutorial:
                // hide tutorial overlay
                break;
        }
    }
    async UniTaskVoid ShowBoxDelayed(bool win)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f));
        if (win) _ = WinBox.Setup(popupHolder, box => box.Show());
        else _ = LoseBox.Setup(popupHolder, box => box.Show());
    }
    void HandleWin()
    {
        string selected = UseProfile.SelectedEnemyCiv.Value;
        string currentEnemy = UseProfile.EnemyCiv.Value;

        var db = DataRepo.Instance.unitDatabase;
        int selectedOrder = db.GetCivOrder(selected);
        int enemyOrder = db.GetCivOrder(currentEnemy);
        int maxOrder = db.GetCivsByOrder().Count;

        if (selectedOrder < enemyOrder) return;     
        if (enemyOrder >= maxOrder)                   
        {
            UseProfile.WonFinalCiv.Value = true;
            return;
        }
        var next = db.GetCivByOrder(enemyOrder + 1);
        if (next != null)
        {
            UseProfile.EnemyCiv.Value = next.civId;
        }
    }
    void SetGameplayPaused(bool paused)
    {
        BattleManager.Instance.SetPause(paused);
        EnemyAI.Instance.SetPause(paused);
        BottomBar.Instance.foodBar.SetPause(paused);
        BattleSpawner.Instance.SetHousesInvincible(paused);
    }
}