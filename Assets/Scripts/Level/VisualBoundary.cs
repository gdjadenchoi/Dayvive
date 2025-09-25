using UnityEngine;

/// <summary>
/// SpawnArea(BoxCollider2D)�� �ٱ� �׵θ��� �׷� "���̴� ��"�� �����.
/// - ��������Ʈ/Ÿ�� ���� ���� LineRenderer�� �簢�� �������� �׸�
/// - SpawnArea ������/�������� �ٲ�� �ڵ� ������Ʈ
/// - ��輱 �β�/��/����(Sorting Order) ���� ����
/// </summary>
[RequireComponent(typeof(LineRenderer))]
[DisallowMultipleComponent]
public class VisualBoundary : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private BoxCollider2D spawnArea;   // _World/SpawnArea ����

    [Header("Style")]
    [SerializeField, Min(0.01f)] private float lineWidth = 0.20f;
    [SerializeField] private Color lineColor = new Color(1f, 1f, 1f, 1f); // �Ķ� ��濡�� �� ���̵��� ���
    [SerializeField] private int sortingOrder = 0;      // �ʿ�� ��ġ �켱���� ����
    [SerializeField] private float zOffset = 0f;        // ��輱 Z ����(�ʿ��)

    [Header("Update")]
    [SerializeField] private bool updateEveryFrame = true; // �ִϸ��̼�/����Ʈ ���� ����

    LineRenderer _lr;
    Bounds _last;

    void Reset()
    {
        if (!spawnArea) spawnArea = FindFirstObjectByType<BoxCollider2D>();
        SetupLR();
        Redraw(true);
    }

    void Awake()
    {
        if (!spawnArea) spawnArea = FindFirstObjectByType<BoxCollider2D>();
        SetupLR();
        Redraw(true);
    }

    void OnValidate()
    {
        if (!_lr) _lr = GetComponent<LineRenderer>();
        ApplyStyle();
        Redraw();
    }

    void LateUpdate()
    {
        if (!updateEveryFrame) return;
        Redraw();
    }

    // ����������������������������������������������������������������������������������������������������������������������������������������������������������

    void SetupLR()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.useWorldSpace = true;
        _lr.loop = true;
        _lr.textureMode = LineTextureMode.Stretch;
        _lr.alignment = LineAlignment.View;
        _lr.numCapVertices = 8;

        var mat = _lr.sharedMaterial;
        if (mat == null)
        {
            _lr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        ApplyStyle();
    }

    void ApplyStyle()
    {
        if (_lr == null) return;
        _lr.widthMultiplier = lineWidth;
        _lr.startColor = _lr.endColor = lineColor;
        _lr.sortingOrder = sortingOrder;
    }

    void Redraw(bool force = false)
    {
        if (!spawnArea) return;

        var b = CalcWorldBounds(spawnArea);
        if (!force && Approximately(_last, b)) return;
        _last = b;

        // �簢�� 4�𼭸�(�ð����) + loop=true
        var p = new Vector3[4];
        p[0] = new Vector3(b.min.x, b.min.y, zOffset);
        p[1] = new Vector3(b.min.x, b.max.y, zOffset);
        p[2] = new Vector3(b.max.x, b.max.y, zOffset);
        p[3] = new Vector3(b.max.x, b.min.y, zOffset);

        _lr.positionCount = 4;
        _lr.SetPositions(p);
    }

    static Bounds CalcWorldBounds(BoxCollider2D box)
    {
        var t = box.transform;
        var size = Vector2.Scale(box.size, t.lossyScale);
        Vector3 center = t.TransformPoint(box.offset);
        return new Bounds(center, size);
    }

    static bool Approximately(Bounds a, Bounds b)
    {
        const float eps = 1e-4f;
        return Vector3.SqrMagnitude(a.center - b.center) < eps &&
               Vector3.SqrMagnitude(a.size - b.size) < eps;
    }
}
