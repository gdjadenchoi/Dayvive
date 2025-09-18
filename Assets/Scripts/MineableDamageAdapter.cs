using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Mineable))]
public class MineableDamageAdapter : MonoBehaviour, IDamageable
{
    Mineable mineable;

    void Awake() => mineable = GetComponent<Mineable>();

    // 자원은 "채굴" 데미지만 수용하고(=데미지 적용), 그 외(총알/폭발)는 무시
    public void ApplyDamage(int amount, DamageType type)
    {
        if (!mineable) return;

        if (type == DamageType.Mining)
        {
            // Mineable의 규격에 맞춰 위임
            mineable.ApplyDamage(Mathf.Max(1, amount), DamageType.Mining);
        }
        // Bullet/Explosive/Generic은 무시
    }
}
