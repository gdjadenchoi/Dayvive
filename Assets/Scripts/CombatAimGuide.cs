using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CombatAimGuide : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform player;            // ���� Player transform
    [SerializeField] Camera cam;                  // ���� ī�޶�
    [SerializeField] PlayerMode playerMode;       // �� ��� Ȯ��

    [Header("Range")]
    [SerializeField] float maxRange = 6f;
    [SerializeField] bool clampToMaxRange = true;

    [Header("Style")]
    [SerializeField] float lineWidth = 0.06f;
    [SerializeField] Color canShootColor = new Color(1f, 0.3f, 0.2f, 0.9f);
    [SerializeField] Color noAmmoColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
    [SerializeField] bool simulateHasAmmo = true; // �ӽ�

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

        ApplyColor();
        UpdateEnable(); // ���� �� ��� �ݿ�
        // PlayerMode �̺�Ʈ�ε� �ݿ� (������)
        if (playerMode != null) playerMode.OnModeChanged += _ => UpdateEnable();
    }

    void OnDestroy()
    {
        if (playerMode != null) playerMode.OnModeChanged -= _ => UpdateEnable();
    }

    void Update()
    {
        // ��� üũ: Combat�� ���� ���� �׸���
        if (!IsCombat())
        {
            if (lr.enabled) lr.enabled = false;
            return;
        }
        if (!lr.enabled) lr.enabled = true;

        if (!player || !cam) return;

        Vector3 m = cam.ScreenToWorldPoint(Input.mousePosition);
        m.z = player.position.z;

        Vector3 start = player.position;
        Vector3 dir = (m - start);
        float dist = dir.magnitude;
        if (dist < 1e-4f) return;
        dir /= dist;

        Vector3 end = clampToMaxRange && dist > maxRange ? start + dir * maxRange : m;

        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lr.startColor = lr.endColor = simulateHasAmmo ? canShootColor : noAmmoColor;
    }

    public void SetHasAmmo(bool hasAmmo)
    {
        simulateHasAmmo = hasAmmo;
        ApplyColor();
    }

    public void SetMaxRange(float r) => maxRange = Mathf.Max(0f, r);

    void ApplyColor()
    {
        if (!lr) return;
        lr.startColor = lr.endColor = simulateHasAmmo ? canShootColor : noAmmoColor;
        lr.widthMultiplier = lineWidth;
    }

    bool IsCombat() => playerMode == null || playerMode.CurrentMode == PlayerMode.Mode.Combat;

    void UpdateEnable()
    {
        // ��� �ٲ� �� �ٷ� �Ѱ�/����
        bool show = IsCombat();
        if (lr) lr.enabled = show;
        // CombatUI�� Mining���� ������ ���� �־, �� ��ũ��Ʈ�� ������ ����
    }
}
