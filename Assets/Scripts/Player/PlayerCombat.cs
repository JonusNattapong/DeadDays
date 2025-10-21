using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    // Singleton instance
    public static PlayerCombat Instance { get; private set; }

    // === COMBAT CONFIGURATION ===
    [Header("Combat Settings")]
    [Tooltip("Layer mask for enemies")]
    public LayerMask enemyLayer;
    [Tooltip("Layer mask for obstacles")]
    public LayerMask obstacleLayer;

    [Header("Melee Combat")]
    [Tooltip("Base melee damage")]
    public float baseMeleeDamage = 10f;
    [Tooltip("Melee attack range")]
    public float meleeRange = 1.5f;
    [Tooltip("Melee attack cooldown")]
    public float meleeCooldown = 0.5f;
    [Tooltip("Melee attack arc angle")]
    public float meleeArcAngle = 90f;
    [Tooltip("Knockback force")]
    public float knockbackForce = 5f;

    [Header("Ranged Combat")]
    [Tooltip("Base ranged damage")]
    public float baseRangedDamage = 25f;
    [Tooltip("Projectile speed")]
    public float projectileSpeed = 20f;
    [Tooltip("Fire rate (shots per second)")]
    public float fireRate = 2f;
    [Tooltip("Bullet spread (degrees)")]
    public float bulletSpread = 5f;
    [Tooltip("Recoil amount")]
    public float recoilAmount = 0.5f;
    [Tooltip("Reload time in seconds")]
    public float reloadTime = 2f;
    [Tooltip("Magazine size")]
    public int magazineSize = 10;

    [Header("Projectile Prefab")]
    [Tooltip("Bullet/projectile prefab")]
    public GameObject projectilePrefab;

    [Header("Visual Effects")]
    [Tooltip("Muzzle flash effect")]
    public GameObject muzzleFlashPrefab;
    [Tooltip("Hit effect prefab")]
    public GameObject hitEffectPrefab;
    [Tooltip("Blood effect prefab")]
    public GameObject bloodEffectPrefab;

    [Header("Aiming")]
    [Tooltip("Use mouse for aiming")]
    public bool useMouseAiming = true;
    [Tooltip("Aim assist range")]
    public float aimAssistRange = 5f;
    [Tooltip("Aim assist strength")]
    public float aimAssistStrength = 0.3f;

    // Runtime variables
    private float lastMeleeTime = 0f;
    private float lastFireTime = 0f;
    private bool isReloading = false;
    private int currentAmmo;
    private Vector2 aimDirection = Vector2.right;
    private Transform aimTarget;
    private bool isAiming = false;

    // Combat state
    private CombatMode currentMode = CombatMode.Melee;
    private bool canAttack = true;

    // Events
    [Header("Events")]
    public UnityEvent<float> onDamageDealt;
    public UnityEvent onMeleeAttack;
    public UnityEvent onRangedAttack;
    public UnityEvent onReloadStart;
    public UnityEvent onReloadComplete;
    public UnityEvent<int, int> onAmmoChanged;

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
    }

    void Start()
    {
        // Initialize combat state
        currentAmmo = magazineSize;
        currentMode = CombatMode.Melee;
        UpdateAmmoDisplay();
    }

    void Update()
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.IsDead())
            return;

        // Update aim direction
        UpdateAimDirection();

        // Handle combat input
        HandleCombatInput();

        // Handle mode switching
        HandleModeSwitch();
    }

    // === AIM DIRECTION ===

    private void UpdateAimDirection()
    {
        if (useMouseAiming)
        {
            // Aim towards mouse position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            Vector2 direction = (mousePos - transform.position).normalized;
            aimDirection = direction;
        }
        else
        {
            // Aim with right stick or WASD
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            if (h != 0f || v != 0f)
            {
                aimDirection = new Vector2(h, v).normalized;
            }
        }

        // Apply aim assist
        ApplyAimAssist();

        // Visual feedback (rotate player sprite or weapon)
        UpdateAimVisuals();
    }

    private void ApplyAimAssist()
    {
        if (aimAssistStrength <= 0f) return;

        // Find nearest enemy in range
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, aimAssistRange, enemyLayer);

        if (enemies.Length > 0)
        {
            Transform nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                Vector2 dirToEnemy = (enemy.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(aimDirection, dirToEnemy);

                // Only assist if enemy is roughly in aim direction
                if (angle < 45f && distance < nearestDistance)
                {
                    nearestEnemy = enemy.transform;
                    nearestDistance = distance;
                }
            }

            if (nearestEnemy != null)
            {
                Vector2 dirToEnemy = (nearestEnemy.position - transform.position).normalized;
                aimDirection = Vector2.Lerp(aimDirection, dirToEnemy, aimAssistStrength);
                aimTarget = nearestEnemy;
            }
            else
            {
                aimTarget = null;
            }
        }
    }

    private void UpdateAimVisuals()
    {
        // Rotate player or weapon sprite to face aim direction
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // TODO: Rotate weapon sprite
        // if (weaponSprite != null)
        // {
        //     weaponSprite.transform.rotation = Quaternion.Euler(0, 0, angle);
        // }

        // Flip sprite based on direction
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = aimDirection.x < 0f;
        }
    }

    // === COMBAT INPUT ===

    private void HandleCombatInput()
    {
        // Primary attack (left mouse or Ctrl)
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.LeftControl))
        {
            isAiming = true;
            if (currentMode == CombatMode.Melee)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftControl))
                {
                    MeleeAttack();
                }
            }
            else if (currentMode == CombatMode.Ranged)
            {
                RangedAttack();
            }
        }
        else
        {
            isAiming = false;
        }

        // Reload (R key)
        if (Input.GetKeyDown(KeyCode.R) && currentMode == CombatMode.Ranged)
        {
            StartReload();
        }
    }

    private void HandleModeSwitch()
    {
        // Switch between melee and ranged with Tab or mouse wheel click
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetMouseButtonDown(2))
        {
            SwitchCombatMode();
        }

        // Quick switch to melee with 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentMode = CombatMode.Melee;
        }

        // Quick switch to ranged with 2 (if weapon equipped)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (HasRangedWeapon())
            {
                currentMode = CombatMode.Ranged;
            }
        }
    }

    // === MELEE ATTACK ===

    public void MeleeAttack()
    {
        if (!canAttack || Time.time < lastMeleeTime + meleeCooldown)
            return;

        lastMeleeTime = Time.time;

        // Calculate damage with modifiers
        float damage = baseMeleeDamage;

        // Apply strength modifier from skills
        if (SkillSystem.Instance != null)
        {
            float strengthMod = SkillSystem.Instance.GetSkillModifier("Strength");
            damage *= (1f + strengthMod * 0.1f);
        }

        // Apply stat modifiers
        if (PlayerStats.Instance != null)
        {
            damage *= PlayerStats.Instance.GetAttackDamageModifier();
        }

        // Apply weapon damage
        if (PlayerInventory.Instance != null && PlayerInventory.Instance.equippedWeapon != null)
        {
            damage += PlayerInventory.Instance.equippedWeapon.damage;
        }

        // Find enemies in range
        Vector2 attackPosition = (Vector2)transform.position + aimDirection * meleeRange * 0.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPosition, meleeRange, enemyLayer);

        int hitCount = 0;
        foreach (var hit in hits)
        {
            // Check if enemy is in attack arc
            Vector2 dirToEnemy = (hit.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(aimDirection, dirToEnemy);

            if (angle <= meleeArcAngle / 2f)
            {
                // Deal damage
                ZombieAI zombie = hit.GetComponent<ZombieAI>();
                if (zombie != null)
                {
                    zombie.TakeDamage(damage, transform.position);
                    hitCount++;

                    // Apply knockback
                    Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.AddForce(dirToEnemy * knockbackForce, ForceMode2D.Impulse);
                    }

                    // Spawn blood effect
                    SpawnBloodEffect(hit.transform.position);
                }
            }
        }

        // Events and feedback
        onMeleeAttack?.Invoke();
        onDamageDealt?.Invoke(damage * hitCount);

        // Play sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHit(transform.position);
            AudioManager.Instance.PropagateSound(transform.position, 0.3f, 10f);
        }

        // Gain XP
        if (SkillSystem.Instance != null && hitCount > 0)
        {
            SkillSystem.Instance.GainXP("Strength", 2f * hitCount);
        }

        // Start attack animation
        StartCoroutine(MeleeAttackAnimation());

        Debug.Log($"Melee attack! Damage: {damage:F1}, Hits: {hitCount}");
    }

    private IEnumerator MeleeAttackAnimation()
    {
        canAttack = false;
        // TODO: Play melee animation
        // animator.SetTrigger("MeleeAttack");
        yield return new WaitForSeconds(meleeCooldown);
        canAttack = true;
    }

    // === RANGED ATTACK ===

    public void RangedAttack()
    {
        if (!canAttack || isReloading || currentAmmo <= 0)
        {
            // Click sound for empty magazine
            if (currentAmmo <= 0 && Time.time > lastFireTime + 0.2f)
            {
                // TODO: Play empty gun sound
                Debug.Log("Out of ammo! Press R to reload.");
                lastFireTime = Time.time;
            }
            return;
        }

        // Check fire rate
        float timeBetweenShots = 1f / fireRate;
        if (Time.time < lastFireTime + timeBetweenShots)
            return;

        lastFireTime = Time.time;

        // Consume ammo
        currentAmmo--;
        UpdateAmmoDisplay();

        // Calculate damage
        float damage = baseRangedDamage;

        // Apply aiming skill modifier
        if (SkillSystem.Instance != null)
        {
            float aimingMod = SkillSystem.Instance.GetSkillModifier("Aiming");
            damage *= (1f + aimingMod * 0.05f);
        }

        // Apply accuracy modifier
        float accuracy = 1f;
        if (PlayerStats.Instance != null)
        {
            accuracy = PlayerStats.Instance.GetAccuracyModifier();
        }

        // Apply weapon damage
        if (PlayerInventory.Instance != null && PlayerInventory.Instance.equippedWeapon != null)
        {
            damage += PlayerInventory.Instance.equippedWeapon.damage;
        }

        // Calculate bullet spread with accuracy
        float spread = bulletSpread * (2f - accuracy); // Better accuracy = less spread
        float randomSpread = Random.Range(-spread, spread);
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg + randomSpread;
        Vector2 shootDirection = Quaternion.Euler(0, 0, angle) * Vector2.right;

        // Spawn projectile
        if (projectilePrefab != null)
        {
            Vector3 spawnPos = transform.position + (Vector3)aimDirection * 0.5f;
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.Euler(0, 0, angle));

            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(shootDirection, projectileSpeed, damage, gameObject);
            }
            else
            {
                // Fallback: apply velocity directly
                Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = shootDirection * projectileSpeed;
                }
            }
        }
        else
        {
            // Raycast-based shooting (hitscan)
            RaycastHit2D hit = Physics2D.Raycast(transform.position, shootDirection, 100f, enemyLayer | obstacleLayer);
            if (hit.collider != null)
            {
                ZombieAI zombie = hit.collider.GetComponent<ZombieAI>();
                if (zombie != null)
                {
                    // Critical hit chance for headshots
                    bool isHeadshot = Random.value < 0.1f * accuracy;
                    float finalDamage = isHeadshot ? damage * 2f : damage;

                    zombie.TakeDamage(finalDamage, transform.position);
                    SpawnBloodEffect(hit.point);

                    if (isHeadshot)
                    {
                        Debug.Log("HEADSHOT!");
                    }
                }

                SpawnHitEffect(hit.point);
            }
        }

        // Spawn muzzle flash
        if (muzzleFlashPrefab != null)
        {
            Vector3 muzzlePos = transform.position + (Vector3)aimDirection * 0.7f;
            GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePos, Quaternion.Euler(0, 0, angle));
            Destroy(flash, 0.1f);
        }

        // Events and feedback
        onRangedAttack?.Invoke();
        onDamageDealt?.Invoke(damage);

        // Play sound (gunshot attracts zombies)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGunshot(transform.position);
        }

        // Gain aiming XP
        if (SkillSystem.Instance != null)
        {
            SkillSystem.Instance.GainXP("Aiming", 1f);
        }

        // Camera shake
        // TODO: CameraShake.Instance.Shake(recoilAmount);

        // Auto reload when empty
        if (currentAmmo <= 0)
        {
            StartReload();
        }
    }

    // === RELOAD ===

    public void StartReload()
    {
        if (isReloading || currentAmmo >= magazineSize)
            return;

        // Check if player has ammo
        if (PlayerInventory.Instance != null)
        {
            // TODO: Check for ammo items in inventory
            // For now, assume unlimited ammo
        }

        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        canAttack = false;
        onReloadStart?.Invoke();

        Debug.Log("Reloading...");

        // TODO: Play reload animation
        // animator.SetTrigger("Reload");

        yield return new WaitForSeconds(reloadTime);

        // Reload complete
        currentAmmo = magazineSize;
        UpdateAmmoDisplay();
        isReloading = false;
        canAttack = true;
        onReloadComplete?.Invoke();

        Debug.Log("Reload complete!");
    }

    // === EFFECTS ===

    private void SpawnHitEffect(Vector2 position)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }

    private void SpawnBloodEffect(Vector2 position)
    {
        if (bloodEffectPrefab != null)
        {
            GameObject effect = Instantiate(bloodEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    // === MODE SWITCHING ===

    public void SwitchCombatMode()
    {
        if (currentMode == CombatMode.Melee)
        {
            if (HasRangedWeapon())
            {
                currentMode = CombatMode.Ranged;
                Debug.Log("Switched to Ranged mode");
            }
            else
            {
                Debug.Log("No ranged weapon equipped!");
            }
        }
        else
        {
            currentMode = CombatMode.Melee;
            Debug.Log("Switched to Melee mode");
        }
    }

    private bool HasRangedWeapon()
    {
        if (PlayerInventory.Instance == null) return false;

        Item weapon = PlayerInventory.Instance.equippedWeapon;
        return weapon != null && weapon.itemType == ItemType.Weapon && weapon.damage > 0;
    }

    // === UI UPDATES ===

    private void UpdateAmmoDisplay()
    {
        onAmmoChanged?.Invoke(currentAmmo, magazineSize);
    }

    // === PUBLIC GETTERS ===

    public CombatMode GetCombatMode() => currentMode;
    public bool IsReloading() => isReloading;
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMagazineSize() => magazineSize;
    public Vector2 GetAimDirection() => aimDirection;
    public bool IsAiming() => isAiming;

    // === DEBUG ===

    void OnDrawGizmosSelected()
    {
        // Draw melee range
        Gizmos.color = Color.red;
        Vector2 attackPos = (Vector2)transform.position + aimDirection * meleeRange * 0.5f;
        Gizmos.DrawWireSphere(attackPos, meleeRange);

        // Draw aim direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)aimDirection * 2f);

        // Draw aim assist range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aimAssistRange);
    }
}

// === COMBAT MODE ENUM ===

public enum CombatMode
{
    Melee,
    Ranged
}

// === PROJECTILE CLASS ===
// Simple projectile script - create as separate file if needed

public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float damage;
    private GameObject owner;

    public void Initialize(Vector2 dir, float spd, float dmg, GameObject own)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        owner = own;

        // Auto-destroy after 5 seconds
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Don't hit owner
        if (collision.gameObject == owner)
            return;

        // Hit zombie
        ZombieAI zombie = collision.GetComponent<ZombieAI>();
        if (zombie != null)
        {
            zombie.TakeDamage(damage, owner.transform.position);
            Destroy(gameObject);
            return;
        }

        // Hit obstacle
        if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
