using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RangeVisualizer : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] CircleCollider2D sensor;   // �� Player�� ���� Sensor(Trigger)

    [Header("Look")]
    [SerializeField] int segments = 64;
    [SerializeField] float width = 0.03f;
    [SerializeField] Color color = new Color(0f, 1f, 0.4f, 0.35f); // ���� ��Ʈ

    LineRenderer lr;
    Vector3[] cache;

    void Reset()
    {
        lr = GetComponent<LineRenderer>();
        lr.loop = true;
        lr.useWorldSpace = true;
        lr.positionCount = segments;
        lr.widthMultiplier = width;
        lr.startColor = lr.endColor = color;
        lr.textureMode = LineTextureMode.Stretch;
        lr.alignment = LineAlignment.View;
    }

    void Awake()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        if (lr.positionCount != segments) lr.positionCount = segments;
        if (cache == null || cache.Length != segments) cache = new Vector3[segments];
    }

    void LateUpdate()
    {
        if (!sensor || !lr) return;

        // Sensor�� "���� �ݰ�" ��� = radius * max(lossyScale.xy)
        var t = sensor.transform;
        var s = t.lossyScale;
        float worldR = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y)) * Mathf.Max(0.0001f, sensor.radius);

        // Sensor�� "���� �߽�" (offset �ݿ�)
        Vector3 center = (Vector2)t.position + sensor.offset;

        // �� �׸���
        float step = Mathf.PI * 2f / segments;
        for (int i = 0; i < segments; i++)
        {
            float a = step * i;
            cache[i] = center + new Vector3(Mathf.Cos(a) * worldR, Mathf.Sin(a) * worldR, 0f);
        }
        lr.SetPositions(cache);
    }
}
