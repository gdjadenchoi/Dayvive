using UnityEngine;

namespace Dayvive.AI
{
    /// <summary>
    /// ������ 2D �þ� ����: �Ÿ� + �þ߰� + Raycast�� ��ֹ� ���� üũ
    /// </summary>
    public class SightSensor : MonoBehaviour
    {
        [Header("Sight")]
        [SerializeField] float viewDistance = 6f;
        [SerializeField, Range(0f, 180f)] float viewAngleDeg = 75f;
        [SerializeField] LayerMask obstacleMask;         // ��/��ֹ� ���̾�
        [SerializeField] string targetTag = "Player";    // �⺻ Ÿ�� �±�

        Transform _cachedTarget;

        public float ViewDistance => viewDistance;
        public float ViewAngleDeg => viewAngleDeg;
        public LayerMask ObstacleMask => obstacleMask;

        /// <summary> �±׷� Ÿ�� �ڵ� Ž��(������ null) </summary>
        public Transform FindTargetByTag()
        {
            if (!string.IsNullOrEmpty(targetTag))
            {
                var go = GameObject.FindGameObjectWithTag(targetTag);
                if (go) _cachedTarget = go.transform;
            }
            return _cachedTarget;
        }

        /// <summary> Ÿ���� �þ�/���ο������Ʈ �ȿ� �ִ��� </summary>
        public bool CanSee(Transform target)
        {
            if (!target) return false;

            Vector2 origin = transform.position;
            Vector2 to = (Vector2)target.position - origin;
            float dist = to.magnitude;
            if (dist > viewDistance) return false;

            // �þ߰�
            Vector2 forward = transform.right; // �⺻������ X+ �� ��������
            float angle = Vector2.Angle(forward, to);
            if (angle > viewAngleDeg * 0.5f) return false;

            // ���ο������Ʈ(��ֹ� ����)
            var hit = Physics2D.Raycast(origin, to.normalized, dist, obstacleMask);
            return hit.collider == null;
        }

        /// <summary> ���� ĳ��(�±� ���) Ÿ���� �þ߿� ������ true </summary>
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

            // �þ߰� ǥ��(���� +X ����)
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
