using UnityEngine;

/// <summary>
/// 2D ����׷��� ī�޶� ������
/// - ���(�÷��̾�)�� �ε巴�� ����
/// - ȭ�� �߾��� �������� ��� ���� ī�޶� �̵�
/// - ī�޶� ����Ʈ�� SpawnArea(BoxCollider2D) ���η� Ŭ����
/// </summary>
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] Transform target;                 // ���� Player
    [SerializeField] float smooth = 0.12f;            // 0.05~0.2 ���� (0=���)

    [Header("Dead Zone (normalized, 0~1)")]
    [Tooltip("ȭ�� �� ��� ������ �ʺ� (0~1). ��: 0.3 = ȭ���� 30%")]
    [Range(0f, 0.9f)] public float deadZoneWidth = 0.30f;
    [Tooltip("ȭ�� ���� ��� ������ ���� (0~1). ��: 0.2 = ȭ���� 20%")]
    [Range(0f, 0.9f)] public float deadZoneHeight = 0.20f;

    [Header("World Bounds (Play Area)")]
    [SerializeField] BoxCollider2D spawnArea;         // �÷��� ������ ����(�ʼ�)
    [SerializeField] Vector2 extraPadding = new(0.0f, 0.0f); // ���� �е�(���� ����)

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
            Debug.LogWarning("[CameraFollow2D] ī�޶� Projection�� Orthographic�� �ƴմϴ�. �ڵ� ��ȯ�մϴ�.", this);
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

        // ���� ī�޶� ���� ���� ��/���� ���
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // ������ ���(ī�޶� �߽� ���� �簢��)
        Vector3 camPos = transform.position;
        Rect dead = new Rect(
            camPos.x - halfW * deadZoneWidth * 0.5f,
            camPos.y - halfH * deadZoneHeight * 0.5f,
            halfW * deadZoneWidth,
            halfH * deadZoneHeight
        );

        Vector3 targetPos = target.position;
        Vector3 wanted = camPos;

        // Ÿ���� �������� ����� �ش� �����θ� �̵�
        if (targetPos.x < dead.xMin) wanted.x = targetPos.x + (dead.width * 0.5f) - halfW * deadZoneWidth * 0.5f;
        else if (targetPos.x > dead.xMax) wanted.x = targetPos.x - (dead.width * 0.5f) + halfW * deadZoneWidth * 0.5f;

        if (targetPos.y < dead.yMin) wanted.y = targetPos.y + (dead.height * 0.5f) - halfH * deadZoneHeight * 0.5f;
        else if (targetPos.y > dead.yMax) wanted.y = targetPos.y - (dead.height * 0.5f) + halfH * deadZoneHeight * 0.5f;

        // �� Ŭ����: ī�޶� ����Ʈ�� SpawnArea ������ ������ �ʵ���
        Bounds b = CalcWorldBounds(spawnArea);
        float minX = b.min.x + halfW + extraPadding.x;
        float maxX = b.max.x - halfW - extraPadding.x;
        float minY = b.min.y + halfH + extraPadding.y;
        float maxY = b.max.y - halfH - extraPadding.y;

        // ���� ī�޶󺸴� ���� ��� �߾� ����
        if (minX > maxX) wanted.x = b.center.x;
        else wanted.x = Mathf.Clamp(wanted.x, minX, maxX);

        if (minY > maxY) wanted.y = b.center.y;
        else wanted.y = Mathf.Clamp(wanted.y, minY, maxY);

        // ������ �̵�
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

        // ������
        Gizmos.color = deadZoneColor;
        Vector3 dzSize = new Vector3(halfW * 2f * deadZoneWidth, halfH * 2f * deadZoneHeight, 0f);
        Gizmos.DrawCube(c, dzSize);

        // �� Ŭ���� ǥ��
        if (spawnArea)
        {
            Gizmos.color = clampColor;
            var b = CalcWorldBounds(spawnArea);
            Gizmos.DrawWireCube(b.center, b.size);
        }
    }
#endif
}
