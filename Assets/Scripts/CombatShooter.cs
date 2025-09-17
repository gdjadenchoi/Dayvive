using UnityEngine;

[RequireComponent(typeof(CombatAmmo))]
public class CombatShooter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] PlayerMode playerMode;
    [SerializeField] Transform player;
    [SerializeField] Camera cam;
    [SerializeField] Transform muzzle;
    [SerializeField] CombatAmmo ammo;

    [Header("Aim")]
    [SerializeField] float maxRange = 12f;

    [Header("Fire")]
    [SerializeField] float fireCooldown = 0.20f;

    [Header("Projectile")]
    [SerializeField] Projectile projectilePrefab;
    [SerializeField] float projectileSpeed = 20f;
    [SerializeField] LayerMask projectileHitMask;   // Resource/Enemy 등
    [SerializeField] int projectileDamage = 1;
    [SerializeField] GameObject hitEffectPrefab;

    float cd;

    void Reset()
    {
        player = transform;
        cam = Camera.main;
        ammo = GetComponent<CombatAmmo>();
    }

    void Awake()
    {
        if (!player) player = transform;
        if (!cam) cam = Camera.main;
        if (!ammo) ammo = GetComponent<CombatAmmo>();
        if (!playerMode) playerMode = GetComponent<PlayerMode>();
    }

    bool IsCombat() => playerMode == null || playerMode.Current == PlayerMode.Mode.Combat;

    void Update()
    {
        if (!IsCombat()) return;

        if (cd > 0f) cd -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.R))
        {
            int loaded = ammo.Reload();
            if (loaded > 0) Debug.Log($"Reload: +{loaded}");
        }

        if (Input.GetMouseButton(0))
            TryShoot();
    }

    void TryShoot()
    {
        if (cd > 0f) return;
        if (!projectilePrefab) return;

        if (!ammo.HasAmmo)
        {
            Debug.Log("No ammo");
            return;
        }

        Vector3 origin = muzzle ? muzzle.position : player.position;
        if (!cam) return;

        Vector3 m = cam.ScreenToWorldPoint(Input.mousePosition);
        m.z = origin.z;

        Vector2 dir = (m - origin).normalized;

        // 발사체 1회 생성 + 초기화
        var proj = Instantiate(projectilePrefab, origin, Quaternion.identity);
        proj.Init(owner: player,
                  direction: dir,
                  speed: projectileSpeed,
                  range: maxRange,
                  damage: projectileDamage,
                  hitMask: projectileHitMask,
                  hitFx: hitEffectPrefab);

        // 탄약/쿨다운
        if (ammo.Consume(1))
            cd = fireCooldown;

        Debug.Log("pew");
    }
}
