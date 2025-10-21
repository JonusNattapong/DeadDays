using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class ZombieAI : MonoBehaviour
{
    // === ZOMBIE CONFIGURATION ===
    [Header("Zombie Type")]
    public ZombieType zombieType = ZombieType.Walker;

    [Header("Stats")]
    [Tooltip("Maximum health")]
    public float maxHealth = 50f;
    [Tooltip("Current health")]
    public float health = 50f;
    [Tooltip("Movement speed")]
    public float moveSpeed = 2f;
    [Tooltip("Attack damage")]
    public float attackDamage = 10f;
    [Tooltip("Attack range")]
    public float attackRange = 1.5f;
    [Tooltip("Attack cooldown in seconds")]
    public float attackCooldown = 1.5f;

    [Header("Senses")]
    [Tooltip("Vision range")]
    public float visionRange = 10f;
    [Tooltip("Vision angle (field of view)")]
    public float visionAngle = 120f;
    [Tooltip("Hearing range")]
    public float hearingRange = 15f;
    [Tooltip("Smell range")]
    public float smellRange = 8f;
    [Tooltip("Memory duration in seconds")]
    public float memoryDuration = 5f;

    [Header("Behavior")]
    [Tooltip("Wander speed multiplier")]
    public float wanderSpeedMultiplier = 0.3f;
    [Tooltip("Chase speed multiplier")]
    public float chaseSpeedMultiplier = 1.5f;
    [Tooltip("Patrol radius")]
    public float patrolRadius = 10f;
    [Tooltip("Give up chase time")]
    public float giveUpTime = 10f;

    [Header("Components")]
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Collider2D zombieCollider;

    [Header("Layers")]
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Events")]
    [System.Serializable]
    public class DamageEvent : UnityEvent<float, Vector3> {}
    public DamageEvent OnDamageTaken;
    public DamageEvent OnDamageDealt;
    public UnityEvent OnDeath;

    // === RUNTIME VARIABLES ===

    // State
    private ZombieState currentState = ZombieState.Idle;
    private ZombieState previousState = ZombieState.Idle;

    // Target tracking
    private Transform targetPlayer;
    private Vector3 lastKnownPlayerPosition;
    private float timeSinceLastSawPlayer = 0f;
    private float timeInCurrentState = 0f;

    // Memory system
    private List<SoundMemory> soundMemories = new List<SoundMemory>();
    private float investigationTimer = 0f;

    // Movement
    private Vector2 currentDestination;
    private Vector2 wanderDirection;
    private float wanderTimer = 0f;
    private float wanderChangeInterval = 3f;
    private Vector2 spawnPosition;

    // Combat
    private float lastAttackTime = 0f;
    private bool canAttack = true;

    // Group behavior
    private List<ZombieAI> nearbyZombies = new List<ZombieAI>();
    private float groupDetectionRange = 5f;

    // Special abilities (based on type)
    private bool isRaging = false;
    private float rageTimer = 0f;
    private bool isSpitting = false;

    // Stats tracking
    private int damageDealtToPlayer = 0;
    private float distanceTraveled = 0f;
    private Vector3 lastPosition;

    void Awake()
    {
        // Get components if not assigned
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (zombieCollider == null)
            zombieCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        // Initialize
        health = maxHealth;
        spawnPosition = transform.position;
        lastPosition = transform.position;

        // Configure rigidbody
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Apply zombie type modifiers
        ApplyZombieTypeModifiers();

        // Start in idle/wander state
        SetState(ZombieState.Wander);

        // Random initial wander direction
        wanderDirection = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        if (health <= 0) return;

        // Update timers
        timeInCurrentState += Time.deltaTime;
        timeSinceLastSawPlayer += Time.deltaTime;

        // Update senses
        UpdateSenses();

        // Update state machine
        UpdateStateMachine();

        // Update special abilities
        UpdateSpecialAbilities();

        // Clean up old sound memories
        CleanupSoundMemories();

        // Track distance traveled
        distanceTraveled += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (health <= 0) return;

        // Execute state behavior
        ExecuteState();
    }

    // === STATE MACHINE ===

    private void UpdateStateMachine()
    {
        switch (currentState)
        {
            case ZombieState.Idle:
                // Transition to wander after a short time
                if (timeInCurrentState > Random.Range(2f, 5f))
                {
                    SetState(ZombieState.Wander);
                }
                // If player detected, chase
                if (targetPlayer != null)
                {
                    SetState(ZombieState.Chase);
                }
                break;

            case ZombieState.Wander:
                // If player detected, chase
                if (targetPlayer != null)
                {
                    SetState(ZombieState.Chase);
                }
                // If heard sound, investigate
                else if (soundMemories.Count > 0)
                {
                    SetState(ZombieState.Investigate);
                }
                // Change wander direction periodically
                wanderTimer += Time.deltaTime;
                if (wanderTimer >= wanderChangeInterval)
                {
                    wanderTimer = 0f;
                    wanderDirection = Random.insideUnitCircle.normalized;
                }
                break;

            case ZombieState.Chase:
                // If player lost, go to last known position
                if (targetPlayer == null && timeSinceLastSawPlayer > 2f)
                {
                    SetState(ZombieState.Investigate);
                }
                // Give up chase after time limit
                else if (timeSinceLastSawPlayer > giveUpTime)
                {
                    SetState(ZombieState.Wander);
                }
                // If reached player, attack
                else if (targetPlayer != null && Vector2.Distance(transform.position, targetPlayer.position) <= attackRange)
                {
                    SetState(ZombieState.Attack);
                }
                break;

            case ZombieState.Attack:
                // If player moves out of range, chase
                if (targetPlayer == null || Vector2.Distance(transform.position, targetPlayer.position) > attackRange * 1.5f)
                {
                    SetState(ZombieState.Chase);
                }
                break;

            case ZombieState.Investigate:
                // If found player, chase
                if (targetPlayer != null)
                {
                    SetState(ZombieState.Chase);
                }
                // If reached investigation point or timeout, return to wander
                else if (Vector2.Distance(transform.position, currentDestination) < 1f || investigationTimer <= 0f)
                {
                    SetState(ZombieState.Wander);
                }
                else
                {
                    investigationTimer -= Time.deltaTime;
                }
                break;

            case ZombieState.Patrol:
                // If player detected, chase
                if (targetPlayer != null)
                {
                    SetState(ZombieState.Chase);
                }
                // If reached patrol point, pick new one
                else if (Vector2.Distance(transform.position, currentDestination) < 1f)
                {
                    SetPatrolDestination();
                }
                break;
        }
    }

    private void ExecuteState()
    {
        switch (currentState)
        {
            case ZombieState.Idle:
                // Stop movement
                rb.velocity = Vector2.zero;
                break;

            case ZombieState.Wander:
                // Move in wander direction
                MoveTowards(transform.position + (Vector3)wanderDirection * 5f, moveSpeed * wanderSpeedMultiplier);
                break;

            case ZombieState.Chase:
                // Move towards player
                if (targetPlayer != null)
                {
                    MoveTowards(targetPlayer.position, moveSpeed * chaseSpeedMultiplier);
                    lastKnownPlayerPosition = targetPlayer.position;
                }
                else
                {
                    MoveTowards(lastKnownPlayerPosition, moveSpeed);
                }
                break;

            case ZombieState.Attack:
                // Stop and attack
                rb.velocity = Vector2.zero;
                if (targetPlayer != null)
                {
                    FaceTarget(targetPlayer.position);
                }
                TryAttack();
                break;

            case ZombieState.Investigate:
                // Move to investigation point
                MoveTowards(currentDestination, moveSpeed * 0.7f);
                break;

            case ZombieState.Patrol:
                // Move to patrol point
                MoveTowards(currentDestination, moveSpeed * 0.5f);
                break;
        }

        // Update animation
        UpdateAnimation();
    }

    private void SetState(ZombieState newState)
    {
        if (currentState == newState) return;

        // Exit current state
        OnStateExit(currentState);

        previousState = currentState;
        currentState = newState;
        timeInCurrentState = 0f;

        // Enter new state
        OnStateEnter(newState);

        Debug.Log($"Zombie {gameObject.name} entered state: {newState}");
    }

    private void OnStateEnter(ZombieState state)
    {
        switch (state)
        {
            case ZombieState.Investigate:
                // Set investigation point
                if (soundMemories.Count > 0)
                {
                    currentDestination = soundMemories[0].position;
                    investigationTimer = 10f;
                }
                else
                {
                    currentDestination = lastKnownPlayerPosition;
                    investigationTimer = 5f;
                }
                break;

            case ZombieState.Patrol:
                SetPatrolDestination();
                break;

            case ZombieState.Chase:
                // Alert nearby zombies
                AlertNearbyZombies();
                break;
        }
    }

    private void OnStateExit(ZombieState state)
    {
        // Cleanup state-specific behavior
    }

    // === SENSES ===

    private void UpdateSenses()
    {
        // Vision
        CheckVision();

        // Hearing (passive - triggered by HearSound method)

        // Smell
        CheckSmell();

        // Find nearby zombies for group behavior
        UpdateNearbyZombies();
    }

    private void CheckVision()
    {
        // Find all players in vision range
        Collider2D[] playersInRange = Physics2D.OverlapCircleAll(transform.position, visionRange, playerLayer);

        foreach (var playerCol in playersInRange)
        {
            Vector2 directionToPlayer = (playerCol.transform.position - transform.position).normalized;
            float angleToPlayer = Vector2.Angle(GetFacingDirection(), directionToPlayer);

            // Check if within vision angle
            if (angleToPlayer <= visionAngle / 2f)
            {
                // Check line of sight (no obstacles)
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, visionRange, playerLayer | obstacleLayer);

                if (hit.collider != null && hit.collider.gameObject == playerCol.gameObject)
                {
                    // Player visible!
                    targetPlayer = playerCol.transform;
                    timeSinceLastSawPlayer = 0f;
                    lastKnownPlayerPosition = targetPlayer.position;
                    return;
                }
            }
        }

        // If no player visible, check if we still have a target
        if (targetPlayer != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, targetPlayer.position);
            if (distanceToTarget > visionRange * 1.5f)
            {
                targetPlayer = null;
            }
        }
    }

    private void CheckSmell()
    {
        if (targetPlayer != null) return; // Already tracking visually

        // Smell can detect player through walls
        Collider2D[] playersInRange = Physics2D.OverlapCircleAll(transform.position, smellRange, playerLayer);

        if (playersInRange.Length > 0)
        {
            Transform nearestPlayer = playersInRange[0].transform;
            float nearestDistance = Vector2.Distance(transform.position, nearestPlayer.position);

            foreach (var playerCol in playersInRange)
            {
                float distance = Vector2.Distance(transform.position, playerCol.transform.position);
                if (distance < nearestDistance)
                {
                    nearestPlayer = playerCol.transform;
                    nearestDistance = distance;
                }
            }

            // Smell detected - add to investigation
            if (currentState == ZombieState.Wander || currentState == ZombieState.Idle)
            {
                lastKnownPlayerPosition = nearestPlayer.position;
                SetState(ZombieState.Investigate);
            }
        }
    }

    public void HearSound(Vector3 soundPosition, float soundIntensity)
    {
        // Calculate distance to sound
        float distance = Vector2.Distance(transform.position, soundPosition);

        // Check if within hearing range
        if (distance <= hearingRange * soundIntensity)
        {
            // Add to sound memory
            soundMemories.Add(new SoundMemory
            {
                position = soundPosition,
                intensity = soundIntensity,
                timestamp = Time.time
            });

            // If not already chasing, investigate
            if (currentState != ZombieState.Chase && currentState != ZombieState.Attack)
            {
                lastKnownPlayerPosition = soundPosition;
                SetState(ZombieState.Investigate);
            }

            Debug.Log($"Zombie heard sound at distance {distance:F1} with intensity {soundIntensity:F2}");
        }
    }

    private void CleanupSoundMemories()
    {
        soundMemories.RemoveAll(memory => Time.time - memory.timestamp > memoryDuration);
    }

    // === MOVEMENT ===

    private void MoveTowards(Vector3 destination, float speed)
    {
        Vector2 direction = (destination - transform.position).normalized;

        // Apply time of day modifier
        float timeModifier = 1f;
        if (TimeManager.Instance != null)
        {
            if (TimeManager.Instance.IsNight())
            {
                timeModifier = 1.2f; // Faster at night
            }
        }

        // Check for obstacles with raycast
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1f, obstacleLayer);
        if (hit.collider != null)
        {
            // Try to avoid obstacle
            Vector2 avoidanceDir = Vector2.Perpendicular(direction);
            if (Random.value > 0.5f)
                avoidanceDir = -avoidanceDir;
            direction = (direction + avoidanceDir * 0.5f).normalized;
        }

        rb.velocity = direction * speed * timeModifier;

        // Face movement direction
        FaceTarget(transform.position + (Vector3)direction);
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        Vector2 direction = (targetPosition - transform.position).normalized;

        // Flip sprite based on direction
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0f;
        }
    }

    private Vector2 GetFacingDirection()
    {
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            return rb.velocity.normalized;
        }
        return spriteRenderer != null && spriteRenderer.flipX ? Vector2.left : Vector2.right;
    }

    private void SetPatrolDestination()
    {
        // Random point within patrol radius of spawn
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        currentDestination = spawnPosition + (Vector3)randomOffset;
    }

    // === COMBAT ===

    private void TryAttack()
    {
        if (!canAttack || Time.time < lastAttackTime + attackCooldown)
            return;

        if (targetPlayer == null)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);
        if (distanceToPlayer <= attackRange)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        lastAttackTime = Time.time;

        // Calculate damage
        float damage = attackDamage;

        // Apply rage modifier
        if (isRaging)
        {
            damage *= 1.5f;
        }

        // Deal damage to player
        if (targetPlayer != null)
        {
            PlayerStats playerStats = targetPlayer.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage, "zombie");
                damageDealtToPlayer += (int)damage;

                // Notify ML agent and listeners about damage dealt
                SmartZombieAgent agent = GetComponent<SmartZombieAgent>();
                if (agent != null)
                {
                    agent.RewardDamageDealt(damage);
                }

                OnDamageDealt?.Invoke(damage, targetPlayer.position);

                // Chance to cause bleeding
                if (Random.value < 0.2f)
                {
                    playerStats.ApplyStatusEffect("bleeding", 10f);
                }

                // Chance to cause infection
                if (Random.value < 0.1f)
                {
                    playerStats.ApplyStatusEffect("infected");
                }
            }
        }

        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Play attack sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayZombieGroan(transform.position);
        }

        Debug.Log($"Zombie attacked for {damage:F1} damage!");
    }

    // === DAMAGE ===

    public void TakeDamage(float damage, Vector3 damageSource)
    {
        if (health <= 0) return;

        health -= damage;
        health = Mathf.Max(0f, health);

        // Invoke damage taken event so other systems (UI, ML, analytics) can react
        OnDamageTaken?.Invoke(damage, damageSource);

        // Notify attached SmartZombieAgent (if present) so the agent receives a penalty for taking damage.
        // The penalty is scaled by normalized damage (damage / maxHealth) so it's proportional.
        SmartZombieAgent agent = GetComponent<SmartZombieAgent>();
        if (agent != null)
        {
            float normalizedDamage = maxHealth > 0f ? damage / maxHealth : damage / 100f;
            agent.AddReward(agent.damagePenalty * normalizedDamage);
        }

        // Visual feedback
        StartCoroutine(DamageFlash());

        // Knockback
        Vector2 knockbackDir = (transform.position - damageSource).normalized;
        if (rb != null)
        {
            rb.AddForce(knockbackDir * 3f, ForceMode2D.Impulse);
        }

        // Alert to attacker if not already chasing
        if (currentState != ZombieState.Chase)
        {
            lastKnownPlayerPosition = damageSource;

            // Try to find player near damage source
            Collider2D[] players = Physics2D.OverlapCircleAll(damageSource, 5f, playerLayer);
            if (players.Length > 0)
            {
                targetPlayer = players[0].transform;
                SetState(ZombieState.Chase);
            }
        }

        // Check for death
        if (health <= 0)
        {
            Die();
        }
        else
        {
            // Play hurt sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayZombieGroan(transform.position);
            }

            // Rage at low health
            if (health < maxHealth * 0.3f && zombieType == ZombieType.Brute)
            {
                EnterRageMode();
            }
        }

        Debug.Log($"Zombie took {damage:F1} damage. Health: {health:F1}/{maxHealth:F1}");
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

    private void Die()
    {
        Debug.Log($"Zombie died! Distance traveled: {distanceTraveled:F1}m, Damage dealt: {damageDealtToPlayer}");

        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Disable components
        if (rb != null)
            rb.velocity = Vector2.zero;
        if (zombieCollider != null)
            zombieCollider.enabled = false;

        // Drop loot
        DropLoot();

        // Update statistics
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.IncrementStatistic("zombiesKilled", 1f);
        }

        // Fire death event so other systems can react (drop tables, stats, ML, UI)
        OnDeath?.Invoke();

        // Destroy after delay
        Destroy(gameObject, 3f);
    }

    private void DropLoot()
    {
        // TODO: Implement loot dropping
        // Random chance to drop items
        if (Random.value < 0.3f)
        {
            Debug.Log("Zombie dropped loot!");
            // Spawn item prefab here
        }
    }

    // === SPECIAL ABILITIES ===

    private void ApplyZombieTypeModifiers()
    {
        switch (zombieType)
        {
            case ZombieType.Walker:
                // Standard zombie - no modifiers
                break;

            case ZombieType.Runner:
                moveSpeed *= 2f;
                health *= 0.7f;
                attackDamage *= 0.8f;
                visionRange *= 1.2f;
                break;

            case ZombieType.Brute:
                moveSpeed *= 0.7f;
                health *= 2f;
                attackDamage *= 1.5f;
                attackRange *= 1.3f;
                break;

            case ZombieType.Crawler:
                moveSpeed *= 0.5f;
                health *= 0.6f;
                attackDamage *= 1.2f;
                visionRange *= 0.7f;
                hearingRange *= 1.5f;
                break;

            case ZombieType.Spitter:
                moveSpeed *= 0.8f;
                health *= 0.8f;
                attackRange *= 3f;
                visionRange *= 1.3f;
                break;

            case ZombieType.Bloater:
                moveSpeed *= 0.5f;
                health *= 1.5f;
                attackDamage *= 2f;
                attackRange *= 2f;
                smellRange *= 1.5f;
                break;

            case ZombieType.Screamer:
                moveSpeed *= 0.9f;
                health *= 0.7f;
                attackDamage *= 0.5f;
                hearingRange *= 2f;
                visionRange *= 1.5f;
                break;
        }
    }

    private void UpdateSpecialAbilities()
    {
        // Rage mode (Brute)
        if (isRaging)
        {
            rageTimer -= Time.deltaTime;
            if (rageTimer <= 0f)
            {
                ExitRageMode();
            }
        }

        // TODO: Implement other special abilities
        // - Spitter: Ranged acid attack
        // - Bloater: Explode on death
        // - Screamer: Alert all zombies in range
    }

    private void EnterRageMode()
    {
        if (isRaging) return;

        isRaging = true;
        rageTimer = 10f;
        moveSpeed *= 1.5f;
        attackDamage *= 1.3f;

        // Visual feedback
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f);
        }

        Debug.Log("Zombie entered RAGE mode!");
    }

    private void ExitRageMode()
    {
        isRaging = false;
        moveSpeed /= 1.5f;
        attackDamage /= 1.3f;

        // Reset color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    // === GROUP BEHAVIOR ===

    private void UpdateNearbyZombies()
    {
        nearbyZombies.Clear();
        Collider2D[] zombiesInRange = Physics2D.OverlapCircleAll(transform.position, groupDetectionRange);

        foreach (var col in zombiesInRange)
        {
            ZombieAI zombie = col.GetComponent<ZombieAI>();
            if (zombie != null && zombie != this)
            {
                nearbyZombies.Add(zombie);
            }
        }
    }

    private void AlertNearbyZombies()
    {
        foreach (var zombie in nearbyZombies)
        {
            if (zombie.currentState != ZombieState.Chase && targetPlayer != null)
            {
                zombie.lastKnownPlayerPosition = targetPlayer.position;
                zombie.SetState(ZombieState.Investigate);
            }
        }
    }

    // === ANIMATION ===

    private void UpdateAnimation()
    {
        if (animator == null) return;

        // Set animation parameters
        bool isMoving = rb != null && rb.velocity.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);
        animator.SetFloat("MoveSpeed", rb != null ? rb.velocity.magnitude : 0f);
        animator.SetBool("IsAttacking", currentState == ZombieState.Attack);
        animator.SetBool("IsRaging", isRaging);
    }

    // === DEBUG ===

    void OnDrawGizmosSelected()
    {
        // Vision range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Vision cone
        Vector2 facingDir = GetFacingDirection();
        Vector2 leftBoundary = Quaternion.Euler(0, 0, visionAngle / 2f) * facingDir;
        Vector2 rightBoundary = Quaternion.Euler(0, 0, -visionAngle / 2f) * facingDir;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)leftBoundary * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)rightBoundary * visionRange);

        // Hearing range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        // Smell range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, smellRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Current destination
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentDestination);
            Gizmos.DrawWireSphere(currentDestination, 0.5f);
        }
    }
}

// === ENUMS ===

public enum ZombieState
{
    Idle,
    Wander,
    Patrol,
    Chase,
    Attack,
    Investigate,
    Flee
}

public enum ZombieType
{
    Walker,     // Standard zombie
    Runner,     // Fast, low health
    Brute,      // Slow, high health, high damage
    Crawler,    // Low profile, hard to see
    Spitter,    // Ranged acid attack
    Bloater,    // Explodes on death
    Screamer    // Alerts other zombies
}

// === HELPER CLASSES ===

[System.Serializable]
public class SoundMemory
{
    public Vector3 position;
    public float intensity;
    public float timestamp;
}
