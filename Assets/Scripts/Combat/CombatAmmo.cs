using System;
using UnityEngine;

public class CombatAmmo : MonoBehaviour
{
    [Header("Magazine / Reserve")]
    [SerializeField] int magazineCapacity = 6; // 장탄수
    [SerializeField] int magazine = 0;         // 현재 장전탄
    [SerializeField] int reserve = 24;         // 예비탄

    public int MagazineCapacity => magazineCapacity;
    public int Magazine => magazine;
    public int Reserve => reserve;
    public bool HasAmmo => magazine > 0;
    public bool CanReload => reserve > 0 && magazine < magazineCapacity;

    public event Action OnAmmoChanged;

    /// <summary>한 발 소비(성공 시 true)</summary>
    public bool Consume(int count = 1)
    {
        if (count <= 0) return false;
        if (magazine < count) return false;
        magazine -= count;
        OnAmmoChanged?.Invoke();
        return true;
    }

    /// <summary>예비탄 추가(드롭/제작 등)</summary>
    public void AddReserve(int amount)
    {
        if (amount <= 0) return;
        reserve += amount;
        OnAmmoChanged?.Invoke();
    }

    /// <summary>장전. 실제로 옮겨진 탄 수를 반환</summary>
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

    // 디버그/치트용
    public void SetInstant(int mag, int res)
    {
        magazine = Mathf.Clamp(mag, 0, magazineCapacity);
        reserve = Mathf.Max(0, res);
        OnAmmoChanged?.Invoke();
    }
}
