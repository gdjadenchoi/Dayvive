using UnityEngine;

/// <summary>
/// Dayvive - Player movement controller (WASD/Arrow).
/// - �⺻ �Է�: Ű���� (Horizontal/Vertical)
/// - �ܺ� �Է�(���� ���̽�ƽ ��) ����: SetExternalMove() ȣ�� �� �� ���͸� �켱 ���
/// - ����: SpawnArea(BoxCollider2D) ���� �̵� Ŭ����
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMover : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private bool useFixedUpdate = true;
    [Tooltip("�ܺ� �Է��� 0�� �ƴ� ��, Ű���� ��� �ܺ� �Է��� ���")]
    [SerializeField] private bool preferExternalInput = true;

    [Header("Clamp")]
    [Tooltip("�÷��̾ �� ����(BoxCollider2D) ���η� ���� (����)")]
    [SerializeField] private BoxCollider2D spawnArea;
    [SerializeField] private Vector2 clampPadding = new Vector2(0.3f, 0.3f); // �����ڸ� ����

    // components
    private Rigidbody2D rb;

    // input caches
    private Vector2 externalMove;   // ���� ���̽�ƽ ��� ����
    private bool hasExternal;       // �ܺ� �Է� ��� ����(������ ����)

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
            // FixedUpdate���� ó��
            // ��, �Է� ������ �� ������ �ݿ��ǰ� Update���� �о ��
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
        // 1) �Է� �б�
        Vector2 input = ReadInput();

        // 2) �̵� ���� ���
        currentMove = input.normalized * moveSpeed;

        // 3) �̵� �ݿ�
        Vector2 targetPos = rb.position + currentMove * deltaTime;

        // 4) Ŭ���� (����)
        if (spawnArea != null)
        {
            targetPos = ClampToSpawnArea(targetPos);
        }

        rb.MovePosition(targetPos);

        // �ܺ� �Է� �÷��״� �����Ӹ��� �ʱ�ȭ
        hasExternal = false;
    }

    private Vector2 ReadInput()
    {
        // �ܺ� �Է� ����(���� ���̽�ƽ ��) �켱
        if (preferExternalInput && hasExternal && externalMove.sqrMagnitude > 0.0001f)
        {
            return Vector2.ClampMagnitude(externalMove, 1f);
        }

        // Ű���� �Է� (Unity �⺻ ��)
        float x = Input.GetAxisRaw("Horizontal"); // A/D, ��/��
        float y = Input.GetAxisRaw("Vertical");   // W/S, ��/��
        return new Vector2(x, y);
    }

    private Vector2 ClampToSpawnArea(Vector2 worldPos)
    {
        // BoxCollider2D�� ���� ��� ���
        Bounds b = spawnArea.bounds;

        // �÷��̾� �ڽ��� �ݶ��̴� ũ�⸦ ����(�ִٸ�)
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
    /// ����� ���� ���̽�ƽ ��� �� ������ ȣ���� �ܺ� �Է��� ����.
    /// expected range: [-1, 1]
    /// </summary>
    public void SetExternalMove(Vector2 moveAxis01)
    {
        externalMove = Vector2.ClampMagnitude(moveAxis01, 1f);
        hasExternal = true;
    }

    // ����׿� �����: Ŭ���� ���� �ð�ȭ
    void OnDrawGizmosSelected()
    {
        if (spawnArea == null) return;
        Gizmos.color = new Color(0f, 1f, 0.6f, 0.35f);
        var b = spawnArea.bounds;
        Gizmos.DrawCube(b.center, b.size);

        // �е� ����
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
