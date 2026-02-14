using UnityEngine;
using UnityEngine.UI;

public class SwitchCharacters : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public bool isActive = false;

    [Header("Camera Settings")]
    public float cameraSmoothSpeed = 5f; // Higher = snappier, Lower = smoother
    public Vector3 cameraOffset = new Vector3(0, 0, -10);

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
    // private PlayerHealth health; // Uncomment if you have the health script

    private static float lastSwitchTime;
    private Transform camTransform;

    // AI State Variables
    private Vector2 currentWanderTarget;
    private bool isWandering = false;
    private float nextWanderTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        if (cam != null)
        {
            camTransform = cam.transform;
        }
        
        // health = GetComponent<PlayerHealth>(); 

        UpdateShootingState();

        if (isActive)
        {
            UpdateHealthUI();
            UpdatePlayerIcon();
            // We don't need to call a camera function here anymore; 
            // LateUpdate handles it automatically every frame.
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
            HandleDynamicAI(); 
        }
    }

    // --- NEW CAMERA LOGIC ---
    // LateUpdate runs after Update and FixedUpdate. 
    // This is the standard place for camera following code to prevent jitter.
    void LateUpdate()
    {
        // Only the active character controls the camera
        if (isActive && camTransform != null)
        {
            // Calculate where the camera should be (Player position + Offset)
            Vector3 targetPosition = transform.position + cameraOffset;

            // Smoothly move the camera from where it is now -> to the target
            // This handles BOTH following the player AND the "swoosh" effect when switching
            camTransform.position = Vector3.Lerp(camTransform.position, targetPosition, Time.deltaTime * cameraSmoothSpeed);
        }
    }
    // ------------------------

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

    // --- DYNAMIC AI ---
    void HandleDynamicAI()
    {
        if (otherCharacter == null) return;

        float distToPlayer = Vector2.Distance(transform.position, otherCharacter.transform.position);

        // STATE 1: CATCH UP (Leashing)
        if (distToPlayer > leashDistance)
        {
            Vector2 direction = (otherCharacter.transform.position - transform.position).normalized;
            rb.linearVelocity = direction * (moveSpeed * catchUpSpeedMult);
            RotateTowards(direction);
            isWandering = false;
            return;
        }

        // STATE 2: IDLING
        if (!isWandering && Time.time < nextWanderTime)
        {
            rb.linearVelocity = Vector2.zero; 
            Vector2 dirToPlayer = (otherCharacter.transform.position - transform.position).normalized;
            RotateTowards(dirToPlayer, 2f); // Slow rotation look at player
            return;
        }

        // STATE 3: PICK NEW TARGET
        if (!isWandering && Time.time >= nextWanderTime)
        {
            PickNewWanderTarget();
        }

        // STATE 4: MOVING TO TARGET
        if (isWandering)
        {
            float distToTarget = Vector2.Distance(transform.position, currentWanderTarget);

            if (distToTarget > 0.5f)
            {
                Vector2 direction = (currentWanderTarget - (Vector2)transform.position).normalized;
                rb.linearVelocity = direction * (moveSpeed * 0.6f); // 60% speed for wandering
                RotateTowards(direction);
            }
            else
            {
                // Arrived
                isWandering = false;
                nextWanderTime = Time.time + Random.Range(wanderWaitTime.x, wanderWaitTime.y);
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void PickNewWanderTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
        currentWanderTarget = (Vector2)otherCharacter.transform.position + randomOffset;
        isWandering = true;
    }

    void RotateTowards(Vector2 direction, float speed = 10f)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = Mathf.LerpAngle(rb.rotation, angle, Time.deltaTime * speed);
    }

    // --- VISUALIZATION ---
    void OnDrawGizmos()
    {
        if (isActive || otherCharacter == null) return;

        // Draw Leash
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawWireSphere(otherCharacter.transform.position, leashDistance);

        // Draw Wander Radius
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(otherCharacter.transform.position, wanderRadius);

        // Draw Target
        if (isWandering)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, currentWanderTarget);
            Gizmos.DrawSphere(currentWanderTarget, 0.3f);
        }
    }

    void PerformSwitch()
    {
        lastSwitchTime = Time.time;

        // Deactivate THIS character
        isActive = false;
        UpdateShootingState();
        rb.linearVelocity = Vector2.zero; // Stop moving immediately

        // Activate OTHER character
        if (otherCharacter != null)
        {
            otherCharacter.isActive = true;
            otherCharacter.UpdateShootingState();

            // Pass UI references to the new active character
            otherCharacter.healthSlider = healthSlider;
            otherCharacter.playerIcon = playerIcon;

            otherCharacter.UpdatePlayerIcon();
            
            // Note: We do NOT need to call a camera function here.
            // Since 'otherCharacter.isActive' is now true, the 
            // LateUpdate loop on *that* script will automatically 
            // start pulling the camera towards it.
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
}