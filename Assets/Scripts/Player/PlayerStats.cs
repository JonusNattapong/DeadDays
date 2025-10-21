using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    // Singleton instance
    public static PlayerStats Instance { get; private set; }

    // === CORE STATS ===
    [Header("Core Stats")]
    [Range(0f, 100f)] public float health = 100f;
    [Range(0f, 100f)] public float hunger = 0f;
    [Range(0f, 100f)] public float thirst = 0f;
    [Range(0f, 100f)] public float fatigue = 0f;
    [Range(0f, 100f)] public float infection = 0f;
    [Range(0f, 100f)] public float panic = 0f;
    [Range(0f, 100f)] public float temperature = 70f;

    // Maximum values
    [Header("Maximum Values")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float maxFatigue = 100f;
    public float maxInfection = 100f;
    public float maxPanic = 100f;
    public float maxTemperature = 100f;

    // Decay rates (per real-time second)
    [Header("Decay Rates")]
    [Tooltip("Hunger increase per second")]
    public float hungerDecayRate = 0.05f;
    [Tooltip("Thirst increase per second")]
    public float thirstDecayRate = 0.08f;
    [Tooltip("Fatigue increase per second")]
    public float fatigueDecayRate = 0.03f;
    [Tooltip("Temperature decay rate towards ambient")]
    public float temperatureChangeRate = 0.5f;
    [Tooltip("Infection progression rate per second")]
    public float infectionProgressRate = 0.02f;
    [Tooltip("Panic decay rate per second (when not in danger)")]
    public float panicDecayRate = 0.1f;

    // Effect thresholds
    [Header("Effect Thresholds")]
    public float hungerDamageThreshold = 80f;
    public float thirstDamageThreshold = 90f;
    public float fatigueDamageThreshold = 85f;
    public float infectionDamageThreshold = 50f;
    public float hypothermiaThreshold = 20f;
    public float hyperthermiaThreshold = 80f;

    // Effect values
    [Header("Effect Values")]
    public float hungerDamagePerSecond = 0.5f;
    public float thirstDamagePerSecond = 1f;
    public float fatigueDamagePerSecond = 0.3f;
    public float infectionDamagePerSecond = 0.4f;
    public float temperatureDamagePerSecond = 0.5f;

    // Status effects
    [Header("Status Effects")]
    public bool isBleeding = false;
    public bool isPoisoned = false;
    public bool isFractured = false;
    public bool isWet = false;
    public bool isCold = false;
    public bool isHot = false;
    public bool isInfected = false;

    // Modifiers
    [Header("Stat Modifiers")]
    public float movementSpeedModifier = 1f;
    public float attackDamageModifier = 1f;
    public float accuracyModifier = 1f;
    public float staminaModifier = 1f;

    // Events
    public UnityEvent onHealthChanged;
    public UnityEvent onPlayerDeath;
    public UnityEvent<string> onStatusEffectApplied;
    public UnityEvent<string> onStatusEffectRemoved;

    // Runtime variables
    private bool isDead = false;
    private float ambientTemperature = 70f;
    private float lastDamageTime = 0f;
    private Dictionary<string, float> statusEffectTimers = new Dictionary<string, float>();

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
        // Initialize stats
        health = maxHealth;
        hunger = 0f;
        thirst = 0f;
        fatigue = 0f;
        infection = 0f;
        panic = 0f;
        temperature = 70f;

        // Get ambient temperature from environment
        UpdateAmbientTemperature();
    }

    void Update()
    {
        if (isDead) return;

        // Decay stats over time
        UpdateStatDecay();

        // Apply status effects
        UpdateStatusEffects();

        // Apply damage from stats
        ApplyStatDamage();

        // Update modifiers based on stats
        UpdateModifiers();

        // Check for death
        CheckDeath();
    }

    // === STAT DECAY ===

    private void UpdateStatDecay()
    {
        // Hunger increases over time
        hunger = Mathf.Clamp(hunger + hungerDecayRate * Time.deltaTime, 0f, maxHunger);

        // Thirst increases over time
        thirst = Mathf.Clamp(thirst + thirstDecayRate * Time.deltaTime, 0f, maxThirst);

        // Fatigue increases over time (faster when running)
        float fatigueRate = fatigueDecayRate;
        if (PlayerController.Instance != null && PlayerController.Instance.IsSprinting())
        {
            fatigueRate *= 3f; // Fatigue increases 3x faster when sprinting
        }
        fatigue = Mathf.Clamp(fatigue + fatigueRate * Time.deltaTime, 0f, maxFatigue);

        // Infection progresses if infected
        if (isInfected || infection > 0f)
        {
            infection = Mathf.Clamp(infection + infectionProgressRate * Time.deltaTime, 0f, maxInfection);
        }

        // Panic decreases naturally over time (when not in danger)
        if (!IsInDanger())
        {
            panic = Mathf.Clamp(panic - panicDecayRate * Time.deltaTime, 0f, maxPanic);
        }

        // Temperature moves towards ambient temperature
        UpdateAmbientTemperature();
        if (temperature < ambientTemperature)
        {
            temperature = Mathf.Clamp(temperature + temperatureChangeRate * Time.deltaTime, 0f, ambientTemperature);
        }
        else if (temperature > ambientTemperature)
        {
            temperature = Mathf.Clamp(temperature - temperatureChangeRate * Time.deltaTime, ambientTemperature, maxTemperature);
        }
    }

    // === STATUS EFFECTS ===

    private void UpdateStatusEffects()
    {
        // Bleeding
        if (isBleeding)
        {
            TakeDamage(0.5f * Time.deltaTime, "bleeding");
        }

        // Poisoned
        if (isPoisoned)
        {
            TakeDamage(0.3f * Time.deltaTime, "poison");
            infection += 0.05f * Time.deltaTime;
        }

        // Wet (increases cold susceptibility)
        if (isWet)
        {
            temperature -= 0.2f * Time.deltaTime;
        }

        // Update temperature status
        isCold = temperature < hypothermiaThreshold;
        isHot = temperature > hyperthermiaThreshold;
        isInfected = infection > 10f;

        // Update status effect timers
        List<string> expiredEffects = new List<string>();
        foreach (var effect in statusEffectTimers.Keys)
        {
            statusEffectTimers[effect] -= Time.deltaTime;
            if (statusEffectTimers[effect] <= 0f)
            {
                expiredEffects.Add(effect);
            }
        }

        // Remove expired effects
        foreach (var effect in expiredEffects)
        {
            RemoveStatusEffect(effect);
        }
    }

    private void ApplyStatDamage()
    {
        // Hunger damage
        if (hunger > hungerDamageThreshold)
        {
            TakeDamage(hungerDamagePerSecond * Time.deltaTime, "starvation");
        }

        // Thirst damage
        if (thirst > thirstDamageThreshold)
        {
            TakeDamage(thirstDamagePerSecond * Time.deltaTime, "dehydration");
        }

        // Fatigue damage
        if (fatigue > fatigueDamageThreshold)
        {
            TakeDamage(fatigueDamagePerSecond * Time.deltaTime, "exhaustion");
        }

        // Infection damage
        if (infection > infectionDamageThreshold)
        {
            TakeDamage(infectionDamagePerSecond * Time.deltaTime, "infection");
        }

        // Temperature damage
        if (isCold)
        {
            TakeDamage(temperatureDamagePerSecond * Time.deltaTime, "hypothermia");
        }
        if (isHot)
        {
            TakeDamage(temperatureDamagePerSecond * Time.deltaTime, "hyperthermia");
        }
    }

    // === MODIFIERS ===

    private void UpdateModifiers()
    {
        // Base modifiers
        movementSpeedModifier = 1f;
        attackDamageModifier = 1f;
        accuracyModifier = 1f;
        staminaModifier = 1f;

        // Hunger effects
        if (hunger > 60f)
        {
            movementSpeedModifier *= 0.9f;
            attackDamageModifier *= 0.85f;
        }
        if (hunger > 80f)
        {
            movementSpeedModifier *= 0.8f;
            attackDamageModifier *= 0.7f;
            staminaModifier *= 0.7f;
        }

        // Thirst effects
        if (thirst > 70f)
        {
            accuracyModifier *= 0.85f;
        }
        if (thirst > 90f)
        {
            movementSpeedModifier *= 0.75f;
            accuracyModifier *= 0.6f;
        }

        // Fatigue effects
        if (fatigue > 60f)
        {
            movementSpeedModifier *= 0.9f;
            accuracyModifier *= 0.9f;
        }
        if (fatigue > 80f)
        {
            movementSpeedModifier *= 0.7f;
            accuracyModifier *= 0.7f;
            attackDamageModifier *= 0.8f;
            staminaModifier *= 0.5f;
        }

        // Panic effects
        if (panic > 50f)
        {
            accuracyModifier *= 0.8f;
        }
        if (panic > 70f)
        {
            accuracyModifier *= 0.6f;
            movementSpeedModifier *= 1.1f; // Panic makes you run faster
            staminaModifier *= 0.7f;
        }

        // Infection effects
        if (infection > 30f)
        {
            movementSpeedModifier *= 0.9f;
            attackDamageModifier *= 0.9f;
        }
        if (infection > 60f)
        {
            movementSpeedModifier *= 0.75f;
            attackDamageModifier *= 0.75f;
            accuracyModifier *= 0.8f;
        }

        // Temperature effects
        if (isCold)
        {
            movementSpeedModifier *= 0.8f;
            accuracyModifier *= 0.7f;
        }
        if (isHot)
        {
            staminaModifier *= 0.7f;
        }

        // Fractured (broken bones)
        if (isFractured)
        {
            movementSpeedModifier *= 0.5f;
            attackDamageModifier *= 0.6f;
        }

        // Clamp modifiers
        movementSpeedModifier = Mathf.Clamp(movementSpeedModifier, 0.1f, 2f);
        attackDamageModifier = Mathf.Clamp(attackDamageModifier, 0.1f, 3f);
        accuracyModifier = Mathf.Clamp(accuracyModifier, 0.1f, 2f);
        staminaModifier = Mathf.Clamp(staminaModifier, 0.1f, 2f);
    }

    // === DAMAGE AND HEALING ===

    public void TakeDamage(float damage, string source = "unknown")
    {
        if (isDead) return;

        health -= damage;
        health = Mathf.Clamp(health, 0f, maxHealth);
        lastDamageTime = Time.time;

        onHealthChanged?.Invoke();

        // Add panic when taking damage
        IncreasePanic(damage * 0.5f);

        // Log damage
        if (damage > 0.1f)
        {
            Debug.Log($"Player took {damage:F1} damage from {source}. Health: {health:F1}");
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        health = Mathf.Clamp(health + amount, 0f, maxHealth);
        onHealthChanged?.Invoke();
        Debug.Log($"Player healed {amount:F1} HP. Health: {health:F1}");
    }

    // === STAT MANAGEMENT ===

    public void Eat(float foodValue, float nutritionQuality = 1f)
    {
        hunger = Mathf.Clamp(hunger - foodValue, 0f, maxHunger);

        // Quality affects other stats
        if (nutritionQuality > 0.5f)
        {
            Heal(5f * nutritionQuality);
        }

        Debug.Log($"Ate food. Hunger reduced by {foodValue:F1}. Current: {hunger:F1}");
    }

    public void Drink(float waterValue)
    {
        thirst = Mathf.Clamp(thirst - waterValue, 0f, maxThirst);
        Debug.Log($"Drank water. Thirst reduced by {waterValue:F1}. Current: {thirst:F1}");
    }

    public void Sleep(float sleepValue)
    {
        fatigue = Mathf.Clamp(fatigue - sleepValue, 0f, maxFatigue);

        // Sleeping also heals slightly
        Heal(sleepValue * 0.2f);

        // Reduce panic while sleeping
        panic = Mathf.Clamp(panic - sleepValue * 0.5f, 0f, maxPanic);

        Debug.Log($"Slept. Fatigue reduced by {sleepValue:F1}. Current: {fatigue:F1}");
    }

    public void IncreasePanic(float amount)
    {
        panic = Mathf.Clamp(panic + amount, 0f, maxPanic);
    }

    public void DecreasePanic(float amount)
    {
        panic = Mathf.Clamp(panic - amount, 0f, maxPanic);
    }

    public void ApplyMedicine(string medicineType)
    {
        switch (medicineType.ToLower())
        {
            case "bandage":
                isBleeding = false;
                Heal(10f);
                break;
            case "antibiotics":
                infection = Mathf.Clamp(infection - 30f, 0f, maxInfection);
                isPoisoned = false;
                break;
            case "painkillers":
                DecreasePanic(30f);
                break;
            case "splint":
                isFractured = false;
                break;
            case "antidote":
                isPoisoned = false;
                infection = Mathf.Clamp(infection - 20f, 0f, maxInfection);
                break;
        }

        Debug.Log($"Applied medicine: {medicineType}");
    }

    // === STATUS EFFECTS ===

    public void ApplyStatusEffect(string effectName, float duration = 0f)
    {
        switch (effectName.ToLower())
        {
            case "bleeding":
                isBleeding = true;
                break;
            case "poisoned":
                isPoisoned = true;
                break;
            case "fractured":
                isFractured = true;
                break;
            case "wet":
                isWet = true;
                break;
            case "infected":
                isInfected = true;
                infection = Mathf.Max(infection, 10f);
                break;
        }

        if (duration > 0f)
        {
            statusEffectTimers[effectName] = duration;
        }

        onStatusEffectApplied?.Invoke(effectName);
        Debug.Log($"Status effect applied: {effectName}");
    }

    public void RemoveStatusEffect(string effectName)
    {
        switch (effectName.ToLower())
        {
            case "bleeding":
                isBleeding = false;
                break;
            case "poisoned":
                isPoisoned = false;
                break;
            case "fractured":
                isFractured = false;
                break;
            case "wet":
                isWet = false;
                break;
            case "infected":
                isInfected = false;
                break;
        }

        if (statusEffectTimers.ContainsKey(effectName))
        {
            statusEffectTimers.Remove(effectName);
        }

        onStatusEffectRemoved?.Invoke(effectName);
        Debug.Log($"Status effect removed: {effectName}");
    }

    // === UTILITY METHODS ===

    private void UpdateAmbientTemperature()
    {
        ambientTemperature = 70f; // Default

        // Adjust based on time of day
        if (TimeManager.Instance != null)
        {
            float hour = TimeManager.Instance.GetCurrentHour();

            // Colder at night
            if (hour >= 20f || hour < 6f)
            {
                ambientTemperature = 40f;
            }
            else if (hour >= 6f && hour < 10f)
            {
                ambientTemperature = 55f;
            }
            else if (hour >= 10f && hour < 16f)
            {
                ambientTemperature = 75f;
            }
            else
            {
                ambientTemperature = 60f;
            }
        }

        // Adjust based on weather
        // TODO: Integrate with WeatherSystem

        // Adjust based on location (indoors vs outdoors)
        // TODO: Integrate with building detection
    }

    private bool IsInDanger()
    {
        // Check if player took damage recently
        if (Time.time - lastDamageTime < 5f)
        {
            return true;
        }

        // Check if zombies are nearby
        // TODO: Integrate with zombie detection system

        return false;
    }

    private void CheckDeath()
    {
        if (health <= 0f && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        health = 0f;

        Debug.Log("Player has died!");
        onPlayerDeath?.Invoke();

        // Trigger game over
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameManager.GameState.GameOver);
        }
    }

    // === PUBLIC GETTERS ===

    public bool IsDead() => isDead;
    public float GetHealthPercentage() => health / maxHealth;
    public float GetHungerPercentage() => hunger / maxHunger;
    public float GetThirstPercentage() => thirst / maxThirst;
    public float GetFatiguePercentage() => fatigue / maxFatigue;
    public float GetInfectionPercentage() => infection / maxInfection;
    public float GetPanicPercentage() => panic / maxPanic;
    public float GetTemperaturePercentage() => temperature / maxTemperature;

    public float GetMovementSpeedModifier() => movementSpeedModifier;
    public float GetAttackDamageModifier() => attackDamageModifier;
    public float GetAccuracyModifier() => accuracyModifier;
    public float GetStaminaModifier() => staminaModifier;
}
