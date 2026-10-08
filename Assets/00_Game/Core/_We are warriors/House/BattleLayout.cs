using Sirenix.OdinInspector;
using UnityEngine;

// Căn 2 house sát 2 mép màn hình (theo tỉ lệ máy thật) rồi trải grid ở giữa.
// Sprite house có pivot bottom-left: house ally đặt pivot ở mép trái, house enemy (xoay 180 Y) đặt pivot ở mép phải.
// Điểm spawn là con của house nên đi theo; điểm dừng đánh (wall) do House tự tính từ visual đang hiện.
public class BattleLayout : MonoBehaviour
{
    public Camera cam;
    public House allyHouse;
    public House enemyHouse;
    public BattleGrid grid;

    [Tooltip("Đẩy house vào trong mép màn hình (world unit). Âm = lấn ra ngoài")]
    public float edgePadding = 0f;

    // Gọi trước grid.Init() vì Init tạo mảng ô theo width
    [Button("Apply Layout")]
    public void Apply()
    {
        if (cam == null) return;
        float left = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f)).x + edgePadding;
        float right = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, 0f)).x - edgePadding;

        PlaceHouse(allyHouse, left);
        PlaceHouse(enemyHouse, right);

        // Grid phủ kín khoảng giữa 2 pivot house, phần lẻ không đủ 1 ô chia đều 2 bên
        if (grid != null)
        {
            float span = right - left;
            grid.width = Mathf.Max(1, Mathf.FloorToInt(span / grid.cellSize));
            float rest = span - grid.width * grid.cellSize;
            var p = grid.transform.position;
            grid.transform.position = new Vector3(left + rest * 0.5f, p.y, p.z);
        }
    }

    // Mọi visual của house dùng chung localPosition.x -> dời house để pivot visual nằm đúng mép
    static void PlaceHouse(House house, float edgeX)
    {
        if (house == null || house.houseVisuals == null || house.houseVisuals.Count == 0) return;
        float pivotOffset = house.houseVisuals[0].transform.position.x - house.transform.position.x;
        var p = house.transform.position;
        house.transform.position = new Vector3(edgeX - pivotOffset, p.y, p.z);
    }
}
