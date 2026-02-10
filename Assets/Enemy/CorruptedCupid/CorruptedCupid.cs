using UnityEngine;

public class CorruptedCupid : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float zigZagAmplitude = 1.5f;
    public float zigZagFrequency = 2f;
    public float chaseRange = 10f;
    public float minDistanceFromPlayer = 3f;

    [Header("Patrol")]
    public float patrolDistance = 3f;

    [Header("Combat")]
    public float attackRange = 7f;
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

    private enum State { Patrol, Chase, Attack }
    private State currentState = State.Patrol;

    private Vector2 startPos;
    private bool movingRight = true;
    private float cooldown;
    private Transform target;
    private float zigZagTimer;
    private Damageable damageable;

    void Start()
    {
        startPos = transform.position;

        // Find the active player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            // Find the active player
            foreach (GameObject player in players)
            {
                SwitchCharacters sc = player.GetComponent<SwitchCharacters>();
                if (sc != null && sc.isActive)
                {
                    target = player.transform;
                    break;
                }
            }
            // Fallback to first player if no active found
            if (target == null)
                target = players[0].transform;
        }

        damageable = GetComponent<Damageable>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;
    }

    void Update()
    {
        if (target == null) return;

        cooldown -= Time.deltaTime;
        zigZagTimer += Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        // State machine
        if (distanceToPlayer <= attackRange)
        {
            currentState = State.Attack;
        }
        else if (distanceToPlayer <= chaseRange)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Patrol;
        }

        // Execute state behavior
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                ChasePlayer();
                break;
            case State.Attack:
                AttackPlayer();
                break;
        }

        // Face the player
        FaceTarget();
    }

    void Patrol()
    {
        float dir = movingRight ? 1f : -1f;
        transform.Translate(Vector2.right * dir * moveSpeed * Time.deltaTime, Space.World);

        if (Vector2.Distance(startPos, transform.position) >= patrolDistance)
            movingRight = !movingRight;
    }

    void ChasePlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        // Don't get too close
        if (distanceToPlayer > minDistanceFromPlayer)
        {
            Vector2 direction = (target.position - transform.position).normalized;

            // Add zig-zag movement
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            float zigZag = Mathf.Sin(zigZagTimer * zigZagFrequency) * zigZagAmplitude;
            Vector2 zigZagOffset = perpendicular * zigZag * Time.deltaTime;

            Vector2 movement = direction * moveSpeed * Time.deltaTime + zigZagOffset;
            transform.Translate(movement, Space.World);
        }
    }

    void AttackPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        // Keep distance while attacking
        if (distanceToPlayer < minDistanceFromPlayer)
        {
            Vector2 direction = (transform.position - target.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }
        else if (distanceToPlayer > attackRange * 0.8f)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * 0.5f * Time.deltaTime, Space.World);
        }

        // Shoot
        if (cooldown <= 0f)
        {
            Shoot();
            cooldown = attackCooldown;
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

        Debug.Log($"{gameObject.name} shot arrow at player");
    }

    DebuffType RandomDebuff()
    {
        int roll = Random.Range(0, 3);
        return (DebuffType)roll;
    }

    void FaceTarget()
    {
        if (target == null) return;

        Vector2 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    // Called by Lily when she throws a flower at this cupid
    public void ReceiveFlower()
    {
        flowersReceived++;
        Debug.Log($"{gameObject.name} received flower ({flowersReceived}/{flowersNeededToConvert})");

        // Update color to show progress
        if (spriteRenderer != null)
        {
            float t = (float)flowersReceived / flowersNeededToConvert;
            spriteRenderer.color = Color.Lerp(normalColor, convertingColor, t);
        }

        // Convert to friendly
        if (flowersReceived >= flowersNeededToConvert)
        {
            ConvertToFriendly();
        }
    }

    void ConvertToFriendly()
    {
        Debug.Log($"{gameObject.name} converted to friendly cupid!");

        // Spawn friendly cupid if prefab exists
        if (friendlyCupidPrefab != null)
        {
            Instantiate(friendlyCupidPrefab, transform.position, Quaternion.identity);
        }

        // Destroy this corrupted cupid
        Destroy(gameObject);
    }

    // Take damage from player attacks
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if hit by player bullet
        if (other.CompareTag("PlayerBullet"))
        {
            if (damageable != null)
            {
                damageable.TakeDamage(1);
            }
            Destroy(other.gameObject);
        }

        // Check if hit by flower (for conversion)
        if (other.CompareTag("Flower"))
        {
            ReceiveFlower();
            Destroy(other.gameObject);
        }
    }
}