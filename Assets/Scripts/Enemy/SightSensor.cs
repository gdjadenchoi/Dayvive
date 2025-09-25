using UnityEngine;

namespace Dayvive.AI
{
    /// <summary>
    /// 간단한 2D 시야 센서: 거리 + 시야각 + Raycast로 장애물 차단 체크
    /// </summary>
    public class SightSensor : MonoBehaviour
    {
        [Header("Sight")]
        [SerializeField] float viewDistance = 6f;
        [SerializeField, Range(0f, 180f)] float viewAngleDeg = 75f;
        [SerializeField] LayerMask obstacleMask;         // 벽/장애물 레이어
        [SerializeField] string targetTag = "Player";    // 기본 타깃 태그

        Transform _cachedTarget;

        public float ViewDistance => viewDistance;
        public float ViewAngleDeg => viewAngleDeg;
        public LayerMask ObstacleMask => obstacleMask;

        /// <summary> 태그로 타깃 자동 탐색(없으면 null) </summary>
        public Transform FindTargetByTag()
        {
            if (!string.IsNullOrEmpty(targetTag))
            {
                var go = GameObject.FindGameObjectWithTag(targetTag);
                if (go) _cachedTarget = go.transform;
            }
            return _cachedTarget;
        }

        /// <summary> 타깃이 시야/라인오브사이트 안에 있는지 </summary>
        public bool CanSee(Transform target)
        {
            if (!target) return false;

            Vector2 origin = transform.position;
            Vector2 to = (Vector2)target.position - origin;
            float dist = to.magnitude;
            if (dist > viewDistance) return false;

            // 시야각
            Vector2 forward = transform.right; // 기본적으로 X+ 를 전방으로
            float angle = Vector2.Angle(forward, to);
            if (angle > viewAngleDeg * 0.5f) return false;

            // 라인오브사이트(장애물 차단)
            var hit = Physics2D.Raycast(origin, to.normalized, dist, obstacleMask);
            return hit.collider == null;
        }

        /// <summary> 내부 캐시(태그 기반) 타깃을 시야에 잡으면 true </summary>
        public bool TryAcquireTarget(out Transform target)
        {
            if (!_cachedTarget) FindTargetByTag();
            target = _cachedTarget;
            return target && CanSee(target);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0.35f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, viewDistance);

            // 시야각 표시(로컬 +X 방향)
            Vector3 o = transform.position;
            float half = viewAngleDeg * 0.5f;
            Vector3 dirL = Quaternion.Euler(0, 0, +half) * transform.right;
            Vector3 dirR = Quaternion.Euler(0, 0, -half) * transform.right;
            Gizmos.color = new Color(0f, 1f, 0.35f, 0.6f);
            Gizmos.DrawLine(o, o + dirL * viewDistance);
            Gizmos.DrawLine(o, o + dirR * viewDistance);
        }
    }
}
