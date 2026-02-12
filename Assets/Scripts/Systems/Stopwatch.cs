using UnityEngine;
using TMPro;

public class Stopwatch : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI timerText; // Drag your text object here

    [Header("Settings")]
    public bool startOnPlay = true;

    private float currentTime;
    private bool isRunning;

    void Start()
    {
        currentTime = 0;
        isRunning = startOnPlay;
    }

    void Update()
    {
        if (isRunning)
        {
            // Add the time passed since last frame
            currentTime += Time.deltaTime;

            // Update the UI
            UpdateTimerDisplay(currentTime);
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        // Calculate the values
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        float milliseconds = (timeToDisplay % 1) * 1000;

        // Format string: 00:00:000
        // {0:00} means "Variable 0, ensure it has at least 2 digits"
        timerText.text = string.Format("TIME: {0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    // Call this from a button to Start/Pause
    public void ToggleStopwatch()
    {
        isRunning = !isRunning;
    }

    // Call this from a button to Reset
    public void ResetStopwatch()
    {
        currentTime = 0;
        isRunning = false;
        UpdateTimerDisplay(0);
    }
}