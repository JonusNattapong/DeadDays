using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieSpawner : MonoBehaviour
{
    // Singleton instance
    public static ZombieSpawner Instance { get; private set; }

    // === SPAWNER CONFIGURATION ===
    [Header("Spawn Settings")]
    [Tooltip("Maximum zombies alive at once")]
    public int maxZombiesAlive = 50;
    [Tooltip("Minimum distance from player to spawn")]
    public float minSpawnDistance = 20f;
    [Tooltip("Maximum distance from player to spawn")]
    public float maxSpawnDistance = 40f;
    [Tooltip("Base spawn interval in seconds")]
    public float baseSpawnInterval = 5f;
    [Tooltip("Enable wave system")]
    public bool useWaveSystem = true;

    [Header("Zombie Prefabs")]
    [Tooltip("Array of zombie prefabs by type")]
    public GameObject[] zombiePrefabs;
    public GameObject walkerPrefab;
    public GameObject runnerPrefab;
    public GameObject brutePrefab;
    public GameObject crawlerPrefab;
    public GameObject spitterPrefab;
    public GameObject bloaterPrefab;
    public GameObject screamerPrefab;

    [Header("Spawn Probabilities (0-1)")]
    [Range(0f, 1f)] public float walkerProbability = 0.5f;
    [Range(0f, 1f)] public float runnerProbability = 0.2f;
    [Range(0f, 1f)] public float bruteProbability = 0.1f;
    [Range(0f, 1f)] public float crawlerProbability = 0.1f;
    [Range(0f, 1f)] public float spitterProbability = 0.05f;
    [Range(0f, 1f)] public float bloaterProbability = 0.03f;
    [Range(0f, 1f)] public float screamerProbability = 0.02f;

    [Header("Wave System")]
    [Tooltip("Zombies per wave")]
    public int zombiesPerWave = 10;
    [Tooltip("Wave interval in seconds")]
    public float waveDuration = 120f; // 2 minutes
    [Tooltip("Time between waves")]
    public float timeBetweenWaves = 60f; // 1 minute
    [Tooltip("Wave difficulty increase per wave")]
    public float waveMultiplier = 1.2f;

    [Header("Difficulty Scaling")]
    [Tooltip("Enable difficulty scaling over time")]
    public bool enableDifficultyScaling = true;
    [Tooltip("Day multiplier for difficulty")]
    public float dayDifficultyMultiplier = 1.1f;
    [Tooltip("Player level multiplier")]
    public float playerLevelMultiplier = 1.05f;

    [Header("Spawn Points")]
    [Tooltip("Use predefined spawn points instead of random")]
    public bool usePredefinedSpawns = false;
    [Tooltip("Array of spawn point transforms")]
    public Transform[] spawnPoints;

    [Header("Object Pooling")]
    [Tooltip("Enable object pooling for performance")]
    public bool useObjectPooling = true;
    [Tooltip("Initial pool size per zombie type")]
    public int initialPoolSize = 10;

    // === RUNTIME VARIABLES ===

    // Active zombies tracking
    private List<GameObject> activeZombies = new List<GameObject>();
    private int totalZombiesSpawned = 0;
    private int totalZombiesKilled = 0;

    // Wave system
    private int currentWave = 0;
    private int zombiesSpawnedThisWave = 0;
    private int zombiesKilledThisWave = 0;
    private bool isWaveActive = false;
    private float waveTimer = 0f;
    private float betweenWaveTimer = 0f;

    // Spawn timing
    private float spawnTimer = 0f;
    private float currentSpawnInterval;

    // Difficulty
    private float currentDifficulty = 1f;

    // Object pooling
    private Dictionary<ZombieType, Queue<GameObject>> zombiePools = new Dictionary<ZombieType, Queue<GameObject>>();
    private Transform poolParent;

    // Player reference
    private Transform playerTransform;

    // Spawn area
    private Vector2 spawnCenter;
    private float spawnRadius = 100f;

    void Awake()
    {
        // Ensure singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Initialize
        currentSpawnInterval = baseSpawnInterval;
        spawnCenter = transform.position;

        // Find player
        FindPlayer();

        // Initialize object pools
        if (useObjectPooling)
        {
            InitializeObjectPools();
        }

        // Build zombie prefab array if not set
        BuildZombiePrefabArray();

        // Start wave system if enabled
        if (useWaveSystem)
        {
            StartCoroutine(WaveSystemCoroutine());
        }

        Debug.Log("ZombieSpawner initialized");
    }

    void Update()
    {
        // Update player reference
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        // Update difficulty
        if (enableDifficultyScaling)
        {
            UpdateDifficulty();
        }

        // Cleanup dead zombies
        CleanupDeadZombies();

        // Continuous spawning (if not using wave system)
        if (!useWaveSystem)
        {
            ContinuousSpawning();
        }
    }

    // === PLAYER REFERENCE ===

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    // === OBJECT POOLING ===

    private void InitializeObjectPools()
    {
        // Create pool parent
        poolParent = new GameObject("ZombiePool").transform;
        poolParent.SetParent(transform);

        // Initialize pools for each zombie type
        foreach (ZombieType type in System.Enum.GetValues(typeof(ZombieType)))
        {
            zombiePools[type] = new Queue<GameObject>();

            GameObject prefab = GetZombiePrefab(type);
            if (prefab != null)
            {
                for (int i = 0; i < initialPoolSize; i++)
                {
                    GameObject zombie = Instantiate(prefab, poolParent);
                    zombie.SetActive(false);
                    zombiePools[type].Enqueue(zombie);
                }
            }
        }

        Debug.Log($"Initialized object pools with {initialPoolSize} zombies per type");
    }

    private GameObject GetPooledZombie(ZombieType type)
    {
        if (!useObjectPooling || !zombiePools.ContainsKey(type))
        {
            return null;
        }

        if (zombiePools[type].Count > 0)
        {
            GameObject zombie = zombiePools[type].Dequeue();
            zombie.SetActive(true);
            return zombie;
        }
        else
        {
            // Pool exhausted, create new instance
            GameObject prefab = GetZombiePrefab(type);
            if (prefab != null)
            {
                GameObject zombie = Instantiate(prefab, poolParent);
                return zombie;
            }
        }

        return null;
    }

    private void ReturnToPool(GameObject zombie, ZombieType type)
    {
        if (!useObjectPooling || !zombiePools.ContainsKey(type))
        {
            Destroy(zombie);
            return;
        }

        zombie.SetActive(false);
        zombie.transform.SetParent(poolParent);
        zombiePools[type].Enqueue(zombie);
    }

    // === SPAWNING ===

    public void SpawnZombie(Vector3 position, ZombieType type = ZombieType.Walker)
    {
        // Check max limit
        if (activeZombies.Count >= maxZombiesAlive)
        {
            return;
        }

        GameObject zombie;

        // Get from pool or instantiate
        if (useObjectPooling)
        {
            zombie = GetPooledZombie(type);
            if (zombie == null) return;

            zombie.transform.position = position;
            zombie.transform.rotation = Quaternion.identity;

            // Reset zombie AI
            ZombieAI ai = zombie.GetComponent<ZombieAI>();
            if (ai != null)
            {
                ai.health = ai.maxHealth;
                // Reset other properties if needed
            }
        }
        else
        {
            GameObject prefab = GetZombiePrefab(type);
            if (prefab == null) return;

            zombie = Instantiate(prefab, position, Quaternion.identity);
        }

        // Apply difficulty scaling
        ApplyDifficultyToZombie(zombie);

        // Track active zombie
        activeZombies.Add(zombie);
        totalZombiesSpawned++;
        zombiesSpawnedThisWave++;

        Debug.Log($"Spawned {type} zombie at {position}. Total active: {activeZombies.Count}");
    }

    public void SpawnZombieNearPlayer(ZombieType type = ZombieType.Walker)
    {
        if (playerTransform == null) return;

        Vector3 spawnPos = GetRandomSpawnPosition();
        SpawnZombie(spawnPos, type);
    }

    public void SpawnRandomZombie()
    {
        ZombieType type = SelectRandomZombieType();
        SpawnZombieNearPlayer(type);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (playerTransform == null)
        {
            return Vector3.zero;
        }

        // Use predefined spawn points if available
        if (usePredefinedSpawns && spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return spawnPoint.position;
        }

        // Random position around player
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance,
            0f
        );

        Vector3 spawnPos = playerTransform.position + offset;

        // TODO: Check if position is valid (not inside walls)
        // For now, just return the position

        return spawnPos;
    }

    private ZombieType SelectRandomZombieType()
    {
        float random = Random.value;
        float cumulative = 0f;

        // Normalize probabilities
        float total = walkerProbability + runnerProbability + bruteProbability +
                     crawlerProbability + spitterProbability + bloaterProbability +
                     screamerProbability;

        if (total <= 0f) return ZombieType.Walker;

        // Select based on cumulative probability
        cumulative += walkerProbability / total;
        if (random < cumulative) return ZombieType.Walker;

        cumulative += runnerProbability / total;
        if (random < cumulative) return ZombieType.Runner;

        cumulative += bruteProbability / total;
        if (random < cumulative) return ZombieType.Brute;

        cumulative += crawlerProbability / total;
        if (random < cumulative) return ZombieType.Crawler;

        cumulative += spitterProbability / total;
        if (random < cumulative) return ZombieType.Spitter;

        cumulative += bloaterProbability / total;
        if (random < cumulative) return ZombieType.Bloater;

        return ZombieType.Screamer;
    }

    private GameObject GetZombiePrefab(ZombieType type)
    {
        switch (type)
        {
            case ZombieType.Walker:
                return walkerPrefab;
            case ZombieType.Runner:
                return runnerPrefab;
            case ZombieType.Brute:
                return brutePrefab;
            case ZombieType.Crawler:
                return crawlerPrefab;
            case ZombieType.Spitter:
                return spitterPrefab;
            case ZombieType.Bloater:
                return bloaterPrefab;
            case ZombieType.Screamer:
                return screamerPrefab;
            default:
                return walkerPrefab;
        }
    }

    private void BuildZombiePrefabArray()
    {
        // Build array from individual prefab references if array is empty
        if (zombiePrefabs == null || zombiePrefabs.Length == 0)
        {
            List<GameObject> prefabs = new List<GameObject>();
            if (walkerPrefab != null) prefabs.Add(walkerPrefab);
            if (runnerPrefab != null) prefabs.Add(runnerPrefab);
            if (brutePrefab != null) prefabs.Add(brutePrefab);
            if (crawlerPrefab != null) prefabs.Add(crawlerPrefab);
            if (spitterPrefab != null) prefabs.Add(spitterPrefab);
            if (bloaterPrefab != null) prefabs.Add(bloaterPrefab);
            if (screamerPrefab != null) prefabs.Add(screamerPrefab);

            zombiePrefabs = prefabs.ToArray();
        }
    }

    // === CONTINUOUS SPAWNING ===

    private void ContinuousSpawning()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= currentSpawnInterval)
        {
            spawnTimer = 0f;

            // Spawn if under limit
            if (activeZombies.Count < maxZombiesAlive)
            {
                SpawnRandomZombie();
            }
        }
    }

    // === WAVE SYSTEM ===

    private IEnumerator WaveSystemCoroutine()
    {
        yield return new WaitForSeconds(5f); // Initial delay

        while (true)
        {
            // Start new wave
            StartWave();

            // Wait for wave to complete
            yield return new WaitForSeconds(waveDuration);

            // End wave
            EndWave();

            // Wait between waves
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private void StartWave()
    {
        currentWave++;
        isWaveActive = true;
        zombiesSpawnedThisWave = 0;
        zombiesKilledThisWave = 0;
        waveTimer = 0f;

        // Calculate zombies for this wave
        int zombiesToSpawn = Mathf.RoundToInt(zombiesPerWave * Mathf.Pow(waveMultiplier, currentWave - 1));

        Debug.Log($"=== WAVE {currentWave} STARTED ===");
        Debug.Log($"Zombies to spawn: {zombiesToSpawn}");
        Debug.Log($"Duration: {waveDuration}s");

        // Start spawning coroutine
        StartCoroutine(SpawnWaveZombies(zombiesToSpawn));

        // TODO: Trigger UI event for wave start
    }

    private IEnumerator SpawnWaveZombies(int count)
    {
        float spawnDelay = waveDuration / count;

        for (int i = 0; i < count; i++)
        {
            if (!isWaveActive) break;

            // Spawn zombie
            if (activeZombies.Count < maxZombiesAlive)
            {
                SpawnRandomZombie();
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void EndWave()
    {
        isWaveActive = false;

        Debug.Log($"=== WAVE {currentWave} ENDED ===");
        Debug.Log($"Zombies spawned: {zombiesSpawnedThisWave}");
        Debug.Log($"Zombies killed: {zombiesKilledThisWave}");
        Debug.Log($"Zombies remaining: {activeZombies.Count}");

        // Reward player
        // TODO: Give XP, items, etc.

        // TODO: Trigger UI event for wave end
    }

    // === DIFFICULTY SCALING ===

    private void UpdateDifficulty()
    {
        float baseDifficulty = 1f;

        // Scale with day count
        if (TimeManager.Instance != null)
        {
            int currentDay = TimeManager.Instance.GetCurrentDay();
            baseDifficulty *= Mathf.Pow(dayDifficultyMultiplier, currentDay - 1);
        }

        // Scale with time of day (harder at night)
        if (TimeManager.Instance != null && TimeManager.Instance.IsNight())
        {
            baseDifficulty *= 1.5f;
        }

        // Scale with player level (average of all skills)
        if (SkillSystem.Instance != null)
        {
            float averageLevel = 0f;
            string[] skills = SkillSystem.Instance.GetAllSkillNames();
            foreach (string skill in skills)
            {
                averageLevel += SkillSystem.Instance.GetSkillLevel(skill);
            }
            averageLevel /= skills.Length;
            baseDifficulty *= Mathf.Pow(playerLevelMultiplier, averageLevel - 1);
        }

        // Scale with wave number
        if (useWaveSystem && currentWave > 0)
        {
            baseDifficulty *= Mathf.Pow(waveMultiplier, currentWave - 1);
        }

        currentDifficulty = baseDifficulty;

        // Adjust spawn interval based on difficulty
        currentSpawnInterval = baseSpawnInterval / Mathf.Sqrt(currentDifficulty);
        currentSpawnInterval = Mathf.Clamp(currentSpawnInterval, 1f, baseSpawnInterval);
    }

    private void ApplyDifficultyToZombie(GameObject zombie)
    {
        ZombieAI ai = zombie.GetComponent<ZombieAI>();
        if (ai == null) return;

        // Scale health
        ai.maxHealth *= Mathf.Sqrt(currentDifficulty);
        ai.health = ai.maxHealth;

        // Scale damage slightly
        ai.attackDamage *= Mathf.Pow(currentDifficulty, 0.3f);

        // Scale speed slightly
        ai.moveSpeed *= Mathf.Pow(currentDifficulty, 0.2f);
    }

    // === CLEANUP ===

    private void CleanupDeadZombies()
    {
        for (int i = activeZombies.Count - 1; i >= 0; i--)
        {
            GameObject zombie = activeZombies[i];

            if (zombie == null)
            {
                activeZombies.RemoveAt(i);
                totalZombiesKilled++;
                zombiesKilledThisWave++;
                continue;
            }

            // Check if zombie is dead
            ZombieAI ai = zombie.GetComponent<ZombieAI>();
            if (ai != null && ai.health <= 0)
            {
                activeZombies.RemoveAt(i);
                totalZombiesKilled++;
                zombiesKilledThisWave++;

                // Return to pool or destroy after delay
                if (useObjectPooling)
                {
                    StartCoroutine(ReturnToPoolAfterDelay(zombie, ai.zombieType, 3f));
                }
            }
        }
    }

    private IEnumerator ReturnToPoolAfterDelay(GameObject zombie, ZombieType type, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(zombie, type);
    }

    // === PUBLIC METHODS ===

    public void ClearAllZombies()
    {
        foreach (GameObject zombie in activeZombies)
        {
            if (zombie != null)
            {
                Destroy(zombie);
            }
        }
        activeZombies.Clear();
        Debug.Log("Cleared all zombies");
    }

    public void SpawnWave(int zombieCount)
    {
        for (int i = 0; i < zombieCount; i++)
        {
            SpawnRandomZombie();
        }
    }

    public void SetSpawnRate(float rate)
    {
        baseSpawnInterval = rate;
        currentSpawnInterval = rate;
    }

    public void SetDifficulty(float difficulty)
    {
        currentDifficulty = difficulty;
    }

    // === GETTERS ===

    public int GetActiveZombieCount() => activeZombies.Count;
    public int GetTotalZombiesSpawned() => totalZombiesSpawned;
    public int GetTotalZombiesKilled() => totalZombiesKilled;
    public int GetCurrentWave() => currentWave;
    public bool IsWaveActive() => isWaveActive;
    public float GetCurrentDifficulty() => currentDifficulty;
    public List<GameObject> GetActiveZombies() => new List<GameObject>(activeZombies);

    // === DEBUG ===

    [ContextMenu("Debug: Spawn 10 Zombies")]
    public void DebugSpawn10()
    {
        for (int i = 0; i < 10; i++)
        {
            SpawnRandomZombie();
        }
    }

    [ContextMenu("Debug: Clear All Zombies")]
    public void DebugClearAll()
    {
        ClearAllZombies();
    }

    [ContextMenu("Debug: Force Start Wave")]
    public void DebugStartWave()
    {
        if (!isWaveActive)
        {
            StartWave();
        }
    }

    [ContextMenu("Debug: Print Stats")]
    public void DebugPrintStats()
    {
        Debug.Log("=== ZOMBIE SPAWNER STATS ===");
        Debug.Log($"Active Zombies: {activeZombies.Count}/{maxZombiesAlive}");
        Debug.Log($"Total Spawned: {totalZombiesSpawned}");
        Debug.Log($"Total Killed: {totalZombiesKilled}");
        Debug.Log($"Current Wave: {currentWave}");
        Debug.Log($"Wave Active: {isWaveActive}");
        Debug.Log($"Current Difficulty: {currentDifficulty:F2}");
        Debug.Log($"Spawn Interval: {currentSpawnInterval:F2}s");
    }

    void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;

        // Draw spawn range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerTransform.position, minSpawnDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerTransform.position, maxSpawnDistance);

        // Draw spawn points if using predefined spawns
        if (usePredefinedSpawns && spawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 2f);
                }
            }
        }
    }
}
