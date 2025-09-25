using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RangeVisualizer : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CircleCollider2D sensor;   // Player 자식 Sensor(Trigger)
    [SerializeField] private PlayerMode playerMode;     // PlayerMode 참조

    [Header("Style")]
    [SerializeField, Range(8, 256)] private int segments = 64;
    [SerializeField] private float width = 0.03f;
    [SerializeField] private Color color = new Color(0f, 1f, 0.4f, 0.35f);

    private LineRenderer lr;
    private Vector3[] cache;

    void Reset()
    {
        lr = GetComponent<LineRenderer>();
        lr.loop = true;
        lr.useWorldSpace = true;
        lr.positionCount = segments;
        lr.widthMultiplier = width;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = color;
        lr.textureMode = LineTextureMode.Stretch;
        lr.alignment = LineAlignment.View;
    }

    void Awake()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        if (lr.positionCount != segments) lr.positionCount = segments;
        if (cache == null || cache.Length != segments) cache = new Vector3[segments];

        if (!playerMode) playerMode = GetComponentInParent<PlayerMode>();
    }

    void LateUpdate()
    {
        // --- Mining 모드에서만 보이게 ---
        bool active = playerMode == null || playerMode.Current == PlayerMode.Mode.Mining;
        lr.enabled = active;
        if (!active || !sensor) return;

        // 센서 반경을 그대로 참조
        Vector3 scale = sensor.transform.lossyScale;
        float worldR = sensor.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
        Vector2 center = (Vector2)sensor.transform.position + sensor.offset;

        float step = Mathf.PI * 2f / segments;
        for (int i = 0; i < segments; i++)
        {
            float a = i * step;
            cache[i] = new Vector3(center.x + Mathf.Cos(a) * worldR, center.y + Mathf.Sin(a) * worldR, 0);
        }
        lr.SetPositions(cache);
    }
}
