// Assets/Scripts/SpawnPoint.cs
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public enum SpawnType { Resource, Enemy }

    [Header("Spawn Settings")]
    [SerializeField] private GameObject prefab;    // ������ ������
    [SerializeField] private int count = 1;        // ���� ����
    [SerializeField] private SpawnType spawnType;  // ���ҽ�/�� ����

    public GameObject Prefab => prefab;
    public int Count => count;
    public SpawnType Type => spawnType;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!prefab) return;

        Gizmos.color = spawnType == SpawnType.Resource ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Gizmos.color;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, $"{spawnType} x{count}", style);
    }
#endif
}
