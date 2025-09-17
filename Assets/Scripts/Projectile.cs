using UnityEngine;

/// <summary>
/// 단순 직진 투사체:
/// - Shooter가 전달한 speed/maxDistance/mask로 런타임 오버라이드(InitRuntime)
/// - 거리 초과 시 자동 파괴
/// - 충돌 레이어가 mask에 포함되면 히트 이펙트 후 파괴
/// 필요 컴포넌트:
/// - Rigidbody2D(추천: Dynamic/Continuous 또는 Kinematic)
/// - CircleCollider2D (IsTrigger = true)
/// 프리팹 인스펙터 속도/사거리는 '예비값'이며, Shooter가 넘기면 덮어씀.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Move (defaults if no runtime override)")]
    [SerializeField] float speed = 20f;           // 프리팹 기본값 (런타임이 없을 때만 사용)
    [SerializeField] float maxDistance = 12f;     // 프리팹 기본값 (런타임이 없을 때만 사용)
    [SerializeField] LayerMask hitMask;           // 프리팹 기본값 (런타임이 없을 때만 사용)

    [Header("VFX (optional)")]
    [SerializeField] GameObject hitEffectPrefab;  // 충돌 지점 이펙트 (선택)

    // 런타임 오버라이드
    bool hasRuntime;
    float rtSpeed, rtMaxDistance;
    LayerMask rtMask;
    Transform owner;

    // 내부 상태
    Vector3 startPos;
    Vector2 moveDir = Vector2.right;

    /// <summary>
    /// Shooter에서 호출. 전달된 값으로 프리팹 값을 덮어씁니다.
    /// </summary>
    public void InitRuntime(Transform owner, Vector2 dir, float speed, float maxDistance, LayerMask mask, GameObject hitFx = null)
    {
        this.owner = owner;
        this.moveDir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;

        hasRuntime = true;
        rtSpeed = speed;
        rtMaxDistance = maxDistance;
        rtMask = mask;
        if (hitFx != null) hitEffectPrefab = hitFx;

        // 활성 직후에도 바로 이동하도록 초기화
        startPos = transform.position;
        enabled = true;
    }

    void OnEnable()
    {
        // 에디터에서 수동 배치 테스트 대비(런타임 인자 없을 때)
        startPos = transform.position;
        if (moveDir.sqrMagnitude < 0.0001f)
            moveDir = transform.right; // 회전으로 방향을 지정한 경우 대응
    }

    void Update()
    {
        float spd = hasRuntime ? rtSpeed : speed;
        float maxD = hasRuntime ? rtMaxDistance : maxDistance;

        transform.position += (Vector3)moveDir * spd * Time.deltaTime;

        // 사거리 초과 시 파괴 (가이드와 1:1 동기화)
        if ((transform.position - startPos).sqrMagnitude >= maxD * maxD)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 자신의 발사자(owner)와 충돌 무시 (있다면)
        if (owner && other.transform == owner) return;

        LayerMask mask = hasRuntime ? rtMask : hitMask;
        int otherBit = 1 << other.gameObject.layer;
        if ((mask.value & otherBit) == 0) return;

        // 히트 이펙트(선택)
        if (hitEffectPrefab)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
        }

        // (데미지 적용은 후속 단계에서 연결)
        // var dmg = other.GetComponent<IDamageable>();
        // if (dmg != null) dmg.ApplyDamage( ... );

        Destroy(gameObject);
    }
}
