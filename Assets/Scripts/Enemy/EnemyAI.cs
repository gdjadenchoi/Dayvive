using UnityEngine;
using Dayvive.AI;

namespace Dayvive.AI
{
    /// <summary>
    /// ���� �ܼ��� �� AI: �þ߿� �÷��̾ ������ ���� �� ���� ���� �� ���� �õ�.
    /// Player�� IDamageable�� �������� �ʾҴٸ� ������ �ܼ� �α׸� ���´�.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyAI : MonoBehaviour
    {
        public enum State { Idle, Chase, Attack }

        [Header("Refs")]
        [SerializeField] SightSensor sight;
        [SerializeField] Transform target; // ����θ� SightSensor�� Player �±׷� �ڵ� ã��

        [Header("Move")]
        [SerializeField] float moveSpeed = 2.0f;
        [SerializeField] float stopDistance = 0.75f; // �� �Ÿ� ���ϸ� ����(���� ����)

        [Header("Attack")]
        [SerializeField] float attackRange = 1.0f;
        [SerializeField] float attackCooldown = 1.2f;
        [SerializeField] int damage = 1;

        [Header("Debug")]
        [SerializeField] bool debugLog = true;

        Rigidbody2D _rb;
        float _cd;
        State _state = State.Idle;

        void Reset()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (!sight) sight = GetComponent<SightSensor>();
        }

        void Awake()
        {
            if (!_rb) _rb = GetComponent<Rigidbody2D>();
            if (!sight) sight = GetComponent<SightSensor>();
            if (!target && sight) target = sight.FindTargetByTag();
        }

        void Update()
        {
            if (_cd > 0f) _cd -= Time.deltaTime;

            // Ÿ�� ������ ��Ž��
            if (!target && sight) target = sight.FindTargetByTag();

            bool see = sight && target ? sight.CanSee(target) : (target != null);

            // ���� ����
            switch (_state)
            {
                case State.Idle:
                    if (target && see) _state = State.Chase;
                    break;

                case State.Chase:
                    if (!target) { _state = State.Idle; break; }

                    float d = Vector2.Distance(transform.position, target.position);
                    if (d <= attackRange) _state = State.Attack;
                    else if (!see) _state = State.Idle; // ������ �þ� ������ ����
                    break;

                case State.Attack:
                    if (!target) { _state = State.Idle; break; }
                    if (Vector2.Distance(transform.position, target.position) > attackRange)
                        _state = State.Chase;
                    break;
            }
        }

        void FixedUpdate()
        {
            switch (_state)
            {
                case State.Idle:
                    // ���(�Ǵ� ���� ��Ʈ��)
                    _rb.linearVelocity = Vector2.zero;
                    break;

                case State.Chase:
                    StepChase();
                    break;

                case State.Attack:
                    _rb.linearVelocity = Vector2.zero;
                    TryAttack();
                    break;
            }
        }

        void StepChase()
        {
            if (!target) return;

            Vector2 pos = _rb.position;
            Vector2 to = (Vector2)target.position - pos;
            float dist = to.magnitude;

            if (dist <= stopDistance)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 dir = to.normalized;
            Vector2 vel = dir * moveSpeed;
            _rb.MovePosition(pos + vel * Time.fixedDeltaTime);

            // ����(���� +X)�� �̵� �������� ���� �þ߰� ��꿡 �ϰ���
            if (dir.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                _rb.MoveRotation(angle);
            }
        }

        void TryAttack()
        {
            if (_cd > 0f) return;
            if (!target) return;

            // ���� ����: ���� ���� ���� Ÿ���� ������ ������.
            if (Vector2.Distance(transform.position, target.position) <= attackRange + 0.001f)
            {
                // �÷��̾ IDamageable�� �����ߴٸ� ���� ����
                if (target.TryGetComponent<IDamageable>(out var dmgable))
                {
                    dmgable.ApplyDamage(Mathf.Max(1, damage), DamageType.Generic);
                    if (debugLog) Debug.Log("[EnemyAI] Hit player for " + damage);
                }
                else
                {
                    if (debugLog) Debug.Log("[EnemyAI] Attack attempted (player has no IDamageable)");
                }

                _cd = attackCooldown;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.25f, 0.25f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
            Gizmos.color = new Color(1f, 0.25f, 0.25f, 0.75f);
            Gizmos.DrawWireSphere(transform.position, stopDistance);
        }
    }
}
