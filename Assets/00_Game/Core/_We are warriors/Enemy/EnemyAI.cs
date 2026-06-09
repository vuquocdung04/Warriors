using System.Threading;
using Cysharp.Threading.Tasks;
using EventDispatcher;
using UnityEngine;
public class WaveInfo
{
    public int current;
    public int total;
    public WaveInfo(int current, int total) { this.current = current; this.total = total; }
}
public class EnemyAI : MonoBehaviour
{
    private BattleSpawner _spawner;
    private EnemyCivConfig _config;
    private CancellationTokenSource _cts;

    public int CurrentWave { get; private set; }
    public int TotalWaves => _config != null ? _config.waves.Count : 0;

    public void Init(BattleSpawner spawner)
    {
        _spawner = spawner;

        string civId = UseProfile.EnemyCiv.Value;
        _config = DataRepo.Instance.enemyWaveDatabase.Get(civId);
        if (_config == null)
        {
            Debug.LogError($"[EnemyAI] không có config civ {civId}");
            return;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        RunWaves(_cts.Token).Forget();
    }

    async UniTaskVoid RunWaves(CancellationToken token)
    {
        try
        {
            for (int w = 0; w < _config.waves.Count; w++)
            {
                var wave = _config.waves[w];

                if (wave.delayBetweenWave > 0f)
                    await UniTask.Delay(System.TimeSpan.FromSeconds(wave.delayBetweenWave), cancellationToken: token);

                CurrentWave = wave.wave;
                this.PostEvent(EventID.ON_ENEMY_WAVE_CHANGED, new WaveInfo(CurrentWave, TotalWaves));

                foreach (var entry in wave.spawns)
                {
                    await UniTask.Delay(System.TimeSpan.FromSeconds(entry.spawnDelay), cancellationToken: token);
                    SpawnEntry(entry);
                }
            }
            // hết wave cuối -> dừng
        }
        catch (System.OperationCanceledException) { }
    }

    void SpawnEntry(SpawnEntry entry)
    {
        ParseUnits(entry.units, (unitId, count) =>
        {
            for (int i = 0; i < count; i++)
                _spawner.SpawnEnemyById(unitId);
        });
    }

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    static void ParseUnits(string units, System.Action<string, int> onEach)
    {
        if (string.IsNullOrEmpty(units)) return;
        foreach (var part in units.Split(','))
        {
            var p = part.Trim();
            if (p.Length == 0) continue;
            int colon = p.IndexOf(':');
            if (colon < 0) continue;
            string id = p.Substring(0, colon).Trim().ToLower();
            if (int.TryParse(p.Substring(colon + 1).Trim(), out int count) && count > 0)
                onEach(id, count);
        }
    }
}