using System.Collections.Generic;
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
    public float spawnDelayMin = 0.1f;
    public float spawnDelayMax = 0.5f;

    public void Init(BattleSpawner spawner)
    {
        _spawner = spawner;

        string civId = UseProfile.SelectedEnemyCiv.Value;
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
                    await SpawnEntry(entry, token);
                }
            }
        }
        catch (System.OperationCanceledException) { }
    }

    async UniTask SpawnEntry(SpawnEntry entry, CancellationToken token)
    {
        var queue = new List<string>();
        ParseUnits(entry.units, (unitId, count) =>
        {
            for (int i = 0; i < count; i++) queue.Add(unitId);
        });

        foreach (var id in queue)
        {
            _spawner.SpawnEnemyById(id);
            float delay = Random.Range(spawnDelayMin, spawnDelayMax);
            await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: token);
        }
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