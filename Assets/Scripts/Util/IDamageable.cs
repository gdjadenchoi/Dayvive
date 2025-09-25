// Assets/Scripts/IDamageable.cs
using UnityEngine;
public enum DamageType
{
    Generic = 0,
    Bullet = 1,
    Mining = 2,
    Explosion = 3,
}

public interface IDamageable
{
    void ApplyDamage(int amount, DamageType type);
}
