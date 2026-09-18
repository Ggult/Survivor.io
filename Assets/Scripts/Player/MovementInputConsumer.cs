using UnityEngine;

public sealed class MovementInputConsumer : MonoBehaviour
{
    [SerializeField] private VirtualJoystick inputSource;

    public Vector2 MoveInput { get; private set; }

    private void OnEnable()
    {
        if (inputSource != null)
        {
            inputSource.MovementInputChanged += HandleMovementInputChanged;
        }
    }

    private void OnDisable()
    {
        if (inputSource != null)
        {
            inputSource.MovementInputChanged -= HandleMovementInputChanged;
        }

        MoveInput = Vector2.zero;
    }

    private void HandleMovementInputChanged(Vector2 input)
    {
        MoveInput = input;
    }
}