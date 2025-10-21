using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class WeatherSystem : MonoBehaviour
{
    // Singleton instance
    public static WeatherSystem Instance { get; private set; }

    // === WEATHER CONFIGURATION ===
    [Header("Weather Settings")]
    [Tooltip("Enable dynamic weather")]
    public bool enableDynamicWeather = true;
    [Tooltip("Weather change interval (seconds)")]
    public float weatherChangeInterval = 600f; // 10 minutes
    [Tooltip("Weather transition duration (seconds)")]
    public float transitionDuration = 30f;

    [Header("Weather Probabilities")]
    [Range(0f, 1f)] public float clearProbability = 0.4f;
    [Range(0f, 1f)] public float cloudyProbability = 0.3f;
    [Range(0f, 1f)] public float foggyProbability = 0.15f;
    [Range(0f, 1f)] public float rainyProbability = 0.15f;
    [Range(0f, 1f)] public float stormProbability = 0.05f;

    [Header("Rain Settings")]
    [Tooltip("Rain particle system")]
    public ParticleSystem rainParticles;
    [Tooltip("Rain sound")]
    public AudioClip rainSound;
    [Tooltip("Rain volume")]
    [Range(0f, 1f)] public float rainVolume = 0.5f;

    [Header("Fog Settings")]
    [Tooltip("Fog intensity")]
    [Range(0f, 1f)] public float fogIntensity = 0.5f;
    [Tooltip("Fog color")]
    public Color fogColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [Tooltip("Fog distance")]
    public float fogDistance = 20f;

    [Header("Temperature Settings")]
    [Tooltip("Base temperature")]
    public float baseTemperature = 20f; // Celsius
    [Tooltip("Temperature variation range")]
    public float temperatureRange = 15f;

    [Header("Lighting")]
    [Tooltip("Global light for weather effects")]
    public UnityEngine.Rendering.Universal.Light2D globalLight;
    [Tooltip("Clear weather light intensity")]
    public float clearLightIntensity = 1f;
    [Tooltip("Rainy weather light intensity")]
    public float rainyLightIntensity = 0.7f;
    [Tooltip("Storm weather light intensity")]
    public float stormLightIntensity = 0.5f;

    [Header("Wind Settings")]
    [Tooltip("Wind strength")]
    [Range(0f, 1f)] public float windStrength = 0.5f;
    [Tooltip("Wind direction (degrees)")]
    public float windDirection = 90f;

    // === RUNTIME VARIABLES ===
    private WeatherType currentWeather = WeatherType.Clear;
    private WeatherType targetWeather = WeatherType.Clear;
    private float weatherTimer = 0f;
    private float transitionProgress = 0f;
    private bool isTransitioning = false;

    // Temperature
    private float currentTemperature = 20f;
    private float targetTemperature = 20f;

    // Effects
    private AudioSource rainAudioSource;
    private float currentRainIntensity = 0f;
    private float currentFogDensity = 0f;
    private float currentWindSpeed = 0f;

    // Events
    [Header("Events")]
    public UnityEvent<WeatherType> onWeatherChanged;
    public UnityEvent onRainStarted;
    public UnityEvent onRainStopped;
    public UnityEvent onStormStarted;
    public UnityEvent onStormStopped;

    // Random
    private System.Random random;

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
        // Initialize random
        if (WorldGenerator.Instance != null)
        {
            random = new System.Random(WorldGenerator.Instance.GetWorldSeed());
        }
        else
        {
            random = new System.Random();
        }

        // Initialize audio
        InitializeAudio();

        // Find global light if not assigned
        if (globalLight == null)
        {
            globalLight = FindObjectOfType<UnityEngine.Rendering.Universal.Light2D>();
        }

        // Set initial weather
        currentWeather = WeatherType.Clear;
        targetWeather = WeatherType.Clear;
        currentTemperature = baseTemperature;
        targetTemperature = baseTemperature;

        // Apply initial weather
        ApplyWeatherEffects(currentWeather, 1f);

        Debug.Log("WeatherSystem initialized");
    }

    void Update()
    {
        if (!enableDynamicWeather)
            return;

        // Weather change timer
        weatherTimer += Time.deltaTime;

        if (weatherTimer >= weatherChangeInterval && !isTransitioning)
        {
            weatherTimer = 0f;
            ChangeWeather();
        }

        // Handle weather transition
        if (isTransitioning)
        {
            UpdateWeatherTransition();
        }

        // Update temperature based on time and weather
        UpdateTemperature();

        // Update wind effects
        UpdateWind();
    }

    // === AUDIO INITIALIZATION ===

    private void InitializeAudio()
    {
        // Create audio source for rain
        GameObject audioObj = new GameObject("RainAudio");
        audioObj.transform.SetParent(transform);
        rainAudioSource = audioObj.AddComponent<AudioSource>();
        rainAudioSource.clip = rainSound;
        rainAudioSource.loop = true;
        rainAudioSource.playOnAwake = false;
        rainAudioSource.volume = 0f;
    }

    // === WEATHER CHANGES ===

    private void ChangeWeather()
    {
        // Select new weather based on probabilities
        WeatherType newWeather = SelectRandomWeather();

        // Don't change if same as current
        if (newWeather == currentWeather)
        {
            // Try again
            newWeather = SelectRandomWeather();
        }

        // Influence by time of day
        if (TimeManager.Instance != null)
        {
            if (TimeManager.Instance.IsNight())
            {
                // More fog at night
                if (random.NextDouble() < 0.3f)
                {
                    newWeather = WeatherType.Foggy;
                }
            }
        }

        targetWeather = newWeather;
        isTransitioning = true;
        transitionProgress = 0f;

        Debug.Log($"Weather changing from {currentWeather} to {targetWeather}");
    }

    private WeatherType SelectRandomWeather()
    {
        float total = clearProbability + cloudyProbability + foggyProbability + rainyProbability + stormProbability;
        float randomValue = (float)random.NextDouble() * total;
        float cumulative = 0f;

        cumulative += clearProbability;
        if (randomValue < cumulative) return WeatherType.Clear;

        cumulative += cloudyProbability;
        if (randomValue < cumulative) return WeatherType.Cloudy;

        cumulative += foggyProbability;
        if (randomValue < cumulative) return WeatherType.Foggy;

        cumulative += rainyProbability;
        if (randomValue < cumulative) return WeatherType.Rainy;

        return WeatherType.Storm;
    }

    private void UpdateWeatherTransition()
    {
        transitionProgress += Time.deltaTime / transitionDuration;

        if (transitionProgress >= 1f)
        {
            // Transition complete
            transitionProgress = 1f;
            isTransitioning = false;
            currentWeather = targetWeather;

            // Trigger events
            onWeatherChanged?.Invoke(currentWeather);

            if (currentWeather == WeatherType.Rainy || currentWeather == WeatherType.Storm)
            {
                onRainStarted?.Invoke();
                if (currentWeather == WeatherType.Storm)
                {
                    onStormStarted?.Invoke();
                }
            }
        }

        // Blend between current and target weather
        float t = Mathf.SmoothStep(0f, 1f, transitionProgress);
        BlendWeatherEffects(currentWeather, targetWeather, t);
    }

    // === WEATHER EFFECTS ===

    private void ApplyWeatherEffects(WeatherType weather, float intensity)
    {
        switch (weather)
        {
            case WeatherType.Clear:
                ApplyClearWeather(intensity);
                break;
            case WeatherType.Cloudy:
                ApplyCloudyWeather(intensity);
                break;
            case WeatherType.Foggy:
                ApplyFoggyWeather(intensity);
                break;
            case WeatherType.Rainy:
                ApplyRainyWeather(intensity);
                break;
            case WeatherType.Storm:
                ApplyStormWeather(intensity);
                break;
        }
    }

    private void BlendWeatherEffects(WeatherType from, WeatherType to, float t)
    {
        // Apply from weather with decreasing intensity
        ApplyWeatherEffects(from, 1f - t);

        // Apply to weather with increasing intensity
        ApplyWeatherEffects(to, t);
    }

    private void ApplyClearWeather(float intensity)
    {
        // Clear skies - bright lighting
        if (globalLight != null)
        {
            float targetIntensity = clearLightIntensity;
            if (TimeManager.Instance != null && TimeManager.Instance.IsNight())
            {
                targetIntensity *= 0.3f;
            }
            globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, intensity);
        }

        // No rain
        SetRainIntensity(0f);

        // No fog
        SetFogDensity(0f);

        // Warm temperature
        targetTemperature = baseTemperature + 5f;
    }

    private void ApplyCloudyWeather(float intensity)
    {
        // Darker lighting
        if (globalLight != null)
        {
            float targetIntensity = clearLightIntensity * 0.8f;
            if (TimeManager.Instance != null && TimeManager.Instance.IsNight())
            {
                targetIntensity *= 0.3f;
            }
            globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, intensity);
        }

        // No rain
        SetRainIntensity(0f);

        // Light fog
        SetFogDensity(0.1f * intensity);

        // Cooler temperature
        targetTemperature = baseTemperature;
    }

    private void ApplyFoggyWeather(float intensity)
    {
        // Dim lighting
        if (globalLight != null)
        {
            float targetIntensity = clearLightIntensity * 0.6f;
            if (TimeManager.Instance != null && TimeManager.Instance.IsNight())
            {
                targetIntensity *= 0.3f;
            }
            globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, intensity);
        }

        // No rain
        SetRainIntensity(0f);

        // Heavy fog
        SetFogDensity(fogIntensity * intensity);

        // Cool temperature
        targetTemperature = baseTemperature - 5f;

        // Reduce visibility for player
        // This would affect zombie detection range, etc.
    }

    private void ApplyRainyWeather(float intensity)
    {
        // Dark lighting
        if (globalLight != null)
        {
            float targetIntensity = rainyLightIntensity;
            if (TimeManager.Instance != null && TimeManager.Instance.IsNight())
            {
                targetIntensity *= 0.3f;
            }
            globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, intensity);
        }

        // Rain
        SetRainIntensity(0.6f * intensity);

        // Light fog
        SetFogDensity(0.2f * intensity);

        // Cold temperature
        targetTemperature = baseTemperature - 8f;

        // Wind
        windStrength = 0.5f * intensity;

        // Make player wet (reduces temperature)
        if (PlayerStats.Instance != null && intensity > 0.5f)
        {
            if (!PlayerStats.Instance.isWet)
            {
                PlayerStats.Instance.ApplyStatusEffect("wet", 300f); // 5 minutes
            }
        }
    }

    private void ApplyStormWeather(float intensity)
    {
        // Very dark lighting
        if (globalLight != null)
        {
            float targetIntensity = stormLightIntensity;
            if (TimeManager.Instance != null && TimeManager.Instance.IsNight())
            {
                targetIntensity *= 0.3f;
            }
            globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, intensity);
        }

        // Heavy rain
        SetRainIntensity(1f * intensity);

        // Moderate fog
        SetFogDensity(0.3f * intensity);

        // Very cold temperature
        targetTemperature = baseTemperature - 12f;

        // Strong wind
        windStrength = 1f * intensity;

        // Lightning flashes (random)
        if (random.NextDouble() < 0.01f) // 1% chance per frame
        {
            StartCoroutine(LightningFlash());
        }

        // Make player wet
        if (PlayerStats.Instance != null && intensity > 0.5f)
        {
            if (!PlayerStats.Instance.isWet)
            {
                PlayerStats.Instance.ApplyStatusEffect("wet", 600f); // 10 minutes
            }
        }

        // Reduce visibility significantly
    }

    // === EFFECT HELPERS ===

    private void SetRainIntensity(float intensity)
    {
        currentRainIntensity = Mathf.Lerp(currentRainIntensity, intensity, Time.deltaTime * 2f);

        // Rain particles
        if (rainParticles != null)
        {
            var emission = rainParticles.emission;
            emission.rateOverTime = currentRainIntensity * 1000f; // Max 1000 particles/sec

            if (currentRainIntensity > 0.01f && !rainParticles.isPlaying)
            {
                rainParticles.Play();
            }
            else if (currentRainIntensity <= 0.01f && rainParticles.isPlaying)
            {
                rainParticles.Stop();
            }
        }

        // Rain audio
        if (rainAudioSource != null && rainSound != null)
        {
            rainAudioSource.volume = currentRainIntensity * rainVolume;

            if (currentRainIntensity > 0.01f && !rainAudioSource.isPlaying)
            {
                rainAudioSource.Play();
            }
            else if (currentRainIntensity <= 0.01f && rainAudioSource.isPlaying)
            {
                rainAudioSource.Stop();
            }
        }
    }

    private void SetFogDensity(float density)
    {
        currentFogDensity = Mathf.Lerp(currentFogDensity, density, Time.deltaTime * 2f);

        // Unity fog (if using)
        RenderSettings.fog = currentFogDensity > 0.01f;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = currentFogDensity * 0.1f;

        // TODO: Implement fog shader or post-processing effect for 2D
    }

    private IEnumerator LightningFlash()
    {
        if (globalLight == null)
            yield break;

        float originalIntensity = globalLight.intensity;

        // Flash bright
        globalLight.intensity = clearLightIntensity * 1.5f;
        yield return new WaitForSeconds(0.1f);

        // Back to normal
        globalLight.intensity = originalIntensity;

        // Thunder sound
        if (AudioManager.Instance != null)
        {
            // TODO: Play thunder sound
            // AudioManager.Instance.PlaySFX("thunder", 1f);
        }

        Debug.Log("Lightning flash!");
    }

    // === TEMPERATURE ===

    private void UpdateTemperature()
    {
        // Gradually change towards target temperature
        currentTemperature = Mathf.Lerp(currentTemperature, targetTemperature, Time.deltaTime * 0.1f);

        // Time of day influences temperature
        if (TimeManager.Instance != null)
        {
            float hour = TimeManager.Instance.GetCurrentHour();

            // Hottest at 14:00, coldest at 04:00
            float timeModifier = Mathf.Sin((hour - 4f) / 24f * Mathf.PI * 2f) * temperatureRange * 0.5f;
            currentTemperature += timeModifier * Time.deltaTime * 0.1f;
        }

        // Clamp temperature
        currentTemperature = Mathf.Clamp(currentTemperature, -20f, 45f);
    }

    // === WIND ===

    private void UpdateWind()
    {
        // Smooth wind speed changes
        float targetWindSpeed = windStrength;
        currentWindSpeed = Mathf.Lerp(currentWindSpeed, targetWindSpeed, Time.deltaTime);

        // Wind affects particles, etc.
        // TODO: Implement wind effects on objects
    }

    // === PUBLIC METHODS ===

    public void SetWeather(WeatherType weather)
    {
        targetWeather = weather;
        isTransitioning = true;
        transitionProgress = 0f;
    }

    public void ForceWeather(WeatherType weather)
    {
        currentWeather = weather;
        targetWeather = weather;
        isTransitioning = false;
        ApplyWeatherEffects(weather, 1f);
        onWeatherChanged?.Invoke(weather);
    }

    public WeatherType GetCurrentWeather()
    {
        return currentWeather;
    }

    public float GetCurrentTemperature()
    {
        return currentTemperature;
    }

    public float GetVisibilityMultiplier()
    {
        // Reduced visibility in fog and rain
        float visibility = 1f;

        switch (currentWeather)
        {
            case WeatherType.Clear:
                visibility = 1f;
                break;
            case WeatherType.Cloudy:
                visibility = 0.9f;
                break;
            case WeatherType.Foggy:
                visibility = 0.5f;
                break;
            case WeatherType.Rainy:
                visibility = 0.7f;
                break;
            case WeatherType.Storm:
                visibility = 0.4f;
                break;
        }

        // Night reduces visibility further
        if (TimeManager.Instance != null && TimeManager.Instance.IsNight())
        {
            visibility *= 0.6f;
        }

        return visibility;
    }

    public bool IsRaining()
    {
        return currentWeather == WeatherType.Rainy || currentWeather == WeatherType.Storm;
    }

    public bool IsStorming()
    {
        return currentWeather == WeatherType.Storm;
    }

    public float GetRainIntensity()
    {
        return currentRainIntensity;
    }

    public float GetWindSpeed()
    {
        return currentWindSpeed;
    }

    public Vector2 GetWindDirection()
    {
        float radians = windDirection * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * currentWindSpeed;
    }

    // === DEBUG ===

    [ContextMenu("Debug: Force Clear Weather")]
    public void DebugForceClear()
    {
        ForceWeather(WeatherType.Clear);
    }

    [ContextMenu("Debug: Force Rain")]
    public void DebugForceRain()
    {
        ForceWeather(WeatherType.Rainy);
    }

    [ContextMenu("Debug: Force Storm")]
    public void DebugForceStorm()
    {
        ForceWeather(WeatherType.Storm);
    }

    [ContextMenu("Debug: Force Fog")]
    public void DebugForceFog()
    {
        ForceWeather(WeatherType.Foggy);
    }

    [ContextMenu("Debug: Print Weather Info")]
    public void DebugPrintInfo()
    {
        Debug.Log("=== WEATHER SYSTEM INFO ===");
        Debug.Log($"Current Weather: {currentWeather}");
        Debug.Log($"Target Weather: {targetWeather}");
        Debug.Log($"Is Transitioning: {isTransitioning}");
        Debug.Log($"Temperature: {currentTemperature:F1}°C");
        Debug.Log($"Rain Intensity: {currentRainIntensity:F2}");
        Debug.Log($"Fog Density: {currentFogDensity:F2}");
        Debug.Log($"Wind Speed: {currentWindSpeed:F2}");
        Debug.Log($"Visibility: {GetVisibilityMultiplier():F2}");
    }
}

// === ENUMS ===

public enum WeatherType
{
    Clear,
    Cloudy,
    Foggy,
    Rainy,
    Storm
}
