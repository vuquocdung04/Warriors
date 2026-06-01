// Strategy: mỗi hiệu ứng (băng, độc, slow, buff...) là 1 class implement cái này.
// Effect tự chỉnh chỉ số của Unit qua u.Effects khi OnApply / OnRemove.
public interface IStatusEffect
{
    bool IsDone { get; }            // hết hạn -> UnitEffects tự gỡ
    void OnApply(Unit u);           // lúc mới dính
    void OnTick(Unit u, float dt);  // mỗi frame (đếm giờ, độc trừ máu...)
    void OnRemove(Unit u);          // lúc gỡ (trả lại chỉ số)
}
