using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class TimeManager : MonoBehaviour
{
    // Singleton instance
    public static TimeManager Instance { get; private set; }

    // Time settings
    [Header("Time Configuration")]
    [Tooltip("Length of a full day in real-time seconds")]
    public float dayLengthInSeconds = 1440f; // 24 minutes for a full day (60s * 24)
    [Tooltip("Starting hour of the day (0-23)")]
    public float startingHour = 8f; // Start at 8 AM
    [Tooltip("Starting day number")]
    public int startingDay = 1;

    // Current time state
    private float currentTime = 0f; // Time elapsed in seconds since midnight
    private int currentDay = 1;
    private bool isDaytime = true;
    private float previousHour = 0f;

    // Lighting settings
    [Header("Lighting Configuration")]
    [Tooltip("Global light for day/night cycle")]
    public Light2D globalLight;
    [Tooltip("Day light intensity")]
    public float dayLightIntensity = 1.0f;
    [Tooltip("Night light intensity")]
    public float nightLightIntensity = 0.2f;
    [Tooltip("Dawn/dusk transition duration in game hours")]
    public float transitionDuration = 1f;
    [Tooltip("Day light color")]
    public Color dayLightColor = new Color(1f, 0.95f, 0.8f);
    [Tooltip("Night light color")]
    public Color nightLightColor = new Color(0.3f, 0.3f, 0.5f);

    // Time of day thresholds
    [Header("Time Thresholds")]
    public float dawnHour = 6f;
    public float dayHour = 7f;
    public float duskHour = 18f;
    public float nightHour = 19f;

    // Events
    [Header("Events")]
    public UnityEvent onDayStart;
    public UnityEvent onNightStart;
    public UnityEvent onHourPassed;
    public UnityEvent onDayPassed;

    // Time multiplier for speeding up/slowing down time
    private float timeMultiplier = 1f;

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

        // Find global light if not assigned
        if (globalLight == null)
        {
            globalLight = FindObjectOfType<Light2D>();
        }
    }

    void Start()
    {
        // Initialize time
        currentDay = startingDay;
        currentTime = startingHour * (dayLengthInSeconds / 24f);
        previousHour = GetCurrentHour();
        UpdateDayNightState();
        UpdateLighting();
    }

    void Update()
    {
        // Advance time with multiplier
        currentTime += Time.deltaTime * timeMultiplier;

        // Check if a full day has passed
        if (currentTime >= dayLengthInSeconds)
        {
            currentTime -= dayLengthInSeconds;
            currentDay++;
            onDayPassed?.Invoke();
            Debug.Log($"Day {currentDay} has begun!");
        }

        // Check if an hour has passed
        float currentHour = GetCurrentHour();
        if (Mathf.Floor(currentHour) != Mathf.Floor(previousHour))
        {
            onHourPassed?.Invoke();
        }
        previousHour = currentHour;

        // Update day/night state
        UpdateDayNightState();

        // Update lighting smoothly
        UpdateLighting();
    }

    private void UpdateDayNightState()
    {
        float hour = GetCurrentHour();
        bool newIsDaytime = (hour >= dayHour && hour < duskHour);

        if (newIsDaytime != isDaytime)
        {
            isDaytime = newIsDaytime;
            if (isDaytime)
            {
                onDayStart?.Invoke();
                Debug.Log($"Day {currentDay} - Dawn has arrived at {hour:F1}:00");
            }
            else
            {
                onNightStart?.Invoke();
                Debug.Log($"Day {currentDay} - Night has fallen at {hour:F1}:00");
            }
        }
    }

    private void UpdateLighting()
    {
        if (globalLight == null) return;

        float hour = GetCurrentHour();
        float targetIntensity = dayLightIntensity;
        Color targetColor = dayLightColor;

        // Dawn transition (6 AM - 7 AM)
        if (hour >= dawnHour && hour < dayHour)
        {
            float t = (hour - dawnHour) / transitionDuration;
            targetIntensity = Mathf.Lerp(nightLightIntensity, dayLightIntensity, t);
            targetColor = Color.Lerp(nightLightColor, dayLightColor, t);
        }
        // Full day (7 AM - 6 PM)
        else if (hour >= dayHour && hour < duskHour)
        {
            targetIntensity = dayLightIntensity;
            targetColor = dayLightColor;
        }
        // Dusk transition (6 PM - 7 PM)
        else if (hour >= duskHour && hour < nightHour)
        {
            float t = (hour - duskHour) / transitionDuration;
            targetIntensity = Mathf.Lerp(dayLightIntensity, nightLightIntensity, t);
            targetColor = Color.Lerp(dayLightColor, nightLightColor, t);
        }
        // Full night (7 PM - 6 AM)
        else
        {
            targetIntensity = nightLightIntensity;
            targetColor = nightLightColor;
        }

        // Smoothly lerp to target values
        globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, Time.deltaTime * 2f);
        globalLight.color = Color.Lerp(globalLight.color, targetColor, Time.deltaTime * 2f);
    }

    // Public getters
    public float GetCurrentHour()
    {
        return (currentTime / dayLengthInSeconds) * 24f;
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }

    public bool IsDaytime()
    {
        return isDaytime;
    }

    public bool IsNight()
    {
        return !isDaytime;
    }

    public float GetTimeOfDay()
    {
        // Returns normalized time (0 = midnight, 0.5 = noon, 1 = midnight)
        return currentTime / dayLengthInSeconds;
    }

    public string GetTimeString()
    {
        float hour = GetCurrentHour();
        int hourInt = Mathf.FloorToInt(hour);
        int minutes = Mathf.FloorToInt((hour - hourInt) * 60f);
        return $"{hourInt:00}:{minutes:00}";
    }

    // Time manipulation
    public void SetTimeMultiplier(float multiplier)
    {
        timeMultiplier = Mathf.Max(0f, multiplier);
    }

    public void SetCurrentHour(float hour)
    {
        currentTime = (hour / 24f) * dayLengthInSeconds;
        UpdateDayNightState();
        UpdateLighting();
    }

    public void SetCurrentDay(int day)
    {
        currentDay = Mathf.Max(1, day);
    }

    public void AdvanceTime(float hours)
    {
        currentTime += (hours / 24f) * dayLengthInSeconds;
        if (currentTime >= dayLengthInSeconds)
        {
            currentTime -= dayLengthInSeconds;
            currentDay++;
            onDayPassed?.Invoke();
        }
        UpdateDayNightState();
        UpdateLighting();
    }

    public void ResetTime()
    {
        currentDay = startingDay;
        currentTime = startingHour * (dayLengthInSeconds / 24f);
        previousHour = GetCurrentHour();
        UpdateDayNightState();
        UpdateLighting();
    }

    // Save/Load support
    [System.Serializable]
    public class TimeData
    {
        public int currentDay;
        public float currentTime;
    }

    public TimeData GetSaveData()
    {
        return new TimeData
        {
            currentDay = currentDay,
            currentTime = currentTime
        };
    }

    public void LoadSaveData(TimeData data)
    {
        if (data != null)
        {
            currentDay = data.currentDay;
            currentTime = data.currentTime;
            previousHour = GetCurrentHour();
            UpdateDayNightState();
            UpdateLighting();
        }
    }

    // Integration with zombie spawner
    public float GetZombieSpawnMultiplier()
    {
        // More zombies at night
        if (IsNight())
        {
            return 2.0f;
        }
        return 1.0f;
    }

    // Integration with weather system
    public float GetVisibilityRange()
    {
        // Reduced visibility at night
        float hour = GetCurrentHour();
        if (hour >= nightHour || hour < dawnHour)
        {
            return 0.5f; // 50% visibility at night
        }
        else if ((hour >= duskHour && hour < nightHour) || (hour >= dawnHour && hour < dayHour))
        {
            return 0.75f; // 75% visibility during transitions
        }
        return 1.0f; // 100% visibility during day
    }
}
