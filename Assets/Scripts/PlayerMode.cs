using UnityEngine;
using System;

public class PlayerMode : MonoBehaviour
{
    public enum Mode { Mining, Combat }
    public Mode CurrentMode { get; private set; } = Mode.Mining;

    [Header("UI References")]
    [SerializeField] GameObject miningUI;   // RangeRing µî
    [SerializeField] GameObject combatUI;   // CombatAimGuide µî

    public event Action<Mode> OnModeChanged;

    void Awake() => UpdateUI();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ToggleMode();
    }

    public void ToggleMode()
    {
        CurrentMode = (CurrentMode == Mode.Mining) ? Mode.Combat : Mode.Mining;
        UpdateUI();
        OnModeChanged?.Invoke(CurrentMode);
        Debug.Log($"Mode: {CurrentMode}");
    }

    void UpdateUI()
    {
        if (miningUI) miningUI.SetActive(CurrentMode == Mode.Mining);
        if (combatUI) combatUI.SetActive(CurrentMode == Mode.Combat);
    }
}
