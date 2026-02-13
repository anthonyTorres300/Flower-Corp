using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Configuration")]
    public int currentWave = 0;
    public float wavePrepTime = 5f; // Time between waves
    public bool waveInProgress = false;
    public bool gameStarted = false; // NEW: Tracks if game has actually started

    [Header("Enemy Prefabs")]
    public GameObject corruptedCupidPrefab;
    public GameObject stoneChocolatePrefab;
    public GameObject diamondCritterPrefab;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints; // Manual spawn points
    public float spawnRadius = 15f; // If no spawn points, spawn in circle around player
    public float spawnInterval = 0.5f; // Time between spawning each enemy
    public float minDistanceFromPlayer = 8f; // Don't spawn too close to player

    [Header("Difficulty Scaling")]
    [Tooltip("Base enemies in wave 1")]
    public int baseEnemyCount = 5;
    [Tooltip("How many more enemies per wave")]
    public float enemyScalingPerWave = 2f;
    [Tooltip("At what wave should we start mixing in harder enemies")]
    public int chocolateStartWave = 3;
    public int diamondStartWave = 5;
    [Tooltip("Enemy health multiplier per wave (1.1 = 10% more HP per wave)")]
    public float healthScalingPerWave = 1.08f;

    [Header("Statistics")]
    public int totalCupidsSaved = 0;
    public int totalEnemiesKilled = 0;
    public int cupidsSavedThisWave = 0;

    [Header("References")]
    public WaveUI waveUI;
    public CupidManager cupidManager;

    private int enemiesRemainingInWave;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Transform playerTransform;

    void Start()
    {
        FindPlayer();
        
        if (waveUI != null)
        {
            waveUI.UpdateWaveNumber(currentWave);
            waveUI.UpdateEnemiesRemaining(0);
            waveUI.UpdateCupidsSaved(totalCupidsSaved);
        }

        // DON'T start waves yet - wait for weapon selection to finish
        // The WeaponSelection script will call StartGame() when ready
        Debug.Log("[WAVE SYSTEM] Waiting for weapon selection...");
    }

    // NEW: Call this method from WeaponSelection after weapon is chosen
    public void StartGame()
    {
        if (gameStarted) return; // Prevent double-start
        
        gameStarted = true;
        Debug.Log("[WAVE SYSTEM] Game starting! First wave incoming...");
        
        // Wait a frame to ensure Time.timeScale is properly set to 1
        StartCoroutine(StartGameSafe());
    }

    IEnumerator StartGameSafe()
    {
        // Wait for Time.timeScale to be normal
        while (Time.timeScale < 0.5f)
        {
            Debug.LogWarning("[WAVE SYSTEM] Waiting for Time.timeScale to resume...");
            yield return null;
        }

        Debug.Log("[WAVE SYSTEM] Time.timeScale is normal, starting waves...");
        
        // Wait one more frame for safety
        yield return null;
        
        // Start first wave after a delay
        StartCoroutine(StartNextWaveWithDelay(3f));
    }

    void Update()
    {
        // Clean up null references (destroyed enemies)
        activeEnemies.RemoveAll(enemy => enemy == null);

        // Check if wave is complete
        if (waveInProgress && activeEnemies.Count == 0 && enemiesRemainingInWave == 0)
        {
            CompleteWave();
        }
    }

    IEnumerator StartNextWaveWithDelay(float delay)
    {
        waveInProgress = false;

        if (waveUI != null)
        {
            waveUI.ShowWavePrep(delay);
        }

        yield return new WaitForSeconds(delay);

        StartWave();
    }

    void StartWave()
    {
        currentWave++;
        waveInProgress = true;
        cupidsSavedThisWave = 0;

        // Calculate how many enemies to spawn
        int totalEnemies = Mathf.RoundToInt(baseEnemyCount + (currentWave - 1) * enemyScalingPerWave);
        
        // SAFETY CAP - prevent too many enemies
        totalEnemies = Mathf.Min(totalEnemies, 100);
        
        enemiesRemainingInWave = totalEnemies;

        Debug.Log($"[WAVE {currentWave}] Starting with {totalEnemies} enemies!");

        // Safety check - make sure we have enemy prefabs!
        if (corruptedCupidPrefab == null && stoneChocolatePrefab == null && diamondCritterPrefab == null)
        {
            Debug.LogError("[WAVE SYSTEM] NO ENEMY PREFABS ASSIGNED! Assign them in WaveManager inspector!");
            return;
        }

        if (waveUI != null)
        {
            waveUI.UpdateWaveNumber(currentWave);
            waveUI.UpdateEnemiesRemaining(enemiesRemainingInWave);
            waveUI.HideWavePrep();
        }

        // Generate enemy composition for this wave
        List<GameObject> enemiesToSpawn = GenerateWaveComposition(totalEnemies);

        if (enemiesToSpawn.Count == 0)
        {
            Debug.LogError("[WAVE SYSTEM] No enemies to spawn! Check prefab assignments!");
            return;
        }

        // Start spawning enemies
        StartCoroutine(SpawnEnemiesOverTime(enemiesToSpawn));
    }

    List<GameObject> GenerateWaveComposition(int totalEnemies)
    {
        List<GameObject> composition = new List<GameObject>();

        // Calculate percentages based on wave number
        float cupidPercentage = 0.6f; // Always 60% cupids (they can be saved)
        float chocolatePercentage = 0f;
        float diamondPercentage = 0f;

        // Introduce chocolate enemies
        if (currentWave >= chocolateStartWave)
        {
            chocolatePercentage = Mathf.Clamp01(0.15f + (currentWave - chocolateStartWave) * 0.05f);
            cupidPercentage = 0.6f - chocolatePercentage * 0.5f; // Slightly reduce cupids
        }

        // Introduce diamond enemies
        if (currentWave >= diamondStartWave)
        {
            diamondPercentage = Mathf.Clamp01(0.15f + (currentWave - diamondStartWave) * 0.05f);
            cupidPercentage = 0.5f - (chocolatePercentage + diamondPercentage) * 0.3f;
        }

        // Cap percentages to make sense
        cupidPercentage = Mathf.Clamp01(cupidPercentage);

        // Calculate counts
        int cupidCount = Mathf.RoundToInt(totalEnemies * cupidPercentage);
        int chocolateCount = Mathf.RoundToInt(totalEnemies * chocolatePercentage);
        int diamondCount = totalEnemies - cupidCount - chocolateCount; // Remainder

        Debug.Log($"Wave {currentWave} Composition: {cupidCount} Cupids, {chocolateCount} Chocolates, {diamondCount} Diamonds");

        // Add enemies to list
        for (int i = 0; i < cupidCount; i++)
            composition.Add(corruptedCupidPrefab);

        for (int i = 0; i < chocolateCount; i++)
            composition.Add(stoneChocolatePrefab);

        for (int i = 0; i < diamondCount; i++)
            composition.Add(diamondCritterPrefab);

        // Shuffle for variety
        ShuffleList(composition);

        return composition;
    }

    IEnumerator SpawnEnemiesOverTime(List<GameObject> enemies)
    {
        int spawnedCount = 0;
        int maxSpawnsPerFrame = 1; // Safety limit
        
        foreach (GameObject enemyPrefab in enemies)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("[WAVE SYSTEM] Enemy prefab is NULL! Check your WaveManager settings.");
                continue;
            }

            Vector2 spawnPos = GetSpawnPosition();
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            if (enemy == null)
            {
                Debug.LogError("[WAVE SYSTEM] Failed to instantiate enemy!");
                continue;
            }

            // Scale enemy health based on wave
            ScaleEnemyHealth(enemy);

            // Track enemy
            activeEnemies.Add(enemy);
            enemiesRemainingInWave--;

            if (waveUI != null)
            {
                waveUI.UpdateEnemiesRemaining(activeEnemies.Count);
            }

            spawnedCount++;
            
            // Safety check - prevent spawning too many
            if (spawnedCount > 200)
            {
                Debug.LogError("[WAVE SYSTEM] SAFETY LIMIT! Stopped spawning after 200 enemies.");
                break;
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        Debug.Log($"[WAVE SYSTEM] Spawned {spawnedCount} enemies for wave {currentWave}");
    }

    void ScaleEnemyHealth(GameObject enemy)
    {
        Damageable damageable = enemy.GetComponent<Damageable>();
        if (damageable != null)
        {
            // Scale health exponentially
            float healthMultiplier = Mathf.Pow(healthScalingPerWave, currentWave - 1);
            damageable.maxHealth = Mathf.RoundToInt(damageable.maxHealth * healthMultiplier);
            damageable.currentHealth = damageable.maxHealth;

            Debug.Log($"Enemy spawned with {damageable.currentHealth} HP (Wave {currentWave} multiplier: {healthMultiplier:F2}x)");
        }
    }

    Vector2 GetSpawnPosition()
    {
        FindPlayer(); // Make sure we have player reference

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Use random spawn point
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return spawnPoint.position;
        }
        else
        {
            // Spawn in circle around player
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector2 spawnPos = (Vector2)playerTransform.position + randomDirection * spawnRadius;

            // Make sure not too close
            while (Vector2.Distance(spawnPos, playerTransform.position) < minDistanceFromPlayer)
            {
                randomDirection = Random.insideUnitCircle.normalized;
                spawnPos = (Vector2)playerTransform.position + randomDirection * spawnRadius;
            }

            return spawnPos;
        }
    }

    void CompleteWave()
    {
        waveInProgress = false;

        Debug.Log($"[WAVE {currentWave}] COMPLETE! Cupids saved: {cupidsSavedThisWave}");

        if (waveUI != null)
        {
            waveUI.ShowWaveComplete(currentWave, cupidsSavedThisWave);
        }

        // Start next wave
        StartCoroutine(StartNextWaveWithDelay(wavePrepTime));
    }

    // Call this from CupidManager when a cupid is converted
    public void OnCupidSaved()
    {
        cupidsSavedThisWave++;
        totalCupidsSaved++;

        Debug.Log($"Cupid saved! Total: {totalCupidsSaved}, This wave: {cupidsSavedThisWave}");

        if (waveUI != null)
        {
            waveUI.UpdateCupidsSaved(totalCupidsSaved);
        }
    }

    public void OnEnemyKilled()
    {
        totalEnemiesKilled++;

        if (waveUI != null)
        {
            waveUI.UpdateEnemiesRemaining(activeEnemies.Count);
        }
    }

    void FindPlayer()
    {
        if (playerTransform != null) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            SwitchCharacters sc = player.GetComponent<SwitchCharacters>();
            if (sc != null && sc.isActive)
            {
                playerTransform = player.transform;
                return;
            }
        }

        if (players.Length > 0)
            playerTransform = players[0].transform;
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Public method to get current wave stats
    public WaveStats GetCurrentStats()
    {
        return new WaveStats
        {
            waveNumber = currentWave,
            totalCupidsSaved = totalCupidsSaved,
            totalEnemiesKilled = totalEnemiesKilled,
            cupidsSavedThisWave = cupidsSavedThisWave
        };
    }
}

[System.Serializable]
public struct WaveStats
{
    public int waveNumber;
    public int totalCupidsSaved;
    public int totalEnemiesKilled;
    public int cupidsSavedThisWave;
}