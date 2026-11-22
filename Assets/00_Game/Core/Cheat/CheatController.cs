using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Cheat dùng chung mọi scene: prefab Resources/Cheat tự sinh khi vào game, giữ qua các scene.
// Nút mở nằm sẵn trong prefab (góc trên phải), bấm 1 lần là hiện popup.
public class CheatController : MonoBehaviour
{
    private const string ResourcePath = "Cheat";

    private static CheatController instance;

    [SerializeField] private Button btnOpen;
    [SerializeField] private GameObject popupCheat;
    [SerializeField] private Button btnClose;

    [Header("Coin")]
    [SerializeField] private TMP_InputField inputCoin;
    [SerializeField] private Button btnApplyCoin;

    [Header("Gem")]
    [SerializeField] private TMP_InputField inputGem;
    [SerializeField] private Button btnApplyGem;

    [Header("Free mode: bỏ qua mọi chi phí coin/gem")]
    [SerializeField] private Toggle toggleFreeMode;

    [Header("Unlock all: mở hết civ enemy + civ hiện tại lên civ cuối")]
    [SerializeField] private Button btnUnlockAll;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        if (instance != null)
            return;

        CheatController prefab = Resources.Load<CheatController>(ResourcePath);

        if (prefab == null)
            return;

        instance = Instantiate(prefab);
        instance.name = prefab.name;

        DontDestroyOnLoad(instance.gameObject);
    }

    private void Awake()
    {
        if (btnOpen != null)
            btnOpen.onClick.AddListener(() => SetPopupActive(true));

        if (btnClose != null)
            btnClose.onClick.AddListener(() => SetPopupActive(false));

        if (btnApplyCoin != null)
            btnApplyCoin.onClick.AddListener(OnClickApplyCoin);

        if (btnApplyGem != null)
            btnApplyGem.onClick.AddListener(OnClickApplyGem);

        if (toggleFreeMode != null)
        {
            toggleFreeMode.SetIsOnWithoutNotify(UseProfile.CheatFreeMode.Value);
            toggleFreeMode.onValueChanged.AddListener(OnFreeModeChanged);
        }

        if (btnUnlockAll != null)
            btnUnlockAll.onClick.AddListener(OnClickUnlockAll);

        SetPopupActive(false);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private void SetPopupActive(bool active)
    {
        if (popupCheat == null)
            return;

        if (active)
            Refresh();

        popupCheat.SetActive(active);
    }

    private void Refresh()
    {
        if (inputCoin != null)
            inputCoin.text = UseProfile.Coin.Value.ToString();

        if (inputGem != null)
            inputGem.text = UseProfile.Gem.Value.ToString();

        if (toggleFreeMode != null)
            toggleFreeMode.SetIsOnWithoutNotify(UseProfile.CheatFreeMode.Value);
    }

    private void OnFreeModeChanged(bool on)
    {
        UseProfile.CheatFreeMode.Value = on;

        // cho các UI đang mở tính lại trạng thái đủ/thiếu tiền (nút evolve, giá đỏ...)
        this.PostEvent(EventID.CHANGE_COIN);
        this.PostEvent(EventID.CHANGE_GEM);
        this.PostEvent(EventID.ON_CIV_CHANGED);
    }

    private void OnClickApplyCoin()
    {
        if (!TryParse(inputCoin, out int coin) || coin < 0)
            return;

        UseProfile.Coin.Value = coin;

        this.PostEvent(EventID.CHANGE_COIN);   // không kèm số: tránh bị tính là coin nhặt được trong trận
    }

    private void OnClickApplyGem()
    {
        if (!TryParse(inputGem, out int gem) || gem < 0)
            return;

        UseProfile.Gem.Value = gem;

        this.PostEvent(EventID.CHANGE_GEM);
    }

    // Mở hết: civ enemy = civ cuối (đánh được mọi màn), civ người chơi = civ cuối, mở unit 2/3. Giữ nguyên coin/gem.
    private void OnClickUnlockAll()
    {
        var repo = DataRepo.Instance;
        if (repo == null || repo.unitDatabase == null)
            return;

        var civs = repo.unitDatabase.GetCivsByOrder();
        if (civs.Count == 0)
            return;

        string last = civs[civs.Count - 1].civId;
        UseProfile.EnemyCiv.Value = last;
        UseProfile.SelectedEnemyCiv.Value = last;
        UseProfile.CurrentCiv.Value = last;
        UseProfile.Unit2Unlock.Value = true;
        UseProfile.Unit3Unlock.Value = true;

        SetPopupActive(false);

        // Đang ở lobby: load lại để mọi box (map, nhà, upgrade, evolution...) dựng theo civ mới
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == SceneName.LOBBY_SCENE && FXManager.Instance != null)
        {
            FXManager.Instance.LoadScene(SceneName.LOBBY_SCENE);
            return;
        }

        this.PostEvent(EventID.ON_CIV_CHANGED);
        this.PostEvent(EventID.ON_EQUIPMENT_CHANGED);
        this.PostEvent(EventID.ON_FOOD_UI_CHANGED);
    }

    private static bool TryParse(TMP_InputField input, out int value)
    {
        value = 0;

        return input != null && int.TryParse(input.text.Trim(), out value);
    }
}
