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

                _ = WinBox.Setup(popupHolder, box =>
                    {
                        box.Show();
                    });
                AudioManager.Instance.PlaySfx("sfx-Win");
                break;
            case GameState.Lose:
                _ = LoseBox.Setup(popupHolder, box =>
                    {
                        box.Show();
                    });
                AudioManager.Instance.PlaySfx("sfx-Lose");
                break;
            case GameState.BoosterActive:
                // open booster UI
                break;
            case GameState.Tutorial:
                // show tutorial overlay
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
    void SetGameplayPaused(bool paused)
    {
        BattleManager.Instance.SetPause(paused);
        EnemyAI.Instance.SetPause(paused);
        BottomBar.Instance.foodBar.SetPause(paused);
        BattleSpawner.Instance.SetHousesInvincible(paused);
    }
}