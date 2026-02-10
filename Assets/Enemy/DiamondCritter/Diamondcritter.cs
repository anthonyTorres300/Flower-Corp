using UnityEngine;

public class DiamondCritter : MonoBehaviour
{
    [Header("Movement")]
    public float detectionRange = 6f;
    public float leapForce = 8f;
    public float leapCooldown = 1.5f;
    public float moveSpeed = 1.5f;

    [Header("Combat")]
    public int damage = 15; // High damage for sacrifice attack
    public bool explodeOnHit = true;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public GameObject explosionEffect;

    private Transform target;
    private Rigidbody2D rb;
    private Damageable damageable;
    private float leapTimer;
    private bool isGrounded;
    private bool hasLeaped;

    private enum State { Idle, Charging, Leaping, Exploding }
    private State currentState = State.Idle;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        damageable = GetComponent<Damageable>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Find active player
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

        leapTimer = leapCooldown;
    }

    void Update()
    {
        if (target == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        switch (currentState)
        {
            case State.Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = State.Charging;
                }
                break;

            case State.Charging:
                leapTimer -= Time.deltaTime;

                // Move slowly toward player while charging
                Vector2 direction = (target.position - transform.position).normalized;
                rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

                // Visual charging effect (could pulse the sprite)
                if (spriteRenderer != null)
                {
                    float pulse = Mathf.PingPong(Time.time * 5f, 0.3f);
                    spriteRenderer.color = Color.white * (1f + pulse);
                }

                // Leap when timer ready and grounded
                if (leapTimer <= 0f && isGrounded)
                {
                    LeapAtPlayer();
                }
                break;

            case State.Leaping:
                // In air, can't control much
                // Will transition back on landing
                break;

            case State.Exploding:
                // Handled in collision
                break;
        }
    }

    void LeapAtPlayer()
    {
        if (target == null) return;

        currentState = State.Leaping;
        hasLeaped = true;

        // Calculate direction and add upward bias
        Vector2 direction = (target.position - transform.position).normalized;
        Vector2 leapDirection = (direction + Vector2.up * 0.5f).normalized;

        // Apply force
        rb.linearVelocity = Vector2.zero; // Reset velocity first
        rb.AddForce(leapDirection * leapForce, ForceMode2D.Impulse);

        Debug.Log($"{gameObject.name} leaping at player!");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if grounded
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;

            // Reset state after landing from leap
            if (currentState == State.Leaping)
            {
                currentState = State.Charging;
                leapTimer = leapCooldown;
                hasLeaped = false;
            }
        }

        // Hit player - deal damage and explode
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"{gameObject.name} sacrificed and dealt {damage} damage!");
            }

            if (explodeOnHit)
            {
                Explode();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Take damage from player attacks
        if (other.CompareTag("PlayerBullet"))
        {
            if (damageable != null)
            {
                damageable.TakeDamage(1);

                // Explode if killed
                if (damageable.currentHealth <= 0)
                {
                    Explode();
                }
            }
            Destroy(other.gameObject);
        }
    }

    void Explode()
    {
        currentState = State.Exploding;

        Debug.Log($"{gameObject.name} exploding!");

        // Spawn explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Could add area damage here if desired

        // Destroy this critter
        Destroy(gameObject);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}