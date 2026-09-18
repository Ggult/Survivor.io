using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stoppingDistance = 1.25f;
    [SerializeField] private float separationRadius = 1.5f;
    [SerializeField] private float separationStrength = 0.6f;
    [SerializeField] private LayerMask enemyLayerMask = 1 << 6;
    [SerializeField] private Transform target;

    private readonly Collider[] separationResults = new Collider[32];
    private Collider ownCollider;

    public bool IsMoving { get; private set; }

    private void Awake()
    {
        ownCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        IsMoving = false;
        if (target == null)
        {
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;
        if (distance <= Mathf.Epsilon)
        {
            return;
        }

        Vector3 normalizedDirection = direction.normalized;
        transform.rotation = Quaternion.LookRotation(normalizedDirection, Vector3.up);

        if (distance <= stoppingDistance)
        {
            return;
        }

        Vector3 separationDirection = CalculateSeparationDirection();
        Vector3 finalDirection = (normalizedDirection + separationDirection * separationStrength).normalized;
        if (finalDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            finalDirection = normalizedDirection;
        }

        float movementDistance = Mathf.Min(moveSpeed * Time.deltaTime, distance - stoppingDistance);
        transform.position += finalDirection * movementDistance;
        IsMoving = movementDistance > 0f;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private Vector3 CalculateSeparationDirection()
    {
        if (ownCollider == null || separationRadius <= 0f || enemyLayerMask.value == 0)
        {
            return Vector3.zero;
        }

        int resultCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            separationRadius,
            separationResults,
            enemyLayerMask,
            QueryTriggerInteraction.Collide);

        Vector3 separation = Vector3.zero;
        float inverseRadius = 1f / separationRadius;
        for (int index = 0; index < resultCount; index++)
        {
            Collider otherCollider = separationResults[index];
            if (otherCollider == null || otherCollider == ownCollider)
            {
                continue;
            }

            Vector3 away = transform.position - otherCollider.transform.position;
            away.y = 0f;
            float distance = away.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                continue;
            }

            separation += away / distance * (1f - distance * inverseRadius);
        }

        return separation.sqrMagnitude > Mathf.Epsilon ? separation.normalized : Vector3.zero;
    }
}