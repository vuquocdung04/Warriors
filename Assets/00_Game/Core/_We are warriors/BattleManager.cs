using System.Collections.Generic;
using EventDispatcher;
using UnityEngine;

public class BattleManager : StaffSingleton<BattleManager>
{
    private readonly List<Unit> _allies = new();
    private readonly List<Unit> _enemies = new();

    public IReadOnlyList<Unit> Allies => _allies;
    public IReadOnlyList<Unit> Enemies => _enemies;
    private readonly List<IDamageable> _allyTargets = new();   // unit + house theo phe
    private readonly List<IDamageable> _enemyTargets = new();
    public bool IsBattleOver { get; private set; }
    public override void Init()
    {
        IsBattleOver = false;
        _allies.Clear(); _enemies.Clear();
        _allyTargets.Clear(); _enemyTargets.Clear();
        this.RegisterListener(EventID.HOUSE_DESTROYED, OnHouseDestroyed);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        this.RemoveListener(EventID.HOUSE_DESTROYED, OnHouseDestroyed);
    }

    void OnHouseDestroyed(object param)
    {
        if (IsBattleOver) return;
        var loserTeam = (Team)param;
        EndBattle();

        bool allyWin = loserTeam == Team.Enemy;
        Debug.Log($"[Battle] House {loserTeam} sập -> {(allyWin ? "THẮNG" : "THUA")}");
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