using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[DisallowMultipleComponent]
public class BoundaryBuilder : MonoBehaviour
{
    [Header("Source Bounds")]
    [SerializeField] private BoxCollider2D spawnArea;

    [Header("Wall Settings")]
    [SerializeField, Min(0f)] private float thickness = 0.5f;
    [SerializeField] private float inset = 0.0f;
    [SerializeField] private string wallLayerName = "Obstacle";

    [Header("Options")]
    [SerializeField] private bool autoUpdateInEditMode = true;
    [SerializeField] private bool logDebug = false;

    const string kContainerName = "_BoundaryWalls";
    const string kNorth = "North";
    const string kSouth = "South";
    const string kEast = "East";
    const string kWest = "West";

    Transform _container;

    void Reset()
    {
        if (!spawnArea) spawnArea = GetComponent<BoxCollider2D>();
    }

    void OnEnable() => TryBuild();
    void OnValidate() => TryBuild();

#if UNITY_EDITOR
    void Update()
    {
        if (!Application.isPlaying && autoUpdateInEditMode)
            TryBuild();
    }
#endif

    void TryBuild()
    {
        if (!spawnArea) return;
        if (IsInPrefabAssetOrPrefabStage(gameObject)) return; // 프리팹 에셋/프리팹 모드 가드

        EnsureContainer();
        BuildWalls();
    }

    void EnsureContainer()
    {
        if (_container && _container.parent == transform) return;

        var t = transform.Find(kContainerName);
        if (t != null) _container = t;
        else
        {
            var go = new GameObject(kContainerName);
            go.transform.SetParent(transform, false);
            // ✨ hideFlags 제거 (DontSaveInEditor 사용 안 함)
            _container = go.transform;
        }
    }

    void BuildWalls()
    {
        Bounds b = CalcWorldBounds(spawnArea, inset);

        float t = Mathf.Max(0.0001f, thickness);
        Vector2 sizeH = new Vector2(b.size.x + t * 2f, t);
        Vector2 sizeV = new Vector2(t, b.size.y + t * 2f);

        Vector3 topPos = new Vector3(b.center.x, b.max.y + t * 0.5f, 0f);
        Vector3 bottomPos = new Vector3(b.center.x, b.min.y - t * 0.5f, 0f);
        Vector3 leftPos = new Vector3(b.min.x - t * 0.5f, b.center.y, 0f);
        Vector3 rightPos = new Vector3(b.max.x + t * 0.5f, b.center.y, 0f);

        SetupWall(kNorth, topPos, sizeH);
        SetupWall(kSouth, bottomPos, sizeH);
        SetupWall(kWest, leftPos, sizeV);
        SetupWall(kEast, rightPos, sizeV);

        if (logDebug) Debug.Log("[BoundaryBuilder] Walls built/updated.", this);
    }

    void SetupWall(string name, Vector3 worldPos, Vector2 size)
    {
        var t = FindOrCreateChild(name);
        t.position = worldPos;
        t.rotation = Quaternion.identity;

        var go = t.gameObject;
        go.layer = GetLayerByNameSafe(wallLayerName);

        if (!go.TryGetComponent<BoxCollider2D>(out var col))
            col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        col.size = size;
        col.offset = Vector2.zero;

        if (!go.TryGetComponent<Rigidbody2D>(out var rb))
            rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        rb.simulated = true;
        rb.interpolation = RigidbodyInterpolation2D.None;
        rb.useFullKinematicContacts = false;
    }

    Transform FindOrCreateChild(string childName)
    {
        var child = _container.Find(childName);
        if (child) return child;

        var go = new GameObject(childName);
        go.transform.SetParent(_container, false);
        return go.transform;
    }

    static Bounds CalcWorldBounds(BoxCollider2D box, float inset)
    {
        var t = box.transform;
        var size = Vector2.Scale(box.size, t.lossyScale);
        var center = (Vector3)(Vector2)t.TransformPoint(box.offset);
        var b = new Bounds(center, size);

        if (Mathf.Abs(inset) > 1e-5f)
        {
            var s = b.size;
            s.x = Mathf.Max(0f, s.x - inset * 2f);
            s.y = Mathf.Max(0f, s.y - inset * 2f);
            b.size = s;
        }
        return b;
    }

    static int GetLayerByNameSafe(string layerName)
    {
        if (string.IsNullOrEmpty(layerName)) return 0;
        int l = LayerMask.NameToLayer(layerName);
        return l < 0 ? 0 : l;
    }

#if UNITY_EDITOR
    static bool IsInPrefabAssetOrPrefabStage(GameObject go)
    {
        if (PrefabUtility.IsPartOfPrefabAsset(go)) return true;
        var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null && go.scene == stage.scene) return true;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (!spawnArea) return;
        var b = CalcWorldBounds(spawnArea, inset);
        Gizmos.color = new Color(1f, 1f, 1f, 0.35f);
        Gizmos.DrawWireCube(b.center, b.size);
    }
#endif
}
