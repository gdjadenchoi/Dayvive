using System;
using UnityEngine;

public class CombatAmmo : MonoBehaviour
{
    [Header("Magazine / Reserve")]
    [SerializeField] int magazineCapacity = 6; // ��ź��
    [SerializeField] int magazine = 0;         // ���� ����ź
    [SerializeField] int reserve = 24;         // ����ź

    public int MagazineCapacity => magazineCapacity;
    public int Magazine => magazine;
    public int Reserve => reserve;
    public bool HasAmmo => magazine > 0;
    public bool CanReload => reserve > 0 && magazine < magazineCapacity;

    public event Action OnAmmoChanged;

    /// <summary>�� �� �Һ�(���� �� true)</summary>
    public bool Consume(int count = 1)
    {
        if (count <= 0) return false;
        if (magazine < count) return false;
        magazine -= count;
        OnAmmoChanged?.Invoke();
        return true;
    }

    /// <summary>����ź �߰�(���/���� ��)</summary>
    public void AddReserve(int amount)
    {
        if (amount <= 0) return;
        reserve += amount;
        OnAmmoChanged?.Invoke();
    }

    /// <summary>����. ������ �Ű��� ź ���� ��ȯ</summary>
    public int Reload()
    {
        if (!CanReload) return 0;
        int need = magazineCapacity - magazine;
        int load = Mathf.Min(need, reserve);
        magazine += load;
        reserve -= load;
        OnAmmoChanged?.Invoke();
        return load;
    }

    // �����/ġƮ��
    public void SetInstant(int mag, int res)
    {
        magazine = Mathf.Clamp(mag, 0, magazineCapacity);
        reserve = Mathf.Max(0, res);
        OnAmmoChanged?.Invoke();
    }
}
