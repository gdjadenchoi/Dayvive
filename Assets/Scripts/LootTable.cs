// Scripts/LootTable.cs  (Ʋ��)
using UnityEngine;

[CreateAssetMenu(menuName = "Dayvive/Loot Table")]
public class LootTable : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string resourceId;  // ��: "wood", "stone", "water"
        public int min = 1;
        public int max = 1;
        public int weight = 1;     // ����ġ (Ȯ��)
    }
    public Entry[] entries;
}
