// Assets/Scripts/LootTable.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Dayvive/LootTable")]
public class LootTable : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string itemId = "a";  // a/b/c 등
        public Vector2Int countRange = new Vector2Int(1, 1); // [min, max]
        [Range(0f, 100f)] public float weight = 1f;          // 가중치
    }

    [Serializable]
    public struct Drop   // ← Mineable.cs가 기대하는 필드 이름
    {
        public string itemId;
        public int count;
        public Drop(string id, int c) { itemId = id; count = c; }
    }

    [Header("Rolls per mine (예: 한 번 캘 때 몇 번 추첨?)")]
    public int rolls = 1;

    public List<Entry> entries = new();

    // Mineable.cs가 호출하는 API
    public List<Drop> Roll()
    {
        var result = new List<Drop>();
        if (entries == null || entries.Count == 0 || rolls <= 0) return result;

        // 누적 가중치
        float totalW = 0f;
        foreach (var e in entries) totalW += Mathf.Max(0f, e.weight);

        for (int r = 0; r < rolls; r++)
        {
            if (totalW <= 0f) break;
            float pick = Random.value * totalW;
            float acc = 0f;

            foreach (var e in entries)
            {
                float w = Mathf.Max(0f, e.weight);
                acc += w;
                if (pick <= acc)
                {
                    int c = Random.Range(e.countRange.x, e.countRange.y + 1);
                    if (c > 0) result.Add(new Drop(e.itemId, c));
                    break;
                }
            }
        }
        return result;
    }
}
