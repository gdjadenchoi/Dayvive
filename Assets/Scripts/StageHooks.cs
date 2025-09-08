// Assets/Scripts/StageHooks.cs
using UnityEngine;

public class StageHooks : MonoBehaviour
{
    [Header("Refs")]
    public StageController stage;      // StageController �巡��
    public MiningSystem mining;        // Player�� MiningSystem �巡�� (����)

    // DayTimer.OnDayStart �� ����
    public void OnDayStart()
    {
        // �Ϸ� ����: ���� + ä�� ON
        if (stage) stage.SpawnForDay();   // dayIndex ���� �ʿ� ������ �⺻��(1) ���
        if (mining) mining.enabled = true;
    }

    // DayTimer.OnDayEnd �� ����
    public void OnDayEnd()
    {
        // �Ϸ� ����: ä�� OFF + �������� ����(��ȹ�� ���� RefillIfNeeded�� �ٲ㵵 OK)
        if (mining) mining.enabled = false;
        if (stage) stage.ClearStage();
        // �Ǵ�: if (stage) stage.RefillIfNeeded();
    }
}
