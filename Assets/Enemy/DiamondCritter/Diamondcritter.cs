using UnityEngine;

public class DiamondCritter : MonoBehaviour
{
    [Header("Movement")]
    public float detectionRange = 8f;
    public float dashSpeed = 12f;
    public float chargeUpTime = 0.8f; // Time before dashing

    [Header("Combat")]
    public int dashDamage = 15;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    private Transform target;
    private Rigidbody2D rb;
    private bool isCharging = false;
    private bool isDashing = false;
    private float chargeTimer = 0f;
    private Vector2 dashTargetPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

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
    }

    void Update()
    {
        if (target == null) return;

        if (isDashing)
        {
            // Continue dashing until reaching target or hitting something
            Vector2 currentPos = transform.position;
            if (Vector2.Distance(currentPos, dashTargetPosition) < 0.5f)
            {
                // Reached target position without hitting player
                Debug.Log($"[ATTACK] {gameObject.name} missed - despawning");
                Destroy(gameObject);
            }
            return;
        }

        if (isCharging)
        {
            chargeTimer -= Time.deltaTime;

            // Visual pulse while charging
            if (spriteRenderer != null)
            {
                float pulse = Mathf.PingPong(Time.time * 10f, 0.5f);
                spriteRenderer.color = Color.Lerp(Color.white, Color.red, pulse);
            }

            // Dash when charge complete
            if (chargeTimer <= 0f)
            {
                Dash();
            }
        }
        else
        {
            // Check if player in range
            float distanceToPlayer = Vector2.Distance(transform.position, target.position);

            if (distanceToPlayer <= detectionRange)
            {
                StartCharging();
            }
        }
    }

    void StartCharging()
    {
        isCharging = true;
        chargeTimer = chargeUpTime;

        // Stop any movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Debug.Log($"[ATTACK] {gameObject.name} detected player - charging dash!");
    }

    void Dash()
    {
        isDashing = true;
        isCharging = false;

        // Lock target position (where player is NOW)
        dashTargetPosition = target.position;

        // Calculate direction and dash
        Vector2 direction = (dashTargetPosition - (Vector2)transform.position).normalized;

        if (rb != null)
        {
            rb.linearVelocity = direction * dashSpeed;
        }

        // Restore color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        Debug.Log($"[ATTACK] {gameObject.name} dashing to {dashTargetPosition}!");

        // Auto-destroy after dash duration (safety)
        Destroy(gameObject, 3f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(dashDamage);
            }

            Debug.Log($"[ATTACK] {gameObject.name} hit player - despawning");
            Destroy(gameObject);
        }
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
}