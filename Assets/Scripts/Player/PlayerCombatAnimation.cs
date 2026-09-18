using UnityEngine;

public sealed class PlayerCombatAnimation : MonoBehaviour
{
    private static readonly int FiringParameter = Animator.StringToHash("Firing");

    [SerializeField] private Animator animator;
    [SerializeField] private string attackStateName = "Attack";
    [SerializeField, Range(0.8f, 1f)] private float attackReleaseNormalizedTime = 0.9f;

    private int attackStateHash;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        attackStateHash = Animator.StringToHash(attackStateName);
    }

    private void Update()
    {
        if (animator == null || !animator.GetBool(FiringParameter) || animator.layerCount < 2)
        {
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);
        if (stateInfo.shortNameHash == attackStateHash && stateInfo.normalizedTime >= attackReleaseNormalizedTime)
        {
            SetFiring(false);
        }
    }

    public void SetFiring(bool value)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool(FiringParameter, value);
    }
}
