using UnityEngine;

public sealed class PlayerAutoAttack : MonoBehaviour
{
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float attackInterval = 0.5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask enemyLayerMask = 1 << 6;
    [SerializeField] private PlayerRotationDecision rotationDecision;
    [SerializeField] private PlayerCombatAnimation combatAnimation;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Transform target;
    [SerializeField] private float currentCooldown;

    private readonly Collider[] detectionResults = new Collider[64];
    private EnemyHealth targetHealth;

    public float CurrentCooldown => currentCooldown;

    private void Awake()
    {
        if (rotationDecision == null)
        {
            rotationDecision = GetComponent<PlayerRotationDecision>();
        }

        if (combatAnimation == null)
        {
            combatAnimation = GetComponent<PlayerCombatAnimation>();
        }

        if (muzzleFlash == null)
        {
            muzzleFlash = GetComponentInChildren<ParticleSystem>(true);
        }

        ConfigureMuzzleFlash();
    }

    private void Update()
    {
        currentCooldown = Mathf.Max(0f, currentCooldown - Time.deltaTime);
        if (!TryKeepOrFindTarget())
        {
            rotationDecision?.ClearOverride();
            combatAnimation?.SetFiring(false);
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        rotationDecision?.SetOverrideDirection(direction);

        if (currentCooldown > 0f)
        {
            return;
        }

        combatAnimation?.SetFiring(true);
        PlayMuzzleFlash();
        targetHealth.TakeDamage(damage);
        Debug.Log($"[PlayerAutoAttack] Attacked Enemy for {damage:0.##} damage.", this);
        currentCooldown = Mathf.Max(0.01f, attackInterval);
    }

    public void ResetAttack()
    {
        currentCooldown = 0f;
        target = null;
        targetHealth = null;
        rotationDecision?.ClearOverride();
        combatAnimation?.SetFiring(false);
    }

    private void ConfigureMuzzleFlash()
    {
        if (muzzleFlash == null)
        {
            return;
        }

        ParticleSystem.MainModule main = muzzleFlash.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.08f;
        main.startLifetime = 0.06f;
        main.startSpeed = 0f;
        main.startSize = 0.12f;
        main.maxParticles = 1;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.EmissionModule emission = muzzleFlash.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });
        muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void PlayMuzzleFlash()
    {
        if (muzzleFlash == null)
        {
            return;
        }

        muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        muzzleFlash.Play(true);
    }

    private bool TryKeepOrFindTarget()
    {
        if (targetHealth != null && !targetHealth.IsDead && IsWithinRange(target))
        {
            return true;
        }

        target = null;
        targetHealth = null;
        int resultCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            attackRange,
            detectionResults,
            enemyLayerMask,
            QueryTriggerInteraction.Collide);

        float closestDistanceSquared = float.PositiveInfinity;
        for (int index = 0; index < resultCount; index++)
        {
            Collider result = detectionResults[index];
            if (result == null)
            {
                continue;
            }

            EnemyHealth candidate = result.GetComponent<EnemyHealth>();
            if (candidate == null || candidate.IsDead)
            {
                continue;
            }

            float distanceSquared = (candidate.transform.position - transform.position).sqrMagnitude;
            if (distanceSquared >= closestDistanceSquared)
            {
                continue;
            }

            closestDistanceSquared = distanceSquared;
            target = candidate.transform;
            targetHealth = candidate;
        }

        return targetHealth != null;
    }

    private bool IsWithinRange(Transform candidate)
    {
        if (candidate == null)
        {
            return false;
        }

        Vector3 direction = candidate.position - transform.position;
        direction.y = 0f;
        return direction.sqrMagnitude <= attackRange * attackRange;
    }
}