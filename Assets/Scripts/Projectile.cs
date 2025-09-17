using UnityEngine;

/// 총알(직진형). 리지드바디로 전진, 사거리 초과/충돌 시 소멸.
/// Owner와의 충돌은 무시.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float speed = 20f;
    [SerializeField] float maxDistance = 12f;
    [SerializeField] LayerMask hitMask;

    [Header("Damage")]
    [SerializeField] int damage = 1;

    [Header("VFX (Optional)")]
    [SerializeField] GameObject hitEffectPrefab;

    Rigidbody2D rb;
    Collider2D col;
    Transform owner;          // 발사 주체 (충돌 무시용)
    Vector2 dir;
    Vector3 startPos;
    bool initialized;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // 기본 안전값
        rb.isKinematic = true;            // Kinematic 권장
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        col.isTrigger = true;             // 트리거로 충돌 판정
    }

    /// 초기화. 발사 직전에 반드시 호출.
    public void Init(Transform owner, Vector2 direction, float speed, float range, int damage, LayerMask hitMask, GameObject hitFx)
    {
        this.owner = owner;
        this.dir = direction.normalized;
        this.speed = speed;
        this.maxDistance = range;
        this.damage = damage;
        this.hitMask = hitMask;
        this.hitEffectPrefab = hitFx;

        startPos = transform.position;

        // Owner와 충돌 무시
        if (owner != null)
        {
            var ownerCol = owner.GetComponent<Collider2D>();
            if (ownerCol != null) Physics2D.IgnoreCollision(col, ownerCol, true);
        }

        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        // 전진
        rb.MovePosition(rb.position + dir * speed * Time.deltaTime);

        // 사거리 초과 시 제거
        if ((transform.position - startPos).sqrMagnitude > (maxDistance * maxDistance))
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;

        // Owner와는 무시
        if (owner != null && other.transform == owner) return;

        // 히트 마스크 체크
        if (((1 << other.gameObject.layer) & hitMask) == 0)
            return;

        // 간단 데미지 훅(상대가 IDamageable이면 전달)
        var dmg = other.GetComponent<IDamageable>();
        if (dmg != null) dmg.ApplyDamage(damage);

        // 이펙트
        if (hitEffectPrefab)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
