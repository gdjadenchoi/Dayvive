// DwellIndicator.cs  (World-space center, mode guard, target-first)
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DwellIndicator : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private MiningSystem mining;     // Sensor에 붙은 MiningSystem
    [SerializeField] private PlayerMode playerMode;   // Player에 붙은 PlayerMode

    [Header("Visual")]
    [SerializeField, Range(16, 256)] private int segments = 64;
    [SerializeField] private float width = 0.025f;
    [SerializeField] private Color progressColor = new Color(0f, 1f, 0.6f, 0.85f);
    [SerializeField] private Color hintColor = new Color(1f, 1f, 1f, 0.25f);

    private LineRenderer lr;

    void Reset()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;                        // ★ 월드 기준으로 고정
        lr.loop = false;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = width;
        lr.textureMode = LineTextureMode.Stretch;
        lr.alignment = LineAlignment.View;
        lr.numCapVertices = 8;
    }

    void Awake()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        if (!mining) mining = GetComponentInParent<MiningSystem>();
        if (!playerMode) playerMode = GetComponentInParent<PlayerMode>();
        ApplyStyle();
    }

    void LateUpdate()
    {
        // 모드 가드
        if (playerMode && playerMode.CurrentMode != PlayerMode.Mode.Mining) { Hide(); return; }
        if (!mining || !mining.enabled) { Hide(); return; }

        // 센터/반경 (월드)
        float r = mining.GetWorldRadius();
        if (r <= 0f) { Hide(); return; }
        Vector3 center = mining.transform.position;     // ★ 센서(채굴 판정) 중심을 항상 사용
        center.z = 0f;

        // 타깃 없으면 숨김
        if (!mining.HasTargetInRange()) { Hide(); return; }

        float t = Mathf.Clamp01(mining.DwellProgress01);
        if (t <= 0f)
        {
            DrawCircle(center, r, segments, hintColor);
        }
        else
        {
            int seg = Mathf.Max(2, Mathf.CeilToInt(segments * t));
            DrawArc(center, r, seg, t, progressColor);
        }
    }

    // ---- helpers ----
    void Hide() { if (lr) lr.positionCount = 0; }

    void DrawCircle(Vector3 c, float r, int seg, Color col)
    {
        lr.startColor = lr.endColor = col;
        lr.positionCount = seg + 1;
        for (int i = 0; i <= seg; i++)
        {
            float a = (i / (float)seg) * Mathf.PI * 2f;
            lr.SetPosition(i, c + new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
        }
    }

    void DrawArc(Vector3 c, float r, int seg, float t01, Color col)
    {
        lr.startColor = lr.endColor = col;
        lr.positionCount = seg;
        float maxA = Mathf.PI * 2f * Mathf.Clamp01(t01);
        for (int i = 0; i < seg; i++)
        {
            float a = (i / (float)(seg - 1)) * maxA;
            lr.SetPosition(i, c + new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
        }
    }

    void ApplyStyle()
    {
        if (!lr) return;
        lr.widthMultiplier = width;
    }
}
