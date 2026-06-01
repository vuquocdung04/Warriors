using UnityEngine;

public interface IDamageable
{
    bool IsAlive { get; }
    Team Team { get; }           
    Transform Transform { get; } 
    void TakeDamage(float dmg);
    float SqrDistanceTo(Vector3 p); 
}