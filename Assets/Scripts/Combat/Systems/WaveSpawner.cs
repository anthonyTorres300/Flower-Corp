using UnityEngine;
using System.Collections;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    // Added CHOOSING_WEAPON state
    public enum SpawnState { SPAWNING, WAITING, COUNTING, CHOOSING_WEAPON };

    [Header("References")]
    public WeaponSelection weaponSelectionScript; // **NEW: Drag your WeaponSelection object here**
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public TextMeshProUGUI statusText;

    [Header("Wave Settings")]
    public float timeBetweenWaves = 5f;
    public float waveCountdown;
    public int initialEnemies = 5;
    public float difficultyMultiplier = 1.2f;

    private int currentWave = 0;
    private int enemiesToSpawn;
    private SpawnState state = SpawnState.COUNTING;
    private float searchCountdown = 1f;

    void Start()
    {
        waveCountdown = timeBetweenWaves;
        enemiesToSpawn = initialEnemies;
        UpdateStatusText("Get Ready!");
    }

    void Update()
    {
        // If we are choosing a weapon, do nothing (wait for player)
        if (state == SpawnState.CHOOSING_WEAPON) return;

        if (state == SpawnState.WAITING)
        {
            if (!EnemyIsAlive())
            {
                WaveCompleted();
            }
            else
            {
                return;
            }
        }

        if (waveCountdown <= 0)
        {
            if (state != SpawnState.SPAWNING)
            {
                StartCoroutine(SpawnWave());
            }
        }
        else
        {
            waveCountdown -= Time.deltaTime;
            
            if(state == SpawnState.COUNTING)
            {
                 statusText.text = $"Next Wave: {Mathf.Round(waveCountdown)}";
            }
        }
    }

    void WaveCompleted()
    {
        Debug.Log("Wave Finished. Opening Weapon Selection...");

        // 1. Stop Spawning Logic
        state = SpawnState.CHOOSING_WEAPON;
        
        // 2. Clear text or show "Choose!"
        UpdateStatusText("Choose Weapon!");

        // 3. Increase Difficulty variables now (so they are ready for next round)
        currentWave++;
        enemiesToSpawn = Mathf.RoundToInt(enemiesToSpawn * difficultyMultiplier);

        // 4. Open the UI
        if(weaponSelectionScript != null)
        {
            weaponSelectionScript.ShowSelection();
        }
        else
        {
            // Fallback if you forgot to assign the script
            OnWeaponSelected(); 
        }
    }

    // This is called by WeaponSelection.cs when the button is clicked
    public void OnWeaponSelected()
    {
        // Start the countdown for the next wave
        state = SpawnState.COUNTING;
        waveCountdown = timeBetweenWaves;
    }

    bool EnemyIsAlive()
    {
        searchCountdown -= Time.deltaTime;
        if (searchCountdown <= 0f)
        {
            searchCountdown = 1f;
            if (GameObject.FindGameObjectWithTag("Enemy") == null)
            {
                return false;
            }
        }
        return true;
    }

    IEnumerator SpawnWave()
    {
        state = SpawnState.SPAWNING;
        UpdateStatusText($"Wave {currentWave + 1}");

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.5f);
        }

        state = SpawnState.WAITING;
    }

    void SpawnEnemy()
    {
        if(enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        Transform _sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        int randomEnemyIndex = Random.Range(0, enemyPrefabs.Length);
        Instantiate(enemyPrefabs[randomEnemyIndex], _sp.position, _sp.rotation);
    }

    void UpdateStatusText(string message)
    {
        if(statusText != null) statusText.text = message;
    }
}