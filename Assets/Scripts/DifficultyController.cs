using UnityEngine;

public sealed class DifficultyController : MonoBehaviour
{
    [SerializeField] private DifficultyConfig easyConfig;
    [SerializeField] private DifficultyConfig mediumConfig;
    [SerializeField] private DifficultyConfig hardConfig;
    [SerializeField] private Difficulty selectedDifficulty = Difficulty.Medium;

    private DifficultyConfig selectedConfig;

    public Difficulty SelectedDifficulty => selectedDifficulty;
    public DifficultyConfig SelectedConfig => selectedConfig;

    public void Initialize(EnemySpawner enemySpawner)
    {
        SelectDifficulty(selectedDifficulty, enemySpawner);
    }

    public bool SelectDifficulty(Difficulty difficulty, EnemySpawner enemySpawner)
    {
        DifficultyConfig config = GetConfig(difficulty);
        if (config == null)
        {
            return false;
        }

        selectedDifficulty = difficulty;
        selectedConfig = config;
        if (enemySpawner != null)
        {
            enemySpawner.ApplyDifficulty(config);
        }

        return true;
    }

    private DifficultyConfig GetConfig(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return easyConfig;
            case Difficulty.Hard:
                return hardConfig;
            default:
                return mediumConfig;
        }
    }
}
