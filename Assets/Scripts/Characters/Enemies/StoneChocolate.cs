using UnityEngine;
using System.Collections;

public class StoneChocolate : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float chaseRange = 12f;

    [Header("Split Attack")]
    public float splitRange = 3f;
    public float splitDistance = 2f;
    public float splitDuration = 1.2f;
    public float splitCooldown = 3f;
    public int splitDamage = 10;

    [Header("Visual - YOU ONLY HAVE TOP AND BOTTOM")]
    public SpriteRenderer spriteRenderer;
    public Sprite topHalfSprite;
    public Sprite bottomHalfSprite;

    private Transform target;
    private Rigidbody2D rb;
    private bool isSplitting = false;
    private float splitCooldownTimer = 0f;
    private GameObject topHalf;
    private GameObject bottomHalf;

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

        // find active player
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

        // hide the main chocolate (which should show BOTH halves stacked)
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // create separate top and bottom halves
        topHalf = CreateHalf(true);
        bottomHalf = CreateHalf(false);

        // split apart
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

        // stay split for a moment
        yield return new WaitForSeconds(splitDuration * 0.2f);

        // merge back together
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

        // destroy the halves
        if (topHalf != null) Destroy(topHalf);
        if (bottomHalf != null) Destroy(bottomHalf);

        // show the main sprite again
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isSplitting = false;
    }

    GameObject CreateHalf(bool isTop)
    {
        GameObject half = new GameObject(isTop ? "TopHalf" : "BottomHalf");
        half.transform.position = transform.position;
        half.transform.localScale = transform.localScale;

        SpriteRenderer sr = half.AddComponent<SpriteRenderer>();

        // use your top/bottom sprites
        sr.sprite = isTop ? topHalfSprite : bottomHalfSprite;
        sr.sortingLayerName = spriteRenderer != null ? spriteRenderer.sortingLayerName : "Default";
        sr.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 1 : 1;

        if (spriteRenderer != null)
            sr.flipX = spriteRenderer.flipX;

        // add trigger collider for damage
        CircleCollider2D col = half.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

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
            }
        }
    }
}