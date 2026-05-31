using UnityEngine;

public class BattleGrid : MonoBehaviour
{
    public int width = 30;
    public int height = 6;
    public float cellSize = 0.5f;

    private Unit[,] _cells;

    public int Width => width;
    public int Height => height;

    public void Init() => _cells = new Unit[width, height];

    public bool IsInside(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;
    public bool IsInside(Vector2Int c) => IsInside(c.x, c.y);
    public bool IsFree(int x, int y) => IsInside(x, y) && _cells[x, y] == null;
    public bool IsFree(Vector2Int c) => IsFree(c.x, c.y);
    public Unit GetOccupant(Vector2Int c) => IsInside(c) ? _cells[c.x, c.y] : null;

    public void Occupy(Vector2Int c, Unit u) { if (IsInside(c)) _cells[c.x, c.y] = u; }
    public void Free(Vector2Int c) { if (IsInside(c)) _cells[c.x, c.y] = null; }

    public Vector3 CellToWorld(Vector2Int c)
        => transform.position + new Vector3((c.x + 0.5f) * cellSize, (c.y + 0.5f) * cellSize, 0f);

    public Vector2Int WorldToCell(Vector3 world)
    {
        Vector3 local = world - transform.position;
        return new Vector2Int(Mathf.FloorToInt(local.x / cellSize), Mathf.FloorToInt(local.y / cellSize));
    }

    void OnDrawGizmos()
    {
        Vector3 origin = transform.position;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3 center = origin + new Vector3((x + 0.5f) * cellSize, (y + 0.5f) * cellSize, 0f);
                bool occupied = Application.isPlaying && _cells != null && _cells[x, y] != null;
                Gizmos.color = occupied ? new Color(0f, 1f, 0f, 0.35f) : new Color(1f, 0f, 0f, 0.12f);
                Gizmos.DrawCube(center, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.01f));
                Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
                Gizmos.DrawWireCube(center, new Vector3(cellSize, cellSize, 0.01f));
            }
    }
}