using UnityEngine;

/// <summary>
/// Dayvive - Stage Orchestrator
/// �� ��������(����) ���� "�ڿ� ������(StageController)"�� "�� ������(EnemySpawner)"��
/// �������� �Բ� ����/������ �ִ� ���ɽ�Ʈ������.
/// 
/// - ���� �� ������Ʈ�� 1�� �ΰ�, StageController / EnemySpawner�� �ν����Ϳ� ����
/// - DayTimer/StageHooks�� UnityEvent���� OnDayStart/OnDayEnd�� �� ������Ʈ�� ����
/// - (����) ��Ÿ�� �������̵� ���Կ� StageConfig / EnemySpawnConfig�� ������
///   ���� ���� �� �ش� �������� ���� ������ ���
/// 
/// ���� Ȯ��:
/// - StageDefinition(SO) / StageVariant(SO)�� �� ���� ���� ������ �����ͷ� ����
/// - ���⼭ �ش� SO�� �о� �Ʒ� �������̵� ���Կ� �����ϸ� ��
/// </summary>
[DisallowMultipleComponent]
public class StageDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StageController stageController;   // �ڿ� ����/���� ��� (���� ������Ʈ)
    [SerializeField] private EnemySpawner enemySpawner;       // �� ���� ��� (�ű� ������Ʈ)

    [Header("Runtime Overrides (Optional)")]
    [Tooltip("���� ���� �� StageController�� ������ StageConfig (���� ���� StageController �� ����)")]
    [SerializeField] private StageConfig overrideStageConfig;
    [Tooltip("���� ���� �� EnemySpawner�� ������ EnemySpawnConfig (���� ���� EnemySpawner �� ����)")]
    [SerializeField] private EnemySpawnConfig overrideEnemyConfig;

    [Header("Options")]
    [Tooltip("�� Play �� �ڵ����� �Ϸ� ����(�׽�Ʈ ���� �ɼ�)")]
    [SerializeField] private bool autoStartOnPlay = false;
    [Tooltip("�α� ���(����/����/���� ����)")]
    [SerializeField] private bool debugLog = false;

    private void Reset()
    {
        // ������ �ڵ� ���� �õ�
        if (!stageController) stageController = FindFirstObjectByType<StageController>();
        if (!enemySpawner) enemySpawner = FindFirstObjectByType<EnemySpawner>();
    }

    private void Start()
    {
        if (autoStartOnPlay)
            OnDayStart();
    }

    /// <summary>
    /// �Ϸ�(����) ����: �� �������� �Բ� ����.
    /// - �ʿ� �� ��Ÿ�� �������̵�(StageConfig/EnemySpawnConfig)�� ����
    /// - StageController: SpawnForDay() ȣ��
    /// - EnemySpawner: StartDay() ȣ��(���� ī���� ���� �� ���� ����)
    /// </summary>
    public void OnDayStart()
    {
        // 1) ��Ÿ�� �������̵� ���� (���� ����)
        if (overrideStageConfig && stageController)
        {
            // StageController�� StageConfig�� �ʵ�� ������ �ִٰ� ����
            // �ʵ���� �ٸ��� ���⿡ ���� ��ü�� �ּ���.
            stageController.SetConfig(overrideStageConfig);
            if (debugLog) Debug.Log("[StageDirector] Applied StageConfig override.", this);
        }

        if (overrideEnemyConfig && enemySpawner)
        {
            enemySpawner.SetConfig(overrideEnemyConfig);
            if (debugLog) Debug.Log("[StageDirector] Applied EnemySpawnConfig override.", this);
        }

        // 2) �ڿ� ���� ����
        if (stageController)
        {
            stageController.SpawnForDay();
            if (debugLog) Debug.Log("[StageDirector] StageController.SpawnForDay()", this);
        }
        else if (debugLog) Debug.LogWarning("[StageDirector] StageController is missing.", this);

        // 3) �� ���� ����
        if (enemySpawner)
        {
            enemySpawner.StartDay();
            if (debugLog) Debug.Log("[StageDirector] EnemySpawner.StartDay()", this);
        }
        else if (debugLog) Debug.LogWarning("[StageDirector] EnemySpawner is missing.", this);
    }

    /// <summary>
    /// �Ϸ�(����) ����: �� ������ ���� �ߴ�(+�ɼ� ����).
    /// �ڿ� �������� ���� ������ ���� �� Ư�� ó�� ����(�ʿ� �� StageController�� �޼��� �߰�).
    /// </summary>
    public void OnDayEnd()
    {
        if (enemySpawner)
        {
            enemySpawner.EndDay();
            if (debugLog) Debug.Log("[StageDirector] EnemySpawner.EndDay()", this);
        }

        // StageController�� ���� ���� �ʿ������� ���⼭ ȣ���ϵ��� Ȯ�� ����.
        // ��) stageController.OnDayEndCleanup();
    }

    /// <summary>
    /// �ܺο��� StageConfig/EnemySpawnConfig �������̵带 ��ü�ϰ� ���� �� ȣ��.
    /// ���� OnDayStart �� �����.
    /// </summary>
    public void SetOverrides(StageConfig stage, EnemySpawnConfig enemy)
    {
        overrideStageConfig = stage;
        overrideEnemyConfig = enemy;
        if (debugLog) Debug.Log("[StageDirector] Overrides set.", this);
    }

    // --- ����(����) : ��Ÿ�� �߿��� ��� �����ϰ� ������ ���� �޼���� ���� ---
    /// <summary>
    /// ��� StageConfig�� �����ϰ�, �ڿ� ������ ������ϰ� ���� �� ���(����).
    /// </summary>
    public void ApplyStageConfigAndRespawn(StageConfig stage)
    {
        overrideStageConfig = stage;
        if (stageController)
        {
            stageController.SetConfig(stage);
            stageController.SpawnForDay();
            if (debugLog) Debug.Log("[StageDirector] StageConfig applied & respawned.", this);
        }
    }

    /// <summary>
    /// ��� EnemySpawnConfig�� �����ϰ�, �� ���� ������ ������ϰ� ���� �� ���(����).
    /// </summary>
    public void ApplyEnemyConfigAndRestart(EnemySpawnConfig enemy)
    {
        overrideEnemyConfig = enemy;
        if (enemySpawner)
        {
            enemySpawner.SetConfig(enemy);
            enemySpawner.StartDay();
            if (debugLog) Debug.Log("[StageDirector] EnemyConfig applied & restarted.", this);
        }
    }
}
