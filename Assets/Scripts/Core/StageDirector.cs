using UnityEngine;

/// <summary>
/// Dayvive - Stage Orchestrator
/// 한 스테이지(라운드) 동안 "자원 스폰러(StageController)"와 "적 스폰러(EnemySpawner)"를
/// 상위에서 함께 시작/종료해 주는 오케스트레이터.
/// 
/// - 씬에 이 컴포넌트를 1개 두고, StageController / EnemySpawner를 인스펙터에 연결
/// - DayTimer/StageHooks의 UnityEvent에서 OnDayStart/OnDayEnd를 이 컴포넌트에 연결
/// - (선택) 런타임 오버라이드 슬롯에 StageConfig / EnemySpawnConfig를 넣으면
///   라운드 시작 시 해당 설정으로 각각 주입해 사용
/// 
/// 향후 확장:
/// - StageDefinition(SO) / StageVariant(SO)로 한 번의 라운드 구성을 데이터로 관리
/// - 여기서 해당 SO를 읽어 아래 오버라이드 슬롯에 주입하면 끝
/// </summary>
[DisallowMultipleComponent]
public class StageDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StageController stageController;   // 자원 스폰/리필 담당 (기존 컴포넌트)
    [SerializeField] private EnemySpawner enemySpawner;       // 적 스폰 담당 (신규 컴포넌트)

    [Header("Runtime Overrides (Optional)")]
    [Tooltip("라운드 시작 시 StageController에 주입할 StageConfig (비우면 기존 StageController 값 유지)")]
    [SerializeField] private StageConfig overrideStageConfig;
    [Tooltip("라운드 시작 시 EnemySpawner에 주입할 EnemySpawnConfig (비우면 기존 EnemySpawner 값 유지)")]
    [SerializeField] private EnemySpawnConfig overrideEnemyConfig;

    [Header("Options")]
    [Tooltip("씬 Play 시 자동으로 하루 시작(테스트 편의 옵션)")]
    [SerializeField] private bool autoStartOnPlay = false;
    [Tooltip("로그 출력(세팅/시작/종료 시점)")]
    [SerializeField] private bool debugLog = false;

    private void Reset()
    {
        // 씬에서 자동 참조 시도
        if (!stageController) stageController = FindFirstObjectByType<StageController>();
        if (!enemySpawner) enemySpawner = FindFirstObjectByType<EnemySpawner>();
    }

    private void Start()
    {
        if (autoStartOnPlay)
            OnDayStart();
    }

    /// <summary>
    /// 하루(라운드) 시작: 두 스폰러를 함께 시작.
    /// - 필요 시 런타임 오버라이드(StageConfig/EnemySpawnConfig)를 주입
    /// - StageController: SpawnForDay() 호출
    /// - EnemySpawner: StartDay() 호출(일일 카운터 리셋 및 루프 시작)
    /// </summary>
    public void OnDayStart()
    {
        // 1) 런타임 오버라이드 주입 (있을 때만)
        if (overrideStageConfig && stageController)
        {
            // StageController가 StageConfig를 필드로 가지고 있다고 가정
            // 필드명이 다르면 여기에 맞춰 교체해 주세요.
            stageController.SetConfig(overrideStageConfig);
            if (debugLog) Debug.Log("[StageDirector] Applied StageConfig override.", this);
        }

        if (overrideEnemyConfig && enemySpawner)
        {
            enemySpawner.SetConfig(overrideEnemyConfig);
            if (debugLog) Debug.Log("[StageDirector] Applied EnemySpawnConfig override.", this);
        }

        // 2) 자원 스폰 시작
        if (stageController)
        {
            stageController.SpawnForDay();
            if (debugLog) Debug.Log("[StageDirector] StageController.SpawnForDay()", this);
        }
        else if (debugLog) Debug.LogWarning("[StageDirector] StageController is missing.", this);

        // 3) 적 스폰 시작
        if (enemySpawner)
        {
            enemySpawner.StartDay();
            if (debugLog) Debug.Log("[StageDirector] EnemySpawner.StartDay()", this);
        }
        else if (debugLog) Debug.LogWarning("[StageDirector] EnemySpawner is missing.", this);
    }

    /// <summary>
    /// 하루(라운드) 종료: 적 스폰러 루프 중단(+옵션 디스폰).
    /// 자원 스폰러는 현재 구조상 종료 시 특별 처리 없음(필요 시 StageController에 메서드 추가).
    /// </summary>
    public void OnDayEnd()
    {
        if (enemySpawner)
        {
            enemySpawner.EndDay();
            if (debugLog) Debug.Log("[StageDirector] EnemySpawner.EndDay()", this);
        }

        // StageController에 종료 훅이 필요해지면 여기서 호출하도록 확장 가능.
        // 예) stageController.OnDayEndCleanup();
    }

    /// <summary>
    /// 외부에서 StageConfig/EnemySpawnConfig 오버라이드를 교체하고 싶을 때 호출.
    /// 다음 OnDayStart 때 적용됨.
    /// </summary>
    public void SetOverrides(StageConfig stage, EnemySpawnConfig enemy)
    {
        overrideStageConfig = stage;
        overrideEnemyConfig = enemy;
        if (debugLog) Debug.Log("[StageDirector] Overrides set.", this);
    }

    // --- 헬퍼(선택) : 런타임 중에도 즉시 적용하고 싶으면 공개 메서드로 노출 ---
    /// <summary>
    /// 즉시 StageConfig를 주입하고, 자원 스폰을 재시작하고 싶을 때 사용(선택).
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
    /// 즉시 EnemySpawnConfig를 주입하고, 적 스폰 루프를 재시작하고 싶을 때 사용(선택).
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
