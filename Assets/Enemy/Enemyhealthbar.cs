using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;
    public Canvas healthBarCanvas;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Position above enemy
    public bool hideWhenFull = true;
    public bool alwaysFaceCamera = true;

    private Damageable damageable;
    private Camera mainCamera;

    void Start()
    {
        damageable = GetComponentInParent<Damageable>();
        mainCamera = Camera.main;

        if (damageable == null)
        {
            Debug.LogError("[HEALTH BAR] No Damageable component found on parent!");
            enabled = false;
            return;
        }

        if (healthSlider == null)
        {
            Debug.LogError("[HEALTH BAR] Health slider not assigned!");
            enabled = false;
            return;
        }

        // Setup canvas for world space
        if (healthBarCanvas != null)
        {
            healthBarCanvas.renderMode = RenderMode.WorldSpace;
            healthBarCanvas.worldCamera = mainCamera;
        }

        UpdateHealthBar();
    }

    void Update()
    {
        UpdateHealthBar();

        // Position above enemy
        transform.position = transform.parent.position + offset;

        // Face camera
        if (alwaysFaceCamera && mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                            mainCamera.transform.rotation * Vector3.up);
        }
    }

    void UpdateHealthBar()
    {
        if (damageable == null || healthSlider == null) return;

        healthSlider.maxValue = damageable.maxHealth;
        healthSlider.value = damageable.currentHealth;

        // Hide when full health
        if (hideWhenFull && healthBarCanvas != null)
        {
            healthBarCanvas.enabled = damageable.currentHealth < damageable.maxHealth;
        }
    }
}