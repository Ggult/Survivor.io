using UnityEngine;

public sealed class PlayerRotation : MonoBehaviour
{
    [SerializeField] private PlayerRotationDecision decision;
    [SerializeField] private float rotationSpeed = 720f;

    private void LateUpdate()
    {
        if (decision == null)
        {
            return;
        }

        Vector3 direction = decision.GetDirection();
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }
}