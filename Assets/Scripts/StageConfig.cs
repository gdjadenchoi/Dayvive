// Scripts/StageConfig.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Dayvive/Stage Config")]
public class StageConfig : ScriptableObject
{
    [System.Serializable]
    public class SpawnEntry
    {
        public string id;            // "A" / "B" ...
        public GameObject prefab;    // 해당 오브젝트 프리팹
        public int startCount = 10;  // 시작 시 배치 개수
        public int minOnField = 6;   // 필드에 이 값 이하로 남으면
        public int refillBatch = 3;  // 이만큼 보충 스폰
    }

    public SpawnEntry[] entries;
}
