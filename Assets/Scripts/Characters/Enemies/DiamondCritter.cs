using UnityEngine;

public class DiamondCritter : MonoBehaviour
{
    [Header("Movement")]
    public float detectionRange = 8f;
    public float dashSpeed = 12f;
    public float chargeUpTime = 0.8f;

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
            rb.freezeRotation = true;
            // enemies dont push players
            rb.mass = 0.1f;
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
            Vector2 currentPos = transform.position;
            if (Vector2.Distance(currentPos, dashTargetPosition) < 0.5f)
            {
                Destroy(gameObject);
            }
            return;
        }

        if (isCharging)
        {
            chargeTimer -= Time.deltaTime;

            if (spriteRenderer != null)
            {
                float pulse = Mathf.PingPong(Time.time * 10f, 0.5f);
                spriteRenderer.color = Color.Lerp(Color.white, Color.red, pulse);
            }

            if (chargeTimer <= 0f)
            {
                Dash();
            }
        }
        else
        {
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

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Dash()
    {
        isDashing = true;
        isCharging = false;

        dashTargetPosition = target.position;

        Vector2 direction = (dashTargetPosition - (Vector2)transform.position).normalized;

        if (rb != null)
        {
            rb.linearVelocity = direction * dashSpeed;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

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