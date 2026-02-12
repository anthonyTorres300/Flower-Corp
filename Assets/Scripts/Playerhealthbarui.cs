using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthBarUI : MonoBehaviour
{
    [Header("Player 1 (Left Side - Cosmos)")]
    public Slider player1HealthSlider;
    public Image player1FillImage;
    public TextMeshProUGUI player1HealthText;
    public Image player1Icon;
    public PlayerHealth player1Health;

    [Header("Player 2 (Right Side - Lily)")]
    public Slider player2HealthSlider;
    public Image player2FillImage;
    public TextMeshProUGUI player2HealthText;
    public Image player2Icon;
    public PlayerHealth player2Health;

    [Header("Colors")]
    public Color fullHealthColor = Color.green;
    public Color halfHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Header("Settings")]
    public bool showNumbers = true;

    void Start()
    {
        // Find both players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                // Check if this is Cosmos or Lily by name
                if (player.name.Contains("Cosmos"))
                {
                    player1Health = ph;
                }
                else if (player.name.Contains("Lily"))
                {
                    player2Health = ph;
                }
            }
        }
    }

    void Update()
    {
        UpdateHealthDisplay(player1Health, player1HealthSlider, player1FillImage, player1HealthText);
        UpdateHealthDisplay(player2Health, player2HealthSlider, player2FillImage, player2HealthText);
    }

    void UpdateHealthDisplay(PlayerHealth health, Slider slider, Image fillImage, TextMeshProUGUI healthText)
    {
        if (health == null || slider == null) return;

        // Update slider
        slider.maxValue = health.maxHealth;
        slider.value = health.currentHealth;

        // Update color based on health percentage
        float healthPercent = (float)health.currentHealth / health.maxHealth;

        if (fillImage != null)
        {
            if (healthPercent > 0.6f)
                fillImage.color = fullHealthColor;
            else if (healthPercent > 0.3f)
                fillImage.color = halfHealthColor;
            else
                fillImage.color = lowHealthColor;
        }

        // Update text
        if (showNumbers && healthText != null)
        {
            healthText.text = $"{health.currentHealth}";
        }
    }
}