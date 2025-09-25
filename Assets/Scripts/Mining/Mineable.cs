// Assets/Scripts/Mineable.cs
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class Mineable : MonoBehaviour, IDamageable
{
    [Header("Identity")]
    public string id = "A";

    [Header("Health")]
    public int hpMax = 3;
    [HideInInspector] public int hp = 3;

    [Header("UI (Optional)")]
    [SerializeField] TextMeshPro hpText;

    [Header("Drops")]
    [SerializeField] LootTable lootTable; // 없으면 id 1개 기본 지급

    // 마지막 타격 타입(드랍 조건 판단)
    DamageType _lastDamageType = DamageType.Generic;

    // 파괴 시 리필 요청을 위한 StageController 캐시 (정적)
    static StageController s_stage;

    void OnValidate()
    {
        if (hpMax < 1) hpMax = 1;
        hp = Mathf.Clamp(hp, 0, hpMax);
        UpdateHpUI();
    }

    void OnEnable()
    {
        hp = hpMax;
        UpdateHpUI();
        EnsureStageCached();
    }

    void EnsureStageCached()
    {
#if UNITY_2022_2_OR_NEWER
        if (s_stage == null) s_stage = Object.FindFirstObjectByType<StageController>();
#else
        if (s_stage == null) s_stage = Object.FindObjectOfType<StageController>();
#endif
    }

    // === IDamageable 구현(확장 시그니처) ===
    public void ApplyDamage(int amount, DamageType type)
    {
        if (hp <= 0) return;

        _lastDamageType = type;
        int dmg = Mathf.Max(0, amount);

        hp = Mathf.Max(0, hp - dmg);
        UpdateHpUI();

        if (hp == 0) Die();
    }

    void UpdateHpUI()
    {
        if (hpText) hpText.text = hp.ToString();
    }

    void Die()
    {
        // 채굴로 파괴된 경우에만 드랍
        if (_lastDamageType == DamageType.Mining)
        {
            if (lootTable != null)
            {
                var drops = lootTable.Roll();
                foreach (var d in drops) RunInventory.I?.Add(d.itemId, d.count);
            }
            else
            {
                RunInventory.I?.Add(id, 1);
            }
        }
        // 총알/폭발/기타는 드랍 없음

        EnsureStageCached();
        s_stage?.RequestRefillNextFrame();

        Destroy(gameObject);
    }
}
