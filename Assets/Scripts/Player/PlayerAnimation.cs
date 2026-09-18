using UnityEngine;

public sealed class PlayerAnimation : MonoBehaviour
{
    private const string SpeedParameter = "Speed";

    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement movement;

    private void Update()
    {
        if (animator == null || movement == null)
        {
            return;
        }

        animator.SetFloat(SpeedParameter, movement.MovementDirection.magnitude);
    }
}