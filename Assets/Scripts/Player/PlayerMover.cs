using UnityEngine;

/// <summary>
/// Dayvive - Player movement controller (WASD/Arrow).
/// - 기본 입력: 키보드 (Horizontal/Vertical)
/// - 외부 입력(예: 터치 드래그) 지원: SetExternalMove() 호출 시 키보드 입력보다 우선 적용
/// - 이동: SpawnArea(BoxCollider2D) 영역 내로 클램프
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMover : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private bool useFixedUpdate = true;
    [Tooltip("외부 입력이 0이 아닐 경우, 키보드 입력보다 외부 입력을 우선 적용")]
    [SerializeField] private bool preferExternalInput = true;

    [Header("Clamp")]
    [Tooltip("플레이어를 이 영역(BoxCollider2D) 내부로 제한 (필드 영역)")]
    [SerializeField] private BoxCollider2D spawnArea;
    [SerializeField] private Vector2 clampPadding = new Vector2(0.3f, 0.3f); // 벽과의 여유 거리

    // components
    private Rigidbody2D rb;

    // input caches
    private Vector2 externalMove;   // 외부 시스템 입력에서 전달된 값
    private bool hasExternal;       // 외부 입력 여부 (한 프레임만 유지)

    // runtime
    private Vector2 currentMove;
    private float dt => useFixedUpdate ? Time.fixedDeltaTime : Time.deltaTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.angularDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        if (!useFixedUpdate)
        {
            Step(Time.deltaTime);
        }
        else
        {
            // FixedUpdate에서 처리
            // 단, 입력만 이곳에서 읽고 FixedUpdate에서 반영할 수도 있음
        }
    }

    void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            Step(Time.fixedDeltaTime);
        }
    }

    private void Step(float deltaTime)
    {
        // 1) 입력 읽기
        Vector2 input = ReadInput();

        // 2) 이동 벡터 계산
        currentMove = input.normalized * moveSpeed;

        // 3) 목표 위치 계산
        Vector2 targetPos = rb.position + currentMove * deltaTime;

        // 4) 스폰 영역으로 클램프
        if (spawnArea != null)
        {
            targetPos = ClampToSpawnArea(targetPos);
        }

        rb.MovePosition(targetPos);

        // 외부 입력은 한 프레임만 유지 → 초기화
        hasExternal = false;
    }

    private Vector2 ReadInput()
    {
        // 외부 입력(예: 터치 드래그)이 있으면 우선
        if (preferExternalInput && hasExternal && externalMove.sqrMagnitude > 0.0001f)
        {
            return Vector2.ClampMagnitude(externalMove, 1f);
        }

        // 키보드 입력 (Unity 기본 축)
        float x = Input.GetAxisRaw("Horizontal"); // A/D, ←/→
        float y = Input.GetAxisRaw("Vertical");   // W/S, ↑/↓
        return new Vector2(x, y);
    }

    private Vector2 ClampToSpawnArea(Vector2 worldPos)
    {
        // BoxCollider2D 기준 영역
        Bounds b = spawnArea.bounds;

        // 플레이어 자신의 콜라이더 크기 고려
        Vector2 pad = clampPadding;
        Collider2D selfCol = GetComponent<Collider2D>();
        if (selfCol != null)
        {
            Bounds sb = selfCol.bounds;
            pad.x = Mathf.Max(pad.x, sb.extents.x);
            pad.y = Mathf.Max(pad.y, sb.extents.y);
        }

        float minX = b.min.x + pad.x;
        float maxX = b.max.x - pad.x;
        float minY = b.min.y + pad.y;
        float maxY = b.max.y - pad.y;

        return new Vector2(Mathf.Clamp(worldPos.x, minX, maxX),
                           Mathf.Clamp(worldPos.y, minY, maxY));
    }

    /// <summary>
    /// 외부 시스템 입력을 통해 이동 방향을 설정.
    /// expected range: [-1, 1]
    /// </summary>
    public void SetExternalMove(Vector2 moveAxis01)
    {
        externalMove = Vector2.ClampMagnitude(moveAxis01, 1f);
        hasExternal = true;
    }

    // 디버그용: 스폰 영역 시각화
    void OnDrawGizmosSelected()
    {
        if (spawnArea == null) return;
        Gizmos.color = new Color(0f, 1f, 0.6f, 0.35f);
        var b = spawnArea.bounds;
        Gizmos.DrawCube(b.center, b.size);

        // 내부 영역 (패딩 적용)
        Gizmos.color = new Color(0f, 0.9f, 0.4f, 0.6f);
        Vector2 pad = clampPadding;
        Collider2D selfCol = GetComponent<Collider2D>();
        if (selfCol != null)
        {
            Bounds sb = selfCol.bounds;
            pad.x = Mathf.Max(pad.x, sb.extents.x);
            pad.y = Mathf.Max(pad.y, sb.extents.y);
        }
        var inner = new Bounds(b.center, new Vector3(
            Mathf.Max(0.01f, b.size.x - 2f * pad.x),
            Mathf.Max(0.01f, b.size.y - 2f * pad.y),
            0.1f));
        Gizmos.DrawWireCube(inner.center, inner.size);
    }
}
