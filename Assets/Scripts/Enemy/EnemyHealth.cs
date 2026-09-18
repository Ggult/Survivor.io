using System.Collections;
using UnityEngine;

public sealed class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float deathAnimationDuration = 2.967f;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyAttack attack;
    [SerializeField] private Collider separationCollider;

    private static readonly int DeathState = Animator.StringToHash("Base Layer.Enemy Death");
    private EnemyPool pool;
    private Coroutine deathRoutine;
    private bool isDead;

    public float CurrentHealth => currentHealth;
    public float MaximumHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        CacheComponents();
        ResetHealth();
    }

    public void SetPool(EnemyPool newPool)
    {
        pool = newPool;
    }

    public void ResetHealth()
    {
        if (deathRoutine != null)
        {
            StopCoroutine(deathRoutine);
            deathRoutine = null;
        }

        maxHealth = Mathf.Max(0f, maxHealth);
        currentHealth = maxHealth;
        isDead = false;
        enabled = true;
        if (movement != null)
        {
            movement.enabled = true;
        }

        if (attack != null)
        {
            attack.enabled = true;
        }

        if (separationCollider != null)
        {
            separationCollider.enabled = true;
        }

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || isDead)
        {
            return;
        }

        float previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
        float appliedDamage = previousHealth - currentHealth;
        Debug.Log($"[EnemyHealth] Damage: {appliedDamage:0.##} | Current Health: {currentHealth:0.##}/{maxHealth:0.##}", this);

        if (currentHealth <= 0f)
        {
            BeginDeath();
        }
    }

    private void BeginDeath()
    {
        isDead = true;
        Debug.Log("[EnemyHealth] Enemy died.", this);

        if (movement != null)
        {
            movement.enabled = false;
        }

        if (attack != null)
        {
            attack.enabled = false;
        }

        if (separationCollider != null)
        {
            separationCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.Play(DeathState, 0, 0f);
        }

        deathRoutine = StartCoroutine(ReturnAfterDeathAnimation());
    }

    private IEnumerator ReturnAfterDeathAnimation()
    {
        yield return new WaitForSeconds(Mathf.Max(0.01f, deathAnimationDuration));
        deathRoutine = null;
        if (pool != null)
        {
            pool.Return(movement);
        }
    }

    private void CacheComponents()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (movement == null)
        {
            movement = GetComponent<EnemyMovement>();
        }

        if (attack == null)
        {
            attack = GetComponent<EnemyAttack>();
        }

        if (separationCollider == null)
        {
            separationCollider = GetComponent<Collider>();
        }
    }
}