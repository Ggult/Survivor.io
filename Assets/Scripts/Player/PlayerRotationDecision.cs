using UnityEngine;

public sealed class PlayerRotationDecision : MonoBehaviour
{
    [SerializeField] private MovementInputConsumer inputConsumer;

    private Vector3 overrideDirection;
    private bool hasOverride;

    public Vector3 GetDirection()
    {
        if (hasOverride)
        {
            return overrideDirection;
        }

        Vector2 input = inputConsumer == null ? Vector2.zero : inputConsumer.MoveInput;
        Vector3 movementDirection = new Vector3(input.x, 0f, input.y);
        return movementDirection.sqrMagnitude > 0.0001f ? movementDirection.normalized : Vector3.zero;
    }

    public void SetOverrideDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            ClearOverride();
            return;
        }

        overrideDirection = direction.normalized;
        hasOverride = true;
    }

    public void ClearOverride()
    {
        overrideDirection = Vector3.zero;
        hasOverride = false;
    }
}