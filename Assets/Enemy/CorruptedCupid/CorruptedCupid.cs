using UnityEngine;

public class CorruptedCupid : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float chaseRange = 12f;
    public float orbitDistance = 4f;
    public float orbitSpeed = 2f;
    public float randomMoveInterval = 1.5f;

    [Header("Combat")]
    public float attackRange = 8f;
    public float attackCooldown = 2.5f;
    public GameObject arrowPrefab;
    public Transform firePoint;

    [Header("Conversion")]
    public GameObject friendlyCupidPrefab;
    public int flowersNeededToConvert = 3;
    private int flowersReceived = 0;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color normalColor = Color.red;
    public Color convertingColor = Color.pink;

    private enum State { Idle, Chase, Orbit }
    private State currentState = State.Idle;

    private float cooldown;
    private Transform target;
    private Rigidbody2D rb;
    private Vector2 randomDirection;
    private float randomMoveTimer;
    private float orbitAngle;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0;
        }

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

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;

        randomDirection = Random.insideUnitCircle.normalized;
        randomMoveTimer = randomMoveInterval;
        orbitAngle = Random.Range(0f, 360f);
    }

    void Update()
    {
        if (target == null) return;

        cooldown -= Time.deltaTime;
        randomMoveTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer <= orbitDistance * 1.2f)
        {
            currentState = State.Orbit;
        }
        else if (distanceToPlayer <= chaseRange)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Idle;
        }

        switch (currentState)
        {
            case State.Idle:
                IdleFloat();
                break;
            case State.Chase:
                ChasePlayer();
                break;
            case State.Orbit:
                OrbitPlayer();
                break;
        }

        if (distanceToPlayer <= attackRange && cooldown <= 0f)
        {
            Shoot();
            cooldown = attackCooldown;
        }

        FaceMovementDirection();
    }

    void IdleFloat()
    {
        if (randomMoveTimer <= 0f)
        {
            randomDirection = Random.insideUnitCircle.normalized;
            randomMoveTimer = randomMoveInterval;
        }

        if (rb != null)
        {
            rb.linearVelocity = randomDirection * moveSpeed * 0.3f;
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (target.position - transform.position).normalized;

        if (rb != null)
        {
            rb.linearVelocity = direction * moveSpeed;
        }
    }

    void OrbitPlayer()
    {
        orbitAngle += orbitSpeed * Time.deltaTime * 50f;

        Vector2 toPlayer = (Vector2)(target.position - transform.position);
        float currentDistance = toPlayer.magnitude;

        Vector2 radialDirection = toPlayer.normalized;
        float radialSpeed = 0f;

        if (currentDistance < orbitDistance * 0.8f)
        {
            radialSpeed = -moveSpeed;
        }
        else if (currentDistance > orbitDistance * 1.2f)
        {
            radialSpeed = moveSpeed;
        }

        Vector2 tangentialDirection = new Vector2(-radialDirection.y, radialDirection.x);

        if (randomMoveTimer <= 0f)
        {
            randomDirection = Random.insideUnitCircle.normalized * 0.3f;
            randomMoveTimer = randomMoveInterval;
        }

        Vector2 velocity = radialDirection * radialSpeed + tangentialDirection * moveSpeed + randomDirection * moveSpeed * 0.5f;

        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }
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
            arrowScript.SetDebuff(RandomDebuff());
        }
    }

    DebuffType RandomDebuff()
    {
        int roll = Random.Range(0, 3);
        return (DebuffType)roll;
    }

    void FaceMovementDirection()
    {
        if (rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    public void ReceiveFlower()
    {
        flowersReceived++;
        Debug.Log($"[CONVERSION] {gameObject.name} received flower ({flowersReceived}/{flowersNeededToConvert})");

        if (spriteRenderer != null)
        {
            float t = (float)flowersReceived / flowersNeededToConvert;
            spriteRenderer.color = Color.Lerp(normalColor, convertingColor, t);
        }

        if (flowersReceived >= flowersNeededToConvert)
        {
            ConvertToFriendly();
        }
    }

    void ConvertToFriendly()
    {
        Debug.Log($"[CONVERSION] {gameObject.name} converted to friendly!");

        if (friendlyCupidPrefab != null)
        {
            Instantiate(friendlyCupidPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
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

        if (other.CompareTag("Flower"))
        {
            ReceiveFlower();
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
                health.TakeDamage(5);
            }
        }
    }
}