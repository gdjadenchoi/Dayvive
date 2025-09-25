// Assets/Scripts/Outgame/OutgameUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dayvive;

public class OutgameUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text dayLabel;   // "Day X"
    [SerializeField] Button startButton;  // "다음날 시작"

    void Start()
    {
        EnsureGM();
        Refresh();
        if (startButton) startButton.onClick.AddListener(OnStartClicked);
    }

    void Refresh()
    {
        int day = (GameManager.I != null) ? GameManager.I.CurrentDay : 1;
        if (dayLabel) dayLabel.text = $"Day {day}";
    }

    void OnStartClicked()
    {
        GameManager.I.StartNextDay();
    }

    static void EnsureGM()
    {
        if (GameManager.I == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>(); // DDOL 자동 적용
        }
    }
}
