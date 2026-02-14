using UnityEngine;
using System.Collections;

public class Damageable : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public bool flashOnHit = true;
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

    [Header("Death Effects")] // --- NEW SECTION ---
    public AudioClip deathSound;
    [Tooltip("How much the camera shakes on death")]
    public float shakeIntensity = 0.2f;
    [Tooltip("How long the camera shakes on death")]
    public float shakeDuration = 0.2f;

    [Header("Drops")]
    public GameObject[] dropItems;
    public float dropChance = 0.3f;

    private Color originalColor;

    void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        // Visual feedback for every hit
        if (flashOnHit)
        {
            StartCoroutine(FlashRed());
        }

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        // 1. Play Death Sound
        // We use PlayClipAtPoint so the sound finishes even after the object is destroyed
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        // 2. Trigger Camera Shake
        // This looks for the CameraShake script on your Main Camera
        if (Camera.main != null)
        {
            CameraShake shake = Camera.main.GetComponent<CameraShake>();
            if (shake != null)
            {
                shake.TriggerShake(shakeDuration, shakeIntensity);
            }
        }

        // 3. Drop items
        if (dropItems.Length > 0 && Random.value <= dropChance)
        {
            GameObject drop = dropItems[Random.Range(0, dropItems.Length)];
            Instantiate(drop, transform.position, Quaternion.identity);
        }

        // Final cleanup
        Destroy(gameObject);
    }

    public bool IsAlive() => currentHealth > 0;

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
}