using UnityEngine;

public class CorruptedCupid : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4.5f;
    public float chaseRange = 20f;
    public float orbitDistance = 5f;
    public float randomMoveInterval = 1.5f;

    [Header("Combat")]
    public float attackRange = 10f;
    public float attackCooldown = 2.5f;
    public GameObject arrowPrefab;
    public Transform firePoint;

    [Header("Debuff Type")]
    public DebuffType debuffType = DebuffType.Poison;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color normalColor = Color.red;

    private enum State { Chase, Orbit }
    private State currentState = State.Chase;

    private float cooldown;
    private Transform target;
    private Rigidbody2D rb;
    private Vector2 randomDirection;
    private float randomMoveTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // SAFETY CHECK: Ensure we have a Rigidbody
        if (rb == null)
        {
            Debug.LogError($"{gameObject.name} is MISSING a Rigidbody2D! Please add one.");
        }
        else
        {
            rb.gravityScale = 0;
        }
        
        // SAFETY CHECK: Ensure we have a Collider
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogError($"{gameObject.name} is MISSING a Collider2D! Please add a CircleCollider2D or BoxCollider2D.");
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            // Simple logic to find active player
            target = players[0].transform;
        }

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.color = normalColor;

        randomDirection = Random.insideUnitCircle.normalized;
        randomMoveTimer = randomMoveInterval;
    }

    void Update()
    {
        if (target == null) return;

        cooldown -= Time.deltaTime;
        randomMoveTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer <= orbitDistance * 1.2f) currentState = State.Orbit;
        else currentState = State.Chase;

        switch (currentState)
        {
            case State.Chase: ChasePlayer(); break;
            case State.Orbit: OrbitPlayer(); break;
        }

        if (distanceToPlayer <= attackRange && cooldown <= 0f)
        {
            Shoot();
            cooldown = attackCooldown;
        }

        FaceMovementDirection();
    }

    void ChasePlayer()
    {
        if (rb == null) return;
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    void OrbitPlayer()
    {
        if (rb == null) return;
        Vector2 toPlayer = (Vector2)(target.position - transform.position);
        float currentDistance = toPlayer.magnitude;
        Vector2 radialDirection = toPlayer.normalized;
        float radialSpeed = 0f;

        if (currentDistance < orbitDistance * 0.8f) radialSpeed = -moveSpeed;
        else if (currentDistance > orbitDistance * 1.2f) radialSpeed = moveSpeed;

        Vector2 tangentialDirection = new Vector2(-radialDirection.y, radialDirection.x);

        if (randomMoveTimer <= 0f)
        {
            randomDirection = Random.insideUnitCircle.normalized * 0.3f;
            randomMoveTimer = randomMoveInterval;
        }

        Vector2 velocity = radialDirection * radialSpeed + tangentialDirection * moveSpeed + randomDirection * moveSpeed * 0.5f;
        rb.linearVelocity = velocity;
    }

    void Shoot()
    {
        if (arrowPrefab == null || firePoint == null) return;
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        Vector2 dir = (target.position - firePoint.position).normalized;
        CupidArrow arrowScript = arrow.GetComponent<CupidArrow>();
        if (arrowScript != null)
        {
            arrowScript.Initialize(dir);
            arrowScript.SetDebuff(debuffType);
        }
    }

    void FaceMovementDirection()
    {
        if (rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    // --- DEBUGGING COLLISION ---
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Log EVERYTHING that hits this object
        Debug.Log($"Something hit Cupid! Object: {other.gameObject.name} | Tag: {other.tag}");

        if (other.CompareTag("PlayerBullet"))
        {
            Damageable damageable = GetComponent<Damageable>();
            if (damageable != null) damageable.TakeDamage(1);
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Flower"))
        {
            Debug.Log(">>> SUCCESS! Flower detected. Killing Cupid.");
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}