using UnityEngine;

public class QuickFade : MonoBehaviour
{
    [SerializeField] float life = 0.15f;
    [SerializeField] float rise = 0.5f;
    SpriteRenderer sr;
    Color c;
    void Awake() { sr = GetComponent<SpriteRenderer>(); if (sr) c = sr.color; }
    void Update()
    {
        life -= Time.deltaTime;
        if (sr) { c.a = Mathf.Clamp01(life * 6f); sr.color = c; }
        transform.position += Vector3.up * rise * Time.deltaTime;
        if (life <= 0f) Destroy(gameObject);
    }
}
