using UnityEngine;

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

        Debug.Log($"{gameObject.name} took {amount} damage. Health: {currentHealth}/{maxHealth}");

        // Visual feedback
        if (flashOnHit)
        {
            StartCoroutine(FlashRed());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} died!");

        // Drop items
        if (dropItems.Length > 0 && Random.value <= dropChance)
        {
            GameObject drop = dropItems[Random.Range(0, dropItems.Length)];
            Instantiate(drop, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // Public method to check if alive
    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    // Public method to heal
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{gameObject.name} healed {amount}. Health: {currentHealth}/{maxHealth}");
    }
}