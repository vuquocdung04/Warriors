using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Nền văn minh enemy")]
    public string civId = "stone";

    [Header("Tốc độ sinh")]
    [Tooltip("Giây giữa 2 lần sinh (nhỏ = sinh nhanh)")]
    public float spawnInterval = 2f;

    [Header("Tỉ lệ chọn lính (stone1 < stone2 < stone3 về độ mạnh)")]
    [Tooltip("Trọng số chọn từng lính theo index. Càng lớn càng hay ra.")]
    public float weightUnit1 = 5f;
    public float weightUnit2 = 3f;
    public float weightUnit3 = 1f;

    private BattleSpawner _spawner;
    private float _timer;
    private bool _running;

    public void Init(BattleSpawner spawner)
    {
        _spawner = spawner;
        _timer = spawnInterval;
        _running = true;
    }

    void Update()
    {
        if (_spawner == null || BattleManager.Instance == null) return;
        if (BattleManager.Instance.IsBattleOver) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _timer = spawnInterval;
            SpawnOne();
        }
    }

    void SpawnOne()
    {
        int index = PickWeightedIndex();
        _spawner.Spawn(Team.Enemy, UseProfile.EnemyCiv.Value, index);
    }

    int PickWeightedIndex()   // 0/1/2 theo trọng số
    {
        float total = weightUnit1 + weightUnit2 + weightUnit3;
        float r = Random.value * total;
        if (r < weightUnit1) return 0;
        if (r < weightUnit1 + weightUnit2) return 1;
        return 2;
    }

    public void SetRunning(bool on) => _running = on;
}