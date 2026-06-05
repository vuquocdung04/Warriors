using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyScene : MonoBehaviour
{
    public NavController navController;
    public async UniTask InitAsync()
    {
        navController.Init();

        await PreLoad();
    }

    private static async UniTask PreLoad()
    {
        var lobbyTcs = new UniTaskCompletionSource();
        var shopTcs = new UniTaskCompletionSource();
        var rankTcs = new UniTaskCompletionSource();
        var upgradesTcs = new UniTaskCompletionSource();
        var holder = LobbyController.Instance.botCanvas;
        _ = LobbyBox.Setup(holder, box =>
        {
            box.Show();
            lobbyTcs.TrySetResult();
        });

        _ = UpgradesBox.Setup(holder, _ => upgradesTcs.TrySetResult());

        _ = ShopBox.Setup(holder, _ => shopTcs.TrySetResult());

        _ = DungeonsBox.Setup(holder, _ => rankTcs.TrySetResult());
        _ = SkillBox.Setup(holder, _ => rankTcs.TrySetResult());

        await UniTask.WhenAll(lobbyTcs.Task, shopTcs.Task, rankTcs.Task);

        FXManager.Instance.isNextSceneReady = true;
    }

    public void NavigateTo(ENavType type) => navController.NavigateTo(type);
}