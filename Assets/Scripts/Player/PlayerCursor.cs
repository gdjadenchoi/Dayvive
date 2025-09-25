using UnityEngine;

public class PlayerCursor : MonoBehaviour
{
    [Header("Follow")]
    public float followSpeed = 24f;

    [Header("Clamp (Play Area)")]
    public BoxCollider2D spawnArea;  // 필드 영역
    public float margin = 0.3f;      // 벽에서 약간 여유

    [Header("Range")]
    public float rangeRadius = 0.45f; // 업그레이드로 키워줄 값
    public bool showRange = true;

    Vector3 targetPos;

    void Reset()
    {
        if (!spawnArea)
            // 기존: spawnArea = FindObjectOfType<BoxCollider2D>();
            spawnArea = Object.FindFirstObjectByType<BoxCollider2D>();
    }


    void Update()
    {
        // 입력 → 화면좌표 → 월드좌표
        Vector3 screenPos = (Input.touchCount > 0)
            ? (Vector3)Input.GetTouch(0).position
            : Input.mousePosition;

        screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
        targetPos = Camera.main.ScreenToWorldPoint(screenPos);
        targetPos.z = 0f;

        // 필드 경계로 클램프
        if (spawnArea)
        {
            var b = spawnArea.bounds;
            float x = Mathf.Clamp(targetPos.x, b.min.x + margin, b.max.x - margin);
            float y = Mathf.Clamp(targetPos.y, b.min.y + margin, b.max.y - margin);
            targetPos = new Vector3(x, y, 0f);
        }

        // 스무딩 이동
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (!showRange) return;
        Gizmos.color = new Color(0f, 1f, 0.6f, 0.35f);
        Gizmos.DrawSphere(transform.position, rangeRadius);
    }
}
