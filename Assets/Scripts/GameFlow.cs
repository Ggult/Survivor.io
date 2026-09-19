using System;
using UnityEngine;

public enum GameState
{
    Playing,
    GameOver,
    Victory
}

public sealed class GameFlow : MonoBehaviour
{
    [SerializeField] private float gameDuration = 180f;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerRotation playerRotation;
    [SerializeField] private PlayerAutoAttack playerAutoAttack;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameFlowUI gameFlowUI;

    private GameState state;
    private float remainingTime;
    private int displayedSeconds = -1;
    private int currentRunKills;
    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;
    private bool hasPlayerStartTransform;
    public GameState State => state;
    public float RemainingTime => remainingTime;
    public int CurrentRunKills => currentRunKills;

    private void Awake()
    {
        CacheReferences();
        if (playerTransform != null)
        {
            playerStartPosition = playerTransform.position;
            playerStartRotation = playerTransform.rotation;
            hasPlayerStartTransform = true;
        }

        if (playerHealth != null)
        {
            playerHealth.Died += HandlePlayerDied;
        }

        EnemyHealth.Died += HandleEnemyDied;
    }

    private void Start()
    {
        if (gameFlowUI != null)
        {
            gameFlowUI.Bind(this);
        }

        StartRun();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= HandlePlayerDied;
        }

        EnemyHealth.Died -= HandleEnemyDied;
    }

    private void Update()
    {
        if (state != GameState.Playing)
        {
            return;
        }

        remainingTime = Mathf.Max(0f, remainingTime - Time.deltaTime);
        int seconds = Mathf.CeilToInt(remainingTime);
        if (seconds != displayedSeconds)
        {
            displayedSeconds = seconds;
            SetTimer(seconds);
        }

        if (remainingTime <= 0f)
        {
            EnterVictory();
        }
    }

    public void StartRun()
    {
        ResetRunObjects();
        remainingTime = Mathf.Max(0f, gameDuration);
        displayedSeconds = -1;
        currentRunKills = 0;
        SetState(GameState.Playing);
        SetTimer(Mathf.CeilToInt(remainingTime));
        HideResult();
    }

    public void RestartRun()
    {
        StartRun();
    }

    public void EnterGameOver()
    {
        if (state != GameState.Playing)
        {
            return;
        }

        StopGameplay();
        SetState(GameState.GameOver);
        ShowGameOver();
    }

    public void EnterVictory()
    {
        if (state != GameState.Playing)
        {
            return;
        }

        remainingTime = 0f;
        SetTimer(0);
        StopGameplay();
        SetState(GameState.Victory);
        ShowVictory();
    }

    private void HandlePlayerDied()
    {
        EnterGameOver();
    }

    private void HandleEnemyDied(EnemyHealth enemy)
    {
        if (state == GameState.Playing)
        {
            currentRunKills++;
        }
    }

    private void CacheReferences()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (playerTransform == null && playerHealth != null)
        {
            playerTransform = playerHealth.transform;
        }

        if (playerMovement == null && playerTransform != null)
        {
            playerMovement = playerTransform.GetComponent<PlayerMovement>();
        }

        if (playerRotation == null && playerTransform != null)
        {
            playerRotation = playerTransform.GetComponent<PlayerRotation>();
        }

        if (playerAutoAttack == null && playerTransform != null)
        {
            playerAutoAttack = playerTransform.GetComponent<PlayerAutoAttack>();
        }

        if (enemySpawner == null)
        {
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
        }

        if (enemyPool == null)
        {
            enemyPool = FindFirstObjectByType<EnemyPool>();
        }
    }

    private void ResetRunObjects()
    {
        if (enemySpawner != null)
        {
            enemySpawner.enabled = false;
        }

        if (enemyPool != null)
        {
            enemyPool.ReturnAll();
        }

        if (playerHealth != null)
        {
            playerHealth.RestoreFullHealth();
            playerHealth.enabled = true;
        }

        if (hasPlayerStartTransform && playerTransform != null)
        {
            playerTransform.SetPositionAndRotation(playerStartPosition, playerStartRotation);
        }

        if (playerAutoAttack != null)
        {
            playerAutoAttack.ResetAttack();
            playerAutoAttack.enabled = true;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (playerRotation != null)
        {
            playerRotation.enabled = true;
        }

        if (enemySpawner != null)
        {
            enemySpawner.ResetSpawner(playerTransform, playerHealth);
            enemySpawner.enabled = true;
        }
    }

    private void StopGameplay()
    {
        if (enemySpawner != null)
        {
            enemySpawner.enabled = false;
        }

        if (playerAutoAttack != null)
        {
            playerAutoAttack.ResetAttack();
            playerAutoAttack.enabled = false;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerRotation != null)
        {
            playerRotation.enabled = false;
        }

        EnemyMovement[] enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
        for (int index = 0; index < enemies.Length; index++)
        {
            EnemyMovement enemy = enemies[index];
            enemy.enabled = false;
            EnemyAttack attack = enemy.GetComponent<EnemyAttack>();
            if (attack != null)
            {
                attack.enabled = false;
            }
        }
    }

    private void SetState(GameState newState)
    {
        state = newState;
    }

    private void SetTimer(int seconds)
    {
        if (gameFlowUI != null)
        {
            gameFlowUI.SetTimer(seconds);
        }
    }

    private void ShowGameOver()
    {
        if (gameFlowUI != null)
        {
            gameFlowUI.ShowGameOver(currentRunKills);
        }
    }

    private void ShowVictory()
    {
        if (gameFlowUI != null)
        {
            gameFlowUI.ShowVictory(currentRunKills);
        }
    }

    private void HideResult()
    {
        if (gameFlowUI != null)
        {
            gameFlowUI.HideResult();
        }
    }
}
