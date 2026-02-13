using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WaveUI : MonoBehaviour
{
    [Header("Wave Info Display")]
    public TMP_Text waveNumberText;
    public TMP_Text enemiesRemainingText;
    public TMP_Text cupidsSavedText;
    public TMP_Text highscoreText;

    [Header("Wave Notifications")]
    public GameObject wavePrepPanel;
    public TMP_Text wavePrepText;
    public TMP_Text wavePrepTimerText;

    [Header("Wave Complete Panel")]
    public GameObject waveCompletePanel;
    public TMP_Text waveCompleteText;
    public TMP_Text cupidsThisWaveText;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public Color successColor = Color.green;

    private Coroutine prepTimerCoroutine;

    void Start()
    {
        // Hide panels at start
        if (wavePrepPanel != null)
            wavePrepPanel.SetActive(false);

        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(false);

        LoadHighscore();
    }

    public void UpdateWaveNumber(int wave)
    {
        if (waveNumberText != null)
        {
            waveNumberText.text = $"Wave {wave}";
            
            // Flash effect
            StartCoroutine(FlashText(waveNumberText, successColor));
        }
    }

    public void UpdateEnemiesRemaining(int count)
    {
        if (enemiesRemainingText != null)
        {
            enemiesRemainingText.text = $"Enemies: {count}";

            // Warning color when low
            if (count <= 3 && count > 0)
                enemiesRemainingText.color = warningColor;
            else if (count == 0)
                enemiesRemainingText.color = successColor;
            else
                enemiesRemainingText.color = normalColor;
        }
    }

    public void UpdateCupidsSaved(int total)
    {
        if (cupidsSavedText != null)
        {
            cupidsSavedText.text = $"Cupids Saved: {total}";
            
            // Update highscore
            int highscore = PlayerPrefs.GetInt("HighscoreCupidsSaved", 0);
            if (total > highscore)
            {
                PlayerPrefs.SetInt("HighscoreCupidsSaved", total);
                PlayerPrefs.Save();
                
                if (highscoreText != null)
                {
                    highscoreText.text = $"Best: {total}";
                    StartCoroutine(FlashText(highscoreText, successColor));
                }
            }
        }
    }

    public void ShowWavePrep(float duration)
    {
        if (wavePrepPanel != null)
        {
            wavePrepPanel.SetActive(true);

            if (prepTimerCoroutine != null)
                StopCoroutine(prepTimerCoroutine);

            prepTimerCoroutine = StartCoroutine(WavePrepTimer(duration));
        }
    }

    public void HideWavePrep()
    {
        if (wavePrepPanel != null)
            wavePrepPanel.SetActive(false);

        if (prepTimerCoroutine != null)
        {
            StopCoroutine(prepTimerCoroutine);
            prepTimerCoroutine = null;
        }
    }

    IEnumerator WavePrepTimer(float duration)
    {
        float timeRemaining = duration;

        while (timeRemaining > 0)
        {
            if (wavePrepTimerText != null)
            {
                wavePrepTimerText.text = $"Next wave in: {timeRemaining:F1}s";
            }

            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        if (wavePrepTimerText != null)
            wavePrepTimerText.text = "FIGHT!";

        yield return new WaitForSeconds(0.5f);
    }

    public void ShowWaveComplete(int waveNumber, int cupidsSaved)
    {
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(true);

            if (waveCompleteText != null)
                waveCompleteText.text = $"Wave {waveNumber} Complete!";

            if (cupidsThisWaveText != null)
                cupidsThisWaveText.text = $"Cupids Saved: {cupidsSaved}";

            StartCoroutine(HideWaveCompleteAfterDelay(2f));
        }
    }

    IEnumerator HideWaveCompleteAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(false);
    }

    IEnumerator FlashText(TMP_Text text, Color flashColor)
    {
        if (text == null) yield break;

        Color originalColor = text.color;

        // Flash to color
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            text.color = Color.Lerp(originalColor, flashColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Flash back
        elapsed = 0f;
        while (elapsed < duration)
        {
            text.color = Color.Lerp(flashColor, originalColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        text.color = originalColor;
    }

    void LoadHighscore()
    {
        int highscore = PlayerPrefs.GetInt("HighscoreCupidsSaved", 0);
        
        if (highscoreText != null)
            highscoreText.text = $"Best: {highscore}";
    }
}