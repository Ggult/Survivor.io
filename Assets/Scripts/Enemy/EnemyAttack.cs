using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public sealed class EnemyAttack : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private PlayerHealth targetHealth;
    [SerializeField] private Animator animator;
    [SerializeField] private float attackRange = 1.25f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float currentCooldown;

    private static readonly int AttackState = Animator.StringToHash("Base Layer.Enemy Attack");

    public float CurrentCooldown => currentCooldown;

    public void SetTarget(Transform newTarget, PlayerHealth newTargetHealth)
    {
        target = newTarget;
        targetHealth = newTargetHealth;
        currentCooldown = 0f;
    }

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (target == null || targetHealth == null || targetHealth.IsDead)
        {
            return;
        }

        if (currentCooldown > 0f)
        {
            currentCooldown = Mathf.Max(0f, currentCooldown - Time.deltaTime);
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        if (currentCooldown > 0f || direction.sqrMagnitude > attackRange * attackRange)
        {
            return;
        }

        if (animator != null)
        {
            animator.Play(AttackState, 0, 0f);
        }

    #if DEVELOPMENT_BUILD || UNITY_EDITOR
        Debug.Log($"[EnemyAttack] Enemy attacked Player for {damage:0.##} damage.", this);
    #endif
        targetHealth.TakeDamage(damage);
        currentCooldown = Mathf.Max(0.01f, attackInterval);
    }

    public void ResetAttack()
    {
        currentCooldown = 0f;
    }
}