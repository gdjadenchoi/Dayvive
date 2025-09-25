using UnityEngine;

/// <summary>
/// StageConfig(�ڿ� ���� ����)�� ��Ÿ�ӿ� "����"�ϰ�, ��ġ�� �������� ���� StageDirector�� �����Ѵ�.
/// - ���� SO�� ���� �������� ����(����)
/// - StartCount / MinOnField / RefillBatch���� factor�� ����(�� ������ �ǵ帮�� ����)
/// - �⺻��: factor = 3.0f
/// </summary>
[DisallowMultipleComponent]
public class StageDensityScaler : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private StageDirector director;       // StageDirector
    [SerializeField] private StageConfig baseStageConfig;  // ���� StageConfig(���� SO)

    [Header("Scaling")]
    [SerializeField, Min(1f)] private float factor = 3f;   // ���ϴ� ���(��û: 3��)
    [SerializeField] private bool ceilValues = true;        // �ø� �ݿø� ����(�⺻ �ø�)
    [SerializeField] private int minValue = 1;              // �ּ� 1 �̻� ����

    StageConfig _runtimeClone;

    void Reset()
    {
        if (!director) director = FindFirstObjectByType<StageDirector>();
        if (!baseStageConfig)
        {
            // ���� StageController�� �޷� ������ �� config�� �⺻������
            var sc = FindFirstObjectByType<StageController>();
            if (sc) baseStageConfig = sc.config;
        }
    }

    void Awake()
    {
        if (!director) director = FindFirstObjectByType<StageDirector>();
        if (!baseStageConfig) return;

        _runtimeClone = MakeScaledClone(baseStageConfig, factor, ceilValues, minValue);

        // �Ϸ� ���� ���� Director�� ����� �� �ֵ��� "�������̵�"�� ����
        director.SetOverrides(_runtimeClone, null);
    }

    // ����������������������������������������������������������������������������������������������������������������������������������������������������������

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
