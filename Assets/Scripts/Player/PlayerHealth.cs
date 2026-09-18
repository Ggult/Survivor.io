using UnityEngine;

public sealed class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaximumHealth => maxHealth;
    public bool IsDead => CurrentHealth <= 0;

    private void Awake()
    {
        maxHealth = Mathf.Max(0f, maxHealth);
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0 || IsDead)
        {
            return;
        }

        float previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
        float appliedDamage = previousHealth - currentHealth;
        Debug.Log($"[PlayerHealth] Damage: {appliedDamage:0.##} | Current Health: {currentHealth:0.##}/{maxHealth:0.##}", this);

        if (currentHealth <= 0f)
        {
            Debug.Log("[PlayerHealth] Player reached 0 health.", this);
        }
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
    }
}