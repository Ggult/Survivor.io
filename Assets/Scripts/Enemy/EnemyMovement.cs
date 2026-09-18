using UnityEngine;

public sealed class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stoppingDistance = 1.25f;
    [SerializeField] private Transform target;

    public bool IsMoving { get; private set; }

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

        float movementDistance = Mathf.Min(moveSpeed * Time.deltaTime, distance - stoppingDistance);
        transform.position += normalizedDirection * movementDistance;
        IsMoving = movementDistance > 0f;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}