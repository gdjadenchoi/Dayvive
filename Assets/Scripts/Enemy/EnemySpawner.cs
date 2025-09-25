using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// SpawnArea(BoxCollider2D)�� �������� �Ϸ� ���� ���� �����Ѵ�.
/// - EnemySpawnConfig�� ��Ģ��� �ʱ� ����Ʈ + �ֱ� ����
/// - maxAlive, totalThisDay ���� ����
/// - Day ����/���� ���� ���� API ���� (StageHooks/DayTimer�� ���� ����)
/// </summary>
[DisallowMultipleComponent]
public class EnemySpawner : MonoBehaviour
{
    [Header("Area & Target")]
    [Tooltip("���� ���� ����. ����θ� �ڽ��� BoxCollider2D ���")]
    [SerializeField] private BoxCollider2D spawnArea;

    [Header("Config")]
    [SerializeField] private EnemySpawnConfig config;

    [Header("Safety/Placement")]
    [Tooltip("Player�� �ּ� �Ÿ�. ������ �ٸ� ��ġ ��õ�")]
    [SerializeField, Min(0f)] private float minDistanceFromPlayer = 2.0f;
    [Tooltip("��ֹ� ���̾�. ��ġ�� �ٸ� ��ġ ��õ� (�� ���� ���� ����)")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private Vector2 overlapBoxHalfExtents = new Vector2(0.25f, 0.25f);
    [SerializeField, Range(8, 64)] private int maxPlacementTries = 24;

    [Header("Lifecycle")]
    [Tooltip("OnEnable �� �Ϸ� �ڵ� ����")]
    [SerializeField] private bool autoStart = true;
    [Tooltip("Day ���� �� ���� ���� ��������")]
    [SerializeField] private bool despawnOnDayEnd = false;

    // ���� ����
    private readonly List<GameObject> _alive = new List<GameObject>();
    private readonly List<Coroutine> _running = new List<Coroutine>();
    private Transform _player;
    private Bounds _bounds;

    // 1 Rule �� ���� ���� ī��Ʈ
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
    /// <summary>StageDirector ��� ��Ÿ�ӿ� Config�� ���Ƴ���� ���� API</summary>
    public void SetConfig(EnemySpawnConfig cfg)
    {
        config = cfg;
    }

    /// <summary>
    /// �Ϸ� ����: ī��Ʈ ����, �ʱ� ����Ʈ + �ֱ� ���� ����
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
            // �ʱ� ����Ʈ
            for (int j = 0; j < r.initialBurst; j++)
                TrySpawnOne(ruleIndex, r);

            // �ֱ� ����
            if (r.respawn && r.interval > 0f)
            {
                var co = StartCoroutine(SpawnLoop(ruleIndex, r));
                _running.Add(co);
            }
        }
    }

    /// <summary>
    /// �Ϸ� ����: �ֱ� �ߴ�, (�ɼ�) ���� �� ����
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
        // Bounds ���
        var area = spawnArea ? spawnArea : GetComponent<BoxCollider2D>();
        if (area) _bounds = CalcWorldBounds(area);

        // Player ĳ��
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

        // ���� ���� ����
        PruneAlive();
        int aliveOfThisRule = CountAliveOf(rule.prefab);
        if (rule.maxAlive > 0 && aliveOfThisRule >= rule.maxAlive) return false;

        // ���� �ѷ� ����
        int spawnedSoFar = _spawnedCountPerRule.TryGetValue(ruleIndex, out var v) ? v : 0;
        if (rule.totalThisDay > 0 && spawnedSoFar >= rule.totalThisDay) return false;

        // ��ġ ����
        if (!TryPickSpawnPosition(out var pos)) return false;

        // ����
        var go = Instantiate(rule.prefab, pos, Quaternion.identity);
        _alive.Add(go);
        _spawnedCountPerRule[ruleIndex] = spawnedSoFar + 1;

        // �ı�/��� �� alive ���� (��ū ����)
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

            // ��ֹ� ��ħ üũ
            if (Physics2D.OverlapBox(p, overlapBoxHalfExtents * 2f, 0f, obstacleMask))
                continue;

            // �÷��̾� �ּ� �Ÿ�
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
    /// ������ ���� �ı��� �� EnemySpawner�� alive ����Ʈ�� ������ �� �ְ� �ϴ� ��ū.
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
