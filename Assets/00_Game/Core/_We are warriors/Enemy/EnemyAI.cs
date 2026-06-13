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

public class EnemyAI : StaffSingleton<EnemyAI>
{
    private BattleSpawner _spawner;
    private EnemyCivConfig _config;
    private CancellationTokenSource _cts;

    public int CurrentWave { get; private set; }
    public int TotalWaves => _config != null ? _config.waves.Count : 0;
    public float spawnDelayMin = 0.1f;
    public float spawnDelayMax = 0.5f;
    private bool _paused;
    public void SetPause(bool p) => _paused = p;

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

    async UniTask PausableDelay(float seconds, CancellationToken token)
    {
        float t = 0f;
        while (t < seconds)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);
            if (_paused) continue;
            t += Time.deltaTime;
        }
    }

    async UniTaskVoid RunWaves(CancellationToken token)
    {
        try
        {
            for (int w = 0; w < _config.waves.Count; w++)
            {
                var wave = _config.waves[w];

                if (wave.delayBetweenWave > 0f)
                    await PausableDelay(wave.delayBetweenWave, token);   // SỬA: truyền đúng giá trị

                if (w == 0)
                    AudioManager.Instance.PlaySfx("StartBattleDrums");

                CurrentWave = wave.wave;
                this.PostEvent(EventID.ON_ENEMY_WAVE_CHANGED, new WaveInfo(CurrentWave, TotalWaves));

                foreach (var entry in wave.spawns)
                {
                    await PausableDelay(entry.spawnDelay, token);        // SỬA: PausableDelay
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
            await PausableDelay(delay, token);   // SỬA: PausableDelay (pause cả lúc spawn từng con)
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
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

    public override void Init()
    {

    }
}