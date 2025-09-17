using UnityEngine;

[RequireComponent(typeof(CombatAmmo))]
public class CombatShooter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] PlayerMode playerMode;
    [SerializeField] Transform player;            // 기준 포인트(없으면 this)
    [SerializeField] Camera cam;                  // 메인 카메라
    [SerializeField] Transform muzzle;            // 없으면 player.position 사용
    [SerializeField] CombatAmmo ammo;
    [SerializeField] CombatAimGuide aimGuide;     // (선택) 에임 가이드 동기화용

    [Header("Aim")]
    [SerializeField] float maxRange = 12f;        // 가이드/사거리

    [Header("Fire")]
    [SerializeField] float fireCooldown = 0.20f;

    [Header("Projectile")]
    [SerializeField] Projectile projectilePrefab; // 프리팹 필수
    [SerializeField] float projectileSpeed = 20f; // ★ 단일 출처 (Shooter가 보유)
    [SerializeField] LayerMask projectileHitMask; // Enemy, Resource 등
    [SerializeField] GameObject hitEffectPrefab;  // (선택)

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
        if (!aimGuide) aimGuide = GetComponentInChildren<CombatAimGuide>(true);
    }

    bool IsCombat()
        => playerMode == null || playerMode.Current == PlayerMode.Mode.Combat;

    void Update()
    {
        if (!IsCombat()) return;

        // 쿨다운
        if (cd > 0f) cd -= Time.deltaTime;

        // 장전
        if (Input.GetKeyDown(KeyCode.R))
        {
            int loaded = ammo.Reload();
            if (loaded > 0) Debug.Log($"Reload: +{loaded}");
        }

        // 좌클릭: 발사
        if (Input.GetMouseButton(0))
            TryShoot();
    }

    void TryShoot()
    {
        if (cd > 0f) return;
        if (!projectilePrefab) return;

        // 탄 없는 경우
        if (!ammo.HasAmmo)
        {
            Debug.Log("No ammo");
            return;
        }

        if (!cam) return;

        // 발사 방향 계산
        Vector3 origin = muzzle ? muzzle.position : player.position;
        Vector3 m = cam.ScreenToWorldPoint(Input.mousePosition);
        m.z = origin.z;
        Vector3 dir = (m - origin).normalized;

        // 발사체 생성 + 런타임 파라미터 전달(★ Shooter가 단일 출처)
        Quaternion rot = Quaternion.FromToRotation(Vector3.right, dir);
        var proj = Instantiate(projectilePrefab, origin, rot);
        proj.InitRuntime(
            owner: player,
            dir: dir,
            speed: projectileSpeed,
            maxDistance: maxRange,          // ★ 가이드와 완전 동기화
            mask: projectileHitMask,
            hitFx: hitEffectPrefab
        );

        // 탄약 소모/쿨다운
        if (ammo.Consume(1))
            cd = fireCooldown;

        Debug.Log("pew");
    }
}
