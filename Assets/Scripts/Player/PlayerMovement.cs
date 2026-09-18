using UnityEngine;

public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private MovementInputConsumer inputConsumer;
    [SerializeField] private ArenaBounds arenaBounds;

    public Vector3 MovementDirection { get; private set; }

    private void Update()
    {
        Vector2 input = inputConsumer == null ? Vector2.zero : inputConsumer.MoveInput;
        MovementDirection = new Vector3(input.x, 0f, input.y);
        Vector3 movement = MovementDirection * (moveSpeed * Time.deltaTime);

        transform.position += movement;
        if (arenaBounds == null)
        {
            return;
        }

        Bounds bounds = arenaBounds.WorldBounds;
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, bounds.min.x, bounds.max.x),
            transform.position.y,
            Mathf.Clamp(transform.position.z, bounds.min.z, bounds.max.z));
    }
}