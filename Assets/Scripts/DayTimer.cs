// Assets/Scripts/DayTimer.cs
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class DayTimer : MonoBehaviour
{
    [Header("Day Settings")]
    [SerializeField] float secondsPerDay = 15f;   // 하루 길이(초)
    [SerializeField] bool autoStart = true;       // 시작 시 자동 시작

    [Header("UI (Optional)")]
    [SerializeField] TMP_Text timerLabel;         // 남은 시간 표시(선택)
    [SerializeField] GameObject endPanel;         // 하루 종료 패널(선택)
    [SerializeField] TMP_Text summaryLabel;       // 종료 시 요약 텍스트(선택)
    [SerializeField] Button nextDayButton;        // 다음 날 버튼(선택)

    [Header("Events")]
    public UnityEvent onDayStart;                 // 다른 시스템이 듣도록
    public UnityEvent onDayEnd;

    public int DayIndex { get; private set; } = 1;
    public float TimeLeft { get; private set; }
    public bool IsRunning { get; private set; }

    void Start()
    {
        if (endPanel) endPanel.SetActive(false);
        if (nextDayButton) nextDayButton.onClick.AddListener(NextDay);

        if (autoStart) StartDay();
        else UpdateTimerLabel();
    }

    void Update()
    {
        if (!IsRunning) return;

        TimeLeft -= Time.deltaTime;
        if (TimeLeft < 0f) TimeLeft = 0f;

        UpdateTimerLabel();

        if (TimeLeft <= 0f)
            EndDay();
    }

    // === Public API ===
    [ContextMenu("Start Day")]
    public void StartDay()
    {
        IsRunning = true;
        TimeLeft = Mathf.Max(0.01f, secondsPerDay);
        if (endPanel) endPanel.SetActive(false);

        onDayStart?.Invoke();
        UpdateTimerLabel();
    }

    [ContextMenu("Force End Day")]
    public void EndDay()
    {
        if (!IsRunning) return;

        IsRunning = false;
        TimeLeft = 0f;
        UpdateTimerLabel();

        // 요약 패널 표시
        if (endPanel) endPanel.SetActive(true);
        if (summaryLabel) summaryLabel.text = BuildSummaryText();

        onDayEnd?.Invoke();
    }

    public void NextDay()
    {
        DayIndex++;
        StartDay();
    }

    public void SetDayLength(float seconds)
    {
        secondsPerDay = Mathf.Max(1f, seconds);
        if (!IsRunning) UpdateTimerLabel();
    }

    // === Internal ===
    void UpdateTimerLabel()
    {
        if (!timerLabel) return;

        if (IsRunning)
        {
            int sec = Mathf.CeilToInt(TimeLeft);
            timerLabel.text = $"Day {DayIndex}  •  {sec}s";
        }
        else
        {
            timerLabel.text = $"Day {DayIndex}  •  0s";
        }
    }

    string BuildSummaryText()
    {
        // RunInventory(I) 가 있으면 내용 보여주고, 없으면 기본 메세지
        var invType = System.Type.GetType("RunInventory");
        if (invType != null)
        {
            // 정석 구현 가정:
            // - public static RunInventory I { get; }
            // - public System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,int>> All()
            var instProp = invType.GetProperty("I");
            var inst = instProp != null ? instProp.GetValue(null) : null;

            if (inst != null)
            {
                var allMethod = invType.GetMethod("All");
                if (allMethod != null)
                {
                    var enumerable = allMethod.Invoke(inst, null) as System.Collections.IEnumerable;

                    var sb = new StringBuilder();
                    sb.AppendLine($"Day {DayIndex} 결과");
                    bool any = false;
                    foreach (var item in enumerable)
                    {
                        // KeyValuePair<string,int> 읽기
                        var t = item.GetType();
                        var k = t.GetProperty("Key")?.GetValue(item)?.ToString();
                        var vObj = t.GetProperty("Value")?.GetValue(item);
                        int v = vObj != null ? (int)vObj : 0;

                        sb.AppendLine($"- {k} x{v}");
                        any = true;
                    }
                    if (!any) sb.AppendLine("- 획득 없음");

                    return sb.ToString();
                }
            }
        }

        // 인벤토리 시스템이 아직 없을 때
        return $"Day {DayIndex} 종료\n- 인벤토리 시스템 없음";
    }
}
