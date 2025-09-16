using UnityEngine;

[RequireComponent(typeof(CombatAmmo))]
public class CombatShooter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] PlayerMode playerMode;
    [SerializeField] Transform player;       // 총알 시작점 기준(없으면 this)
    [SerializeField] Camera cam;             // 메인 카메라
    [SerializeField] Transform muzzle;       // 없으면 player.position 사용
    [SerializeField] CombatAmmo ammo;

    [Header("Aim")]
    [SerializeField] float maxRange = 12f;

    [Header("Fire")]
    [SerializeField] float fireCooldown = 0.20f;

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

    bool IsCombat()
        => playerMode == null || playerMode.Current == PlayerMode.Mode.Combat;

    void Update()
    {
        // 모드 가드
        if (!IsCombat()) return;

        // 쿨다운
        if (cd > 0f) cd -= Time.deltaTime;

        // 장전
        if (Input.GetKeyDown(KeyCode.R))
        {
            int loaded = ammo.Reload();              // ← CombatAmmo.Reload() 사용
            if (loaded > 0) Debug.Log($"Reload: +{loaded}");
        }

        // 발사
        if (Input.GetMouseButton(0))
        {
            TryShoot();
        }
    }

    void TryShoot()
    {
        if (cd > 0f) return;

        // 탄 없는 경우
        if (!ammo.HasAmmo)
        {
            // 여기서 자동장전 원하면: if (ammo.Reload() > 0) return; 등으로 처리 가능
            Debug.Log("No ammo");
            return;
        }

        // 소비
        if (!ammo.Consume(1)) return;               // ← CombatAmmo.Consume() 사용

        // 방향 계산
        Vector3 origin = muzzle ? muzzle.position : player.position;
        Vector3 m = cam ? cam.ScreenToWorldPoint(Input.mousePosition) : origin;
        m.z = origin.z;
        Vector3 dir = (m - origin).normalized;

        // 여기서는 레이만 쏘고 로그(나중에 총알/이펙트로 교체)
        Vector3 end = origin + dir * maxRange;
        Debug.DrawLine(origin, end, Color.red, 0.05f);

        Debug.Log("pew");
        cd = fireCooldown;
    }
}
