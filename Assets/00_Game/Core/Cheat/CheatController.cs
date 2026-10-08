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

    private static bool TryParse(TMP_InputField input, out int value)
    {
        value = 0;

        return input != null && int.TryParse(input.text.Trim(), out value);
    }
}
