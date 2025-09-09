// Assets/Scripts/Mineable.cs
using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class Mineable : MonoBehaviour
{
    [Header("Identity")]
    public string id = "A";

    [Header("Health")]
    public int hpMax = 3;
    [HideInInspector] public int hp = 3; // 인스펙터 혼동 방지

    [Header("UI (Optional)")]
    [SerializeField] TextMeshPro hpText;

    [Header("Drops")]
    [SerializeField] LootTable lootTable; // 없으면 id 1개 기본 지급

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
        hp = hpMax;           // 항상 풀피로 시작
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

    // 기존 MiningSystem과 호환
    public void ApplyDamage(int dmg = 1) => TakeDamage(dmg);

    public void TakeDamage(int dmg = 1)
    {
        if (hp <= 0) return;
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
        // 드롭
        if (lootTable != null)
        {
            var drops = lootTable.Roll(); // drop.itemId, drop.count
            foreach (var d in drops) RunInventory.I?.Add(d.itemId, d.count);
        }
        else
        {
            RunInventory.I?.Add(id, 1);
        }

        // 다음 프레임에 리필 체크 (이 오브젝트가 실제로 사라진 뒤 카운트하게)
        // StartCoroutine(_RefillNextFrame());
        EnsureStageCached();
        s_stage?.RequestRefillNextFrame();

        Destroy(gameObject);
    }

    IEnumerator _RefillNextFrame()
    {
        yield return null;
        if (s_stage != null)
        {
            s_stage.RefillIfNeeded();
        }
    }
}
