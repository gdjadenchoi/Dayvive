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
    [SerializeField] private TMP_Text hpText; // 3D/UGUI ��� ����

    void Awake()
    {
        // �ڵ� ���ε� (��Ÿ��)
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
        // �����Ϳ��� ���� �ٲ�ų� ������ ������ �� �ڵ� ����
        if (hpText == null)
            hpText = GetComponentInChildren<TMP_Text>(true);
        // �̸����� ���� ����
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
