using UnityEngine;

/// <summary>
/// StageConfig(자원 스폰 설정)를 런타임에 "복제"하고, 수치에 스케일을 곱해 StageDirector에 주입한다.
/// - 원본 SO는 절대 수정하지 않음(안전)
/// - StartCount / MinOnField / RefillBatch에만 factor를 적용(적 스폰은 건드리지 않음)
/// - 기본값: factor = 3.0f
/// </summary>
[DisallowMultipleComponent]
public class StageDensityScaler : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private StageDirector director;       // StageDirector
    [SerializeField] private StageConfig baseStageConfig;  // 기존 StageConfig(원본 SO)

    [Header("Scaling")]
    [SerializeField, Min(1f)] private float factor = 3f;   // 원하는 배수(요청: 3배)
    [SerializeField] private bool ceilValues = true;        // 올림 반올림 선택(기본 올림)
    [SerializeField] private int minValue = 1;              // 최소 1 이상 보장

    StageConfig _runtimeClone;

    void Reset()
    {
        if (!director) director = FindFirstObjectByType<StageDirector>();
        if (!baseStageConfig)
        {
            // 씬에 StageController가 달려 있으면 그 config를 기본값으로
            var sc = FindFirstObjectByType<StageController>();
            if (sc) baseStageConfig = sc.config;
        }
    }

    void Awake()
    {
        if (!director) director = FindFirstObjectByType<StageDirector>();
        if (!baseStageConfig) return;

        _runtimeClone = MakeScaledClone(baseStageConfig, factor, ceilValues, minValue);

        // 하루 시작 전에 Director가 사용할 수 있도록 "오버라이드"로 주입
        director.SetOverrides(_runtimeClone, null);
    }

    // ─────────────────────────────────────────────────────────────────────────────

    public static StageConfig MakeScaledClone(StageConfig src, float f, bool ceil, int minV)
    {
        if (!src) return null;

        var clone = ScriptableObject.CreateInstance<StageConfig>();
        if (src.entries != null)
        {
            clone.entries = new StageConfig.SpawnEntry[src.entries.Length];
            for (int i = 0; i < src.entries.Length; i++)
            {
                var e = src.entries[i];
                var c = new StageConfig.SpawnEntry
                {
                    id = e.id,
                    prefab = e.prefab,
                    startCount = Scale(e.startCount, f, ceil, minV),
                    minOnField = Scale(e.minOnField, f, ceil, minV),
                    refillBatch = Scale(e.refillBatch, f, ceil, minV)
                };
                clone.entries[i] = c;
            }
        }
        return clone;
    }

    static int Scale(int v, float f, bool ceil, int minV)
    {
        float raw = v * Mathf.Max(1f, f);
        int r = ceil ? Mathf.CeilToInt(raw) : Mathf.RoundToInt(raw);
        return Mathf.Max(minV, r);
    }
}
