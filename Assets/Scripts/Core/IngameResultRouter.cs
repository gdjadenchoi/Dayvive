// Assets/Scripts/Systems/IngameResultRouter.cs  (v1.1)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dayvive;

public class IngameResultRouter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] DayTimer dayTimer;        // SampleScene�� DayTimer
    [SerializeField] Button nextDayButton;     // CenterStage/EndPanel/NextDayButton

    void Reset()
    {
        if (!dayTimer) dayTimer = FindFirstObjectByType<DayTimer>();
        if (!nextDayButton)
        {
            var btnGO = GameObject.Find("NextDayButton");
            if (btnGO) nextDayButton = btnGO.GetComponent<Button>();
        }
    }

    void OnEnable()
    {
        // Day ���� ���� �� �� �� �����ε� (Ÿ�̹� ����)
        if (dayTimer != null)
            dayTimer.onDayEnd.AddListener(RebindButton);

        // ���ʿ��� �����ε� �õ�
        RebindButton();
    }

    void OnDisable()
    {
        if (dayTimer != null)
            dayTimer.onDayEnd.RemoveListener(RebindButton);
    }

    void RebindButton()
    {
        if (nextDayButton == null)
        {
            var btnGO = GameObject.Find("NextDayButton");
            if (btnGO) nextDayButton = btnGO.GetComponent<Button>();
        }
        if (nextDayButton != null)
        {
            nextDayButton.onClick.RemoveAllListeners();
            nextDayButton.onClick.AddListener(RouteToOutgame);
        }
    }

    void RouteToOutgame()
    {
        EnsureGM();

        // RunInventory ������
        Dictionary<string, int> loot = new();
        if (RunInventory.I != null)
            foreach (var kv in RunInventory.I.All()) loot[kv.Key] = kv.Value;

        // Outgame Day ǥ���
        int dayIndex = (GameManager.I != null) ? GameManager.I.CurrentDay : (dayTimer ? dayTimer.DayIndex : 1);

        var result = new DayResult
        {
            dayIndex = dayIndex,
            loot = loot,
            dayLengthSeconds = 0f // DayTimer�� public getter �߰� ������ 0
        };

        GameManager.I.GoToOutgame(result);
    }

    static void EnsureGM()
    {
        if (GameManager.I == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>(); // DDOL �ڵ� ����
        }
    }
}
