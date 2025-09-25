// MiningSystem.cs  (target-aware + Combat guard)
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class MiningSystem : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform cursor;                 // 보통 Player transform
    [SerializeField] CircleCollider2D sensor;          // Trigger = true
    [SerializeField] LayerMask mineableMask;
    [SerializeField] PlayerMode playerMode;            // ← 전투/채굴 모드 확인

    [Header("Mining Tick")]
    [SerializeField] float ticksPerSecond = 3f;
    [SerializeField] int damagePerTick = 1;
    [SerializeField] float candidatePadding = 0.5f;
    [SerializeField] float epsilon = 0.01f;

    [Header("Dwell (start mining only when settled)")]
    [SerializeField] float settleTime = 0.35f;
    [SerializeField] float startSpeedThreshold = 0.40f;
    [SerializeField] float stopSpeedThreshold = 0.80f;
    [SerializeField] float jitterRadius = 0.03f;
    [SerializeField, Range(0f, 1f)] float speedSmoothing = 0.20f;

    [Header("Range Settings (단일 소스)")]
    [SerializeField] float rangeRadius = 0.60f;

    [Header("Debug")]
    [SerializeField] bool alwaysMine = false;
    [SerializeField] bool debugLog = false;
    public string debugState;

    public float DwellProgress01 { get; private set; }

    enum State { Moving, Settling, Mining }
    State state;
    Vector3 prevPos, anchorPos;
    float smoothedSpeed, dwellT, tickT;

    static readonly Collider2D[] tmp = new Collider2D[256];
    ContactFilter2D filter;

    Rigidbody2D rb;

    void OnValidate()
    {
        if (!sensor) sensor = GetComponent<CircleCollider2D>();
        if (sensor)
        {
            sensor.isTrigger = true;
            sensor.offset = Vector2.zero;
            sensor.radius = Mathf.Max(0.001f, rangeRadius);
        }
    }

    void Awake()
    {
        if (!cursor) cursor = transform;
        if (!sensor) sensor = GetComponent<CircleCollider2D>();
        sensor.isTrigger = true;

        if (!playerMode) playerMode = GetComponentInParent<PlayerMode>(); // 자동 참조

        rb = GetComponentInParent<Rigidbody2D>() ?? GetComponent<Rigidbody2D>();

        if (mineableMask.value == 0) mineableMask = LayerMask.GetMask("Resource");
        filter = new ContactFilter2D { useLayerMask = true, layerMask = mineableMask, useTriggers = true };

        prevPos = anchorPos = cursor.position;
        state = State.Moving;

        OnValidate();
    }

    void Update()
    {
        // === Combat 모드 가드: 채굴 완전 비활성 ===
        if (playerMode && playerMode.Current != PlayerMode.Mode.Mining)
        {
            ResetDwell();                 // 게이지/상태 초기화
            if (debugLog) debugState = "DISABLED (Combat)";
            return;
        }

        float dt = Time.deltaTime;

        // 속도 추정
        float instSpeed;
        if (rb != null) instSpeed = rb.linearVelocity.magnitude;
        else
        {
            Vector3 p = cursor.position;
            instSpeed = (p - prevPos).magnitude / Mathf.Max(dt, 1e-5f);
            prevPos = p;
        }
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, instSpeed, speedSmoothing);

        if (alwaysMine)
        {
            TickMine(dt);
            if (debugLog) debugState = $"ALWAYS | v={smoothedSpeed:F2}";
            return;
        }

        switch (state)
        {
            case State.Moving:
                DwellProgress01 = 0f;
                dwellT = 0f;
                anchorPos = cursor.position;

                if (smoothedSpeed <= startSpeedThreshold)
                    state = State.Settling;
                break;

            case State.Settling:
                if (((Vector3)cursor.position - anchorPos).sqrMagnitude > jitterRadius * jitterRadius)
                {
                    anchorPos = cursor.position;
                    dwellT = 0f;
                    DwellProgress01 = 0f;
                }

                if (smoothedSpeed <= startSpeedThreshold)
                {
                    dwellT += dt;
                    DwellProgress01 = Mathf.Clamp01(dwellT / settleTime);

                    // 타깃이 있어야만 채굴 진입
                    if (dwellT >= settleTime)
                    {
                        if (HasTargetInRange())
                        {
                            state = State.Mining;
                            tickT = 0f;
                            DwellProgress01 = 1f;
                        }
                        else
                        {
                            dwellT = 0f;
                            DwellProgress01 = 0f;
                        }
                    }
                }
                else
                {
                    state = State.Moving;
                }
                break;

            case State.Mining:
                if (smoothedSpeed >= stopSpeedThreshold ||
                    (((Vector3)cursor.position - anchorPos).sqrMagnitude > (jitterRadius * jitterRadius * 4f)))
                {
                    state = State.Moving;
                    break;
                }

                // 채굴 중 타깃이 사라지면 대기로 복귀
                if (!HasTargetInRange())
                {
                    state = State.Settling;
                    dwellT = 0f;
                    DwellProgress01 = 0f;
                    break;
                }

                TickMine(dt);
                break;
        }

        if (debugLog) debugState = $"{state} | v={smoothedSpeed:F2} | dwell={DwellProgress01:P0}";
    }

    void TickMine(float dt)
    {
        tickT += dt;
        float interval = 1f / Mathf.Max(1f, ticksPerSecond);
        if (tickT >= interval)
        {
            tickT = 0f;
            MineTick();
        }
    }

    void MineTick()
    {
        Physics2D.SyncTransforms();

        Vector2 c = (Vector2)sensor.transform.position + sensor.offset;
        float r = GetWorldRadius();
        float r2 = r * r + epsilon * epsilon;

        int n = Physics2D.OverlapCircle(c, r + candidatePadding, filter, tmp);
        for (int i = 0; i < n; i++)
        {
            var col = tmp[i];
            if (!col) continue;

            Vector2 q = col.ClosestPoint(c);
            if ((q - c).sqrMagnitude <= r2 && col.TryGetComponent(out Mineable m))
            {
                m.ApplyDamage(damagePerTick, DamageType.Mining);   // ✅ 통일
            }
        }
    }

    // === 공개 쿼리: 타깃 존재 여부 ===
    public bool HasTargetInRange()
    {
        Physics2D.SyncTransforms();

        Vector2 c = (Vector2)sensor.transform.position + sensor.offset;
        float r = GetWorldRadius();
        float r2 = r * r + epsilon * epsilon;

        int n = Physics2D.OverlapCircle(c, r + candidatePadding, filter, tmp);
        for (int i = 0; i < n; i++)
        {
            var col = tmp[i];
            if (!col) continue;

            Vector2 q = col.ClosestPoint(c);
            if ((q - c).sqrMagnitude <= r2 && col.TryGetComponent<Mineable>(out _))
                return true;
        }
        return false;
    }

    public void ResetDwell()
    {
        state = State.Moving;
        dwellT = 0f;
        DwellProgress01 = 0f;
    }

    public float GetWorldRadius()
    {
        if (!sensor) sensor = GetComponent<CircleCollider2D>();
        if (!sensor) return 0f;
        var s = sensor.transform.lossyScale;
        float scale = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
        return sensor.radius * scale;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!sensor) sensor = GetComponent<CircleCollider2D>();
        if (!sensor) return;
        var s = sensor.transform.lossyScale;
        float scale = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
        float r = sensor.radius * scale;

        Gizmos.color = Color.green;
        const int seg = 64;
        Vector3 c = sensor.transform.position + (Vector3)sensor.offset;
        Vector3 prev = c + new Vector3(r, 0, 0);
        for (int i = 1; i <= seg; i++)
        {
            float a = (i / (float)seg) * Mathf.PI * 2f;
            Vector3 p = c + new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0);
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }
#endif
}
