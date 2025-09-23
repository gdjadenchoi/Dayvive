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
        [Min(0)] public int initialBurst = 0;      // �Ϸ� ���� ��� ���� ��
        [Min(0)] public int totalThisDay = 10;     // �Ϸ� ���� �� ���� ���� (0 �̸� ���� ����)
        [Min(0)] public int maxAlive = 5;          // ���� ���� ����

        [Header("Timing")]
        [Min(0f)] public float interval = 2.0f;    // �ֱ��� ���� ����(��)
        public bool respawn = true;                // ������ �ֱ������� �ٽ� ��������
    }

    [Tooltip("���� ��Ģ ��� (�������� 1���� �߰�)")]
    public List<SpawnRule> rules = new List<SpawnRule>();
}
