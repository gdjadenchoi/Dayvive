using System;
using UnityEngine;

public class PlayerMode : MonoBehaviour
{
    public enum Mode { Mining, Combat }

    [SerializeField] Mode mode = Mode.Mining;
    public Mode Current => mode;

    // �ܺο��� ���� ���� ����
    public bool IsMining => mode == Mode.Mining;
    public bool IsCombat => mode == Mode.Combat;

    // UI/�ý��� �˸���
    public event Action<Mode> OnModeChanged;

    [Header("UI References")]
    [SerializeField] GameObject miningUI;
    [SerializeField] GameObject combatUI;

    void Awake() => ApplyModeVisuals();

    void Update()
    {
        // Space ���(���ϸ� �ٸ� �Է����� �ٲ㵵 ��)
        if (Input.GetKeyDown(KeyCode.Space))
            Toggle();
    }

    public void Toggle()
    {
        mode = (mode == Mode.Mining) ? Mode.Combat : Mode.Mining;
        ApplyModeVisuals();
        OnModeChanged?.Invoke(mode);
        Debug.Log($"Mode: {mode}");
    }

    public void SetMode(Mode m)
    {
        if (mode == m) return;
        mode = m;
        ApplyModeVisuals();
        OnModeChanged?.Invoke(mode);
    }

    void ApplyModeVisuals()
    {
        if (miningUI) miningUI.SetActive(IsMining);
        if (combatUI) combatUI.SetActive(IsCombat);
    }
}
