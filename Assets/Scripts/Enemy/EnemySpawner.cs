using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyMovement enemyPrefab;
    [SerializeField] private ArenaBounds arenaBounds;
    [SerializeField] private Transform target;
    [SerializeField] private PlayerHealth targetHealth;
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxActiveEnemies = 20;
    [SerializeField] private float minimumSpawnDistance = 4f;
    [SerializeField] private float spawnMargin = 1f;

    private readonly List<EnemyMovement> activeEnemies = new List<EnemyMovement>();
    private readonly Dictionary<EnemyMovement, EnemyAttack> enemyAttacks = new Dictionary<EnemyMovement, EnemyAttack>();
    private readonly Dictionary<EnemyMovement, EnemyHealth> enemyHealth = new Dictionary<EnemyMovement, EnemyHealth>();
    private WaitForSeconds spawnWait;
    private Coroutine spawnRoutine;
    private int spawnPointIndex;

    private void Awake()
    {
        spawnWait = new WaitForSeconds(Mathf.Max(0.01f, spawnInterval));
        if (enemyPool == null)
        {
            enemyPool = GetComponent<EnemyPool>();
        }
        if (targetHealth == null && target != null)
        {
            targetHealth = target.GetComponent<PlayerHealth>();
        }
    }

    private void OnEnable()
    {
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public int MaxActiveEnemies => maxActiveEnemies;
    public float SpawnInterval => spawnInterval;

    public void ApplyDifficulty(DifficultyConfig config)
    {
        if (config == null)
        {
            return;
        }

        spawnInterval = config.SpawnInterval;
        maxActiveEnemies = config.MaxActiveEnemies;
        spawnWait = new WaitForSeconds(spawnInterval);
    }

    public void ResetSpawner(Transform newTarget, PlayerHealth newTargetHealth)
    {
        target = newTarget;
        targetHealth = newTargetHealth;
        activeEnemies.Clear();
        spawnPointIndex = 0;
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return spawnWait;
            TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        CleanupInactiveEnemies();

        if (target == null || targetHealth == null || enemyPool == null || arenaBounds == null)
        {
            return;
        }

        if (activeEnemies.Count >= Mathf.Max(0, maxActiveEnemies))
        {
            return;
        }

        Bounds bounds = arenaBounds.WorldBounds;
        if (!TryGetSpawnPosition(bounds, out Vector3 spawnPosition))
        {
            return;
        }

        EnemyMovement enemy = enemyPool.Get(spawnPosition, Quaternion.identity);
        if (enemy == null)
        {
            return;
        }

        CacheEnemyComponents(enemy);
        enemy.SetTarget(target);
        enemyAttacks[enemy].SetTarget(target, targetHealth);
        enemyAttacks[enemy].ResetAttack();
        activeEnemies.Add(enemy);
    }

    private void CleanupInactiveEnemies()
    {
        for (int index = activeEnemies.Count - 1; index >= 0; index--)
        {
            EnemyMovement enemy = activeEnemies[index];
            if (enemy != null)
            {
                CacheEnemyComponents(enemy);
            }

            EnemyHealth health = enemy == null ? null : enemyHealth[enemy];
            if (enemy == null || health == null || health.IsDead || !enemy.gameObject.activeInHierarchy)
            {
                activeEnemies.RemoveAt(index);
            }
        }
    }

    private void CacheEnemyComponents(EnemyMovement enemy)
    {
        if (enemy == null)
        {
            return;
        }

        if (!enemyAttacks.ContainsKey(enemy))
        {
            enemyAttacks.Add(enemy, enemy.GetComponent<EnemyAttack>());
        }

        if (!enemyHealth.ContainsKey(enemy))
        {
            enemyHealth.Add(enemy, enemy.GetComponent<EnemyHealth>());
        }
    }

    private bool TryGetSpawnPosition(Bounds bounds, out Vector3 spawnPosition)
    {
        float minimumDistanceSquared = minimumSpawnDistance * minimumSpawnDistance;
        float minX = bounds.min.x + spawnMargin;
        float maxX = bounds.max.x - spawnMargin;
        float minZ = bounds.min.z + spawnMargin;
        float maxZ = bounds.max.z - spawnMargin;
        int candidateCount = 25;

        for (int offset = 0; offset < candidateCount; offset++)
        {
            int candidateIndex = (spawnPointIndex + offset) % candidateCount;
            int xIndex = candidateIndex % 5;
            int zIndex = candidateIndex / 5;
            float x = Mathf.Lerp(minX, maxX, xIndex / 4f);
            float z = Mathf.Lerp(minZ, maxZ, zIndex / 4f);
            Vector3 candidate = new Vector3(x, target.position.y, z);

            if ((candidate - target.position).sqrMagnitude >= minimumDistanceSquared)
            {
                spawnPointIndex = (candidateIndex + 1) % candidateCount;
                spawnPosition = candidate;
                return true;
            }
        }

        spawnPosition = default;
        return false;
    }
}