using UnityEngine;

public class SwitchCharacters : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float followDistance = 2f;
    public bool isActive = false;

    [Header("References")]
    public SwitchCharacters otherCharacter;
    public MonoBehaviour shootingScript;

    private Rigidbody2D rb;
    private Camera cam;

    // STATIC variable shared by all instances of this script
    // This tracks the last time a switch happened globally
    private static float lastSwitchTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        // Ensure the shooting script matches the starting state
        UpdateShootingState();
    }

    void Update()
    {
        // Check for Tab input AND if enough time has passed since the last switch (0.2 seconds)
        if (isActive && Input.GetKeyDown(KeyCode.Tab) && Time.time > lastSwitchTime + 0.2f)
        {
            PerformSwitch();
        }

        if (isActive) HandlePlayerInput();
        else HandleFollowAI();
    }

    void PerformSwitch()
    {
        // 1. Record the time so the other character doesn't switch back instantly
        lastSwitchTime = Time.time;

        // 2. Turn myself OFF
        isActive = false;
        UpdateShootingState();

        // 3. Turn the other character ON
        if (otherCharacter != null)
        {
            otherCharacter.isActive = true;
            otherCharacter.UpdateShootingState();
        }
    }

    public void UpdateShootingState()
    {
        if (shootingScript != null)
        {
            shootingScript.enabled = isActive;
        }
    }

    void HandlePlayerInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 moveDir = new Vector2(moveX, moveY).normalized;

        // Using linearVelocity for Unity 6 (use .velocity for older versions)
        rb.linearVelocity = moveDir * moveSpeed;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = (Vector2)mousePos - rb.position;
        rb.rotation = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
    }

    void HandleFollowAI()
    {
        if (otherCharacter == null) return;

        Vector3 targetPos = otherCharacter.transform.position - (otherCharacter.transform.up * followDistance);
        float distanceToTarget = Vector2.Distance(transform.position, targetPos);

        if (distanceToTarget > 0.1f)
        {
            Vector2 direction = (targetPos - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = Mathf.LerpAngle(rb.rotation, angle, Time.deltaTime * 10f);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}