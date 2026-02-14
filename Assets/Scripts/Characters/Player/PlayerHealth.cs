using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Slider

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI Setup")]
    [Tooltip("Drag your Health Slider from the Canvas here")]
    public Slider healthSlider;

    void Start()
    {
        currentHealth = maxHealth;
        
        // Initialize the slider settings
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        Debug.Log($"[PLAYER] {gameObject.name} initialized with {currentHealth}/{maxHealth} health");
    }

    public void TakeDamage(int amount)
    {
        int oldHealth = currentHealth;
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update the slider immediately
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        Debug.Log($"[PLAYER DAMAGE] {gameObject.name} took {amount} damage! Health: {oldHealth} -> {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log($"[PLAYER DEATH] {gameObject.name} has died!");
            // Add death logic here (e.g., Destroy(gameObject) or show Game Over screen)
        }
    }
}