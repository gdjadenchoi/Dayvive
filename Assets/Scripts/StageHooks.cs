// Assets/Scripts/StageHooks.cs
using UnityEngine;

public class StageHooks : MonoBehaviour
{
    [Header("Refs")]
    public StageController stage;      // StageController 드래그
    public MiningSystem mining;        // Player의 MiningSystem 드래그 (선택)

    // DayTimer.OnDayStart 에 연결
    public void OnDayStart()
    {
        // 하루 시작: 스폰 + 채굴 ON
        if (stage) stage.SpawnForDay();   // dayIndex 전달 필요 없으면 기본값(1) 사용
        if (mining) mining.enabled = true;
    }

    // DayTimer.OnDayEnd 에 연결
    public void OnDayEnd()
    {
        // 하루 종료: 채굴 OFF + 스테이지 정리(기획에 따라 RefillIfNeeded로 바꿔도 OK)
        if (mining) mining.enabled = false;
        if (stage) stage.ClearStage();
        // 또는: if (stage) stage.RefillIfNeeded();
    }
}
