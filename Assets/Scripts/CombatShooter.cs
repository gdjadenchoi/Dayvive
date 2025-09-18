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
    [SerializeField] CombatAimGuide aimGuide;     // 에임 가이드(선택)

    [Header("Aim")]
    [SerializeField] float maxRange = 12f;        // 가이드/사거리 (에임 가이드 있으면 그 값 사용)

    [Header("Fire")]
    [SerializeField] float fireCooldown = 0.20f;
    float cd;

    [Header("Projectile")]
    [SerializeField] Projectile projectilePrefab; // 프리팹 필수
    [SerializeField] float projectileSpeed = 20f;
    [SerializeField] LayerMask projectileHitMask; // Enemy, Resource 등
    [SerializeField] int damageToEnemy = 1;       // 현재 Projectile은 단일 damage만 받음
    [SerializeField] GameObject hitEffectPrefab;

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

    bool IsCombat()
        => playerMode == null || playerMode.Current == PlayerMode.Mode.Combat;

    void Update()
    {
        if (!IsCombat()) return;

        if (cd > 0f) cd -= Time.deltaTime;

        // R: 장전
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

        // 탄 확인
        if (!ammo.HasAmmo) return;
        if (!cam) return;

        // 발사 방향 계산
        Vector3 origin = muzzle ? muzzle.position : player.position;
        Vector3 m = cam.ScreenToWorldPoint(Input.mousePosition);
        m.z = origin.z;
        Vector2 dir = ((Vector2)(m - origin)).sqrMagnitude > 0.0001f
                      ? (m - origin).normalized
                      : Vector2.right;

        // 에임 가이드와 동기화된 사거리 사용(없으면 로컬 maxRange)
        float range = aimGuide ? aimGuide.MaxRange : maxRange;

        // 발사체 생성 + 런타임 파라미터 주입
        var proj = Instantiate(projectilePrefab, origin, Quaternion.identity);
        // NOTE: Projectile은 Init(...)만 존재 (InitRuntime 없음)  :contentReference[oaicite:1]{index=1}
        proj.Init(
            owner: player,
            dir: dir,
            speed: projectileSpeed,
            maxDist: range,
            dmg: damageToEnemy,
            mask: projectileHitMask,
            hitFx: hitEffectPrefab
        );

        // 탄약 소모/쿨다운
        if (ammo.Consume(1))
            cd = fireCooldown;

        // Debug.DrawLine(origin, origin + (Vector3)dir * range, Color.red, 0.05f);
    }
}
