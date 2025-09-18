// Assets/Scripts/Projectile.cs
using UnityEngine;

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

    Transform _owner;
    Vector2 _dir;
    Vector2 _start;

    void OnEnable()
    {
        _start = transform.position;
    }

    public void Init(Transform owner, Vector2 dir, float speed, float maxDist, int dmg, LayerMask mask, GameObject hitFx = null)
    {
        _owner = owner;
        _dir = dir.normalized;
        this.speed = speed;
        this.maxDistance = maxDist;
        this.damage = dmg;
        this.hitMask = mask;
        this.hitEffectPrefab = hitFx;
        _start = transform.position;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // 이동
        transform.position += (Vector3)(_dir * speed * dt);

        // 사거리 초과 시 제거
        if ((Vector2)transform.position == _start) return;
        if (((Vector2)transform.position - _start).sqrMagnitude >= (maxDistance * maxDistance))
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & hitMask.value) == 0) return;

        if (other.TryGetComponent<IDamageable>(out var d))
        {
            d.ApplyDamage(damage, DamageType.Bullet);   // ✅ 통일
        }

        if (hitEffectPrefab)
        {
            Vector2 hitPoint = other.ClosestPoint(transform.position);
            Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
