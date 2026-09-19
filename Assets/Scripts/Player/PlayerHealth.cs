using System.Collections;
using System;
using UnityEngine;

public sealed class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private Animator animator;
    [SerializeField] private int hitReactionLayer = -1;
    [SerializeField] private float hitReactionDuration = 0.6f;

    private static readonly int HitReactionState = Animator.StringToHash("Hit Reaction");
    private Coroutine hitReactionRoutine;

    public event Action Died;

    public float CurrentHealth => currentHealth;
    public float MaximumHealth => maxHealth;
    public bool IsDead => CurrentHealth <= 0;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator != null)
        {
            hitReactionLayer = animator.GetLayerIndex("Hit Reaction");
        }

        maxHealth = Mathf.Max(0f, maxHealth);
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0 || IsDead)
        {
            return;
        }

        float previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
        float appliedDamage = previousHealth - currentHealth;
        Debug.Log($"[PlayerHealth] Damage: {appliedDamage:0.##} | Current Health: {currentHealth:0.##}/{maxHealth:0.##}", this);

        if (currentHealth <= 0f)
        {
            Debug.Log("[PlayerHealth] Player reached 0 health.", this);
            Died?.Invoke();
            return;
        }

        PlayHitReaction(appliedDamage);
    }

    public void RestoreFullHealth()
    {
        if (hitReactionRoutine != null)
        {
            StopCoroutine(hitReactionRoutine);
            hitReactionRoutine = null;
        }

        currentHealth = maxHealth;
        if (animator != null && hitReactionLayer >= 0 && hitReactionLayer < animator.layerCount)
        {
            animator.SetLayerWeight(hitReactionLayer, 0f);
        }
    }

    private void PlayHitReaction(float appliedDamage)
    {
        if (appliedDamage <= 0f || animator == null || hitReactionLayer < 0 || hitReactionLayer >= animator.layerCount)
        {
            return;
        }

        if (hitReactionRoutine != null)
        {
            StopCoroutine(hitReactionRoutine);
        }

        animator.SetLayerWeight(hitReactionLayer, 1f);
        animator.Play(HitReactionState, hitReactionLayer, 0f);
        hitReactionRoutine = StartCoroutine(ReleaseHitReactionLayer());
    }

    private IEnumerator ReleaseHitReactionLayer()
    {
        yield return new WaitForSeconds(Mathf.Max(0.01f, hitReactionDuration));
        animator.SetLayerWeight(hitReactionLayer, 0f);
        hitReactionRoutine = null;
    }
}