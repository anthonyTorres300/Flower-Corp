using UnityEngine;

public class PlayerDebuffHandler : MonoBehaviour, IDebuffable
{
    [Header("Debuff Effects - BUFFED")]
    public float poisonDamagePerSecond = 15f; // Increased from 5
    public float confusionControlMultiplier = -1f;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color poisonColor = new Color(0f, 1f, 0f, 0.8f); // Bright green
    public Color stunColor = new Color(1f, 1f, 0f, 0.8f); // Bright yellow
    public Color confusionColor = new Color(1f, 0f, 1f, 0.8f); // Bright magenta

    private DebuffType currentDebuff;
    private float debuffTimer;
    private bool isDebuffed;
    private Color originalColor;
    private PlayerHealth health;
    private SwitchCharacters characterController;
    private Rigidbody2D rb;

    void Start()
    {
        health = GetComponent<PlayerHealth>();
        characterController = GetComponent<SwitchCharacters>();
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (isDebuffed)
        {
            debuffTimer -= Time.deltaTime;

            // Apply debuff effects
            switch (currentDebuff)
            {
                case DebuffType.Poison:
                    if (health != null)
                    {
                        int damage = Mathf.RoundToInt(poisonDamagePerSecond * Time.deltaTime);
                        if (damage > 0)
                            health.TakeDamage(damage);
                    }
                    break;

                case DebuffType.Stun:
                    // Completely stop movement
                    if (rb != null)
                        rb.linearVelocity = Vector2.zero;
                    break;

                case DebuffType.Confusion:
                    // Confusion is handled in SwitchCharacters movement
                    break;
            }

            // Remove debuff when timer expires
            if (debuffTimer <= 0f)
            {
                RemoveDebuff();
            }
        }
    }

    public void ApplyDebuff(DebuffType type, float duration)
    {
        // Ignore if None type
        if (type == DebuffType.None) return;

        // Remove previous debuff first
        if (isDebuffed)
            RemoveDebuff();

        currentDebuff = type;
        debuffTimer = duration;
        isDebuffed = true;

        Debug.Log($"[DEBUFF] {gameObject.name} hit with {type} for {duration}s!");

        // Apply visual feedback and mechanical effects
        switch (type)
        {
            case DebuffType.Poison:
                if (spriteRenderer != null)
                    spriteRenderer.color = poisonColor;
                Debug.Log($"[DEBUFF] Poison dealing {poisonDamagePerSecond} damage/sec");
                break;

            case DebuffType.Stun:
                if (spriteRenderer != null)
                    spriteRenderer.color = stunColor;
                // Disable movement completely
                if (characterController != null)
                    characterController.enabled = false;
                Debug.Log($"[DEBUFF] STUNNED - Cannot move!");
                break;

            case DebuffType.Confusion:
                if (spriteRenderer != null)
                    spriteRenderer.color = confusionColor;
                Debug.Log($"[DEBUFF] CONFUSED - Controls reversed!");
                break;
        }
    }

    void RemoveDebuff()
    {
        Debug.Log($"[DEBUFF] {currentDebuff} worn off");

        // Restore visual
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        // Re-enable movement if stunned
        if (currentDebuff == DebuffType.Stun && characterController != null)
            characterController.enabled = true;

        isDebuffed = false;
    }

    // Public getters for other scripts
    public bool IsDebuffed() => isDebuffed;
    public DebuffType GetCurrentDebuff() => currentDebuff;
    public bool IsConfused() => isDebuffed && currentDebuff == DebuffType.Confusion;
    public float GetConfusionMultiplier() => IsConfused() ? confusionControlMultiplier : 1f;
    public bool IsStunned() => isDebuffed && currentDebuff == DebuffType.Stun;
}