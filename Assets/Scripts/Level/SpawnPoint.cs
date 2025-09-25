// Assets/Scripts/SpawnPoint.cs
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public enum SpawnType { Resource, Enemy }

    [Header("Spawn Settings")]
    [SerializeField] private GameObject prefab;    // 스폰할 프리팹
    [SerializeField] private int count = 1;        // 생성 개수
    [SerializeField] private SpawnType spawnType;  // 리소스/적 구분

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
