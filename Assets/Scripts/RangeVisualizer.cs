using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RangeVisualizer : MonoBehaviour
{
    public Color color = new Color(0f, 1f, 0.6f, 0.25f);
    public float width = 0.03f;
    public int segments = 96;

    LineRenderer lr;
    CircleCollider2D sensor;
    MiningSystem mining;
    float lastWorldR = -1f;
    Vector3 lastCenter;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.loop = true;
        lr.useWorldSpace = true;                // 월드 좌표로 그린다
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = width;
        lr.startColor = lr.endColor = color;
        lr.positionCount = segments;
        lr.sortingOrder = 10;

        sensor = GetComponentInParent<CircleCollider2D>();
        mining = GetComponentInParent<MiningSystem>();
        if (!sensor) Debug.LogWarning("[RangeVisualizer] CircleCollider2D parent not found.");
        if (!mining) Debug.LogWarning("[RangeVisualizer] MiningSystem parent not found.");
    }

    void LateUpdate()
    {
        if (!sensor || !mining) return;

        // ‘판정’과 동일한 월드 반경/중심을 사용
        float r = Mathf.Max(0f, mining.GetWorldRadius());
        Vector3 c = sensor.transform.TransformPoint(sensor.offset);

        if (Mathf.Abs(r - lastWorldR) > 1e-4f || (c - lastCenter).sqrMagnitude > 1e-6f)
        {
            lastWorldR = r;
            lastCenter = c;

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(t) * r, Mathf.Sin(t) * r, 0f);
                lr.SetPosition(i, c + p);
            }
        }

        lr.widthMultiplier = width;
        lr.startColor = lr.endColor = color;
    }
}
