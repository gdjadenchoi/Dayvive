// Scripts/LootTable.cs  (틀만)
using UnityEngine;

[CreateAssetMenu(menuName = "Dayvive/Loot Table")]
public class LootTable : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string resourceId;  // 예: "wood", "stone", "water"
        public int min = 1;
        public int max = 1;
        public int weight = 1;     // 가중치 (확률)
    }
    public Entry[] entries;
}
