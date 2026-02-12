using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"[PLAYER] {gameObject.name} initialized with {currentHealth}/{maxHealth} health");
    }

    public void TakeDamage(int amount)
    {
        int oldHealth = currentHealth;
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"[PLAYER DAMAGE] {gameObject.name} took {amount} damage! Health: {oldHealth} -> {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log($"[PLAYER DEATH] {gameObject.name} has died!");
            // Add death logic here
        }
    }
}