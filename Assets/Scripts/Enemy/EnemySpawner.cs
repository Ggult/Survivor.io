using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyMovement enemyPrefab;
    [SerializeField] private ArenaBounds arenaBounds;
    [SerializeField] private Transform target;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxActiveEnemies = 20;
    [SerializeField] private float minimumSpawnDistance = 4f;
    [SerializeField] private float spawnMargin = 1f;

    private readonly List<EnemyMovement> activeEnemies = new List<EnemyMovement>();
    private WaitForSeconds spawnWait;
    private Coroutine spawnRoutine;
    private int spawnPointIndex;

    private void Awake()
    {
        spawnWait = new WaitForSeconds(Mathf.Max(0.01f, spawnInterval));
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

        if (target == null || enemyPrefab == null || arenaBounds == null)
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

        EnemyMovement enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.SetTarget(target);
        activeEnemies.Add(enemy);
    }

    private void CleanupInactiveEnemies()
    {
        for (int index = activeEnemies.Count - 1; index >= 0; index--)
        {
            EnemyMovement enemy = activeEnemies[index];
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                activeEnemies.RemoveAt(index);
            }
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