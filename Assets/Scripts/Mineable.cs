// Assets/Scripts/Mineable.cs
using UnityEngine;
using TMPro;

public class Mineable : MonoBehaviour
{
    [Header("Identity")]
    public string id = "A";

    [Header("Health")]
    public int maxHP = 5;
    [HideInInspector] public int hp;

    [Header("UI")]
    [SerializeField] private TMP_Text hpText; // 3D/UGUI 모두 가능

    void Awake()
    {
        // 자동 바인딩 (런타임)
        if (hpText == null) hpText = GetComponentInChildren<TMP_Text>(true);

        if (hpText == null)
        {
            Debug.LogError($"[Mineable] TMP_Text not found under '{name}'. " +
                           $"Add a TextMeshPro(3D) or TextMeshProUGUI child named 'HPText' (or any), " +
                           $"or drag it to 'hpText' slot in Inspector.", this);
        }

        hp = maxHP;
        UpdateText();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // 에디터에서 값이 바뀌거나 프리팹 열었을 때 자동 연결
        if (hpText == null)
            hpText = GetComponentInChildren<TMP_Text>(true);
        // 미리보기 숫자 갱신
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
        Destroy(gameObject);
    }
}
