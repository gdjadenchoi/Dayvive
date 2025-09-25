using UnityEngine;
using UnityEngine.UI;
using Dayvive; // GameManager 접근

[RequireComponent(typeof(BoxCollider2D))]
public class ExitZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float dwellTime = 5f;
    [Tooltip("탈출 성공 시 결과 패널(EndPanel)을 먼저 띄우고, 버튼으로 Outgame으로 나가게 합니다.")]
    [SerializeField] private bool showResultPanelFirst = true;

    [Header("UI")]
    [SerializeField] private Slider progressBar; // Optional: 탈출 게이지

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    private float dwellTimer;
    private bool playerInside;
    private bool done; // 중복 실행 방지

    void Reset()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col) col.isTrigger = true;
    }

    void OnEnable()
    {
        dwellTimer = 0f;
        playerInside = false;
        done = false;
        if (progressBar) progressBar.gameObject.SetActive(false);
    }

    void Update()
    {
        if (done || !playerInside) return;

        dwellTimer += Time.deltaTime;
        float t = Mathf.Clamp01(dwellTimer / dwellTime);
        if (progressBar) progressBar.value = t;

        if (dwellTimer >= dwellTime)
        {
            done = true;
            if (debugLog) Debug.Log("[ExitZone] 탈출 조건 충족. 결과 처리로 전환!");

            if (showResultPanelFirst)
            {
                var dayTimer = FindFirstObjectByType<DayTimer>();
                if (dayTimer != null) dayTimer.EndDay();
                else if (GameManager.I != null) GameManager.I.GoToOutgame(null);
            }
            else
            {
                if (GameManager.I != null) GameManager.I.GoToOutgame(null);
            }

            if (progressBar)
            {
                progressBar.value = 0f;
                progressBar.gameObject.SetActive(false);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (done || !other.CompareTag(playerTag)) return;
        playerInside = true;
        dwellTimer = 0f;
        if (progressBar) progressBar.gameObject.SetActive(true);
        if (debugLog) Debug.Log("[ExitZone] Player entered. Dwell timer started.");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (done || !other.CompareTag(playerTag)) return;
        playerInside = false;
        dwellTimer = 0f;
        if (progressBar)
        {
            progressBar.value = 0f;
            progressBar.gameObject.SetActive(false);
        }
        if (debugLog) Debug.Log("[ExitZone] Player exited. Dwell timer reset.");
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        var col = GetComponent<BoxCollider2D>();
        if (!col) return;

        Gizmos.color = new Color(0f, 0.5f, 1f, 0.15f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        // Handles.Label 등 Editor GUI 사용은 의도적으로 제거 (일부 에디터 버전에서 assertion 유발)
    }
#endif
}
