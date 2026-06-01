using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour, IDamageable
{
    public Team team;
    public Team Team => team;
    public Transform Transform => transform;
    [Header("Kích thước tường (theo Y)")]
    public float halfHeight = 2.25f;
    public float wallOffsetX = 0f;
    [Header("Điểm cho lính")]
    public List<Transform> spawnPoints;   // lính ra rải từ nhiều điểm (không cùng 1 đường)

    [Header("Visual / HP bar (tuỳ)")]
    public UnitHpBar hpBar;

    private float _maxHp, _hp;
    private System.Action<Team> _onDestroyed;

    public bool IsAlive => _hp > 0f;

    public void Init(HouseData data, Team team, BattleGrid grid, System.Action<Team> onDestroyed, float hpMultiplier = 1f)
    {
        _maxHp = _hp = data.houseHp * hpMultiplier;
        this.team = team;
        _onDestroyed = onDestroyed;
        halfHeight = grid.Height * grid.cellSize / 2f;

        hpBar?.Set(1f);
        BattleManager.Instance.RegisterHouse(this);
    }
    public Vector3 GetSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return transform.position;
        return spawnPoints[Random.Range(0, spawnPoints.Count)].position;
    }
    private Vector3 ClosestPoint(Vector3 from)
    {
        Vector3 c = transform.position;
        float wallX = c.x + wallOffsetX;
        float y = Mathf.Clamp(from.y, c.y - halfHeight, c.y + halfHeight);
        return new Vector3(wallX, y, c.z);
    }
    public void TakeDamage(float dmg)
    {
        if (!IsAlive) return;
        _hp -= dmg;
        hpBar?.Set(_hp / _maxHp);
        if (_hp <= 0f) Die();
    }
    public float SqrDistanceTo(Vector3 p) => (ClosestPoint(p) - p).sqrMagnitude;
    void Die()
    {
        _hp = 0f;
        _onDestroyed?.Invoke(team);
    }
    void OnDrawGizmos()
    {
        Vector3 c = transform.position;
        float wallX = c.x + wallOffsetX;
        Vector3 top = new Vector3(wallX, c.y + halfHeight, c.z);
        Vector3 bot = new Vector3(wallX, c.y - halfHeight, c.z);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(top, bot);
        Gizmos.DrawWireSphere(top, 0.15f);
        Gizmos.DrawWireSphere(bot, 0.15f);
    }
}