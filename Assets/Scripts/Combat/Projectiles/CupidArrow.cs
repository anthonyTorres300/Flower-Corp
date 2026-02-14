using UnityEngine;

public class CupidArrow : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;
    public float lifetime = 5f;

    [Header("Damage & Debuff")]
    public int damage = 10; // Instant damage on hit
    public float debuffDuration = 2f;

    private Vector2 direction;
    private DebuffType debuffType;

    public void Initialize(Vector2 dir)
    {
        direction = dir;
        Destroy(gameObject, lifetime);
    }

    public void SetDebuff(DebuffType type)
    {
        debuffType = type;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we hit the player
        if (other.CompareTag("Player"))
        {
            // Deal immediate damage
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"[ARROW HIT] Dealt {damage} damage to {other.name}");
            }

            // Apply debuff effect
            IDebuffable debuffable = other.GetComponent<IDebuffable>();
            if (debuffable != null)
            {
                debuffable.ApplyDebuff(debuffType, debuffDuration);
                Debug.Log($"[ARROW HIT] Applied {debuffType} debuff to {other.name}");
            }

            // Destroy the arrow
            Destroy(gameObject);
        }
    }
}