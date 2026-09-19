using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "Survivor/Difficulty Config")]
public sealed class DifficultyConfig : ScriptableObject
{
    [SerializeField] private Difficulty difficulty;
    [SerializeField] private string difficultyName;
    [SerializeField] private int maxActiveEnemies = 15;
    [SerializeField] private float spawnInterval = 1.2f;

    public Difficulty Difficulty => difficulty;
    public string DifficultyName => string.IsNullOrWhiteSpace(difficultyName) ? difficulty.ToString() : difficultyName;
    public int MaxActiveEnemies => Mathf.Max(0, maxActiveEnemies);
    public float SpawnInterval => Mathf.Max(0.01f, spawnInterval);
}
