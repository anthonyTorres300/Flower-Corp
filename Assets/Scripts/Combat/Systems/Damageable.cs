using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Events")]
    public UnityEvent onDeath;

    void Start()
    {
        currentHealth = maxHealth;

        if (onDeath == null)
        {
            onDeath = new UnityEvent();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        onDeath?.Invoke();

        // Notify wave manager
        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.OnEnemyKilled();
        }

        // Check if this is a CorruptedCupid and call OnEliminated if it exists
        CorruptedCupid corruptedCupid = GetComponent<CorruptedCupid>();
        if (corruptedCupid != null)
        {
            corruptedCupid.OnEliminated();
            return; // Don't destroy the game object here, let OnEliminated handle it
        }

        Destroy(gameObject);
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
}