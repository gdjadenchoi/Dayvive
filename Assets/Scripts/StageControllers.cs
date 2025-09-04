// Scripts/StageController.cs
using UnityEngine;

public class StageController : MonoBehaviour
{
    public StageConfig stage;
    public BoxCollider2D spawnArea;   // �� ���� �ȿ����� ����

    void Start()
    {
        if (stage == null || spawnArea == null)
        {
            Debug.LogError("StageController: stage or spawnArea not assigned.");
            enabled = false; return;
        }

        // �ʱ� ��ġ
        foreach (var e in stage.entries)
            for (int i = 0; i < e.startCount; i++) Spawn(e);
    }

    void Update()
    {
        // ���� ����
        foreach (var e in stage.entries)
        {
            int current = CountOnField(e.id);
            if (current < e.minOnField)
                for (int i = 0; i < e.refillBatch; i++) Spawn(e);
        }
    }

    int CountOnField(string id)
    {
        // ����: var all = FindObjectsOfType<Mineable>();
        var all = Object.FindObjectsByType<Mineable>(FindObjectsSortMode.None);
        int c = 0;
        for (int i = 0; i < all.Length; i++)
            if (all[i].id == id) c++;
        return c;
    }


    // StageController.cs �� Spawn �Լ� ��ü
    void Spawn(StageConfig.SpawnEntry e)
    {
        int safety = 0;
        while (safety < 50) // ���ѷ��� ����
        {
            Vector2 p = RandomPointInBounds(spawnArea.bounds);
            bool overlap = false;
            Collider2D[] hits = Physics2D.OverlapCircleAll(p, 0.6f); // �ݰ� 0.6 �� ��ġ�� OK
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
