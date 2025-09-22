using UnityEngine;

/// <summary>
/// 직선 이동형 프로젝타일.
/// - Init(...)로 파라미터 주입
/// - 빠른 속도에서도 누락되지 않도록 스윕(CircleCast)으로 충돌 판정
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Defaults (Init로 대체됨)")]
    [SerializeField] float speed = 20f;
    [SerializeField] float maxDistance = 12f;
    [SerializeField] int damage = 1;
    [SerializeField] LayerMask hitMask;
    [SerializeField] GameObject hitEffectPrefab;

    [Header("Sweep Cast")]
    [Tooltip("이동 경로 스윕 시 사용할 반지름(프로젝타일 반지름 권장)")]
    [SerializeField] float probeRadius = 0.05f;
    [Tooltip("스윕 캐스트 사용(권장). 비활성화하면 기존 OnTrigger만 사용")]
    [SerializeField] bool useSweepCast = true;

    Transform _owner;
    Vector2 _dir = Vector2.right;
    Vector2 _start;
    bool _initialized;

    void OnEnable()
    {
        _start = transform.position;
    }

    /// <summary>
    /// Shooter에서 호출해야 함.
    /// </summary>
    public void Init(Transform owner, Vector2 dir, float speed, float maxDist, int dmg, LayerMask mask, GameObject hitFx = null)
    {
        _owner = owner;
        _dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
        this.speed = speed;
        this.maxDistance = maxDist;
        this.damage = dmg;
        this.hitMask = mask;
        this.hitEffectPrefab = hitFx;

        _start = transform.position;
        _initialized = true;
    }

    void Update()
    {
        if (!_initialized) return;

        float dt = Time.deltaTime;
        Vector2 pos = transform.position;
        Vector2 delta = _dir * speed * dt;
        float stepDist = delta.magnitude;

        // 1) 스윕 캐스트로 이동 경로 상의 충돌 미리 체크
        if (useSweepCast && stepDist > 0f)
        {
            RaycastHit2D hit;
            if (probeRadius > 0f)
            {
                hit = Physics2D.CircleCast(pos, probeRadius, _dir, stepDist, hitMask);
            }
            else
            {
                hit = Physics2D.Raycast(pos, _dir, stepDist, hitMask);
            }

            if (hit.collider != null && !IsOwner(hit.collider))
            {
                // 충돌 지점으로 붙이고 처리
                transform.position = hit.point;
                ProcessHit(hit.collider, hit.point);
                return; // 파괴되므로 이후 로직 불필요
            }
        }

        // 2) 충돌 없었으면 이동
        transform.position = pos + delta;

        // 3) 최대 사거리 초과 시 파괴
        if ((Vector2.Distance(_start, transform.position)) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    // 트리거 충돌(스윕 비활성이나 여분 안전망용)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_initialized) return;
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;
        if (IsOwner(other)) return;

        ProcessHit(other, other.ClosestPoint(transform.position));
    }

    void ProcessHit(Collider2D other, Vector2 hitPoint)
    {
        // 데미지 적용
        if (other.TryGetComponent<IDamageable>(out var d))
        {
            d.ApplyDamage(Mathf.Max(1, damage), DamageType.Bullet);
        }

        // 히트 이펙트
        if (hitEffectPrefab)
        {
            Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    bool IsOwner(Collider2D col)
    {
        if (_owner == null) return false;
        return col.transform == _owner || col.transform.IsChildOf(_owner);
    }
}
