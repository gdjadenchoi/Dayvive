using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// SpawnArea(BoxCollider2D)�� ���� ��踦 ��������
/// ������ �ʴ� 4�� ��(BoxCollider2D)�� �ڵ� ����/�����Ѵ�.
/// - ���̾�: ������ ���̾�(��: "Obstacle")
/// - �þ� ����(SightSensor.obstacleMask) �� �̵�/�浹�� ���
/// - Projectile�� hitMask�� Obstacle�� �������� ������ ���(����)
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class BoundaryBuilder : MonoBehaviour
{
    [Header("Source Bounds")]
    [Tooltip("���� ���� �ڽ�. ���� �� ������Ʈ�� BoxCollider2D�� ���")]
    public BoxCollider2D sourceBounds;

    [Header("Wall Settings")]
    [Tooltip("�� �β�(���� ����)")]
    [Min(0.01f)] public float thickness = 0.5f;
    [Tooltip("��迡�� ����/�ٱ������� ������ �� ������(+�� ��������, -�� �ٱ�������)")]
    public float inset = 0f;
    [Tooltip("���� ������ ���̾��(��: Obstacle)")]
    public string wallLayerName = "Obstacle";
    [Tooltip("��Ÿ�ӿ����� ����/����(üũ ���� �� �����Ϳ����� ����)")]
    public bool buildAtRuntime = true;

    const string RootName = "_BoundaryWalls";
    Transform _root;

    void Reset()
    {
        if (!sourceBounds) sourceBounds = GetComponent<BoxCollider2D>();
    }

    void OnEnable()
    {
        if (!Application.isPlaying || buildAtRuntime)
            Build();
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        // �����Ϳ��� ������ ������ ���� ���� �� ���� ���� �� ����(����)
        if (!Application.isPlaying) { /* ���ܵα� */ }
#endif
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
            Build();
    }

    /// <summary> ��踦 ����� </summary>
    public void Build()
    {
        if (!sourceBounds)
        {
            Debug.LogWarning("[BoundaryBuilder] sourceBounds(BoxCollider2D)�� �����ϴ�.", this);
            return;
        }

        EnsureRoot();

        // ��� �ڽ��� ���� ��� ���
        var t = sourceBounds.transform;
        Vector2 size = Vector2.Scale(sourceBounds.size, t.lossyScale);
        Vector2 center = (Vector2)t.TransformPoint(sourceBounds.offset);

        // �μ� ����(�������� ������: +, �ٱ����� ������: -)
        size -= Vector2.one * inset * 2f;

        float left = center.x - size.x * 0.5f;
        float right = center.x + size.x * 0.5f;
        float bottom = center.y - size.y * 0.5f;
        float top = center.y + size.y * 0.5f;

        // 4�� �� ����/����
        MakeWall("North", new Vector2(center.x, top + thickness * 0.5f), new Vector2(size.x + thickness * 2f, thickness));
        MakeWall("South", new Vector2(center.x, bottom - thickness * 0.5f), new Vector2(size.x + thickness * 2f, thickness));
        MakeWall("East", new Vector2(right + thickness * 0.5f, center.y), new Vector2(thickness, size.y));
        MakeWall("West", new Vector2(left - thickness * 0.5f, center.y), new Vector2(thickness, size.y));
    }

    void EnsureRoot()
    {
        if (_root == null)
        {
            var child = transform.Find(RootName);
            _root = child ? child : new GameObject(RootName).transform;
            _root.SetParent(transform, false);
            _root.localPosition = Vector3.zero;
            _root.localRotation = Quaternion.identity;
            _root.localScale = Vector3.one;
        }
    }

    void MakeWall(string name, Vector2 pos, Vector2 size)
    {
        var wall = _root.Find(name);
        if (!wall)
        {
            wall = new GameObject(name).transform;
            wall.SetParent(_root, false);
            wall.gameObject.hideFlags = HideFlags.None;
        }
        wall.position = pos;
        wall.rotation = Quaternion.identity;
        wall.localScale = Vector3.one;

        var col = wall.GetComponent<BoxCollider2D>();
        if (!col) col = wall.gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        col.size = size;

        // ���̾� ����
        int layer = LayerMask.NameToLayer(wallLayerName);
        if (layer < 0)
        {
            // ���̾ ���� ���� �� ���� �� Default�� �ΰ� ���
            if (Application.isEditor)
                Debug.LogWarning($"[BoundaryBuilder] '{wallLayerName}' ���̾ �������� �ʽ��ϴ�. Project Settings > Tags and Layers ���� ���� �߰��ϼ���.", this);
            layer = 0; // Default
        }
        wall.gameObject.layer = layer;

        // RigidBody2D ���̵� static collider�� ���
    }

#if UNITY_EDITOR
    [ContextMenu("Rebuild Now")]
    void RebuildNow() => Build();

    void OnDrawGizmosSelected()
    {
        if (!sourceBounds) return;
        var t = sourceBounds.transform;
        Vector2 size = Vector2.Scale(sourceBounds.size, t.lossyScale) - Vector2.one * inset * 2f;
        Vector2 center = (Vector2)t.TransformPoint(sourceBounds.offset);

        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.25f);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
