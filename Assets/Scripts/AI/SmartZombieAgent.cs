using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class SmartZombieAgent : Agent
{
    // === COMPONENT REFERENCES ===
    [Header("Components")]
    public ZombieAI zombieAI;
    public Rigidbody2D rb;
    public Transform playerTransform;

    [Header("ML Settings")]
    [Tooltip("Observation radius")]
    public float observationRadius = 20f;
    [Tooltip("Number of raycasts for perception")]
    public int raycastCount = 16;
    [Tooltip("Raycast distance")]
    public float raycastDistance = 15f;
    [Tooltip("Reward for surviving (per step)")]
    public float survivalReward = 0.001f;
    [Tooltip("Reward for damaging player")]
    public float damageReward = 1.0f;
    [Tooltip("Penalty for taking damage")]
    public float damagePenalty = -0.5f;
    [Tooltip("Reward for getting closer to player")]
    public float approachReward = 0.01f;
    [Tooltip("Penalty for death")]
    public float deathPenalty = -1.0f;

    [Header("Training Settings")]
    [Tooltip("Use heuristic for testing")]
    public bool useHeuristic = false;
    [Tooltip("Training mode (faster time scale)")]
    public bool trainingMode = false;

    // === RUNTIME VARIABLES ===
    private float previousHealth;
    private float previousDistanceToPlayer;
    private Vector3 spawnPosition;
    private int stepCount = 0;
    private float episodeReward = 0f;
    private bool hasDealtDamage = false;

    // Behavior tracking
    private float timeAlive = 0f;
    private float totalDamageDealt = 0f;
    private float totalDamageTaken = 0f;
    private int attackAttempts = 0;
    private int successfulAttacks = 0;

    // Player behavior data (from BehaviorTracker)
    private string playerCombatStyle = "Melee";
    private float playerRiskTolerance = 0.5f;
    private bool playerPrefersDay = true;
    private bool playerPrefersStealth = false;

    void Awake()
    {
        // Get components
        if (zombieAI == null)
            zombieAI = GetComponent<ZombieAI>();
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        spawnPosition = transform.position;
    }

    void Start()
    {
        // Find player
        FindPlayer();

        // Initialize
        previousHealth = zombieAI != null ? zombieAI.health : 100f;
        if (playerTransform != null)
        {
            previousDistanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        }

        // Load player behavior patterns
        LoadPlayerBehavior();
    }

    void Update()
    {
        timeAlive += Time.deltaTime;
    }

    // === ML-AGENTS CORE METHODS ===

    public override void OnEpisodeBegin()
    {
        // Reset zombie to initial state
        if (zombieAI != null)
        {
            zombieAI.health = zombieAI.maxHealth;
        }

        // Reset position (random spawn point)
        Vector3 randomOffset = Random.insideUnitCircle * 10f;
        transform.position = spawnPosition + randomOffset;

        // Reset velocity
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Reset tracking
        previousHealth = zombieAI != null ? zombieAI.health : 100f;
        episodeReward = 0f;
        stepCount = 0;
        hasDealtDamage = false;
        timeAlive = 0f;
        totalDamageDealt = 0f;
        totalDamageTaken = 0f;
        attackAttempts = 0;
        successfulAttacks = 0;

        // Find player if lost
        if (playerTransform == null)
        {
            FindPlayer();
        }

        // Update player behavior
        LoadPlayerBehavior();

        if (playerTransform != null)
        {
            previousDistanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (zombieAI == null || playerTransform == null)
        {
            // Add default observations (all zeros)
            for (int i = 0; i < GetObservationSize(); i++)
            {
                sensor.AddObservation(0f);
            }
            return;
        }

        // === SELF STATE (7 observations) ===
        sensor.AddObservation(zombieAI.health / zombieAI.maxHealth); // Normalized health
        sensor.AddObservation(transform.position.x / 100f); // Normalized position
        sensor.AddObservation(transform.position.y / 100f);
        sensor.AddObservation(rb != null ? rb.velocity.x / 10f : 0f); // Normalized velocity
        sensor.AddObservation(rb != null ? rb.velocity.y / 10f : 0f);
        sensor.AddObservation((int)zombieAI.zombieType / 7f); // Zombie type (normalized)
        sensor.AddObservation(zombieAI.moveSpeed / 10f); // Speed capability

        // === PLAYER STATE (11 observations) ===
        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        sensor.AddObservation(dirToPlayer.x); // Direction to player
        sensor.AddObservation(dirToPlayer.y);
        sensor.AddObservation(distToPlayer / observationRadius); // Normalized distance
        sensor.AddObservation(playerTransform.position.x / 100f); // Player position
        sensor.AddObservation(playerTransform.position.y / 100f);

        // Player stats (if available)
        if (PlayerStats.Instance != null)
        {
            sensor.AddObservation(PlayerStats.Instance.health / 100f);
            sensor.AddObservation(PlayerStats.Instance.GetMovementSpeedModifier());
            sensor.AddObservation(PlayerStats.Instance.panic / 100f);
        }
        else
        {
            sensor.AddObservation(1f); // Assume full health
            sensor.AddObservation(1f); // Normal speed
            sensor.AddObservation(0f); // No panic
        }

        // Player combat state
        if (PlayerCombat.Instance != null)
        {
            sensor.AddObservation(PlayerCombat.Instance.IsAiming() ? 1f : 0f);
            sensor.AddObservation(PlayerCombat.Instance.GetCombatMode() == CombatMode.Ranged ? 1f : 0f);
            sensor.AddObservation(PlayerCombat.Instance.GetCurrentAmmo() / (float)PlayerCombat.Instance.GetMagazineSize());
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(1f);
        }

        // === ENVIRONMENT STATE (5 observations) ===
        if (TimeManager.Instance != null)
        {
            sensor.AddObservation(TimeManager.Instance.GetCurrentHour() / 24f); // Time of day
            sensor.AddObservation(TimeManager.Instance.IsDaytime() ? 1f : 0f); // Day/night
            sensor.AddObservation(TimeManager.Instance.GetCurrentDay() / 100f); // Game day
        }
        else
        {
            sensor.AddObservation(0.5f);
            sensor.AddObservation(1f);
            sensor.AddObservation(0.01f);
        }

        // Nearby zombies count
        int nearbyZombies = CountNearbyZombies(10f);
        sensor.AddObservation(nearbyZombies / 10f); // Normalized

        // Obstacles/walls nearby (simple check)
        sensor.AddObservation(IsObstacleNearby(5f) ? 1f : 0f);

        // === PLAYER BEHAVIOR PATTERNS (8 observations) ===
        if (BehaviorTracker.Instance != null)
        {
            sensor.AddObservation(BehaviorTracker.Instance.GetPreferredCombatStyle() == "Melee" ? 1f : 0f);
            sensor.AddObservation(BehaviorTracker.Instance.GetRiskTolerance());
            sensor.AddObservation(BehaviorTracker.Instance.PrefersDayActivity() ? 1f : 0f);
            sensor.AddObservation(BehaviorTracker.Instance.PrefersStealthApproach() ? 1f : 0f);
            sensor.AddObservation(BehaviorTracker.Instance.GetAverageHealthThreshold() / 100f);
            sensor.AddObservation(BehaviorTracker.Instance.GetAverageCombatDistance() / 20f);
            sensor.AddObservation(BehaviorTracker.Instance.GetCombatSuccessRate());
            sensor.AddObservation(BehaviorTracker.Instance.GetPatternConfidence("AggressiveCombat"));
        }
        else
        {
            sensor.AddObservation(1f);
            sensor.AddObservation(0.5f);
            sensor.AddObservation(1f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0.5f);
            sensor.AddObservation(5f / 20f);
            sensor.AddObservation(0.5f);
            sensor.AddObservation(0.5f);
        }

        // === RAYCAST PERCEPTION (raycastCount observations) ===
        // Each raycast returns normalized distance to obstacle/player
        float angleStep = 360f / raycastCount;
        for (int i = 0; i < raycastCount; i++)
        {
            float angle = angleStep * i;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, raycastDistance);

            if (hit.collider != null)
            {
                float normalizedDistance = hit.distance / raycastDistance;
                sensor.AddObservation(normalizedDistance);

                // What did we hit? (player = 1, wall = 0.5, other = 0)
                if (hit.collider.CompareTag("Player"))
                {
                    sensor.AddObservation(1f);
                }
                else if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Obstacle"))
                {
                    sensor.AddObservation(0.5f);
                }
                else
                {
                    sensor.AddObservation(0f);
                }
            }
            else
            {
                sensor.AddObservation(1f); // Max distance
                sensor.AddObservation(0f); // Nothing
            }
        }

        // Total observations: 7 + 11 + 5 + 8 + (raycastCount * 2) = 31 + (16 * 2) = 63
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (zombieAI == null || playerTransform == null)
            return;

        stepCount++;

        // === PARSE ACTIONS ===
        // Continuous actions: [moveX, moveY, rotationSpeed]
        // Discrete actions: [attackDecision]

        Vector2 moveDirection = Vector2.zero;
        float rotationSpeed = 0f;
        bool shouldAttack = false;

        if (actions.ContinuousActions.Length >= 2)
        {
            moveDirection.x = actions.ContinuousActions[0];
            moveDirection.y = actions.ContinuousActions[1];
            moveDirection = moveDirection.normalized;
        }

        if (actions.ContinuousActions.Length >= 3)
        {
            rotationSpeed = actions.ContinuousActions[2];
        }

        if (actions.DiscreteActions.Length >= 1)
        {
            shouldAttack = actions.DiscreteActions[0] == 1;
        }

        // === APPLY ACTIONS ===

        // Movement
        if (rb != null)
        {
            float speedMultiplier = 1f;

            // Adapt to player behavior
            if (playerPrefersStealth)
            {
                speedMultiplier = 0.8f; // Move slower to listen
            }
            else if (playerRiskTolerance > 0.7f)
            {
                speedMultiplier = 1.2f; // Be more aggressive
            }

            Vector2 velocity = moveDirection * zombieAI.moveSpeed * speedMultiplier;
            rb.velocity = velocity;
        }

        // Attack
        if (shouldAttack)
        {
            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distToPlayer <= zombieAI.attackRange * 1.5f)
            {
                attackAttempts++;

                // Use ZombieAI's attack system
                // Normally handled by ZombieAI state machine, but we can influence it
                // For training, we directly check attack conditions

                if (distToPlayer <= zombieAI.attackRange)
                {
                    // Would attack - give reward
                    AddReward(0.1f);
                    successfulAttacks++;
                }
            }
        }

        // === CALCULATE REWARDS ===

        // Survival reward (small positive for staying alive)
        AddReward(survivalReward);

        // Distance-based reward (getting closer to player)
        if (playerTransform != null)
        {
            float currentDistance = Vector3.Distance(transform.position, playerTransform.position);
            float distanceChange = previousDistanceToPlayer - currentDistance;

            if (distanceChange > 0)
            {
                // Got closer - reward
                AddReward(distanceChange * approachReward);
            }
            else
            {
                // Got farther - small penalty
                AddReward(distanceChange * approachReward * 0.5f);
            }

            previousDistanceToPlayer = currentDistance;
        }

        // Health-based rewards/penalties
        if (zombieAI.health < previousHealth)
        {
            float damageTaken = previousHealth - zombieAI.health;
            AddReward(damagePenalty * (damageTaken / zombieAI.maxHealth));
            totalDamageTaken += damageTaken;
        }
        previousHealth = zombieAI.health;

        // Check if player was damaged (would need to track this separately)
        // For now, we check if attack was successful through ZombieAI

        // Adaptive rewards based on player behavior
        AdaptiveRewardShaping();

        // Episode termination conditions
        if (zombieAI.health <= 0)
        {
            // Zombie died
            AddReward(deathPenalty);
            EndEpisode();
        }
        else if (stepCount >= MaxStep)
        {
            // Max steps reached
            float survivalBonus = (timeAlive / MaxStep) * 0.5f;
            AddReward(survivalBonus);
            EndEpisode();
        }

        // Track episode reward
        episodeReward = GetCumulativeReward();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // For testing without ML model
        // Simple behavior: move towards player and attack when in range

        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;

        if (playerTransform != null)
        {
            Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // Move towards player
            continuousActions[0] = dirToPlayer.x;
            continuousActions[1] = dirToPlayer.y;
            continuousActions[2] = 0f; // No rotation

            // Attack if in range
            discreteActions[0] = distToPlayer <= zombieAI.attackRange * 1.5f ? 1 : 0;
        }
        else
        {
            // Random movement if no player
            continuousActions[0] = Random.Range(-1f, 1f);
            continuousActions[1] = Random.Range(-1f, 1f);
            continuousActions[2] = 0f;
            discreteActions[0] = 0;
        }
    }

    // === HELPER METHODS ===

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void LoadPlayerBehavior()
    {
        if (BehaviorTracker.Instance != null)
        {
            playerCombatStyle = BehaviorTracker.Instance.GetPreferredCombatStyle();
            playerRiskTolerance = BehaviorTracker.Instance.GetRiskTolerance();
            playerPrefersDay = BehaviorTracker.Instance.PrefersDayActivity();
            playerPrefersStealth = BehaviorTracker.Instance.PrefersStealthApproach();
        }
    }

    private void AdaptiveRewardShaping()
    {
        // Give extra rewards for adapting to player behavior

        // If player prefers melee, reward for getting close
        if (playerCombatStyle == "Melee")
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist < 5f)
            {
                AddReward(0.01f);
            }
        }
        else // Ranged
        {
            // Reward for flanking or dodging
            if (rb != null && rb.velocity.magnitude > zombieAI.moveSpeed * 0.8f)
            {
                AddReward(0.005f);
            }
        }

        // Adapt to time of day
        if (TimeManager.Instance != null)
        {
            bool isDay = TimeManager.Instance.IsDaytime();

            if (isDay && playerPrefersDay)
            {
                // Player is likely more active during day - be more cautious
                AddReward(0.002f);
            }
            else if (!isDay && !playerPrefersDay)
            {
                // Player avoids night - be more aggressive
                AddReward(0.003f);
            }
        }

        // Adapt to player stealth approach
        if (playerPrefersStealth)
        {
            // Reward for staying hidden/ambushing
            if (zombieAI.currentState == ZombieState.Investigate || zombieAI.currentState == ZombieState.Idle)
            {
                AddReward(0.001f);
            }
        }
    }

    private int CountNearbyZombies(float radius)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
        int count = 0;

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Zombie") && col.gameObject != gameObject)
            {
                count++;
            }
        }

        return count;
    }

    private bool IsObstacleNearby(float checkDistance)
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, 1f, Vector2.zero, checkDistance);
        return hit.collider != null && (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Obstacle"));
    }

    private int GetObservationSize()
    {
        // 7 (self) + 11 (player) + 5 (environment) + 8 (behavior) + (raycastCount * 2)
        return 31 + (raycastCount * 2);
    }

    // === PUBLIC METHODS ===

    public void RewardDamageDealt(float damage)
    {
        // Called by ZombieAI when it damages player
        AddReward(damageReward * (damage / 100f));
        totalDamageDealt += damage;
        hasDealtDamage = true;
    }

    public float GetEpisodeReward()
    {
        return episodeReward;
    }

    public float GetSuccessRate()
    {
        if (attackAttempts == 0) return 0f;
        return (float)successfulAttacks / attackAttempts;
    }

    // === GIZMOS ===

    void OnDrawGizmosSelected()
    {
        // Draw observation radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, observationRadius);

        // Draw raycasts
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            float angleStep = 360f / raycastCount;

            for (int i = 0; i < raycastCount; i++)
            {
                float angle = angleStep * i;
                Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction * raycastDistance);
            }
        }

        // Draw line to player
        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
}
