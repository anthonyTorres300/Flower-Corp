using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public WaveManager waveManager;
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public TMP_Text finalWaveText;
    public TMP_Text highscoreText;
    public Button restartButton;

    [Header("Player References")]
    public PlayerHealth player1Health;
    public PlayerHealth player2Health;

    private bool gameOver = false;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        Time.timeScale = 1f; // Make sure game is running
    }

    void Update()
    {
        if (gameOver) return;

        // Check if both players are dead
        bool player1Dead = player1Health != null && player1Health.currentHealth <= 0;
        bool player2Dead = player2Health != null && player2Health.currentHealth <= 0;

        if (player1Dead && player2Dead)
        {
            TriggerGameOver();
        }
    }

    void TriggerGameOver()
    {
        if (gameOver) return;

        gameOver = true;
        Debug.Log("GAME OVER!");

        // Stop the game
        Time.timeScale = 0f;

        // Get final stats
        if (waveManager != null)
        {
            WaveStats stats = waveManager.GetCurrentStats();

            if (finalScoreText != null)
                finalScoreText.text = $"Cupids Saved: {stats.totalCupidsSaved}";

            if (finalWaveText != null)
                finalWaveText.text = $"Reached Wave: {stats.waveNumber}";

            // Update highscore
            int currentHighscore = PlayerPrefs.GetInt("HighscoreCupidsSaved", 0);
            if (stats.totalCupidsSaved > currentHighscore)
            {
                PlayerPrefs.SetInt("HighscoreCupidsSaved", stats.totalCupidsSaved);
                PlayerPrefs.Save();

                if (highscoreText != null)
                    highscoreText.text = "NEW HIGHSCORE!";
            }
            else
            {
                if (highscoreText != null)
                    highscoreText.text = $"Highscore: {currentHighscore}";
            }
        }

        // Show game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Show cursor for restart button
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}