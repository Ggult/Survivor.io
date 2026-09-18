using UnityEngine;

public sealed class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maximumHealth = 100;

    public int CurrentHealth { get; private set; }
    public int MaximumHealth => maximumHealth;
    public bool IsDead => CurrentHealth <= 0;

    private void Awake()
    {
        CurrentHealth = maximumHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
    }

    public void RestoreFullHealth()
    {
        CurrentHealth = maximumHealth;
    }
}