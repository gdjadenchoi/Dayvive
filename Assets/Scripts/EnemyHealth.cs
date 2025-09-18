// Assets/Scripts/EnemyHealth.cs
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] int maxHP = 3;
    int hp;

    void OnEnable() => hp = Mathf.Max(1, maxHP);

    public void ApplyDamage(int amount, DamageType type)
    {
        if (hp <= 0) return;
        hp = Mathf.Max(0, hp - Mathf.Max(1, amount));
        if (hp == 0) Die();
    }

    void Die()
    {
        // TODO: 적 사망 드롭/연출
        Destroy(gameObject);
    }
}
