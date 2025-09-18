using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CombatAimGuide : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform player;        // 보통 Player transform
    [SerializeField] Camera cam;              // 메인 카메라
    [SerializeField] PlayerMode playerMode;   // 모드 확인

    [Header("Ammo")]
    [SerializeField] CombatAmmo ammo;         // ← 이 필드가 인스펙터 슬롯으로 보임

    [Header("Range")]
    [SerializeField] float maxRange = 6f;
    [SerializeField] bool clampToMaxRange = true;

    [Header("Style")]
    [SerializeField] float lineWidth = 0.06f;
    [SerializeField] Color canShootColor = new Color(1f, 0.3f, 0.2f, 0.9f);
    [SerializeField] Color noAmmoColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
    [SerializeField] bool simulateHasAmmo = false; // 테스트용 토글(실탄 연결 없을 때 사용)

    LineRenderer lr;

    void Reset()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.widthMultiplier = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.numCapVertices = 8;
    }

    void Awake()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        if (!cam) cam = Camera.main;
        if (!player) player = transform.root;
        if (!playerMode) playerMode = GetComponentInParent<PlayerMode>();
        if (!ammo) ammo = GetComponentInParent<CombatAmmo>(); // 있으면 자동 바인딩

        ApplyColor();
        UpdateEnable();

        if (playerMode) playerMode.OnModeChanged += OnModeChanged;
        if (ammo) ammo.OnAmmoChanged += OnAmmoChanged;
    }

    void OnDestroy()
    {
        if (playerMode) playerMode.OnModeChanged -= OnModeChanged;
        if (ammo) ammo.OnAmmoChanged -= OnAmmoChanged;
    }

    void OnModeChanged(PlayerMode.Mode _)
    {
        UpdateEnable();
    }

    void OnAmmoChanged()
    {
        ApplyColor();
    }

    void Update()
    {
        // 전투 모드가 아니면 숨김
        if (!IsCombat())
        {
            if (lr.enabled) lr.enabled = false;
            return;
        }
        if (!lr.enabled) lr.enabled = true;

        if (!player || !cam) return;

        // 마우스 방향 계산
        Vector3 m = cam.ScreenToWorldPoint(Input.mousePosition);
        m.z = player.position.z;

        Vector3 start = player.position;
        Vector3 dir = (m - start);
        float dist = dir.magnitude;
        if (dist < 1e-4f) return;
        dir /= dist;

        Vector3 end = (clampToMaxRange && dist > maxRange) ? start + dir * maxRange : m;

        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // 현재 탄 보유에 따라 색상 갱신
        lr.startColor = lr.endColor = HasAmmoNow() ? canShootColor : noAmmoColor;
    }

    bool HasAmmoNow()
    {
        // ammo 컴포넌트가 있으면 그 값을, 없으면 테스트 토글 사용
        return ammo ? ammo.HasAmmo : simulateHasAmmo;
    }

    public void SetMaxRange(float r) => maxRange = Mathf.Max(0f, r);

    public float MaxRange => maxRange;

    void ApplyColor()
    {
        if (!lr) return;
        lr.widthMultiplier = lineWidth;
        lr.startColor = lr.endColor = HasAmmoNow() ? canShootColor : noAmmoColor;
    }

    bool IsCombat() => playerMode == null || playerMode.Current == PlayerMode.Mode.Combat;

    void UpdateEnable()
    {
        if (lr) lr.enabled = IsCombat();
    }
}
