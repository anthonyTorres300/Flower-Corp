using UnityEngine;
using UnityEngine.UI;

public class SwitchCharacters : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float followDistance = 2f;
    public bool isActive = false;

    [Header("References")]
    public SwitchCharacters otherCharacter;
    public MonoBehaviour shootingScript;

    [Header("Health UI")]
    public Slider healthSlider;

    [Header("Player Icon UI")]
    public Image playerIcon;     // UI image that changes
    public Sprite myIcon;        // This character's portrait

    private Rigidbody2D rb;
    private Camera cam;
    private PlayerHealth health;

    private static float lastSwitchTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        health = GetComponent<PlayerHealth>();

        UpdateShootingState();

        // If this character starts active, set UI immediately
        if (isActive)
        {
            UpdateHealthUI();
            UpdatePlayerIcon(); // NEW
        }
    }

    void Update()
    {
        if (isActive && Input.GetKeyDown(KeyCode.Tab) && Time.time > lastSwitchTime + 0.2f)
        {
            PerformSwitch();
        }

        if (isActive)
        {
            HandlePlayerInput();
            UpdateHealthUI();
        }
        else
        {
            HandleFollowAI();
        }
    }

    void PerformSwitch()
    {
        lastSwitchTime = Time.time;

        isActive = false;
        UpdateShootingState();

        if (otherCharacter != null)
        {
            otherCharacter.isActive = true;
            otherCharacter.UpdateShootingState();

            // Transfer UI control
            otherCharacter.healthSlider = healthSlider;
            otherCharacter.playerIcon = playerIcon;

            // Update the icon for the new player
            otherCharacter.UpdatePlayerIcon(); // NEW
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
        if (healthSlider == null || health == null) return;

        healthSlider.maxValue = health.maxHealth;
        healthSlider.value = health.currentHealth;
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
