using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CombatAimGuide : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform player;        // ���� Player transform
    [SerializeField] Camera cam;              // ���� ī�޶�
    [SerializeField] PlayerMode playerMode;   // ��� Ȯ��

    [Header("Ammo")]
    [SerializeField] CombatAmmo ammo;         // �� �� �ʵ尡 �ν����� �������� ����

    [Header("Range")]
    [SerializeField] float maxRange = 6f;
    [SerializeField] bool clampToMaxRange = true;

    [Header("Style")]
    [SerializeField] float lineWidth = 0.06f;
    [SerializeField] Color canShootColor = new Color(1f, 0.3f, 0.2f, 0.9f);
    [SerializeField] Color noAmmoColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
    [SerializeField] bool simulateHasAmmo = false; // �׽�Ʈ�� ���(��ź ���� ���� �� ���)

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
        if (!ammo) ammo = GetComponentInParent<CombatAmmo>(); // ������ �ڵ� ���ε�

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
        // ���� ��尡 �ƴϸ� ����
        if (!IsCombat())
        {
            if (lr.enabled) lr.enabled = false;
            return;
        }
        if (!lr.enabled) lr.enabled = true;

        if (!player || !cam) return;

        // ���콺 ���� ���
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

        // ���� ź ������ ���� ���� ����
        lr.startColor = lr.endColor = HasAmmoNow() ? canShootColor : noAmmoColor;
    }

    bool HasAmmoNow()
    {
        // ammo ������Ʈ�� ������ �� ����, ������ �׽�Ʈ ��� ���
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
