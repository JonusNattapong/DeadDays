using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // Singleton instance
    public static PlayerController Instance { get; private set; }

    // === MOVEMENT CONFIGURATION ===
    [Header("Movement Settings")]
    [Tooltip("Base movement speed")]
    public float moveSpeed = 5f;
    [Tooltip("Sprint speed multiplier")]
    public float sprintMultiplier = 1.5f;
    [Tooltip("Crouch speed multiplier")]
    public float crouchMultiplier = 0.5f;
    [Tooltip("Acceleration rate")]
    public float acceleration = 10f;
    [Tooltip("Deceleration rate")]
    public float deceleration = 15f;

    [Header("Isometric Settings")]
    [Tooltip("Enable isometric projection")]
    public bool isIsometric = true;
    [Tooltip("Isometric Y compression factor")]
    public float isometricYFactor = 0.5f;
    [Tooltip("Isometric angle offset")]
    public float isometricAngle = 45f;

    [Header("Stamina Settings")]
    [Tooltip("Maximum stamina")]
    public float maxStamina = 100f;
    [Tooltip("Stamina drain rate while sprinting")]
    public float staminaDrainRate = 10f;
    [Tooltip("Stamina regen rate")]
    public float staminaRegenRate = 5f;
    [Tooltip("Minimum stamina to start sprinting")]
    public float minSprintStamina = 20f;

    [Header("Components")]
    [Tooltip("Reference to the character's Rigidbody2D")]
    public Rigidbody2D rb;
    [Tooltip("Reference to the character's Animator")]
    public Animator animator;
    [Tooltip("Reference to the character's SpriteRenderer")]
    public SpriteRenderer spriteRenderer;
    [Tooltip("Reference to interaction collider")]
    public Collider2D interactionCollider;

    [Header("Interaction")]
    [Tooltip("Interaction range")]
    public float interactionRange = 2f;
    [Tooltip("Layer mask for interactable objects")]
    public LayerMask interactableLayer;

    [Header("Audio")]
    [Tooltip("Footstep interval in seconds")]
    public float footstepInterval = 0.5f;

    // Input variables
    private Vector2 movementInput;
    private Vector2 currentVelocity;
    private bool isSprinting = false;
    private bool isCrouching = false;
    private bool canMove = true;

    // Stamina
    private float currentStamina;
    private bool isExhausted = false;

    // Animation
    private bool isMoving = false;
    private Vector2 lastMovementDirection = Vector2.down;
    private float footstepTimer = 0f;

    // Interaction
    private GameObject nearestInteractable;
    private float interactionCooldown = 0f;

    // State
    private bool isDead = false;

    void Awake()
    {
        // Ensure singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Get components if not assigned
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (interactionCollider == null)
            interactionCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        // Initialize stamina
        currentStamina = maxStamina;

        // Configure rigidbody
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void Update()
    {
        // Check if player can act
        if (isDead || !canMove)
        {
            movementInput = Vector2.zero;
            return;
        }

        // Get movement input
        GetMovementInput();

        // Handle sprint/crouch input
        HandleMovementModes();

        // Update stamina
        UpdateStamina();

        // Handle interaction input
        HandleInteractionInput();

        // Update footstep sounds
        UpdateFootsteps();

        // Detect nearby interactables
        DetectInteractables();

        // Update interaction cooldown
        if (interactionCooldown > 0f)
            interactionCooldown -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        // Apply movement
        if (!isDead && canMove)
        {
            ApplyMovement();
        }
        else
        {
            // Stop movement when dead or can't move
            rb.velocity = Vector2.zero;
        }

        // Update animations
        UpdateAnimations();
    }

    // === INPUT HANDLING ===

    private void GetMovementInput()
    {
        // Get raw input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        movementInput = new Vector2(horizontal, vertical);

        // Normalize diagonal movement
        if (movementInput.magnitude > 1f)
        {
            movementInput.Normalize();
        }

        // Track movement state
        isMoving = movementInput.magnitude > 0.1f;

        // Update last movement direction
        if (isMoving)
        {
            lastMovementDirection = movementInput;
        }
    }

    private void HandleMovementModes()
    {
        // Sprint (Left Shift)
        if (Input.GetKey(KeyCode.LeftShift) && !isExhausted && currentStamina > minSprintStamina)
        {
            isSprinting = true;
            isCrouching = false;
        }
        else
        {
            isSprinting = false;
        }

        // Crouch (Left Ctrl or C)
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
        {
            isCrouching = true;
            isSprinting = false;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
        {
            isCrouching = false;
        }

        // Can't sprint if fatigued
        if (PlayerStats.Instance != null)
        {
            float fatigue = PlayerStats.Instance.fatigue;
            if (fatigue > 80f)
            {
                isSprinting = false;
                isExhausted = true;
            }
        }
    }

    private void HandleInteractionInput()
    {
        // Interact (E key or F key)
        if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F)) && interactionCooldown <= 0f)
        {
            Interact();
        }
    }

    // === MOVEMENT ===

    private void ApplyMovement()
    {
        if (rb == null) return;

        // Calculate target speed
        float targetSpeed = moveSpeed;

        // Apply stat modifiers
        if (PlayerStats.Instance != null)
        {
            targetSpeed *= PlayerStats.Instance.GetMovementSpeedModifier();
        }

        // Apply skill modifiers
        if (SkillSystem.Instance != null)
        {
            float fitnessMod = SkillSystem.Instance.GetSkillModifier("Fitness");
            targetSpeed *= (1f + fitnessMod * 0.05f);
        }

        // Apply movement mode modifiers
        if (isSprinting)
        {
            targetSpeed *= sprintMultiplier;
        }
        else if (isCrouching)
        {
            targetSpeed *= crouchMultiplier;
        }

        // Calculate target velocity
        Vector2 targetVelocity = movementInput * targetSpeed;

        // Apply isometric transformation if enabled
        if (isIsometric)
        {
            targetVelocity = TransformToIsometric(targetVelocity);
        }

        // Smoothly interpolate to target velocity
        if (movementInput.magnitude > 0.1f)
        {
            // Accelerate
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Decelerate
            currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        // Apply velocity to rigidbody
        rb.velocity = currentVelocity;
    }

    private Vector2 TransformToIsometric(Vector2 input)
    {
        // Transform input to isometric coordinates
        // Standard isometric projection: X and Y map to diagonal movement
        float isoX = (input.x - input.y);
        float isoY = (input.x + input.y) * isometricYFactor;

        return new Vector2(isoX, isoY);
    }

    // === STAMINA ===

    private void UpdateStamina()
    {
        if (isSprinting && isMoving)
        {
            // Drain stamina while sprinting
            currentStamina -= staminaDrainRate * Time.deltaTime;

            // Apply to player stats fatigue
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.fatigue += 0.1f * Time.deltaTime;
            }

            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isExhausted = true;
                isSprinting = false;
            }
        }
        else
        {
            // Regenerate stamina
            currentStamina += staminaRegenRate * Time.deltaTime;

            if (currentStamina >= maxStamina)
            {
                currentStamina = maxStamina;
                isExhausted = false;
            }

            // Recover faster if not moving
            if (!isMoving)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
            }
        }

        // Apply stamina modifier from stats
        if (PlayerStats.Instance != null)
        {
            float staminaMod = PlayerStats.Instance.GetStaminaModifier();
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina * staminaMod);
        }
    }

    // === ANIMATION ===

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Set animation parameters
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsSprinting", isSprinting);
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetFloat("MoveSpeed", currentVelocity.magnitude);

        // Set direction parameters for 8-directional animation
        if (isMoving)
        {
            animator.SetFloat("MoveX", lastMovementDirection.x);
            animator.SetFloat("MoveY", lastMovementDirection.y);
        }

        // Flip sprite based on direction
        if (spriteRenderer != null && movementInput.x != 0f)
        {
            spriteRenderer.flipX = movementInput.x < 0f;
        }
    }

    // === FOOTSTEPS ===

    private void UpdateFootsteps()
    {
        if (!isMoving) return;

        footstepTimer += Time.deltaTime;

        float interval = footstepInterval;
        if (isSprinting)
        {
            interval *= 0.6f; // Faster footsteps when sprinting
        }
        else if (isCrouching)
        {
            interval *= 1.5f; // Slower footsteps when crouching
        }

        if (footstepTimer >= interval)
        {
            footstepTimer = 0f;
            PlayFootstepSound();
        }
    }

    private void PlayFootstepSound()
    {
        if (AudioManager.Instance != null)
        {
            float volume = isCrouching ? 0.1f : 0.3f;
            AudioManager.Instance.PlayFootstep(transform.position);

            // Footsteps attract zombies
            if (!isCrouching)
            {
                AudioManager.Instance.PropagateSound(transform.position, volume, 5f);
            }
        }

        // Gain fitness XP
        if (SkillSystem.Instance != null && isMoving)
        {
            SkillSystem.Instance.GainXP("Fitness", 0.1f);
        }
    }

    // === INTERACTION ===

    private void DetectInteractables()
    {
        // Find nearest interactable object
        Collider2D[] interactables = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayer);

        float nearestDistance = float.MaxValue;
        GameObject nearest = null;

        foreach (var col in interactables)
        {
            float distance = Vector2.Distance(transform.position, col.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = col.gameObject;
            }
        }

        // Update nearest interactable
        if (nearest != nearestInteractable)
        {
            nearestInteractable = nearest;

            // TODO: Show interaction prompt UI
            if (nearestInteractable != null)
            {
                Debug.Log($"Near interactable: {nearestInteractable.name}");
            }
        }
    }

    private void Interact()
    {
        if (nearestInteractable == null) return;

        // Get interactable component
        IInteractable interactable = nearestInteractable.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interactable.Interact(gameObject);
            interactionCooldown = 0.5f;
            Debug.Log($"Interacted with {nearestInteractable.name}");
        }
        else
        {
            // Try generic interactions
            if (nearestInteractable.CompareTag("Door"))
            {
                OpenDoor(nearestInteractable);
            }
            else if (nearestInteractable.CompareTag("Container"))
            {
                OpenContainer(nearestInteractable);
            }
            else if (nearestInteractable.CompareTag("Item"))
            {
                PickupItem(nearestInteractable);
            }
        }
    }

    private void OpenDoor(GameObject door)
    {
        // TODO: Implement door opening
        Debug.Log("Opening door...");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDoorOpen(door.transform.position);
        }

        // Gain carpentry XP
        if (SkillSystem.Instance != null)
        {
            SkillSystem.Instance.GainXP("Carpentry", 1f);
        }
    }

    private void OpenContainer(GameObject container)
    {
        // TODO: Open container UI
        Debug.Log("Opening container...");
    }

    private void PickupItem(GameObject itemObject)
    {
        // Get item component
        ItemPickup pickup = itemObject.GetComponent<ItemPickup>();
        if (pickup != null && PlayerInventory.Instance != null)
        {
            if (PlayerInventory.Instance.AddItem(pickup.item, pickup.quantity))
            {
                Destroy(itemObject);
                Debug.Log($"Picked up {pickup.item.itemName}");
            }
        }
    }

    // === PUBLIC METHODS ===

    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!canMove)
        {
            movementInput = Vector2.zero;
            currentVelocity = Vector2.zero;
        }
    }

    public void Teleport(Vector3 position)
    {
        transform.position = position;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        currentVelocity = Vector2.zero;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        canMove = false;
        currentVelocity = Vector2.zero;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        Debug.Log("Player died!");
    }

    public void Respawn(Vector3 position)
    {
        isDead = false;
        canMove = true;
        Teleport(position);

        if (animator != null)
        {
            animator.SetTrigger("Respawn");
        }

        Debug.Log("Player respawned!");
    }

    public void ApplyKnockback(Vector2 force)
    {
        if (rb != null)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    public void StartInteractionWithObject(GameObject obj)
    {
        nearestInteractable = obj;
    }

    public void StopInteractionWithObject()
    {
        nearestInteractable = null;
    }

    // === GETTERS ===

    public bool IsSprinting() => isSprinting && isMoving;
    public bool IsCrouching() => isCrouching;
    public bool IsMoving() => isMoving;
    public bool IsDead() => isDead;
    public bool CanMove() => canMove;
    public Vector2 GetMovementDirection() => lastMovementDirection;
    public Vector2 GetVelocity() => currentVelocity;
    public float GetStamina() => currentStamina;
    public float GetStaminaPercentage() => currentStamina / maxStamina;
    public GameObject GetNearestInteractable() => nearestInteractable;

    // === DEBUG ===

    void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Draw movement direction
        if (Application.isPlaying && isMoving)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)lastMovementDirection * 2f);
        }
    }
}

// === INTERACTABLE INTERFACE ===

public interface IInteractable
{
    void Interact(GameObject player);
    string GetInteractionPrompt();
    bool CanInteract(GameObject player);
}

// === ITEM PICKUP CLASS ===

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public int quantity = 1;
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        if (spriteRenderer != null && item != null)
        {
            spriteRenderer.sprite = item.icon;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.StartInteractionWithObject(gameObject);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.StopInteractionWithObject();
            }
        }
    }
}
