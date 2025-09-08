// Assets/Scripts/Mineable.cs
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class Mineable : MonoBehaviour
{
    [Header("Identity")]
    public string id = "A";

    [Header("Health")]
    public int maxHP = 5;
    [HideInInspector] public int hp;

    [Header("UI")]
    [SerializeField] private TMP_Text hpText; // 3D/UGUI ��� ����

    [Header("Loot")]
    [SerializeField] private LootTable lootTable; // �ν����Ϳ��� ����(A/B��)

    void Awake()
    {
        if (hpText == null) hpText = GetComponentInChildren<TMP_Text>(true);
        if (hpText == null)
        {
            Debug.LogError($"[Mineable] TMP_Text not found under '{name}'. " +
                           "HP ǥ�ÿ� �ؽ�Ʈ�� �ڽĿ� �ΰų�, hpText ���Կ� �巡���ϼ���.", this);
        }

        hp = maxHP;
        UpdateText();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (hpText == null)
            hpText = GetComponentInChildren<TMP_Text>(true);

        if (!Application.isPlaying) UpdateText();
    }
#endif

    public void ApplyDamage(int damage)
    {
        hp -= damage;
        UpdateText();
        if (hp <= 0) OnMined();
    }

    void UpdateText()
    {
        if (hpText != null) hpText.text = hp.ToString();
    }

    void OnMined()
    {
        // ��� ����
        if (lootTable != null && RunInventory.I != null)
        {
            var drops = lootTable.Roll();           // drop.itemId, drop.count ����
            foreach (var drop in drops)
                RunInventory.I.Add(drop.itemId, drop.count);
        }
        else
        {
            // ���̺��� ������ �ӽ÷� �ڱ� id 1�� ����
            if (RunInventory.I != null) RunInventory.I.Add(id, 1);
        }

        Destroy(gameObject);
    }
}
