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
                var unlocked = HandleWin();
                GameScene.SetBlockRaycast(true);
                AudioManager.Instance.PlaySfx("sfx-Win");
                ShowBoxDelayed(true, unlocked).Forget();
                break;
            case GameState.Lose:
                GameScene.SetBlockRaycast(true);
                AudioManager.Instance.PlaySfx("sfx-Lose");
                ShowBoxDelayed(false, null).Forget();
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
    // Thắng hay thua đều dùng chung ResultBox (coin/gem giữ lại cả 2 trường hợp)
    async UniTaskVoid ShowBoxDelayed(bool win, HouseData unlocked)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f));
        _ = ResultBox.Setup(popupHolder, box =>
        {
            box.SetResult(win, unlocked);
            box.Show();
        });
    }

    // Trả về house vừa được mở nhờ trận thắng này, null nếu đánh lại civ cũ hoặc đã hết civ
    HouseData HandleWin()
    {
        string selected = UseProfile.SelectedEnemyCiv.Value;
        string currentEnemy = UseProfile.EnemyCiv.Value;

        var db = DataRepo.Instance.unitDatabase;
        int selectedOrder = db.GetCivOrder(selected);
        int enemyOrder = db.GetCivOrder(currentEnemy);
        int maxOrder = db.GetCivsByOrder().Count;

        if (selectedOrder < enemyOrder) return null;
        if (enemyOrder >= maxOrder)
        {
            UseProfile.WonFinalCiv.Value = true;
            return null;
        }
        var next = db.GetCivByOrder(enemyOrder + 1);
        if (next != null)
        {
            UseProfile.EnemyCiv.Value = next.civId;
        }
        return next;
    }
    void SetGameplayPaused(bool paused)
    {
        BattleManager.Instance.SetPause(paused);
        EnemyAI.Instance.SetPause(paused);
        BottomBar.Instance.foodBar.SetPause(paused);
        BattleSpawner.Instance.SetHousesInvincible(paused);
    }
}