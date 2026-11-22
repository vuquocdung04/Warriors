using EventDispatcher;
using UnityEngine;

// Điều phối chế độ Dungeon trong GamePlay: ẩn nhà enemy, spawn boss khủng long ở mép phải, gắn thanh máu boss,
// và tính phần thưởng khi thắng theo tốc độ kiếm tiền của civ người chơi (EconomyConfig.coinPerKill / gemPerKill)
public class DungeonController : MonoBehaviour
{
    public static DungeonController Instance { get; private set; }

    [Header("Boss")]
    public DungeonBoss bossPrefab;
    [Tooltip("Scale boss trong trận (rig gốc khá to so với lính)")]
    public float bossScale = 0.75f;
    [Tooltip("Boss xuất hiện cách mép phải màn hình bao nhiêu (world unit, dương = ngoài màn hình)")]
    public float spawnOutside = 1.5f;
    [Tooltip("Chân boss (đáy rig, local y = -3.75) đặt thấp hơn tâm làn bao nhiêu")]
    public float feetBelowLane = 1.3f;
    [Tooltip("Unit thứ mấy (0-based) của civ dùng làm gốc stat boss")]
    public int baseUnitIndex = 2;

    [Header("UI")]
    public BossHpBar hpBar;
    public string dungeonTitle = "Dungeon";

    [Header("Reward khi thắng = tiền 1 lần giết của civ người chơi x hệ số")]
    public int coinKillMultiplier = 40;
    public int gemKillMultiplier = 4;

    const float RIG_FEET_LOCAL_Y = -3.75f;   // đáy chân của rig dino (đo lúc dựng pivot)

    public DungeonBoss Boss { get; private set; }

    void Awake() => Instance = this;

    public void Init(House enemyHouse, Camera cam)
    {
        if (enemyHouse != null) enemyHouse.gameObject.SetActive(false);   // dungeon không có nhà enemy

        var db = DataRepo.Instance.unitDatabase;
        var units = db.GetCivUnits(UseProfile.CurrentCiv.Value);
        UnitData baseUnit = units.Count > 0 ? units[Mathf.Clamp(baseUnitIndex, 0, units.Count - 1)] : null;

        var grid = BattleGrid.Instance;
        float laneHalf = grid.Height * grid.cellSize * 0.5f;
        float laneY = grid.CellToWorld(new Vector2Int(0, 0)).y - grid.cellSize * 0.5f + laneHalf;

        float halfW = cam.orthographicSize * cam.aspect;
        float x = cam.transform.position.x + halfW + spawnOutside;
        float y = laneY - feetBelowLane - RIG_FEET_LOCAL_Y * bossScale;

        Boss = Instantiate(bossPrefab, new Vector3(x, y, 0f), Quaternion.identity);
        Boss.transform.localScale = new Vector3(-bossScale, bossScale, 1f);   // quay mặt sang trái
        Boss.frontOffset *= bossScale;
        Boss.aimOffset *= bossScale;
        Boss.Init(baseUnit, laneY, laneHalf);

        if (hpBar != null) hpBar.Bind(Boss);
    }

    // Gọi khi thắng dungeon: cộng thưởng + bắn event có số lượng để GameScene/ResultBox hiển thị
    public static void GrantWinReward()
    {
        var ctrl = Instance;
        int coinMul = ctrl != null ? ctrl.coinKillMultiplier : 40;
        int gemMul = ctrl != null ? ctrl.gemKillMultiplier : 4;

        var config = DataRepo.Instance.economyDatabase.Get(UseProfile.CurrentCiv.Value);
        if (config == null) return;

        int coin = (int)System.Math.Min(int.MaxValue, (long)config.coinPerKill * coinMul);
        int gem = (int)System.Math.Min(int.MaxValue, (long)config.gemPerKill * gemMul);

        if (coin > 0)
        {
            UseProfile.Coin.Value = (int)System.Math.Min(int.MaxValue, (long)UseProfile.Coin.Value + coin);
            ctrl?.PostEvent(EventID.CHANGE_COIN, coin);
        }
        if (gem > 0)
        {
            UseProfile.Gem.Value = (int)System.Math.Min(int.MaxValue, (long)UseProfile.Gem.Value + gem);
            ctrl?.PostEvent(EventID.CHANGE_GEM, gem);
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
