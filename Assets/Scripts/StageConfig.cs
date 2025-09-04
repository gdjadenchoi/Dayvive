// Scripts/StageConfig.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Dayvive/Stage Config")]
public class StageConfig : ScriptableObject
{
    [System.Serializable]
    public class SpawnEntry
    {
        public string id;            // "A" / "B" ...
        public GameObject prefab;    // �ش� ������Ʈ ������
        public int startCount = 10;  // ���� �� ��ġ ����
        public int minOnField = 6;   // �ʵ忡 �� �� ���Ϸ� ������
        public int refillBatch = 3;  // �̸�ŭ ���� ����
    }

    public SpawnEntry[] entries;
}
