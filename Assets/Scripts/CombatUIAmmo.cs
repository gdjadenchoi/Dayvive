using TMPro;
using UnityEngine;

public class CombatUIAmmo : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] CombatAmmo ammo;
    [SerializeField] TMP_Text ammoText; // ¿¹: "6 / 24"

    void Awake()
    {
    #if UNITY_2022_2_OR_NEWER
        if (!ammo) ammo = FindFirstObjectByType<CombatAmmo>();
    #else
        if (!ammo) ammo = FindObjectOfType<CombatAmmo>();
    #endif
        Refresh();
        if (ammo) ammo.OnAmmoChanged += Refresh;
    }
    void OnDestroy()
    {
        if (ammo) ammo.OnAmmoChanged -= Refresh;
    }

    void Refresh()
    {
        if (!ammo || !ammoText) return;
        ammoText.text = $"{ammo.Magazine} / {ammo.Reserve}";
    }
}
