// Assets/Scripts/RegionVolume.cs
using UnityEngine;

public class RegionVolume : MonoBehaviour
{
    public enum SpawnType { Resource, Enemy }

    [Header("Region Settings")]
    [SerializeField] private BoxCollider2D area;       // ���� ����
    [SerializeField] private SpawnType spawnType;      // ���ҽ� / �� ����
    [SerializeField] private GameObject prefab;        // �⺻ ������
    [SerializeField] private float density = 0.1f;     // 1���� ������ ����
    [SerializeField] private int maxSpawn = 20;        // �ִ� ���� ����

    public BoxCollider2D Area => area;
    public SpawnType Type => spawnType;
    public GameObject Prefab => prefab;
    public float Density => density;
    public int MaxSpawn => maxSpawn;

    void Reset()
    {
        if (!area) area = GetComponent<BoxCollider2D>();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!area) area = GetComponent<BoxCollider2D>();
        if (!area) return;

        Gizmos.color = spawnType == SpawnType.Resource ? new Color(0f, 1f, 0f, 0.25f) : new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawCube(area.bounds.center, area.bounds.size);
        Gizmos.color = spawnType == SpawnType.Resource ? Color.green : Color.red;
        Gizmos.DrawWireCube(area.bounds.center, area.bounds.size);

#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Gizmos.color;
        UnityEditor.Handles.Label(area.bounds.center, $"{spawnType}\nDensity={density}\nMax={maxSpawn}", style);
#endif
    }
#endif
}
