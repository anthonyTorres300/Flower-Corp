using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public List<EnemyGroup> enemyGroups;
        public float spawnInterval = 1f;
        public float waveDelay = 5f;
    }

    [System.Serializable]
    public class EnemyGroup
    {
        public GameObject enemyPrefab;
        public int count = 1;
        public float spawnDelay = 0f;
        public DebuffType debuffType = DebuffType.None;
    }

    [Header("Wave Configuration")]
    public List<Wave> waves;
    public int currentWaveIndex = 0;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints; // manual spawn points
    public float spawnRadius = 1f;
    public bool useProceduralSpawnPoints = true; // use map generator spawn points

    [Header("State")]
    public bool autoStart = false;
    public bool waveInProgress = false;

    private int enemiesRemainingInWave = 0;
    private ProceduralMapGenerator mapGenerator;
    private List<Vector3> proceduralSpawnPoints;

    void Start()
    {
        // find map generator if using procedural spawn points
        if (useProceduralSpawnPoints)
        {
            mapGenerator = FindObjectOfType<ProceduralMapGenerator>();
            if (mapGenerator != null)
            {
                // wait a frame for map to generate
                StartCoroutine(InitializeSpawnPoints());
            }
        }

        if (autoStart)
        {
            StartCoroutine(StartNextWave());
        }
    }

    IEnumerator InitializeSpawnPoints()
    {
        yield return new WaitForEndOfFrame();
        proceduralSpawnPoints = mapGenerator.GetSpawnPoints();
    }

    void Update()
    {
        // manually start wave with space
        if (Input.GetKeyDown(KeyCode.Space) && !waveInProgress)
        {
            StartCoroutine(StartNextWave());
        }
    }

    public IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            yield break;
        }

        waveInProgress = true;
        Wave currentWave = waves[currentWaveIndex];

        enemiesRemainingInWave = 0;

        foreach (EnemyGroup group in currentWave.enemyGroups)
        {
            if (group.spawnDelay > 0f)
            {
                yield return new WaitForSeconds(group.spawnDelay);
            }

            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyPrefab, group.debuffType);
                enemiesRemainingInWave++;

                if (i < group.count - 1)
                {
                    yield return new WaitForSeconds(currentWave.spawnInterval);
                }
            }
        }

        // wait for all enemies to die
        while (enemiesRemainingInWave > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        currentWaveIndex++;
        waveInProgress = false;

        // wait before next wave
        if (currentWaveIndex < waves.Count)
        {
            yield return new WaitForSeconds(currentWave.waveDelay);
            if (autoStart)
            {
                StartCoroutine(StartNextWave());
            }
        }
    }

    void SpawnEnemy(GameObject enemyPrefab, DebuffType debuffType)
    {
        if (enemyPrefab == null) return;

        Vector3 spawnPos = GetRandomSpawnPosition();

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // assign debuff type for cupids
        CorruptedCupid cupid = enemy.GetComponent<CorruptedCupid>();
        if (cupid != null)
        {
            cupid.debuffType = debuffType;
        }

        // track enemy death
        Damageable damageable = enemy.GetComponent<Damageable>();
        if (damageable != null)
        {
            damageable.onDeath.AddListener(() => OnEnemyKilled());
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        // use procedural spawn points if available
        if (useProceduralSpawnPoints && proceduralSpawnPoints != null && proceduralSpawnPoints.Count > 0)
        {
            Vector3 basePos = proceduralSpawnPoints[Random.Range(0, proceduralSpawnPoints.Count)];
            return basePos + (Vector3)Random.insideUnitCircle * spawnRadius;
        }
        // fallback to manual spawn points
        else if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return spawnPoint.position + (Vector3)Random.insideUnitCircle * spawnRadius;
        }

        // last resort: spawn near origin
        return (Vector3)Random.insideUnitCircle * 10f;
    }

    void OnEnemyKilled()
    {
        enemiesRemainingInWave--;
    }

    public void TriggerNextWave()
    {
        if (!waveInProgress)
        {
            StartCoroutine(StartNextWave());
        }
    }
}