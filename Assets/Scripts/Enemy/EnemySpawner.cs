using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// SpawnArea(BoxCollider2D)를 기준으로 하루 동안 적을 스폰한다.
/// - EnemySpawnConfig의 규칙대로 초기 버스트 + 주기 스폰
/// - maxAlive, totalThisDay 제한 관리
/// - Day 시작/종료 수동 제어 API 제공 (StageHooks/DayTimer와 연결 용이)
/// </summary>
[DisallowMultipleComponent]
public class EnemySpawner : MonoBehaviour
{
    [Header("Area & Target")]
    [Tooltip("스폰 범위 기준. 비워두면 자신의 BoxCollider2D 사용")]
    [SerializeField] private BoxCollider2D spawnArea;

    [Header("Config")]
    [SerializeField] private EnemySpawnConfig config;

    [Header("Safety/Placement")]
    [Tooltip("Player와 최소 거리. 가까우면 다른 위치 재시도")]
    [SerializeField, Min(0f)] private float minDistanceFromPlayer = 2.0f;
    [Tooltip("장애물 레이어. 겹치면 다른 위치 재시도 (벽 내부 스폰 방지)")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private Vector2 overlapBoxHalfExtents = new Vector2(0.25f, 0.25f);
    [SerializeField, Range(8, 64)] private int maxPlacementTries = 24;

    [Header("Lifecycle")]
    [Tooltip("OnEnable 시 하루 자동 시작")]
    [SerializeField] private bool autoStart = true;
    [Tooltip("Day 종료 시 남은 적을 제거할지")]
    [SerializeField] private bool despawnOnDayEnd = false;

    // 내부 상태
    private readonly List<GameObject> _alive = new List<GameObject>();
    private readonly List<Coroutine> _running = new List<Coroutine>();
    private Transform _player;
    private Bounds _bounds;

    // 1 Rule 당 당일 스폰 카운트
    private readonly Dictionary<int, int> _spawnedCountPerRule = new Dictionary<int, int>();

    #region Unity
    private void Reset()
    {
        if (!spawnArea) spawnArea = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        if (autoStart) StartDay();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _running.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        var area = spawnArea ? spawnArea : GetComponent<BoxCollider2D>();
        if (!area) return;
        var b = CalcWorldBounds(area);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(b.center, b.size);
    }
    #endregion

    #region Public API
    /// <summary>StageDirector 등에서 런타임에 Config를 갈아끼우기 위한 API</summary>
    public void SetConfig(EnemySpawnConfig cfg)
    {
        config = cfg;
    }

    /// <summary>
    /// 하루 시작: 카운트 리셋, 초기 버스트 + 주기 스폰 시작
    /// </summary>
    public void StartDay()
    {
        CacheWorld();
        ResetCounters();
        StopAllCoroutines();
        _running.Clear();

        if (config == null || config.rules == null) return;

        for (int i = 0; i < config.rules.Count; i++)
        {
            int ruleIndex = i;
            var r = config.rules[ruleIndex];
            // 초기 버스트
            for (int j = 0; j < r.initialBurst; j++)
                TrySpawnOne(ruleIndex, r);

            // 주기 스폰
            if (r.respawn && r.interval > 0f)
            {
                var co = StartCoroutine(SpawnLoop(ruleIndex, r));
                _running.Add(co);
            }
        }
    }

    /// <summary>
    /// 하루 종료: 주기 중단, (옵션) 남은 적 제거
    /// </summary>
    public void EndDay()
    {
        StopAllCoroutines();
        _running.Clear();

        if (despawnOnDayEnd)
        {
            for (int i = _alive.Count - 1; i >= 0; i--)
            {
                var go = _alive[i];
                if (go) Destroy(go);
            }
            _alive.Clear();
        }
    }
    #endregion

    #region Internals
    private void CacheWorld()
    {
        // Bounds 계산
        var area = spawnArea ? spawnArea : GetComponent<BoxCollider2D>();
        if (area) _bounds = CalcWorldBounds(area);

        // Player 캐시
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        _player = playerGO ? playerGO.transform : null;
    }

    private void ResetCounters()
    {
        _spawnedCountPerRule.Clear();
        _alive.RemoveAll(go => go == null);
    }

    private Bounds CalcWorldBounds(BoxCollider2D box)
    {
        var t = box.transform;
        var size = Vector2.Scale(box.size, t.lossyScale);
        Vector3 center = t.TransformPoint(box.offset);
        return new Bounds(center, size);
    }

    private IEnumerator SpawnLoop(int ruleIndex, EnemySpawnConfig.SpawnRule rule)
    {
        var wait = new WaitForSeconds(rule.interval);
        while (true)
        {
            TrySpawnOne(ruleIndex, rule);
            yield return wait;
        }
    }

    private bool TrySpawnOne(int ruleIndex, EnemySpawnConfig.SpawnRule rule)
    {
        if (!rule.prefab) return false;

        // 동시 생존 제한
        PruneAlive();
        int aliveOfThisRule = CountAliveOf(rule.prefab);
        if (rule.maxAlive > 0 && aliveOfThisRule >= rule.maxAlive) return false;

        // 일일 총량 제한
        int spawnedSoFar = _spawnedCountPerRule.TryGetValue(ruleIndex, out var v) ? v : 0;
        if (rule.totalThisDay > 0 && spawnedSoFar >= rule.totalThisDay) return false;

        // 위치 선정
        if (!TryPickSpawnPosition(out var pos)) return false;

        // 스폰
        var go = Instantiate(rule.prefab, pos, Quaternion.identity);
        _alive.Add(go);
        _spawnedCountPerRule[ruleIndex] = spawnedSoFar + 1;

        // 파괴/사망 시 alive 관리 (토큰 부착)
        var token = go.AddComponent<SpawnedToken>();
        token.Init(this);

        return true;
    }

    private void PruneAlive()
    {
        for (int i = _alive.Count - 1; i >= 0; i--)
        {
            if (_alive[i] == null) _alive.RemoveAt(i);
        }
    }

    private int CountAliveOf(GameObject prefab)
    {
        int c = 0;
        for (int i = 0; i < _alive.Count; i++)
        {
            var go = _alive[i];
            if (!go) continue;
            if (go.name.StartsWith(prefab.name)) c++;
        }
        return c;
    }

    private bool TryPickSpawnPosition(out Vector2 pos)
    {
        pos = Vector2.zero;
        if (_bounds.size == Vector3.zero) return false;

        for (int i = 0; i < maxPlacementTries; i++)
        {
            var p = new Vector2(
                Random.Range(_bounds.min.x, _bounds.max.x),
                Random.Range(_bounds.min.y, _bounds.max.y)
            );

            // 장애물 겹침 체크
            if (Physics2D.OverlapBox(p, overlapBoxHalfExtents * 2f, 0f, obstacleMask))
                continue;

            // 플레이어 최소 거리
            if (_player)
            {
                float d = Vector2.Distance(p, _player.position);
                if (d < minDistanceFromPlayer) continue;
            }

            pos = p;
            return true;
        }

        return false;
    }
    #endregion

    #region Token
    /// <summary>
    /// 스폰된 적이 파괴될 때 EnemySpawner가 alive 리스트를 정리할 수 있게 하는 토큰.
    /// </summary>
    private class SpawnedToken : MonoBehaviour
    {
        private EnemySpawner _owner;
        public void Init(EnemySpawner owner) => _owner = owner;

        private void OnDestroy()
        {
            if (_owner == null) return;
            _owner._alive.Remove(gameObject);
        }
    }
    #endregion
}
