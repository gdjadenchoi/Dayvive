using UnityEngine;
using Dayvive.Data;      // WeaponData
using Dayvive.Weapons;   // Weapon

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
    [SerializeField] Weapon weapon;               // WeaponData 래퍼(선택)

    [Header("Aim")]
    [SerializeField] float maxRange = 12f;        // 기본 사거리 (WeaponData가 있으면 대체)
    [SerializeField] LayerMask projectileHitMask; // 피격 레이어

    [Header("Fire")]
    [SerializeField] GameObject projectilePrefab; // 기본 발사체 (WeaponData가 있으면 대체)
    [SerializeField] GameObject hitEffectPrefab;  // 히트 이펙트 (WeaponData가 있으면 대체)
    [SerializeField] float fireCooldown = 0.2f;   // 발사 간격(초) (WeaponData가 있으면 대체)
    [SerializeField] float projectileSpeed = 20f; // 탄속 (WeaponData가 있으면 대체)
    [SerializeField] int damageToEnemy = 1;       // 데미지 (WeaponData가 있으면 대체)

    [Header("Debug")]
    [SerializeField] bool debugLog = true;

    float cd;                                     // 쿨다운

    // ─────────────────────────────────────────────────────────────────────────────

    void Reset()
    {
        if (!player) player = transform;
        if (!cam) cam = Camera.main;
        if (!ammo) TryGetComponent(out ammo);
        if (!playerMode) playerMode = GetComponentInParent<PlayerMode>();
        if (!muzzle) muzzle = player;
        if (aimGuide) aimGuide.SetMaxRange(maxRange);
        if (!weapon) weapon = GetComponent<Weapon>();
    }

    void Awake()
    {
        if (!player) player = transform;
        if (!cam) cam = Camera.main;
        if (!ammo) TryGetComponent(out ammo);
        if (!playerMode) playerMode = GetComponentInParent<PlayerMode>();
        if (!muzzle) muzzle = player;
        if (!weapon) weapon = GetComponent<Weapon>();
        ApplyWeaponDataIfAny();
    }

    void OnEnable()
    {
        ApplyWeaponDataIfAny();
    }

    // WeaponData → 런타임 값으로 적용 (있으면 덮어씀, 없으면 기존 값 유지)
    void ApplyWeaponDataIfAny()
    {
        if (weapon != null && weapon.Data != null)
        {
            WeaponData d = weapon.Data;

            // 전투 수치
            maxRange = d.Range;
            projectileSpeed = d.ProjectileSpeed;
            damageToEnemy = d.Damage;
            fireCooldown = d.FireCooldown;

            // 프리팹/연출
            if (d.ProjectilePrefab) projectilePrefab = d.ProjectilePrefab;
            if (d.HitEffectPrefab) hitEffectPrefab = d.HitEffectPrefab;

            if (debugLog)
                Debug.Log($"[CombatShooter] WeaponData 적용: {d.WeaponId}, dmg={d.Damage}, range={d.Range}, cd={d.FireCooldown}, speed={d.ProjectileSpeed}");
        }

        if (aimGuide) aimGuide.SetMaxRange(maxRange);
    }

    void Update()
    {
        // Combat 모드일 때만 동작
        if (playerMode && !playerMode.IsCombat) return;

        // 쿨다운 감소
        if (cd > 0f) cd -= Time.deltaTime;

        // 장전
        if (Input.GetKeyDown(KeyCode.R))
        {
            int moved = ammo ? ammo.Reload() : 0;
            if (debugLog && moved > 0) Debug.Log($"[CombatShooter] Reloaded {moved}");
        }

        // 🔸 발사 입력: IsAutomatic에 따라 처리
        bool isAuto = weapon != null && weapon.Data != null && weapon.Data.IsAutomatic;

        if (isAuto)
        {
            // 연사 모드: 버튼을 누르고 있는 동안 쿨다운마다 발사
            if (Input.GetMouseButton(0))
                TryShoot();
        }
        else
        {
            // 단발 모드: 버튼을 누른 프레임에만 1발
            if (Input.GetMouseButtonDown(0))
                TryShoot();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────

    void TryShoot()
    {
        string reason;
        if (!CanShoot(out reason))
        {
            if (debugLog && !string.IsNullOrEmpty(reason))
                Debug.LogWarning($"[CombatShooter] 사격 불가: {reason}", this);
            return;
        }

        // 마우스 월드 좌표
        Vector3 mp = Input.mousePosition;
        Vector3 w = cam.ScreenToWorldPoint(mp);
        w.z = player.position.z;

        // 발사 방향
        Vector2 dir = (w - (muzzle ? muzzle.position : player.position));
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir.Normalize();

        // 사거리: 에임 가이드 우선 → 없으면 maxRange
        float range = aimGuide ? aimGuide.MaxRange : maxRange;

        // 프리팹 인스턴스
        GameObject go = Instantiate(projectilePrefab, (muzzle ? muzzle.position : player.position), Quaternion.identity);
        if (!go.TryGetComponent(out Projectile proj))
        {
            Debug.LogWarning("[CombatShooter] Projectile 프리팹에 Projectile 컴포넌트가 없습니다.", go);
            Destroy(go);
            return;
        }

        // 초기화
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
    }

    bool CanShoot(out string reason)
    {
        if (cd > 0f) { reason = $"쿨다운 {cd:0.00}s 남음"; return false; }
        if (!cam) { reason = "Camera 참조 없음"; return false; }
        if (!player) { reason = "player 참조 없음"; return false; }
        if (!ammo) { reason = "CombatAmmo 없음"; return false; }
        if (!ammo.HasAmmo) { reason = "장탄 0 (리로드 필요)"; return false; }
        if (!projectilePrefab) { reason = "projectilePrefab 미할당 (WeaponData 또는 인스펙터에서 설정)"; return false; }
        reason = null; return true;
    }
}
