using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private readonly List<Unit> _allies = new List<Unit>();
    private readonly List<Unit> _enemies = new List<Unit>();

    public void Init()
    {
        Instance = this;
        _allies.Clear();
        _enemies.Clear();
    }

    public void Register(Unit u) => (u.team == Team.Ally ? _allies : _enemies).Add(u);
    public List<Unit> GetEnemiesOf(Team team) => team == Team.Ally ? _enemies : _allies;
    void Update()
    {
        Tick(Time.deltaTime);
    }
    public void Tick(float dt)   // gọi từ GameManager.Update của bạn
    {
        TickList(_allies, dt);
        TickList(_enemies, dt);
        Cleanup(_allies);
        Cleanup(_enemies);
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

    public Unit FindNearestEnemyInRange(Unit self, float range)
    {
        List<Unit> enemies = GetEnemiesOf(self.team);
        float rangeSqr = range * range;
        Vector3 p = self.transform.position;
        Unit best = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < enemies.Count; i++)
        {
            Unit e = enemies[i];
            if (e == null || !e.IsAlive) continue;
            float d = (e.transform.position - p).sqrMagnitude;
            if (d <= rangeSqr && d < bestSqr) { bestSqr = d; best = e; }
        }
        return best;
    }
}