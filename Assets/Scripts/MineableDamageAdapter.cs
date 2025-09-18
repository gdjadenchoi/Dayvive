using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Mineable))]
public class MineableDamageAdapter : MonoBehaviour, IDamageable
{
    Mineable mineable;

    void Awake() => mineable = GetComponent<Mineable>();

    // �ڿ��� "ä��" �������� �����ϰ�(=������ ����), �� ��(�Ѿ�/����)�� ����
    public void ApplyDamage(int amount, DamageType type)
    {
        if (!mineable) return;

        if (type == DamageType.Mining)
        {
            // Mineable�� �԰ݿ� ���� ����
            mineable.ApplyDamage(Mathf.Max(1, amount), DamageType.Mining);
        }
        // Bullet/Explosive/Generic�� ����
    }
}
