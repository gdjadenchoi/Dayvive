using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dayvive/Enemy Spawn Config", fileName = "EnemySpawnConfig")]
public class EnemySpawnConfig : ScriptableObject
{
    [Serializable]
    public class SpawnRule
    {
        [Header("Prefab & Limits")]
        public GameObject prefab;
        [Min(0)] public int initialBurst = 0;      // 하루 시작 즉시 스폰 수
        [Min(0)] public int totalThisDay = 10;     // 하루 동안 총 스폰 상한 (0 이면 제한 없음)
        [Min(0)] public int maxAlive = 5;          // 동시 생존 상한

        [Header("Timing")]
        [Min(0f)] public float interval = 2.0f;    // 주기적 스폰 간격(초)
        public bool respawn = true;                // 죽으면 주기적으로 다시 스폰할지
    }

    [Tooltip("스폰 규칙 목록 (종류별로 1개씩 추가)")]
    public List<SpawnRule> rules = new List<SpawnRule>();
}
