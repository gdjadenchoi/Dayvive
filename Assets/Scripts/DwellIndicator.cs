// DwellIndicator.cs
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DwellIndicator : MonoBehaviour
{
    public MiningSystem mining;          // Player에 있는 MiningSystem 참조
    public float radius = 0.18f;
    public int segments = 48;
    public Color color = new(0f, 1f, 0.6f, 0.6f);

    LineRenderer lr;
    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = false;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = 0.025f;
        lr.startColor = lr.endColor = color;
    }

    void LateUpdate()
    {
        if (!mining) { lr.positionCount = 0; return; }
        float t = Mathf.Clamp01(mining.DwellProgress01);
        if (t <= 0f) { lr.positionCount = 0; return; }

        int n = Mathf.Max(2, Mathf.CeilToInt(segments * t));
        lr.positionCount = n;
        float maxAngle = Mathf.PI * 2f * t;
        for (int i = 0; i < n; i++)
        {
            float ang = maxAngle * i / (n - 1);
            lr.SetPosition(i, new Vector3(Mathf.Cos(ang) * radius, Mathf.Sin(ang) * radius, 0));
        }
    }
}
