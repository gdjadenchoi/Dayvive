using UnityEngine;

[RequireComponent(typeof(Mineable))]
public class MineableDamageAdapter : MonoBehaviour, IDamageable
{
    Mineable mineable;

    void Awake()
    {
        mineable = GetComponent<Mineable>();
    }

    public void ApplyDamage(int amount)
    {
        if (mineable != null)
            mineable.TakeDamage(amount);  // Mineable.cs �ȿ� TakeDamage �޼��� �ִٰ� ����
    }
}
