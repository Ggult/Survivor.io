using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyPool : MonoBehaviour
{
    [SerializeField] private EnemyMovement enemyPrefab;
    [SerializeField] private int initialSize = 20;

    private readonly Queue<EnemyMovement> availableEnemies = new Queue<EnemyMovement>();
    private readonly HashSet<EnemyMovement> pooledEnemies = new HashSet<EnemyMovement>();
    private readonly HashSet<EnemyMovement> availableEnemySet = new HashSet<EnemyMovement>();

    private void Awake()
    {
        Prewarm();
    }

    public EnemyMovement Get(Vector3 position, Quaternion rotation)
    {
        if (availableEnemies.Count == 0)
        {
            CreateEnemy();
        }

        if (availableEnemies.Count == 0)
        {
            return null;
        }

        EnemyMovement enemy = availableEnemies.Dequeue();
        availableEnemySet.Remove(enemy);
        enemy.transform.SetPositionAndRotation(position, rotation);
        enemy.gameObject.SetActive(true);

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health == null)
        {
            enemy.gameObject.SetActive(false);
            availableEnemySet.Add(enemy);
            availableEnemies.Enqueue(enemy);
            return null;
        }

        health.SetPool(this);
        health.ResetHealth();
        Debug.Log("[EnemyPool] Enemy retrieved from pool.", enemy);
        return enemy;
    }

    public void Return(EnemyMovement enemy)
    {
        if (enemy == null || !pooledEnemies.Contains(enemy) || !availableEnemySet.Add(enemy))
        {
            return;
        }

        enemy.gameObject.SetActive(false);
        availableEnemies.Enqueue(enemy);
        Debug.Log("[EnemyPool] Enemy returned to pool.", this);
    }

    private void Prewarm()
    {
        for (int index = 0; index < Mathf.Max(0, initialSize); index++)
        {
            CreateEnemy();
        }
    }

    private void CreateEnemy()
    {
        if (enemyPrefab == null)
        {
            return;
        }

        EnemyMovement enemy = Instantiate(enemyPrefab, transform);
        enemy.gameObject.SetActive(false);
        pooledEnemies.Add(enemy);
        availableEnemySet.Add(enemy);
        availableEnemies.Enqueue(enemy);
    }
}