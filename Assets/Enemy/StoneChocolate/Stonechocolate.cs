using UnityEngine;
using System.Collections;

public class StoneChocolate : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float chaseRange = 12f;

    [Header("Split Attack")]
    public float splitRange = 3f; // Distance to trigger split attack
    public float splitDuration = 0.8f; // How long the split lasts
    public float splitCooldown = 3f; // Cooldown between splits
    public float expandScale = 2.5f; // How much bigger it gets
    public int splitDamage = 10;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color normalColor = Color.white;
    public Color splitColor = Color.red;

    private Transform target;
    private Rigidbody2D rb;
    private bool isSplitting = false;
    private float splitCooldownTimer = 0f;
    private Vector3 originalScale;
    private Collider2D bodyCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            foreach (GameObject player in players)
            {
                SwitchCharacters sc = player.GetComponent<SwitchCharacters>();
                if (sc != null && sc.isActive)
                {
                    target = player.transform;
                    break;
                }
            }
            if (target == null)
                target = players[0].transform;
        }

        originalScale = transform.localScale;
    }

    void Update()
    {
        if (target == null || isSplitting) return;

        splitCooldownTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        // Check if should trigger split attack
        if (distanceToPlayer <= splitRange && splitCooldownTimer <= 0f)
        {
            StartCoroutine(SplitAttack());
        }
        // Otherwise chase player
        else if (distanceToPlayer <= chaseRange)
        {
            ChasePlayer();
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (target.position - transform.position).normalized;

        if (rb != null)
        {
            rb.linearVelocity = direction * moveSpeed;
        }

        // Flip sprite based on direction
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    IEnumerator SplitAttack()
    {
        isSplitting = true;
        splitCooldownTimer = splitCooldown;

        // Stop moving
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Debug.Log($"[ATTACK] {gameObject.name} starting split attack!");

        // Change color to indicate attack
        if (spriteRenderer != null)
        {
            spriteRenderer.color = splitColor;
        }

        // Expand body over time
        float elapsed = 0f;
        float expandTime = splitDuration * 0.4f;

        while (elapsed < expandTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / expandTime;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * expandScale, t);
            yield return null;
        }

        // Hold expanded state
        yield return new WaitForSeconds(splitDuration * 0.2f);

        // Shrink back
        elapsed = 0f;
        while (elapsed < expandTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / expandTime;
            transform.localScale = Vector3.Lerp(originalScale * expandScale, originalScale, t);
            yield return null;
        }

        // Restore
        transform.localScale = originalScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        isSplitting = false;

        Debug.Log($"[ATTACK] {gameObject.name} split attack complete!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            Damageable damageable = GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(1);
            }
            Destroy(other.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                // Deal more damage if currently splitting
                int damage = isSplitting ? splitDamage : splitDamage / 2;
                health.TakeDamage(damage);
            }
        }
    }
}