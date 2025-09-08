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
    [SerializeField] private LayerMask blockMask;     // ��ġ�� �� �Ǵ� ���̾�(��: Resource)
    [SerializeField] private float spawnPadding = 0.05f; // ���� �� �ּ� ����
    [SerializeField] private int maxSpawnTries = 50;     // �� �ڸ� ã�� �ִ� �õ�

    private readonly List<GameObject> spawned = new List<GameObject>();

    void OnValidate()
    {
        if (!spawnArea) spawnArea = GetComponent<BoxCollider2D>();
        if (!container) container = transform;
    }

    // �Ϸ� ���� �� ���� ����
    public void SpawnForDay(int dayIndex = 1)
    {
        ClearStage();
        if (!config || config.entries == null || !spawnArea) return;

        foreach (var e in config.entries)
        {
            if (!e.prefab || e.startCount <= 0) continue;

            for (int i = 0; i < e.startCount; i++)
                SpawnOne(e.prefab);
        }
    }

    // �ʵ忡 �ʹ� �پ������� ����
    public void RefillIfNeeded()
    {
        if (!config) return;

#if UNITY_2022_2_OR_NEWER
        var all = Object.FindObjectsByType<Mineable>(FindObjectsSortMode.None);
#else
        var all = Object.FindObjectsOfType<Mineable>(false);
#endif

        foreach (var entry in config.entries)
        {
            int alive = 0;
            foreach (var m in all)
                if (m && m.id == entry.id) alive++;

            if (alive < entry.minOnField)
            {
                int need = Mathf.Max(0, entry.minOnField - alive);
                int spawnCount = Mathf.Max(need, entry.refillBatch);
                for (int i = 0; i < spawnCount; i++)
                    SpawnOne(entry.prefab);
            }
        }
    }

    public void ClearStage()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
            if (spawned[i]) Destroy(spawned[i]);
        spawned.Clear();

        // ���� ��ġ�� ���ҽ����� ���� �����Ϸ��� �Ʒ��� ���(����)
        // foreach (var m in FindObjectsOfType<Mineable>(false)) Destroy(m.gameObject);
    }

    // ====== ���� ���� ======

    void SpawnOne(GameObject prefab)
    {
        if (!spawnArea) return;

        // �������� �뷫���� �ݰ� ����
        float radius = GetPrefabRadius(prefab);

        // �� �ڸ� Ž��
        for (int tries = 0; tries < maxSpawnTries; tries++)
        {
            Vector2 p = RandomPointIn(spawnArea);
            if (IsFreeAt(p, radius))
            {
                var go = Instantiate(prefab, p, Quaternion.identity, container);
                spawned.Add(go);
                return;
            }
        }

        // ���� �� �������� �׳� ����(ȥ�� ���)
        {
            Vector2 p = RandomPointIn(spawnArea);
            var go = Instantiate(prefab, p, Quaternion.identity, container);
            spawned.Add(go);
            Debug.LogWarning($"[StageController] �� �ڸ� Ž�� ����({maxSpawnTries}ȸ). ȥ�� ���·� �����߽��ϴ�.", this);
        }
    }

    // �������� �뷫�� �ݰ� ���(�ݶ��̴�/������ bounds ����)
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

    // �ش� ��ġ�� ������� �˻�(���� �ٻ�)
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

    // �����Ϳ�
    [ContextMenu("Spawn For Test")]
    void _EditorSpawn() => SpawnForDay(1);

    [ContextMenu("Clear Stage")]
    void _EditorClear() => ClearStage();
}
