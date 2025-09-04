// Scripts/StageController.cs
using UnityEngine;

public class StageController : MonoBehaviour
{
    public StageConfig stage;
    public BoxCollider2D spawnArea;   // 이 범위 안에서만 스폰

    void Start()
    {
        if (stage == null || spawnArea == null)
        {
            Debug.LogError("StageController: stage or spawnArea not assigned.");
            enabled = false; return;
        }

        // 초기 배치
        foreach (var e in stage.entries)
            for (int i = 0; i < e.startCount; i++) Spawn(e);
    }

    void Update()
    {
        // 유지 보충
        foreach (var e in stage.entries)
        {
            int current = CountOnField(e.id);
            if (current < e.minOnField)
                for (int i = 0; i < e.refillBatch; i++) Spawn(e);
        }
    }

    int CountOnField(string id)
    {
        // 기존: var all = FindObjectsOfType<Mineable>();
        var all = Object.FindObjectsByType<Mineable>(FindObjectsSortMode.None);
        int c = 0;
        for (int i = 0; i < all.Length; i++)
            if (all[i].id == id) c++;
        return c;
    }


    // StageController.cs 안 Spawn 함수 교체
    void Spawn(StageConfig.SpawnEntry e)
    {
        int safety = 0;
        while (safety < 50) // 무한루프 방지
        {
            Vector2 p = RandomPointInBounds(spawnArea.bounds);
            bool overlap = false;
            Collider2D[] hits = Physics2D.OverlapCircleAll(p, 0.6f); // 반경 0.6 안 겹치면 OK
            foreach (var h in hits)
            {
                if (h.CompareTag("Resource"))
                {
                    overlap = true; break;
                }
            }

            if (!overlap)
            {
                Instantiate(e.prefab, p, Quaternion.identity);
                return;
            }
            safety++;
        }
    }


    Vector2 RandomPointInBounds(Bounds b)
    {
        return new Vector2(Random.Range(b.min.x, b.max.x),
                           Random.Range(b.min.y, b.max.y));
    }
}
