using UnityEngine;

/// <summary>
/// StageController에 런타임으로 StageConfig를 주입하기 위한 확장 메서드.
/// (기존 StageController.cs를 수정하지 않아도 됨)
/// </summary>
public static class StageControllerConfigExtensions
{
    /// <summary>StageController.config 필드에 그대로 대입.</summary>
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
