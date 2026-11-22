using UnityEngine;
using UnityEngine.UI;

// Danh sách dungeon. Ô 1: boss khủng long (thumbnail + Play), chơi không giới hạn lượt.
public class DungeonsBox : BaseBox<DungeonsBox>
{
    [Header("Dino dungeon")]
    public Button btnPlayDino;

    private bool _leaving;

    protected override void Init()
    {
        if (btnPlayDino != null) btnPlayDino.OnClicked(PlayDino);
    }

    protected override void InitState() => _leaving = false;

    void PlayDino()
    {
        if (_leaving) return;
        _leaving = true;
        UseProfile.GameMode = GameMode.Dungeon;
        FXManager.Instance.LoadScene(SceneName.GAME_PLAY);
    }

    protected override void OnDestroy() => base.OnDestroy();
}
