using UnityEngine;
using Dayvive.Data;

namespace Dayvive.Weapons
{
    /// <summary>
    /// 무기 래퍼: 인스펙터에서 WeaponData를 연결해 사용
    /// 현재는 읽기 전용. 이후 CombatShooter 등에서 값을 읽도록 확장 예정.
    /// </summary>
    public class Weapon : MonoBehaviour
    {
        [Header("무기 데이터 참조")]
        [SerializeField] private WeaponData weaponData;

        public WeaponData Data => weaponData;

        // 🔎 임시로 확인할 수 있게 Debug 출력
        private void Start()
        {
            if (weaponData != null)
            {
                Debug.Log($"[Weapon] 장착: {weaponData.WeaponId}, Damage={weaponData.Damage}, Range={weaponData.Range}");
            }
            else
            {
                Debug.LogWarning("[Weapon] WeaponData 미할당 상태입니다.");
            }
        }
    }
}
