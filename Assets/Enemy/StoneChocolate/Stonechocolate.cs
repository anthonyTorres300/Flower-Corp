using UnityEngine;
using System.Collections;

public class StoneChocolate : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float chaseRange = 12f;

    [Header("Split Attack")]
    public float splitRange = 3f;
    public float splitDistance = 2f; // How far the halves separate
    public float splitDuration = 1.2f; // Total time for split + merge
    public float splitCooldown = 3f;
    public int splitDamage = 10;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Sprite topHalfSprite; // Assign top half of chocolate sprite
    public Sprite bottomHalfSprite; // Assign bottom half of chocolate sprite

    private Transform target;
    private Rigidbody2D rb;
    private bool isSplitting = false;
    private float splitCooldownTimer = 0f;
    private GameObject topHalf;
    private GameObject bottomHalf;
    private Sprite originalSprite;

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

        if (spriteRenderer != null)
            originalSprite = spriteRenderer.sprite;

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
        if (target == null || isSplitting) return;

        splitCooldownTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer <= splitRange && splitCooldownTimer <= 0f)
        {
            StartCoroutine(SplitAttack());
        }
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

        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    IEnumerator SplitAttack()
    {
        isSplitting = true;
        splitCooldownTimer = splitCooldown;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Debug.Log($"[ATTACK] {gameObject.name} splitting into halves!");

        // Hide main sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Create top and bottom halves
        topHalf = CreateHalf(true);
        bottomHalf = CreateHalf(false);

        // Split apart - move in opposite directions
        float elapsed = 0f;
        float splitTime = splitDuration * 0.4f;

        Vector3 startPos = transform.position;

        while (elapsed < splitTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / splitTime;

            if (topHalf != null)
                topHalf.transform.position = Vector3.Lerp(startPos, startPos + Vector3.up * splitDistance, t);
            if (bottomHalf != null)
                bottomHalf.transform.position = Vector3.Lerp(startPos, startPos + Vector3.down * splitDistance, t);

            yield return null;
        }

        // Hold position
        yield return new WaitForSeconds(splitDuration * 0.2f);

        // Merge back
        elapsed = 0f;
        Vector3 topPos = topHalf != null ? topHalf.transform.position : startPos;
        Vector3 bottomPos = bottomHalf != null ? bottomHalf.transform.position : startPos;

        while (elapsed < splitTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / splitTime;

            if (topHalf != null)
                topHalf.transform.position = Vector3.Lerp(topPos, startPos, t);
            if (bottomHalf != null)
                bottomHalf.transform.position = Vector3.Lerp(bottomPos, startPos, t);

            yield return null;
        }

        // Destroy halves
        if (topHalf != null) Destroy(topHalf);
        if (bottomHalf != null) Destroy(bottomHalf);

        // Show main sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isSplitting = false;

        Debug.Log($"[ATTACK] {gameObject.name} merged back together!");
    }

    GameObject CreateHalf(bool isTop)
    {
        GameObject half = new GameObject(isTop ? "TopHalf" : "BottomHalf");
        half.transform.position = transform.position;
        half.transform.localScale = transform.localScale;

        // Add sprite renderer
        SpriteRenderer sr = half.AddComponent<SpriteRenderer>();

        // Use specific half sprites if assigned, otherwise use full sprite
        if (isTop && topHalfSprite != null)
        {
            sr.sprite = topHalfSprite;
        }
        else if (!isTop && bottomHalfSprite != null)
        {
            sr.sprite = bottomHalfSprite;
        }
        else if (spriteRenderer != null)
        {
            // Fallback to full sprite with color tint
            sr.sprite = spriteRenderer.sprite;
            sr.color = Color.red;
        }

        sr.sortingLayerName = spriteRenderer != null ? spriteRenderer.sortingLayerName : "Default";
        sr.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 1 : 1;

        // Match flip
        if (spriteRenderer != null)
            sr.flipX = spriteRenderer.flipX;

        // Add collider for damage detection
        CircleCollider2D col = half.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        // Add damage component
        ChocolateHalfDamage damageScript = half.AddComponent<ChocolateHalfDamage>();
        damageScript.damage = splitDamage;

        return half;
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
        if (collision.gameObject.CompareTag("Player") && !isSplitting)
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(splitDamage / 2);
            }
        }
    }
}

// Helper component for chocolate halves
public class ChocolateHalfDamage : MonoBehaviour
{
    public int damage = 10;
    private bool hasHit = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                hasHit = true;
                Debug.Log($"[DAMAGE] Chocolate half hit player!");
            }
        }
    }
}