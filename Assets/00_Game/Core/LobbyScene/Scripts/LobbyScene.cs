using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyScene : MonoBehaviour
{
    public NavController navController;
    public async UniTask InitAsync()
    {
        FXManager.Instance.HoldReveal();
        navController.Init();

        await PreLoad();
    }

    private static async UniTask PreLoad()
    {
        var lobbyTcs = new UniTaskCompletionSource();
        var shopTcs = new UniTaskCompletionSource();
        var upgradesTcs = new UniTaskCompletionSource();
        var dungeonsBox = new UniTaskCompletionSource();
        var skillBox = new UniTaskCompletionSource();

        var holder = LobbyController.Instance.botCanvas;
        _ = LobbyBox.Setup(holder, box =>
        {
            box.Show();
            lobbyTcs.TrySetResult();
        });

        _ = UpgradesBox.Setup(holder, _ => upgradesTcs.TrySetResult());
        _ = ShopBox.Setup(holder, _ => shopTcs.TrySetResult());
        _ = DungeonsBox.Setup(holder, _ => dungeonsBox.TrySetResult());
        _ = SkillBox.Setup(holder, _ => skillBox.TrySetResult());

        await UniTask.WhenAll(lobbyTcs.Task, shopTcs.Task, dungeonsBox.Task, skillBox.Task,upgradesTcs.Task);

        FXManager.Instance.NotifySceneReady();

        // Load ngầm các popup mở từ lobby để lần đầu bấm không bị delay
        UniTask.WhenAll(
            SettingLobbyBox.Preload(),
            EquipmentBox.Preload(),
            DetailEquipBox.Preload(),
            DetailSkillBox.Preload(),
            GachaResultBox.Preload(),
            RateRelicsBox.Preload(),
            AgesTimelineBox.Preload()).Forget();
    }

    public void NavigateTo(ENavType type) => navController.NavigateTo(type);
}