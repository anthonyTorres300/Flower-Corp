using UnityEngine;
using UnityEngine.UI;

public class SwitchCharacters : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public bool isActive = false;

    [Header("Dynamic AI Settings")]
    public float wanderRadius = 3f;      // How far they can roam from the player
    public float leashDistance = 6f;     // If player gets this far, AI sprints back
    public float catchUpSpeedMult = 1.5f; // How much faster they run when catching up
    public Vector2 wanderWaitTime = new Vector2(1f, 3f); // Min/Max wait time between wanders

    [Header("References")]
    public SwitchCharacters otherCharacter;
    public MonoBehaviour shootingScript;

    [Header("Health UI")]
    public Slider healthSlider;

    [Header("Player Icon UI")]
    public Image playerIcon;
    public Sprite myIcon;

    private Rigidbody2D rb;
    private Camera cam;
    // private PlayerHealth health; // Commented out as I don't have this script, uncomment in your project

    private static float lastSwitchTime;
    private Transform camTransform;
    public Vector3 cameraOffset = new Vector3(0, 0, -10);

    // AI State Variables
    private Vector2 currentWanderTarget;
    private bool isWandering = false;
    private float nextWanderTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        camTransform = Camera.main.transform;
        // health = GetComponent<PlayerHealth>(); 

        UpdateShootingState();

        if (isActive)
        {
            UpdateHealthUI();
            UpdatePlayerIcon();
            MoveCameraToMe();
        }
    }

    void Update()
    {
        // Handle Switching
        if (isActive && Input.GetKeyDown(KeyCode.Tab) && Time.time > lastSwitchTime + 0.2f)
        {
            PerformSwitch();
        }

        // State Machine
        if (isActive)
        {
            HandlePlayerInput();
            UpdateHealthUI();
        }
        else
        {
            HandleDynamicAI(); // New AI Logic
        }
    }

    void HandlePlayerInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 moveDir = new Vector2(moveX, moveY).normalized;

        // Unity 6 uses linearVelocity, older versions use velocity
        rb.linearVelocity = moveDir * moveSpeed;

        // Aiming
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = (Vector2)mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }

    // --- NEW DYNAMIC AI ---
    void HandleDynamicAI()
    {
        if (otherCharacter == null) return;

        float distToPlayer = Vector2.Distance(transform.position, otherCharacter.transform.position);

        // STATE 1: CATCH UP (Leashing)
        // If we are too far, ignore wandering and run to the player
        if (distToPlayer > leashDistance)
        {
            Vector2 direction = (otherCharacter.transform.position - transform.position).normalized;

            // Move faster to catch up
            rb.linearVelocity = direction * (moveSpeed * catchUpSpeedMult);

            // Look at player while running
            RotateTowards(direction);

            // Reset wander state so we pick a new spot once we arrive
            isWandering = false;
            return;
        }

        // STATE 2: WANDERING / IDLING
        // If we are currently waiting...
        if (!isWandering && Time.time < nextWanderTime)
        {
            rb.linearVelocity = Vector2.zero; // Stand still

            // Optional: Slowly rotate to look at player while idle
            Vector2 dirToPlayer = (otherCharacter.transform.position - transform.position).normalized;
            RotateTowards(dirToPlayer, 2f); // Slow rotation
            return;
        }

        // If we need a new target...
        if (!isWandering && Time.time >= nextWanderTime)
        {
            PickNewWanderTarget();
        }

        // Move to the wander target
        if (isWandering)
        {
            float distToTarget = Vector2.Distance(transform.position, currentWanderTarget);

            // If we haven't reached the target yet
            if (distToTarget > 0.5f)
            {
                Vector2 direction = (currentWanderTarget - (Vector2)transform.position).normalized;

                // Wander at 60% speed for a more relaxed feel
                rb.linearVelocity = direction * (moveSpeed * 0.6f);
                RotateTowards(direction);
            }
            else
            {
                // We arrived! Start waiting.
                isWandering = false;
                nextWanderTime = Time.time + Random.Range(wanderWaitTime.x, wanderWaitTime.y);
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void PickNewWanderTarget()
    {
        // Pick a random point inside a circle around the PLAYER (not ourselves)
        // This ensures they stick close to the player but explore the immediate area
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        currentWanderTarget = (Vector2)otherCharacter.transform.position + randomOffset;
        isWandering = true;
    }

    void RotateTowards(Vector2 direction, float speed = 10f)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = Mathf.LerpAngle(rb.rotation, angle, Time.deltaTime * speed);
    }

    // --- VISUALIZATION (Draws circles in Editor) ---
    void OnDrawGizmos()
    {
        if (isActive || otherCharacter == null) return;

        // Draw the Leash Distance (Red)
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawWireSphere(otherCharacter.transform.position, leashDistance);

        // Draw the Wander Radius (Green)
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(otherCharacter.transform.position, wanderRadius);

        // Draw line to current target
        if (isWandering)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, currentWanderTarget);
            Gizmos.DrawSphere(currentWanderTarget, 0.3f);
        }
    }
    // ------------------------------------------------

    void PerformSwitch()
    {
        lastSwitchTime = Time.time;
        isActive = false;
        UpdateShootingState();

        // Stop moving immediately when switching to AI mode
        rb.linearVelocity = Vector2.zero;

        if (otherCharacter != null)
        {
            otherCharacter.isActive = true;
            otherCharacter.UpdateShootingState();
            otherCharacter.healthSlider = healthSlider;
            otherCharacter.playerIcon = playerIcon;
            otherCharacter.UpdatePlayerIcon();
            otherCharacter.MoveCameraToMe();
        }
    }

    void UpdatePlayerIcon()
    {
        if (playerIcon != null && myIcon != null)
        {
            playerIcon.sprite = myIcon;
        }
    }

    void UpdateHealthUI()
    {
        // if (healthSlider == null || health == null) return;
        // healthSlider.maxValue = health.maxHealth;
        // healthSlider.value = health.currentHealth;
    }

    public void UpdateShootingState()
    {
        if (shootingScript != null)
        {
            shootingScript.enabled = isActive;
        }
    }

    void MoveCameraToMe()
    {
        if (camTransform != null)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothCameraMove());
        }
    }

    System.Collections.IEnumerator SmoothCameraMove()
    {
        Vector3 start = camTransform.position;
        // Ensure we maintain the Z offset correctly during the lerp
        Vector3 target = transform.position;
        target.z = cameraOffset.z;

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 6f;
            Vector3 currentPos = Vector3.Lerp(start, target, t);
            // Re-apply Z offset explicitly to avoid clipping
            currentPos.z = cameraOffset.z;
            camTransform.position = currentPos;
            yield return null;
        }
    }
}