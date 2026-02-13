using UnityEngine;
using UnityEngine.UI;

public class SwitchCharacters : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float followDistance = 1.5f; // distance to maintain behind leader
    public float followSmoothTime = 0.15f; // how smoothly follower catches up
    public bool isActive = false;

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
    private PlayerHealth health;
    private static float lastSwitchTime;
    private Vector2 followVelocity = Vector2.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        health = GetComponent<PlayerHealth>();

        UpdateShootingState();

        if (isActive)
        {
            UpdateHealthUI();
            UpdatePlayerIcon();
        }
    }

    void Update()
    {
        // switch with tab
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
            FollowOtherPlayer();
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
            otherCharacter.healthSlider = healthSlider;
            otherCharacter.playerIcon = playerIcon;
            otherCharacter.UpdatePlayerIcon();
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

        // aim towards mouse
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = (Vector2)mousePos - rb.position;
        rb.rotation = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
    }

    void FollowOtherPlayer()
    {
        if (otherCharacter == null) return;

        // target position is behind the active player based on their facing direction
        Vector3 targetPos = otherCharacter.transform.position - (otherCharacter.transform.up * followDistance);

        // smooth follow using SmoothDamp for natural catch-up behavior
        Vector2 currentPos = rb.position;
        Vector2 smoothTarget = Vector2.SmoothDamp(currentPos, targetPos, ref followVelocity, followSmoothTime);

        rb.linearVelocity = (smoothTarget - currentPos) / Time.fixedDeltaTime;

        // face movement direction smoothly
        Vector2 moveDirection = smoothTarget - currentPos;
        if (moveDirection.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = Mathf.LerpAngle(rb.rotation, targetAngle, Time.deltaTime * 10f);
        }
    }
}