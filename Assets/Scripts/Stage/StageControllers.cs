// Assets/Scripts/StageController.cs
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
    [Header("Config")]
    public StageConfig config;
    public BoxCollider2D spawnArea;
    public Transform container;

    [Header("Spawn Collision")]
    [SerializeField] private LayerMask blockMask;        // 겹치면 안 되는 레이어(예: Resource)
    [SerializeField] private float spawnPadding = 0.05f; // 서로 간 최소 간격
    [SerializeField] private int maxSpawnTries = 50;     // 빈 자리 찾기 최대 시도

    private readonly List<GameObject> spawned = new List<GameObject>();

    void OnValidate()
    {
        if (!spawnArea) spawnArea = GetComponent<BoxCollider2D>();
        if (!container) container = transform;
    }

    // 하루 시작 시 최초 스폰
    public void SpawnForDay(int dayIndex = 1)
    {
        ClearStage();
        if (!config || config.entries == null || !spawnArea) return;

        foreach (var e in config.entries)
        {
            if (!e.prefab || e.startCount <= 0) continue;
            for (int i = 0; i < e.startCount; i++)
                SpawnOne(e.prefab, e.id);
        }

#if UNITY_EDITOR
        Debug.Log("[StageController] SpawnForDay completed.");
#endif
    }
    public void RequestRefillNextFrame()
    {
        StartCoroutine(_RefillNextFrame());
    }

    System.Collections.IEnumerator _RefillNextFrame()
    {
        yield return null;      // 다음 프레임까지 대기
        RefillIfNeeded();       // 여기서 실제 리필
    }
    // 부족할 때만 리필
    public void RefillIfNeeded()
    {
        if (!config || config.entries == null || !spawnArea) return;

        foreach (var entry in config.entries)
        {
            if (!entry.prefab || string.IsNullOrEmpty(entry.id)) continue;

            int current = CountInContainer(entry.id);
            int need = entry.minOnField - current;
            if (need <= 0) continue;

            int spawnNow = Mathf.Min(need, Mathf.Max(1, entry.refillBatch));

            for (int i = 0; i < spawnNow; i++)
                SpawnOne(entry.prefab, entry.id);

#if UNITY_EDITOR
            Debug.Log($"[StageController] Refilled {entry.id}: {current} -> {current + spawnNow} (min {entry.minOnField})");
#endif
        }
    }

    // 컨테이너 자식들만 검사 (씬의 다른 Mineable은 무시)
    int CountInContainer(string id)
    {
        if (!container) return 0;
        int count = 0;
        for (int i = 0; i < container.childCount; i++)
        {
            var m = container.GetChild(i).GetComponent<Mineable>();
            if (m && m.id == id) count++;
        }
        return count;
    }

    public void ClearStage()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
            if (spawned[i]) Destroy(spawned[i]);
        spawned.Clear();

#if UNITY_EDITOR
        Debug.Log("[StageController] ClearStage done.");
#endif
        // 수동 배치까지 싹 지우고 싶으면 아래도 사용(선택)
        // foreach (var m in FindObjectsOfType<Mineable>(false)) Destroy(m.gameObject);
    }

    // ====== 내부 구현 ======

    void SpawnOne(GameObject prefab, string idForLog = "")
    {
        if (!spawnArea) return;

        float radius = GetPrefabRadius(prefab);

        // 빈 자리 탐색
        for (int tries = 0; tries < maxSpawnTries; tries++)
        {
            Vector2 p = RandomPointIn(spawnArea);
            if (IsFreeAt(p, radius))
            {
                var go = Instantiate(prefab, p, Quaternion.identity, container);
                spawned.Add(go);
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(idForLog))
                    Debug.Log($"[StageController] Spawned {idForLog} at ({p.x:F2}, {p.y:F2})");
#endif
                return;
            }
        }

        // 실패 시 마지막에 그냥 스폰(혼잡 경고)
        {
            Vector2 p = RandomPointIn(spawnArea);
            var go = Instantiate(prefab, p, Quaternion.identity, container);
            spawned.Add(go);
#if UNITY_EDITOR
            Debug.LogWarning($"[StageController] 빈 자리 탐색 실패({maxSpawnTries}회). 혼잡 상태로 스폰했습니다.");
#endif
        }
    }

    float GetPrefabRadius(GameObject prefab)
    {
        float r = 0.2f; // fallback
        var cols = prefab.GetComponentsInChildren<Collider2D>(true);
        if (cols != null && cols.Length > 0)
        {
            Bounds b = cols[0].bounds;
            for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);

            if (b.size == Vector3.zero)
            {
                var rds = prefab.GetComponentsInChildren<Renderer>(true);
                if (rds.Length > 0)
                {
                    b = rds[0].bounds;
                    for (int i = 1; i < rds.Length; i++) b.Encapsulate(rds[i].bounds);
                }
            }
            r = 0.5f * Mathf.Max(b.size.x, b.size.y);
        }
        return Mathf.Max(0.01f, r);
    }

    bool IsFreeAt(Vector2 p, float radius)
    {
        float r = radius + spawnPadding;
        return Physics2D.OverlapCircle(p, r, blockMask) == null;
    }

    static Vector2 RandomPointIn(BoxCollider2D box)
    {
        Bounds b = box.bounds;
        float x = Random.Range(b.min.x, b.max.x);
        float y = Random.Range(b.min.y, b.max.y);
        return new Vector2(x, y);
    }

    [ContextMenu("Spawn For Test")] void _EditorSpawn() => SpawnForDay(1);
    [ContextMenu("Clear Stage")] void _EditorClear() => ClearStage();
}
