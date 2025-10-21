using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BehaviorTracker : MonoBehaviour
{
    // Singleton instance
    public static BehaviorTracker Instance { get; private set; }

    // === TRACKING CONFIGURATION ===
    [Header("Tracking Settings")]
    [Tooltip("Enable behavior tracking")]
    public bool enableTracking = true;
    [Tooltip("Sample rate in seconds")]
    public float sampleRate = 1f;
    [Tooltip("Maximum history size")]
    public int maxHistorySize = 1000;
    [Tooltip("Enable debug logging")]
    public bool debugLogging = false;

    [Header("Analysis Settings")]
    [Tooltip("Window size for pattern detection (samples)")]
    public int patternWindowSize = 10;
    [Tooltip("Minimum pattern confidence (0-1)")]
    public float minPatternConfidence = 0.7f;

    // === BEHAVIOR DATA ===

    // Movement patterns
    private List<MovementSample> movementHistory = new List<MovementSample>();
    private Vector3 lastPosition;
    private float totalDistanceTraveled = 0f;
    private float averageSpeed = 0f;

    // Combat patterns
    private List<CombatEvent> combatHistory = new List<CombatEvent>();
    private int totalCombatEngagements = 0;
    private int totalMeleeAttacks = 0;
    private int totalRangedAttacks = 0;
    private float averageCombatDistance = 5f;
    private string preferredCombatStyle = "Melee";

    // Resource management patterns
    private List<ResourceEvent> resourceHistory = new List<ResourceEvent>();
    private float averageHealthThreshold = 50f; // When player heals
    private float averageHungerThreshold = 60f; // When player eats
    private float averageThirstThreshold = 70f; // When player drinks

    // Exploration patterns
    private List<Vector3> visitedLocations = new List<Vector3>();
    private Dictionary<string, int> locationTypeVisits = new Dictionary<string, int>();
    private float explorationRadius = 0f;
    private string preferredLocationType = "House";

    // Time management patterns
    private bool prefersDayActivity = true;
    private bool prefersSafeApproach = true;
    private float averageRestTime = 0f;

    // Risk assessment
    private float averageRiskTolerance = 0.5f; // 0 = very cautious, 1 = very aggressive
    private int dangerousEncounters = 0;
    private int retreatCount = 0;
    private int standFightCount = 0;

    // Sound behavior
    private int loudActionsCount = 0; // Gunshots, breaking things
    private int stealthActionsCount = 0; // Crouching, melee
    private bool prefersStealthApproach = false;

    // Sampling timer
    private float sampleTimer = 0f;

    // Player reference
    private Transform playerTransform;
    private PlayerStats playerStats;
    private PlayerCombat playerCombat;
    private PlayerController playerController;

    // === PATTERN DETECTION ===
    private Dictionary<string, float> detectedPatterns = new Dictionary<string, float>();

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
        // Find player and components
        FindPlayerComponents();

        // Initialize patterns dictionary
        InitializePatterns();

        Debug.Log("BehaviorTracker initialized");
    }

    void Update()
    {
        if (!enableTracking || playerTransform == null)
            return;

        // Sample behavior at intervals
        sampleTimer += Time.deltaTime;
        if (sampleTimer >= sampleRate)
        {
            sampleTimer = 0f;
            SampleBehavior();
        }

        // Analyze patterns periodically
        if (movementHistory.Count > 0 && movementHistory.Count % patternWindowSize == 0)
        {
            AnalyzePatterns();
        }
    }

    // === INITIALIZATION ===

    private void FindPlayerComponents()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerStats = player.GetComponent<PlayerStats>();
            playerCombat = player.GetComponent<PlayerCombat>();
            playerController = player.GetComponent<PlayerController>();
            lastPosition = playerTransform.position;
        }
    }

    private void InitializePatterns()
    {
        detectedPatterns["AggressiveCombat"] = 0f;
        detectedPatterns["DefensiveCombat"] = 0f;
        detectedPatterns["StealthApproach"] = 0f;
        detectedPatterns["RushApproach"] = 0f;
        detectedPatterns["ResourceConservative"] = 0f;
        detectedPatterns["ResourceLiberal"] = 0f;
        detectedPatterns["ExplorationFocused"] = 0f;
        detectedPatterns["CombatFocused"] = 0f;
        detectedPatterns["DayActive"] = 0f;
        detectedPatterns["NightActive"] = 0f;
        detectedPatterns["RiskTaker"] = 0f;
        detectedPatterns["Cautious"] = 0f;
    }

    // === BEHAVIOR SAMPLING ===

    private void SampleBehavior()
    {
        // Sample movement
        SampleMovement();

        // Sample combat state
        SampleCombat();

        // Sample resource usage
        SampleResources();

        // Sample exploration
        SampleExploration();

        // Sample time preferences
        SampleTimePreferences();
    }

    private void SampleMovement()
    {
        if (playerTransform == null) return;

        Vector3 currentPosition = playerTransform.position;
        float distance = Vector3.Distance(lastPosition, currentPosition);
        totalDistanceTraveled += distance;

        bool isSprinting = playerController != null && playerController.IsSprinting();
        bool isCrouching = playerController != null && playerController.IsCrouching();
        bool isMoving = playerController != null && playerController.IsMoving();

        MovementSample sample = new MovementSample
        {
            timestamp = Time.time,
            position = currentPosition,
            velocity = (currentPosition - lastPosition) / sampleRate,
            isSprinting = isSprinting,
            isCrouching = isCrouching,
            isMoving = isMoving
        };

        movementHistory.Add(sample);
        if (movementHistory.Count > maxHistorySize)
        {
            movementHistory.RemoveAt(0);
        }

        // Update stealth counter
        if (isCrouching)
        {
            stealthActionsCount++;
        }

        lastPosition = currentPosition;

        // Update average speed
        if (movementHistory.Count > 0)
        {
            averageSpeed = movementHistory.Average(s => s.velocity.magnitude);
        }
    }

    private void SampleCombat()
    {
        if (playerCombat == null) return;

        // Check if player is in combat (aiming or recently attacked)
        bool isInCombat = playerCombat.IsAiming();

        if (isInCombat)
        {
            // Find nearest zombie
            GameObject nearestZombie = FindNearestZombie();
            if (nearestZombie != null)
            {
                float distance = Vector3.Distance(playerTransform.position, nearestZombie.transform.position);

                // Track combat distance preference
                if (combatHistory.Count > 0)
                {
                    averageCombatDistance = combatHistory.Average(e => e.distance);
                }
            }
        }
    }

    private void SampleResources()
    {
        if (playerStats == null) return;

        // Track resource usage patterns (when player uses items at certain thresholds)
        float health = playerStats.health;
        float hunger = playerStats.hunger;
        float thirst = playerStats.thirst;

        // These would be updated when player actually uses items
        // For now, just sample current state
    }

    private void SampleExploration()
    {
        if (playerTransform == null) return;

        Vector3 currentPos = playerTransform.position;

        // Check if this is a new location (not visited recently)
        bool isNewLocation = true;
        foreach (Vector3 visitedPos in visitedLocations)
        {
            if (Vector3.Distance(currentPos, visitedPos) < 5f)
            {
                isNewLocation = false;
                break;
            }
        }

        if (isNewLocation)
        {
            visitedLocations.Add(currentPos);

            // Update exploration radius
            if (visitedLocations.Count > 1)
            {
                float maxDist = 0f;
                Vector3 center = GetExplorationCenter();
                foreach (Vector3 pos in visitedLocations)
                {
                    float dist = Vector3.Distance(center, pos);
                    if (dist > maxDist)
                        maxDist = dist;
                }
                explorationRadius = maxDist;
            }
        }
    }

    private void SampleTimePreferences()
    {
        if (TimeManager.Instance == null) return;

        bool isDaytime = TimeManager.Instance.IsDaytime();
        bool isMoving = playerController != null && playerController.IsMoving();

        // Track when player is most active
        if (isMoving)
        {
            if (isDaytime)
            {
                detectedPatterns["DayActive"] += 0.1f;
            }
            else
            {
                detectedPatterns["NightActive"] += 0.1f;
            }
        }

        // Normalize
        float total = detectedPatterns["DayActive"] + detectedPatterns["NightActive"];
        if (total > 0f)
        {
            prefersDayActivity = detectedPatterns["DayActive"] > detectedPatterns["NightActive"];
        }
    }

    // === EVENT TRACKING ===

    public void TrackCombatEvent(string combatType, float distance, bool successful)
    {
        if (!enableTracking) return;

        CombatEvent evt = new CombatEvent
        {
            timestamp = Time.time,
            combatType = combatType,
            distance = distance,
            successful = successful,
            healthBefore = playerStats != null ? playerStats.health : 100f
        };

        combatHistory.Add(evt);
        if (combatHistory.Count > maxHistorySize)
        {
            combatHistory.RemoveAt(0);
        }

        totalCombatEngagements++;

        if (combatType == "Melee")
        {
            totalMeleeAttacks++;
        }
        else if (combatType == "Ranged")
        {
            totalRangedAttacks++;
            loudActionsCount++; // Gunshots are loud
        }

        // Update preferred combat style
        if (totalMeleeAttacks > totalRangedAttacks)
        {
            preferredCombatStyle = "Melee";
        }
        else
        {
            preferredCombatStyle = "Ranged";
        }

        if (debugLogging)
        {
            Debug.Log($"Combat event: {combatType} at {distance:F1}m, Success: {successful}");
        }
    }

    public void TrackResourceUsage(string resourceType, float valueBefore, float valueAfter)
    {
        if (!enableTracking) return;

        ResourceEvent evt = new ResourceEvent
        {
            timestamp = Time.time,
            resourceType = resourceType,
            valueBefore = valueBefore,
            valueAfter = valueAfter
        };

        resourceHistory.Add(evt);
        if (resourceHistory.Count > maxHistorySize)
        {
            resourceHistory.RemoveAt(0);
        }

        // Update thresholds
        if (resourceType == "Health")
        {
            // Player healed at this health level
            if (resourceHistory.Count(r => r.resourceType == "Health") > 5)
            {
                averageHealthThreshold = resourceHistory
                    .Where(r => r.resourceType == "Health")
                    .Average(r => r.valueBefore);
            }
        }
        else if (resourceType == "Hunger")
        {
            if (resourceHistory.Count(r => r.resourceType == "Hunger") > 5)
            {
                averageHungerThreshold = resourceHistory
                    .Where(r => r.resourceType == "Hunger")
                    .Average(r => r.valueBefore);
            }
        }
        else if (resourceType == "Thirst")
        {
            if (resourceHistory.Count(r => r.resourceType == "Thirst") > 5)
            {
                averageThirstThreshold = resourceHistory
                    .Where(r => r.resourceType == "Thirst")
                    .Average(r => r.valueBefore);
            }
        }

        if (debugLogging)
        {
            Debug.Log($"Resource usage: {resourceType} from {valueBefore:F1} to {valueAfter:F1}");
        }
    }

    public void TrackLocationVisit(string locationType)
    {
        if (!enableTracking) return;

        if (!locationTypeVisits.ContainsKey(locationType))
        {
            locationTypeVisits[locationType] = 0;
        }
        locationTypeVisits[locationType]++;

        // Update preferred location type
        int maxVisits = 0;
        foreach (var kvp in locationTypeVisits)
        {
            if (kvp.Value > maxVisits)
            {
                maxVisits = kvp.Value;
                preferredLocationType = kvp.Key;
            }
        }
    }

    public void TrackDangerousEncounter(bool retreated)
    {
        if (!enableTracking) return;

        dangerousEncounters++;

        if (retreated)
        {
            retreatCount++;
            detectedPatterns["Cautious"] += 0.2f;
        }
        else
        {
            standFightCount++;
            detectedPatterns["RiskTaker"] += 0.2f;
        }

        // Update risk tolerance
        if (dangerousEncounters > 0)
        {
            averageRiskTolerance = (float)standFightCount / dangerousEncounters;
        }

        // Update safe approach preference
        prefersSafeApproach = retreatCount > standFightCount;
    }

    // === PATTERN ANALYSIS ===

    private void AnalyzePatterns()
    {
        // Analyze combat patterns
        AnalyzeCombatPatterns();

        // Analyze movement patterns
        AnalyzeMovementPatterns();

        // Analyze resource patterns
        AnalyzeResourcePatterns();

        // Calculate stealth preference
        if (loudActionsCount + stealthActionsCount > 0)
        {
            float stealthRatio = (float)stealthActionsCount / (loudActionsCount + stealthActionsCount);
            prefersStealthApproach = stealthRatio > 0.6f;
            detectedPatterns["StealthApproach"] = stealthRatio;
        }
    }

    private void AnalyzeCombatPatterns()
    {
        if (combatHistory.Count < 5) return;

        // Recent combat events (last 10)
        var recentCombat = combatHistory.TakeLast(10).ToList();

        // Aggressive vs Defensive
        float avgDistance = recentCombat.Average(e => e.distance);
        float successRate = recentCombat.Count(e => e.successful) / (float)recentCombat.Count;

        if (avgDistance < 5f)
        {
            detectedPatterns["AggressiveCombat"] += 0.1f;
        }
        else
        {
            detectedPatterns["DefensiveCombat"] += 0.1f;
        }

        // Normalize
        float totalCombatPattern = detectedPatterns["AggressiveCombat"] + detectedPatterns["DefensiveCombat"];
        if (totalCombatPattern > 0f)
        {
            detectedPatterns["AggressiveCombat"] /= totalCombatPattern;
            detectedPatterns["DefensiveCombat"] /= totalCombatPattern;
        }
    }

    private void AnalyzeMovementPatterns()
    {
        if (movementHistory.Count < patternWindowSize) return;

        var recentMovement = movementHistory.TakeLast(patternWindowSize).ToList();

        // Calculate average speed
        float avgSpeed = recentMovement.Average(s => s.velocity.magnitude);

        // Rush vs Cautious approach
        if (avgSpeed > 5f)
        {
            detectedPatterns["RushApproach"] += 0.1f;
        }
        else if (avgSpeed < 2f)
        {
            detectedPatterns["Cautious"] += 0.1f;
        }

        // Normalize
        float totalApproach = detectedPatterns["RushApproach"] + detectedPatterns["Cautious"];
        if (totalApproach > 0f)
        {
            detectedPatterns["RushApproach"] /= totalApproach;
            detectedPatterns["Cautious"] /= totalApproach;
        }
    }

    private void AnalyzeResourcePatterns()
    {
        if (resourceHistory.Count < 5) return;

        // Calculate resource usage frequency
        float timeSinceLastResourceUse = Time.time - resourceHistory.Last().timestamp;

        // Conservative vs Liberal resource use
        if (timeSinceLastResourceUse > 60f) // Uses resources sparingly
        {
            detectedPatterns["ResourceConservative"] += 0.1f;
        }
        else
        {
            detectedPatterns["ResourceLiberal"] += 0.1f;
        }
    }

    // === UTILITY METHODS ===

    private GameObject FindNearestZombie()
    {
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        GameObject nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject zombie in zombies)
        {
            float distance = Vector3.Distance(playerTransform.position, zombie.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = zombie;
            }
        }

        return nearest;
    }

    private Vector3 GetExplorationCenter()
    {
        if (visitedLocations.Count == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (Vector3 pos in visitedLocations)
        {
            sum += pos;
        }
        return sum / visitedLocations.Count;
    }

    // === PUBLIC GETTERS (for ML-Agents) ===

    public float GetAverageSpeed() => averageSpeed;
    public float GetTotalDistanceTraveled() => totalDistanceTraveled;
    public string GetPreferredCombatStyle() => preferredCombatStyle;
    public float GetAverageCombatDistance() => averageCombatDistance;
    public float GetCombatSuccessRate()
    {
        if (combatHistory.Count == 0) return 0f;
        return combatHistory.Count(e => e.successful) / (float)combatHistory.Count;
    }

    public float GetAverageHealthThreshold() => averageHealthThreshold;
    public float GetAverageHungerThreshold() => averageHungerThreshold;
    public float GetAverageThirstThreshold() => averageThirstThreshold;
    public string GetPreferredLocationType() => preferredLocationType;
    public float GetExplorationRadius() => explorationRadius;
    public bool PrefersDayActivity() => prefersDayActivity;
    public bool PrefersSafeApproach() => prefersSafeApproach;
    public float GetRiskTolerance() => averageRiskTolerance;
    public bool PrefersStealthApproach() => prefersStealthApproach;

    public float GetPatternConfidence(string patternName)
    {
        if (detectedPatterns.ContainsKey(patternName))
        {
            return Mathf.Clamp01(detectedPatterns[patternName]);
        }
        return 0f;
    }

    public Dictionary<string, float> GetAllPatterns()
    {
        return new Dictionary<string, float>(detectedPatterns);
    }

    // === SAVE/LOAD SUPPORT ===

    public BehaviorData GetBehaviorData()
    {
        return new BehaviorData
        {
            totalDistanceTraveled = totalDistanceTraveled,
            averageSpeed = averageSpeed,
            preferredCombatStyle = preferredCombatStyle,
            averageCombatDistance = averageCombatDistance,
            averageHealthThreshold = averageHealthThreshold,
            averageHungerThreshold = averageHungerThreshold,
            averageThirstThreshold = averageThirstThreshold,
            preferredLocationType = preferredLocationType,
            explorationRadius = explorationRadius,
            prefersDayActivity = prefersDayActivity,
            prefersSafeApproach = prefersSafeApproach,
            averageRiskTolerance = averageRiskTolerance,
            prefersStealthApproach = prefersStealthApproach,
            detectedPatterns = new Dictionary<string, float>(detectedPatterns)
        };
    }

    public void LoadBehaviorData(BehaviorData data)
    {
        if (data == null) return;

        totalDistanceTraveled = data.totalDistanceTraveled;
        averageSpeed = data.averageSpeed;
        preferredCombatStyle = data.preferredCombatStyle;
        averageCombatDistance = data.averageCombatDistance;
        averageHealthThreshold = data.averageHealthThreshold;
        averageHungerThreshold = data.averageHungerThreshold;
        averageThirstThreshold = data.averageThirstThreshold;
        preferredLocationType = data.preferredLocationType;
        explorationRadius = data.explorationRadius;
        prefersDayActivity = data.prefersDayActivity;
        prefersSafeApproach = data.prefersSafeApproach;
        averageRiskTolerance = data.averageRiskTolerance;
        prefersStealthApproach = data.prefersStealthApproach;
        detectedPatterns = new Dictionary<string, float>(data.detectedPatterns);

        Debug.Log("Behavior data loaded");
    }

    // === DEBUG ===

    [ContextMenu("Debug: Print Behavior Summary")]
    public void DebugPrintSummary()
    {
        Debug.Log("=== BEHAVIOR TRACKER SUMMARY ===");
        Debug.Log($"Total Distance: {totalDistanceTraveled:F1}m");
        Debug.Log($"Average Speed: {averageSpeed:F2} m/s");
        Debug.Log($"Combat Style: {preferredCombatStyle}");
        Debug.Log($"Combat Distance: {averageCombatDistance:F1}m");
        Debug.Log($"Health Threshold: {averageHealthThreshold:F1}%");
        Debug.Log($"Hunger Threshold: {averageHungerThreshold:F1}%");
        Debug.Log($"Preferred Location: {preferredLocationType}");
        Debug.Log($"Exploration Radius: {explorationRadius:F1}m");
        Debug.Log($"Day Active: {prefersDayActivity}");
        Debug.Log($"Safe Approach: {prefersSafeApproach}");
        Debug.Log($"Risk Tolerance: {averageRiskTolerance:F2}");
        Debug.Log($"Stealth Approach: {prefersStealthApproach}");
        Debug.Log("\nDetected Patterns:");
        foreach (var pattern in detectedPatterns)
        {
            Debug.Log($"  {pattern.Key}: {pattern.Value:F2}");
        }
    }

    [ContextMenu("Debug: Clear History")]
    public void DebugClearHistory()
    {
        movementHistory.Clear();
        combatHistory.Clear();
        resourceHistory.Clear();
        visitedLocations.Clear();
        locationTypeVisits.Clear();
        detectedPatterns.Clear();
        InitializePatterns();
        Debug.Log("Behavior history cleared");
    }
}

// === DATA CLASSES ===

[System.Serializable]
public class MovementSample
{
    public float timestamp;
    public Vector3 position;
    public Vector3 velocity;
    public bool isSprinting;
    public bool isCrouching;
    public bool isMoving;
}

[System.Serializable]
public class CombatEvent
{
    public float timestamp;
    public string combatType; // "Melee" or "Ranged"
    public float distance;
    public bool successful;
    public float healthBefore;
}

[System.Serializable]
public class ResourceEvent
{
    public float timestamp;
    public string resourceType; // "Health", "Hunger", "Thirst", etc.
    public float valueBefore;
    public float valueAfter;
}

[System.Serializable]
public class BehaviorData
{
    public float totalDistanceTraveled;
    public float averageSpeed;
    public string preferredCombatStyle;
    public float averageCombatDistance;
    public float averageHealthThreshold;
    public float averageHungerThreshold;
    public float averageThirstThreshold;
    public string preferredLocationType;
    public float explorationRadius;
    public bool prefersDayActivity;
    public bool prefersSafeApproach;
    public float averageRiskTolerance;
    public bool prefersStealthApproach;
    public Dictionary<string, float> detectedPatterns;
}
