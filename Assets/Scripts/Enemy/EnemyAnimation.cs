using UnityEngine;

[RequireComponent(typeof(Animator), typeof(EnemyMovement))]
public sealed class EnemyAnimation : MonoBehaviour
{
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");

    [SerializeField] private Animator animator;
    [SerializeField] private EnemyMovement movement;

    private bool hasMovementState;
    private bool wasMoving;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (movement == null)
        {
            movement = GetComponent<EnemyMovement>();
        }
    }

    private void Update()
    {
        if (animator == null || movement == null)
        {
            return;
        }

        bool isMoving = movement.IsMoving;
        if (hasMovementState && isMoving == wasMoving)
        {
            return;
        }

        animator.SetFloat(SpeedParameter, isMoving ? 1f : 0f);
        wasMoving = isMoving;
        hasMovementState = true;
    }
}