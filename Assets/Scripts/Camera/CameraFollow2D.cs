using UnityEngine;

/// <summary>
/// 2D 오쏘그래픽 카메라 추적기
/// - 대상(플레이어)을 부드럽게 추적
/// - 화면 중앙의 데드존을 벗어날 때만 카메라 이동
/// - 카메라 뷰포트를 SpawnArea(BoxCollider2D) 내부로 클램프
/// </summary>
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] Transform target;                 // 보통 Player
    [SerializeField] float smooth = 0.12f;            // 0.05~0.2 권장 (0=즉시)

    [Header("Dead Zone (normalized, 0~1)")]
    [Tooltip("화면 폭 대비 데드존 너비 (0~1). 예: 0.3 = 화면의 30%")]
    [Range(0f, 0.9f)] public float deadZoneWidth = 0.30f;
    [Tooltip("화면 높이 대비 데드존 높이 (0~1). 예: 0.2 = 화면의 20%")]
    [Range(0f, 0.9f)] public float deadZoneHeight = 0.20f;

    [Header("World Bounds (Play Area)")]
    [SerializeField] BoxCollider2D spawnArea;         // 플레이 가능한 영역(필수)
    [SerializeField] Vector2 extraPadding = new(0.0f, 0.0f); // 여유 패딩(월드 유닛)

    [Header("Debug")]
    [SerializeField] bool drawGizmos = true;
    [SerializeField] Color deadZoneColor = new(1f, 1f, 1f, 0.15f);
    [SerializeField] Color clampColor = new(0f, 1f, 0.4f, 0.35f);

    Camera cam;
    float z;

    void Reset()
    {
        cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;
        if (!target)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
        }
        if (!spawnArea)
            spawnArea = FindFirstObjectByType<BoxCollider2D>();
    }

    void Awake()
    {
        cam = GetComponent<Camera>();
        z = transform.position.z;
        if (cam.orthographic == false)
        {
            Debug.LogWarning("[CameraFollow2D] 카메라 Projection이 Orthographic이 아닙니다. 자동 전환합니다.", this);
            cam.orthographic = true;
        }
        if (!target)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
        }
        if (!spawnArea)
            spawnArea = FindFirstObjectByType<BoxCollider2D>();
    }

    void LateUpdate()
    {
        if (!target || !spawnArea) return;

        // 현재 카메라 뷰의 절반 폭/높이 계산
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // 데드존 계산(카메라 중심 기준 사각형)
        Vector3 camPos = transform.position;
        Rect dead = new Rect(
            camPos.x - halfW * deadZoneWidth * 0.5f,
            camPos.y - halfH * deadZoneHeight * 0.5f,
            halfW * deadZoneWidth,
            halfH * deadZoneHeight
        );

        Vector3 targetPos = target.position;
        Vector3 wanted = camPos;

        // 타깃이 데드존을 벗어나면 해당 축으로만 이동
        if (targetPos.x < dead.xMin) wanted.x = targetPos.x + (dead.width * 0.5f) - halfW * deadZoneWidth * 0.5f;
        else if (targetPos.x > dead.xMax) wanted.x = targetPos.x - (dead.width * 0.5f) + halfW * deadZoneWidth * 0.5f;

        if (targetPos.y < dead.yMin) wanted.y = targetPos.y + (dead.height * 0.5f) - halfH * deadZoneHeight * 0.5f;
        else if (targetPos.y > dead.yMax) wanted.y = targetPos.y - (dead.height * 0.5f) + halfH * deadZoneHeight * 0.5f;

        // 맵 클램프: 카메라 뷰포트가 SpawnArea 밖으로 나가지 않도록
        Bounds b = CalcWorldBounds(spawnArea);
        float minX = b.min.x + halfW + extraPadding.x;
        float maxX = b.max.x - halfW - extraPadding.x;
        float minY = b.min.y + halfH + extraPadding.y;
        float maxY = b.max.y - halfH - extraPadding.y;

        // 맵이 카메라보다 작은 경우 중앙 고정
        if (minX > maxX) wanted.x = b.center.x;
        else wanted.x = Mathf.Clamp(wanted.x, minX, maxX);

        if (minY > maxY) wanted.y = b.center.y;
        else wanted.y = Mathf.Clamp(wanted.y, minY, maxY);

        // 스무스 이동
        Vector3 final = Vector3.Lerp(camPos, new Vector3(wanted.x, wanted.y, z), 1f - Mathf.Exp(-smooth * 60f * Time.deltaTime));
        final.z = z;
        transform.position = final;
    }

    Bounds CalcWorldBounds(BoxCollider2D box)
    {
        var t = box.transform;
        var size = Vector2.Scale(box.size, t.lossyScale);
        Vector3 center = t.TransformPoint(box.offset);
        return new Bounds(center, size);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        if (!cam) cam = GetComponent<Camera>();
        if (!cam) return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 c = transform.position;

        // 데드존
        Gizmos.color = deadZoneColor;
        Vector3 dzSize = new Vector3(halfW * 2f * deadZoneWidth, halfH * 2f * deadZoneHeight, 0f);
        Gizmos.DrawCube(c, dzSize);

        // 맵 클램프 표시
        if (spawnArea)
        {
            Gizmos.color = clampColor;
            var b = CalcWorldBounds(spawnArea);
            Gizmos.DrawWireCube(b.center, b.size);
        }
    }
#endif
}
