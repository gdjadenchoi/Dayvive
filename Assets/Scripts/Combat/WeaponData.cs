using UnityEngine;

namespace Dayvive.Data
{
    /// <summary>
    /// 무기 데이터 정의 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Dayvive/Weapon Data", order = 0)]
    public class WeaponData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string weaponId = "weapon_001";
        [SerializeField] private WeaponCategory category = WeaponCategory.Ranged;
        [SerializeField] private ProjectileType projectileType = ProjectileType.Bullet;

        [Header("전투 수치")]
        [SerializeField] private float range = 10f;                // 사거리
        [SerializeField] private float projectileSpeed = 20f;      // 탄속
        [SerializeField] private int damage = 1;                   // 기본 데미지
        [SerializeField] private float fireCooldown = 0.2f;        // 발사 간격
        [SerializeField] private int magazineSize = 6;             // 탄창 크기
        [SerializeField] private int reserveAmmo = 24;             // 예비 탄

        [Header("연출/프리팹")]
        [SerializeField] private GameObject projectilePrefab;      // 발사체 프리팹
        [SerializeField] private GameObject hitEffectPrefab;       // 히트 이펙트
        [SerializeField] private bool isAutomatic = false;         // 연사 여부

        // 🔓 프로퍼티로 노출
        public string WeaponId => weaponId;
        public WeaponCategory Category => category;
        public ProjectileType ProjectileType => projectileType;
        public float Range => range;
        public float ProjectileSpeed => projectileSpeed;
        public int Damage => damage;
        public float FireCooldown => fireCooldown;
        public int MagazineSize => magazineSize;
        public int ReserveAmmo => reserveAmmo;
        public GameObject ProjectilePrefab => projectilePrefab;
        public GameObject HitEffectPrefab => hitEffectPrefab;
        public bool IsAutomatic => isAutomatic;
    }

    public enum WeaponCategory
    {
        Melee,
        Ranged,
        Thrown
    }

    public enum ProjectileType
    {
        None,
        Bullet,
        Explosive,
        FireCloud,
        Slug
    }
}
