using UnityEngine;

public class PlayerDebuffHandler : MonoBehaviour, IDebuffable
{
    [Header("Debuff Effects")]
    public float poisonDamagePerSecond = 5f;
    public float confusionControlMultiplier = -1f; // -1 reverses controls

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color poisonColor = Color.green;
    public Color stunColor = Color.yellow;
    public Color confusionColor = Color.magenta;

    private DebuffType currentDebuff;
    private float debuffTimer;
    private bool isDebuffed;
    private Color originalColor;
    private PlayerHealth health;
    private SwitchCharacters characterController;

    void Start()
    {
        health = GetComponent<PlayerHealth>();
        characterController = GetComponent<SwitchCharacters>();

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
                        health.TakeDamage(Mathf.RoundToInt(poisonDamagePerSecond * Time.deltaTime));
                    break;

                case DebuffType.Stun:
                    // Stun is handled by disabling movement in the effect
                    break;

                case DebuffType.Confusion:
                    // Confusion is handled in movement logic
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
        // Remove previous debuff first
        if (isDebuffed)
            RemoveDebuff();

        currentDebuff = type;
        debuffTimer = duration;
        isDebuffed = true;

        Debug.Log($"{gameObject.name} debuffed with {type} for {duration} seconds");

        // Apply visual feedback and mechanical effects
        switch (type)
        {
            case DebuffType.Poison:
                if (spriteRenderer != null)
                    spriteRenderer.color = poisonColor;
                break;

            case DebuffType.Stun:
                if (spriteRenderer != null)
                    spriteRenderer.color = stunColor;
                // Disable movement
                if (characterController != null)
                    characterController.enabled = false;
                break;

            case DebuffType.Confusion:
                if (spriteRenderer != null)
                    spriteRenderer.color = confusionColor;
                break;
        }
    }

    void RemoveDebuff()
    {
        Debug.Log($"{gameObject.name} debuff removed: {currentDebuff}");

        // Restore visual
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        // Re-enable movement if stunned
        if (currentDebuff == DebuffType.Stun && characterController != null)
            characterController.enabled = true;

        isDebuffed = false;
    }

    // Public getters for other scripts to check debuff status
    public bool IsDebuffed() => isDebuffed;
    public DebuffType GetCurrentDebuff() => currentDebuff;
    public bool IsConfused() => isDebuffed && currentDebuff == DebuffType.Confusion;
    public float GetConfusionMultiplier() => IsConfused() ? confusionControlMultiplier : 1f;
}