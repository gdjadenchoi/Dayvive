using UnityEngine;

/// <summary>
/// StageController�� ��Ÿ������ StageConfig�� �����ϱ� ���� Ȯ�� �޼���.
/// (���� StageController.cs�� �������� �ʾƵ� ��)
/// </summary>
public static class StageControllerConfigExtensions
{
    /// <summary>StageController.config �ʵ忡 �״�� ����.</summary>
    public static void SetConfig(this StageController sc, StageConfig cfg)
    {
        if (sc == null)
        {
            Debug.LogWarning("[StageControllerConfigExtensions] StageController is null.");
            return;
        }
        sc.config = cfg;
    }
}
