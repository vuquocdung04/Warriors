using System.Collections.Generic;
using EventDispatcher;
using UnityEngine;

public class BattleManager : StaffSingleton<BattleManager>
{
    private readonly List<Unit> _allies = new();
    private readonly List<Unit> _enemies = new();
    private readonly List<IDamageable> _allyTargets = new();   // unit + house theo phe
    private readonly List<IDamageable> _enemyTargets = new();
    public bool IsBattleOver { get; private set; }
    public override void Init()
    {
        IsBattleOver = false;
        _allies.Clear(); _enemies.Clear();
        _allyTargets.Clear(); _enemyTargets.Clear();
        this.RegisterListener(EventID.HOUSE_DESTROYED, OnHouseDestroyed);
        this.RegisterListener(EventID.APPLY_EFFECT_ALL_ALLIES, OnEffectAllAllies);
        this.RegisterListener(EventID.APPLY_EFFECT_ALL_ENEMIES, OnEffectAllEnemies);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        this.RemoveListener(EventID.HOUSE_DESTROYED, OnHouseDestroyed);
        this.RemoveListener(EventID.APPLY_EFFECT_ALL_ALLIES, OnEffectAllAllies);
        this.RemoveListener(EventID.APPLY_EFFECT_ALL_ENEMIES, OnEffectAllEnemies);
    }

    // param: Func<IStatusEffect> -> tạo effect MỚI cho từng unit (mỗi con đếm giờ riêng)
    void OnEffectAllAllies(object param) => ApplyToAll(_allies, param);
    void OnEffectAllEnemies(object param) => ApplyToAll(_enemies, param);

    void ApplyToAll(List<Unit> list, object param)
    {
        if (param is not System.Func<IStatusEffect> factory) return;
        for (int i = 0; i < list.Count; i++)
        {
            var u = list[i];
            if (u != null && u.IsAlive) u.Effects.Add(u, factory());
        }
    }

    void OnHouseDestroyed(object param)
    {
        if (IsBattleOver) return;
        var loserTeam = (Team)param;
        EndBattle();

        bool allyWin = loserTeam == Team.Enemy;
        Debug.Log($"[Battle] House {loserTeam} sập -> {(allyWin ? "THẮNG" : "THUA")}");
        // TODO (khi bật GameFlow.Init): GameFlow nghe event này để show popup Win/Lose
    }

    void Update()
    {
          if (IsBattleOver) return;
        Tick(Time.deltaTime);
    }
    public void Register(Unit u)
    {
        (u.team == Team.Ally ? _allies : _enemies).Add(u);
        (u.team == Team.Ally ? _allyTargets : _enemyTargets).Add(u);
    }

    public void RegisterHouse(House h)
    {
        (h.team == Team.Ally ? _allyTargets : _enemyTargets).Add(h);
    }
    public void EndBattle() 
    {
        IsBattleOver = true;
    }

    List<IDamageable> TargetsOf(Team team) => team == Team.Ally ? _enemyTargets : _allyTargets;

    public void Tick(float dt)
    {
        TickList(_allies, dt);
        TickList(_enemies, dt);
        Cleanup(_allies); Cleanup(_enemies);
        CleanupTargets(_allyTargets); CleanupTargets(_enemyTargets);
    }

    void TickList(List<Unit> list, float dt)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i] != null && list[i].IsAlive) list[i].Tick(dt);
    }

    void Cleanup(List<Unit> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
            if (list[i] == null || !list[i].IsAlive) list.RemoveAt(i);
    }

    void CleanupTargets(List<IDamageable> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
            if (list[i] == null || !list[i].IsAlive) list.RemoveAt(i);
    }

    public IDamageable FindNearestEnemyInRange(Unit self, float range)
    {
        var targets = TargetsOf(self.team);
        float rangeSqr = range * range;
        Vector3 p = self.transform.position;

        IDamageable best = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < targets.Count; i++)
        {
            var t = targets[i];
            if (t == null || (t is Object o && o == null) || !t.IsAlive) continue;
            float d = t.SqrDistanceTo(p);
            if (d <= rangeSqr && d < bestSqr) { bestSqr = d; best = t; }
        }
        return best;
    }
}