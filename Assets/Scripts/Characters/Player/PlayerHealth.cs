using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI Setup")]
    public Slider healthSlider;

    [Header("Death Settings")]
    public GameObject tombstonePrefab;

    [Tooltip("Drag your Movement and Weapon scripts here so they stop working when dead")]
    public MonoBehaviour[] scriptsToDisable; 

    // Internal flag to prevent dying multiple times
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return; // Don't take damage if already dead

        int oldHealth = currentHealth;
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        Debug.Log($"[PLAYER DAMAGE] {gameObject.name} took {amount} damage! Health: {oldHealth} -> {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[PLAYER DEATH] {gameObject.name} has died (Hidden for Revival)");

        // 1. Spawn Tombstone
        if (tombstonePrefab != null)
        {
            Instantiate(tombstonePrefab, transform.position, Quaternion.identity);
        }

        // 2. Hide the Visuals (Invisible)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        // 3. Disable Collision (Can't be hit)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 4. Disable Control Scripts (Stop Moving/Shooting)
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null) script.enabled = false;
        }

        // Optional: Disable the Rigidbody to stop sliding
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Stop moving immediately
            rb.simulated = false;       // Stop physics interactions
        }
    }

    // Call this function later to bring the player back!
    public void Revive(int healAmount)
    {
        isDead = false;
        currentHealth = healAmount;
        
        // Update UI
        if (healthSlider != null) healthSlider.value = currentHealth;

        // Re-enable Visuals
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;

        // Re-enable Collision
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        // Re-enable Physics
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = true;

        // Re-enable Scripts
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null) script.enabled = true;
        }

        Debug.Log("Player Revived!");
    }
}