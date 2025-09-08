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
    [SerializeField] private TMP_Text hpText; // 3D/UGUI 모두 가능

    [Header("Loot")]
    [SerializeField] private LootTable lootTable; // 인스펙터에서 연결(A/B용)

    void Awake()
    {
        if (hpText == null) hpText = GetComponentInChildren<TMP_Text>(true);
        if (hpText == null)
        {
            Debug.LogError($"[Mineable] TMP_Text not found under '{name}'. " +
                           "HP 표시용 텍스트를 자식에 두거나, hpText 슬롯에 드래그하세요.", this);
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
        // 드롭 적립
        if (lootTable != null && RunInventory.I != null)
        {
            var drops = lootTable.Roll();           // drop.itemId, drop.count 가정
            foreach (var drop in drops)
                RunInventory.I.Add(drop.itemId, drop.count);
        }
        else
        {
            // 테이블이 없으면 임시로 자기 id 1개 적립
            if (RunInventory.I != null) RunInventory.I.Add(id, 1);
        }

        Destroy(gameObject);
    }
}
