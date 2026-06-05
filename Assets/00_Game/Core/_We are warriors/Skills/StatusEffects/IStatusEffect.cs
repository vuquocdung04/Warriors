public interface IStatusEffect
{
    bool IsDone { get; }
    void Tick(Unit unit, float dt);   
    void OnApply(Unit unit) { }       
    void OnRemove(Unit unit) { }   
}