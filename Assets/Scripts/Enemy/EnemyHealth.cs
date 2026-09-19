using System.Collections;
using System;
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
    [SerializeField] private ParticleSystem hitParticle;

    private static readonly int DeathState = Animator.StringToHash("Base Layer.Enemy Death");
    private static readonly int HitReactionState = Animator.StringToHash("Hit Reaction");
    private const string HitReactionLayerName = "Hit Reaction Layer";
    private EnemyPool pool;
    private Coroutine deathRoutine;
    private bool isDead;
    private int hitReactionLayer = -1;

    public static event Action<EnemyHealth> Died;

    public float CurrentHealth => currentHealth;
    public float MaximumHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        CacheComponents();
        hitReactionLayer = animator == null ? -1 : animator.GetLayerIndex(HitReactionLayerName);
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

        if (hitParticle != null)
        {
            hitParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (animator != null)
        {
            if (hitReactionLayer >= 0)
            {
                animator.SetLayerWeight(hitReactionLayer, 1f);
            }

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
            return;
        }

        PlayHitReaction(appliedDamage);
    }

    private void PlayHitReaction(float appliedDamage)
    {
        if (appliedDamage <= 0f || isDead)
        {
            return;
        }

        if (animator != null && hitReactionLayer >= 0)
        {
            animator.Play(HitReactionState, hitReactionLayer, 0f);
        }

        if (hitParticle != null)
        {
            hitParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            hitParticle.Play(true);
        }
    }

    private void BeginDeath()
    {
        isDead = true;
        Debug.Log("[EnemyHealth] Enemy died.", this);
        Died?.Invoke(this);

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

        if (hitParticle != null)
        {
            hitParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (animator != null)
        {
            if (hitReactionLayer >= 0)
            {
                animator.SetLayerWeight(hitReactionLayer, 0f);
            }

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

        if (hitParticle == null)
        {
            hitParticle = GetComponentInChildren<ParticleSystem>(true);
        }

        ConfigureHitParticle();
    }

    private void ConfigureHitParticle()
    {
        if (hitParticle == null)
        {
            return;
        }

        ParticleSystem.MainModule main = hitParticle.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.12f;
        main.startLifetime = 0.12f;
        main.startSpeed = 1.25f;
        main.startSize = 0.12f;
        main.maxParticles = 3;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = hitParticle.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 3) });
        hitParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}