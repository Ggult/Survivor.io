using UnityEngine;

public sealed class PlayerAutoAttack : MonoBehaviour
{
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float attackInterval = 0.5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask enemyLayerMask = 1 << 6;
    [SerializeField] private PlayerRotationDecision rotationDecision;
    [SerializeField] private Transform target;
    [SerializeField] private float currentCooldown;

    private readonly Collider[] detectionResults = new Collider[64];
    private EnemyHealth targetHealth;

    public float CurrentCooldown => currentCooldown;

    private void Awake()
    {
        if (rotationDecision == null)
        {
            rotationDecision = GetComponent<PlayerRotationDecision>();
        }
    }

    private void Update()
    {
        currentCooldown = Mathf.Max(0f, currentCooldown - Time.deltaTime);
        if (!TryKeepOrFindTarget())
        {
            rotationDecision?.ClearOverride();
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        rotationDecision?.SetOverrideDirection(direction);

        if (currentCooldown > 0f)
        {
            return;
        }

        targetHealth.TakeDamage(damage);
        Debug.Log($"[PlayerAutoAttack] Attacked Enemy for {damage:0.##} damage.", this);
        currentCooldown = Mathf.Max(0.01f, attackInterval);
    }

    public void ResetAttack()
    {
        currentCooldown = 0f;
        target = null;
        targetHealth = null;
        rotationDecision?.ClearOverride();
    }

    private bool TryKeepOrFindTarget()
    {
        if (targetHealth != null && !targetHealth.IsDead && IsWithinRange(target))
        {
            return true;
        }

        target = null;
        targetHealth = null;
        int resultCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            attackRange,
            detectionResults,
            enemyLayerMask,
            QueryTriggerInteraction.Collide);

        float closestDistanceSquared = float.PositiveInfinity;
        for (int index = 0; index < resultCount; index++)
        {
            Collider result = detectionResults[index];
            if (result == null)
            {
                continue;
            }

            EnemyHealth candidate = result.GetComponent<EnemyHealth>();
            if (candidate == null || candidate.IsDead)
            {
                continue;
            }

            float distanceSquared = (candidate.transform.position - transform.position).sqrMagnitude;
            if (distanceSquared >= closestDistanceSquared)
            {
                continue;
            }

            closestDistanceSquared = distanceSquared;
            target = candidate.transform;
            targetHealth = candidate;
        }

        return targetHealth != null;
    }

    private bool IsWithinRange(Transform candidate)
    {
        if (candidate == null)
        {
            return false;
        }

        Vector3 direction = candidate.position - transform.position;
        direction.y = 0f;
        return direction.sqrMagnitude <= attackRange * attackRange;
    }
}