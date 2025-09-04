// MiningSystem.cs
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class MiningSystem : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform cursor;                 // 보통 Player transform
    [SerializeField] CircleCollider2D sensor;
    [SerializeField] LayerMask mineableMask;

    [Header("Mining Tick")]
    [SerializeField] float ticksPerSecond = 3f;
    [SerializeField] int damagePerTick = 1;
    [SerializeField] float candidatePadding = 0.5f; // 후보 수집 반경 여유(자원 크기 다양성 대비)
    [SerializeField] float epsilon = 0.01f;         // 수치 안정용

    [Header("Dwell (start mining only when settled)")]
    [SerializeField] float settleTime = 0.35f;
    [SerializeField] float startSpeedThreshold = 0.40f;
    [SerializeField] float stopSpeedThreshold = 0.80f;
    [SerializeField] float jitterRadius = 0.03f;
    [SerializeField, Range(0f, 1f)] float speedSmoothing = 0.20f;

    [Header("Range Settings (단일 소스)")]
    [SerializeField] float rangeRadius = 0.60f;        // ← 인스펙터에서 조절하면 '판정'에 바로 적용됨

    [Header("Debug")]
    [SerializeField] bool alwaysMine = false;          // 테스트용: true면 정지 판정 무시하고 항상 채굴
    [SerializeField] bool debugLog = false;            // 인스펙터에 상태/속도 노출
    public string debugState;

    public float DwellProgress01 { get; private set; } // UI 게이지용

    enum State { Moving, Settling, Mining }
    State state;
    Vector3 prevPos, anchorPos;
    float smoothedSpeed, dwellT, tickT;

    static readonly Collider2D[] buf = new Collider2D[256];
    static readonly Collider2D[] tmp = new Collider2D[256];
    ContactFilter2D filter;

    // ---------- 반경 단일 소스 적용 ----------
    void OnValidate()
    {
        if (!sensor) sensor = GetComponent<CircleCollider2D>();
        if (sensor)
        {
            sensor.isTrigger = true;
            sensor.offset = Vector2.zero;              // 중심 밀림 방지
            sensor.radius = Mathf.Max(0.001f, rangeRadius);
        }
    }

    void Awake()
    {
        if (!cursor) cursor = transform;
        if (!sensor) sensor = GetComponent<CircleCollider2D>();
        sensor.isTrigger = true;

        if (mineableMask.value == 0) mineableMask = LayerMask.GetMask("Resource");
        filter = new ContactFilter2D { useLayerMask = true, layerMask = mineableMask, useTriggers = true };

        prevPos = anchorPos = cursor.position;
        state = State.Moving;

        OnValidate(); // 시작 시에도 한 번 동기화
    }

    void Update()
    {
        // --- 속도 계산(지수평활) ---
        Vector3 p = cursor.position;
        float dt = Time.deltaTime;
        float instSpeed = (p - prevPos).magnitude / Mathf.Max(dt, 1e-5f);
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, instSpeed, speedSmoothing);
        prevPos = p;

        // ---- 항상 채굴(디버그) ----
        if (alwaysMine)
        {
            tickT += dt;
            float interval = 1f / Mathf.Max(1f, ticksPerSecond);
            if (tickT >= interval) { tickT = 0f; MineTick(); }
            if (debugLog) debugState = $"ALWAYS | speed={smoothedSpeed:F2}";
            return;
        }

        // --- 상태머신 ---
        switch (state)
        {
            case State.Moving:
                DwellProgress01 = 0f;
                dwellT = 0f;
                anchorPos = p;

                if (smoothedSpeed <= startSpeedThreshold)
                    state = State.Settling;
                break;

            case State.Settling:
                // 앵커에서 너무 멀어지면 리셋(손떨림 허용)
                if ((p - anchorPos).sqrMagnitude > jitterRadius * jitterRadius)
                {
                    state = (smoothedSpeed <= startSpeedThreshold) ? State.Settling : State.Moving;
                    anchorPos = p;
                    dwellT = 0f;
                    DwellProgress01 = 0f;
                    break;
                }

                // 느린 상태 유지 시간 누적
                if (smoothedSpeed <= startSpeedThreshold)
                {
                    dwellT += dt;
                    DwellProgress01 = Mathf.Clamp01(dwellT / settleTime);

                    if (dwellT >= settleTime)
                    {
                        state = State.Mining;
                        tickT = 0f;
                        DwellProgress01 = 1f;
                    }
                }
                else
                {
                    state = State.Moving;
                }
                break;

            case State.Mining:
                // 빠르게 움직이거나 앵커에서 벗어나면 해제(히스테리시스)
                if (smoothedSpeed >= stopSpeedThreshold ||
                    (p - anchorPos).sqrMagnitude > (jitterRadius * jitterRadius * 4f))
                {
                    state = State.Moving;
                    break;
                }

                // 틱 딜 적용
                tickT += dt;
                float interval = 1f / Mathf.Max(1f, ticksPerSecond);
                if (tickT >= interval)
                {
                    tickT = 0f;
                    MineTick();
                }
                break;
        }

        if (debugLog) debugState = $"{state} | speed={smoothedSpeed:F2} | dwell={DwellProgress01:P0}";
    }

    void MineTick()
    {
        Physics2D.SyncTransforms();

        // 센터/반경 (시각화 링과 1:1)
        Vector2 c = (Vector2)sensor.transform.position + sensor.offset;
        float r = GetWorldRadius();
        float r2 = r * r + epsilon * epsilon;

        // 1) 링보다 약간 넓게 후보 수집 (겹치지 않아도 "가까운" 것들 포함)
        int n = Physics2D.OverlapCircle(c, r + candidatePadding, filter, tmp);

        // 2) 후보들에 대해 "가장 가까운 점" 거리로 최종 판정
        for (int i = 0; i < n; i++)
        {
            var col = tmp[i];
            if (!col) continue;

            Vector2 q = col.ClosestPoint(c);          // 어떤 모양(Box/Poly/Circle)이라도 OK
            if ((q - c).sqrMagnitude <= r2)           // 링 안이라면 히트
            {
                if (col.TryGetComponent(out Mineable m))
                    m.ApplyDamage(damagePerTick);
            }
        }
    }

    public void ResetDwell()
    {
        state = State.Moving;
        dwellT = 0f;
        DwellProgress01 = 0f;
    }

    // MiningSystem.cs 내부 어디든(클래스 안) 추가
    public float GetWorldRadius()
    {
        if (!sensor) sensor = GetComponent<CircleCollider2D>();
        if (!sensor) return 0f;
        var s = sensor.transform.lossyScale;
        float scale = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
        return sensor.radius * scale; // = 실제 물리 판정 반경(월드 단위)
    }

#if UNITY_EDITOR
    // 디버그: '실제 물리 판정' 월드 반경을 씬에 그림 (RangeVisualizer와 1:1 비교)
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
