using UnityEngine;

public class StoneChocolate : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float jumpForce = 5f;
    public float jumpInterval = 2f;

    [Header("Split Behavior")]
    public GameObject smallerChocolatePrefab;
    public int splitCount = 2; // How many pieces it splits into
    public float splitForce = 3f;
    public int currentSize = 3; // 3 = large, 2 = medium, 1 = small

    [Header("Combat")]
    public int damage = 10;
    public float chaseRange = 8f;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    private Transform target;
    private Rigidbody2D rb;
    private Damageable damageable;
    private float jumpTimer;
    private bool isGrounded;

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

        jumpTimer = jumpInterval;

        // Scale based on size
        transform.localScale = Vector3.one * currentSize * 0.4f;
    }

    void Update()
    {
        if (target == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        // Chase player if in range
        if (distanceToPlayer <= chaseRange)
        {
            ChasePlayer();
        }

        // Jump periodically
        jumpTimer -= Time.deltaTime;
        if (jumpTimer <= 0f && isGrounded)
        {
            Jump();
            jumpTimer = jumpInterval;
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (target.position - transform.position).normalized;

        // Only move horizontally
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        // Flip sprite based on direction
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    void Jump()
    {
        if (rb != null && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if grounded
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        // Damage player on contact
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"{gameObject.name} dealt {damage} damage to player");
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

                // Check if dead, then split
                if (damageable.currentHealth <= 0)
                {
                    Split();
                }
            }
            Destroy(other.gameObject);
        }
    }

    void Split()
    {
        // Don't split if already smallest size
        if (currentSize <= 1 || smallerChocolatePrefab == null)
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log($"{gameObject.name} splitting into {splitCount} pieces");

        // Spawn smaller pieces
        for (int i = 0; i < splitCount; i++)
        {
            GameObject piece = Instantiate(smallerChocolatePrefab, transform.position, Quaternion.identity);

            // Set smaller size
            StoneChocolate pieceScript = piece.GetComponent<StoneChocolate>();
            if (pieceScript != null)
            {
                pieceScript.currentSize = currentSize - 1;
                pieceScript.target = target;
            }

            // Add split force in random directions
            Rigidbody2D pieceRb = piece.GetComponent<Rigidbody2D>();
            if (pieceRb != null)
            {
                float angle = (360f / splitCount) * i + Random.Range(-20f, 20f);
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                pieceRb.AddForce(direction * splitForce, ForceMode2D.Impulse);
            }
        }

        // Destroy original
        Destroy(gameObject);
    }
}