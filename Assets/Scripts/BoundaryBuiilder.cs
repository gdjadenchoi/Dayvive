using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// SpawnArea(BoxCollider2D)의 월드 경계를 기준으로
/// 보이지 않는 4면 벽(BoxCollider2D)을 자동 생성/갱신한다.
/// - 레이어: 지정한 레이어(예: "Obstacle")
/// - 시야 차단(SightSensor.obstacleMask) 및 이동/충돌용 경계
/// - Projectile은 hitMask에 Obstacle을 포함하지 않으면 통과(권장)
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class BoundaryBuilder : MonoBehaviour
{
    [Header("Source Bounds")]
    [Tooltip("경계로 삼을 박스. 비우면 이 오브젝트의 BoxCollider2D를 사용")]
    public BoxCollider2D sourceBounds;

    [Header("Wall Settings")]
    [Tooltip("벽 두께(월드 단위)")]
    [Min(0.01f)] public float thickness = 0.5f;
    [Tooltip("경계에서 안쪽/바깥쪽으로 여유를 둘 오프셋(+면 안쪽으로, -면 바깥쪽으로)")]
    public float inset = 0f;
    [Tooltip("벽에 적용할 레이어명(예: Obstacle)")]
    public string wallLayerName = "Obstacle";
    [Tooltip("런타임에서도 생성/유지(체크 해제 시 에디터에서만 유지)")]
    public bool buildAtRuntime = true;

    const string RootName = "_BoundaryWalls";
    Transform _root;

    void Reset()
    {
        if (!sourceBounds) sourceBounds = GetComponent<BoxCollider2D>();
    }

    void OnEnable()
    {
        if (!Application.isPlaying || buildAtRuntime)
            Build();
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        // 에디터에서 프리뷰 때문에 껐다 켰을 때 벽이 남는 걸 방지(선택)
        if (!Application.isPlaying) { /* 남겨두기 */ }
#endif
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
            Build();
    }

    /// <summary> 경계를 재생성 </summary>
    public void Build()
    {
        if (!sourceBounds)
        {
            Debug.LogWarning("[BoundaryBuilder] sourceBounds(BoxCollider2D)가 없습니다.", this);
            return;
        }

        EnsureRoot();

        // 대상 박스의 월드 경계 계산
        var t = sourceBounds.transform;
        Vector2 size = Vector2.Scale(sourceBounds.size, t.lossyScale);
        Vector2 center = (Vector2)t.TransformPoint(sourceBounds.offset);

        // 인셋 적용(안쪽으로 좁히기: +, 바깥으로 넓히기: -)
        size -= Vector2.one * inset * 2f;

        float left = center.x - size.x * 0.5f;
        float right = center.x + size.x * 0.5f;
        float bottom = center.y - size.y * 0.5f;
        float top = center.y + size.y * 0.5f;

        // 4면 벽 생성/갱신
        MakeWall("North", new Vector2(center.x, top + thickness * 0.5f), new Vector2(size.x + thickness * 2f, thickness));
        MakeWall("South", new Vector2(center.x, bottom - thickness * 0.5f), new Vector2(size.x + thickness * 2f, thickness));
        MakeWall("East", new Vector2(right + thickness * 0.5f, center.y), new Vector2(thickness, size.y));
        MakeWall("West", new Vector2(left - thickness * 0.5f, center.y), new Vector2(thickness, size.y));
    }

    void EnsureRoot()
    {
        if (_root == null)
        {
            var child = transform.Find(RootName);
            _root = child ? child : new GameObject(RootName).transform;
            _root.SetParent(transform, false);
            _root.localPosition = Vector3.zero;
            _root.localRotation = Quaternion.identity;
            _root.localScale = Vector3.one;
        }
    }

    void MakeWall(string name, Vector2 pos, Vector2 size)
    {
        var wall = _root.Find(name);
        if (!wall)
        {
            wall = new GameObject(name).transform;
            wall.SetParent(_root, false);
            wall.gameObject.hideFlags = HideFlags.None;
        }
        wall.position = pos;
        wall.rotation = Quaternion.identity;
        wall.localScale = Vector3.one;

        var col = wall.GetComponent<BoxCollider2D>();
        if (!col) col = wall.gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        col.size = size;

        // 레이어 세팅
        int layer = LayerMask.NameToLayer(wallLayerName);
        if (layer < 0)
        {
            // 레이어가 아직 없을 수 있음 → Default로 두고 경고
            if (Application.isEditor)
                Debug.LogWarning($"[BoundaryBuilder] '{wallLayerName}' 레이어가 존재하지 않습니다. Project Settings > Tags and Layers 에서 먼저 추가하세요.", this);
            layer = 0; // Default
        }
        wall.gameObject.layer = layer;

        // RigidBody2D 없이도 static collider로 충분
    }

#if UNITY_EDITOR
    [ContextMenu("Rebuild Now")]
    void RebuildNow() => Build();

    void OnDrawGizmosSelected()
    {
        if (!sourceBounds) return;
        var t = sourceBounds.transform;
        Vector2 size = Vector2.Scale(sourceBounds.size, t.lossyScale) - Vector2.one * inset * 2f;
        Vector2 center = (Vector2)t.TransformPoint(sourceBounds.offset);

        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.25f);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
