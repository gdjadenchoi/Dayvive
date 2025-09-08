// Assets/Scripts/RunInventory.cs
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class RunInventory : MonoBehaviour
{
    public static RunInventory I;

    // �̹� '�Ϸ�' �հ�
    readonly Dictionary<string, int> day = new();
    // ��ü ����(���߿� �ƿ����� �ڿ����� Ȱ��)
    readonly Dictionary<string, int> total = new();

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Add(string itemId, int amount)
    {
        if (amount <= 0 || string.IsNullOrEmpty(itemId)) return;

        if (!day.ContainsKey(itemId)) day[itemId] = 0;
        if (!total.ContainsKey(itemId)) total[itemId] = 0;

        day[itemId] += amount;
        total[itemId] += amount;
    }

    public void ClearDay() => day.Clear();

    public string BuildDaySummary()
    {
        if (day.Count == 0) return "������ �ƹ��͵� ���� ���߾��.";
        var sb = new StringBuilder();
        foreach (var kv in day)
            sb.AppendLine($"{kv.Key} �� {kv.Value}");
        return sb.ToString();
    }
}
