using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // Singleton instance
    public static AudioManager Instance { get; private set; }

    // Audio sources
    [Header("Audio Sources")]
    [Tooltip("AudioSource for background music")]
    public AudioSource musicSource;
    [Tooltip("AudioSource for ambient sounds")]
    public AudioSource ambientSource;
    [Tooltip("Prefab for 3D sound effects")]
    public GameObject sfxSourcePrefab;

    // Volume controls
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float ambientVolume = 0.6f;

    // Audio clips
    [Header("Music Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip dayMusic;
    public AudioClip nightMusic;
    public AudioClip combatMusic;
    public AudioClip gameOverMusic;

    [Header("Ambient Clips")]
    public AudioClip dayAmbient;
    public AudioClip nightAmbient;
    public AudioClip rainAmbient;

    [Header("SFX Clips")]
    public AudioClip footstepClip;
    public AudioClip zombieGroanClip;
    public AudioClip doorOpenClip;
    public AudioClip itemPickupClip;
    public AudioClip craftingClip;
    public AudioClip hitClip;
    public AudioClip gunShotClip;

    // Sound propagation settings
    [Header("Sound Propagation")]
    [Tooltip("Maximum range for sound propagation")]
    public float maxSoundRange = 50f;
    [Tooltip("Sound falloff rate")]
    public float soundFalloff = 2f;

    // Runtime variables
    private Dictionary<string, AudioClip> sfxLibrary = new Dictionary<string, AudioClip>();
    private List<AudioSource> activeSFXSources = new List<AudioSource>();
    private float musicFadeSpeed = 1f;
    private AudioClip targetMusicClip;
    private bool isFadingMusic = false;
    private string currentMusicTrack = "";

    // Object pooling for SFX
    private Queue<AudioSource> sfxSourcePool = new Queue<AudioSource>();
    private int initialPoolSize = 10;

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

        // Initialize audio sources if not assigned
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (ambientSource == null)
        {
            GameObject ambientObj = new GameObject("AmbientSource");
            ambientObj.transform.SetParent(transform);
            ambientSource = ambientObj.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
        }

        // Initialize SFX object pool
        InitializeSFXPool();

        // Build SFX library
        BuildSFXLibrary();
    }

    void Start()
    {
        // Subscribe to time events
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.onDayStart.AddListener(OnDayStart);
            TimeManager.Instance.onNightStart.AddListener(OnNightStart);
        }
    }

    void Update()
    {
        // Apply volume changes
        musicSource.volume = masterVolume * musicVolume;
        ambientSource.volume = masterVolume * ambientVolume;

        // Handle music fading
        if (isFadingMusic)
        {
            HandleMusicFade();
        }

        // Clean up finished SFX sources
        CleanupSFXSources();
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.onDayStart.RemoveListener(OnDayStart);
            TimeManager.Instance.onNightStart.RemoveListener(OnNightStart);
        }
    }

    // === MUSIC MANAGEMENT ===

    public void PlayMusic(AudioClip clip, bool fade = true)
    {
        if (clip == null || currentMusicTrack == clip.name) return;

        if (fade)
        {
            targetMusicClip = clip;
            isFadingMusic = true;
        }
        else
        {
            musicSource.clip = clip;
            musicSource.Play();
            currentMusicTrack = clip.name;
        }
    }

    public void PlayMusic(string trackName, bool fade = true)
    {
        AudioClip clip = GetMusicClip(trackName);
        if (clip != null)
        {
            PlayMusic(clip, fade);
        }
    }

    public void StopMusic(bool fade = true)
    {
        if (fade)
        {
            targetMusicClip = null;
            isFadingMusic = true;
        }
        else
        {
            musicSource.Stop();
            currentMusicTrack = "";
        }
    }

    private void HandleMusicFade()
    {
        if (targetMusicClip != null)
        {
            // Fade out current, then fade in new
            if (musicSource.volume > 0.01f)
            {
                musicSource.volume -= Time.deltaTime * musicFadeSpeed;
            }
            else
            {
                musicSource.clip = targetMusicClip;
                musicSource.Play();
                currentMusicTrack = targetMusicClip.name;
                musicSource.volume = 0f;
                targetMusicClip = null;
            }
        }
        else
        {
            // Just fade out
            if (musicSource.volume > 0.01f)
            {
                musicSource.volume -= Time.deltaTime * musicFadeSpeed;
            }
            else
            {
                musicSource.Stop();
                currentMusicTrack = "";
                isFadingMusic = false;
            }
        }
    }

    private AudioClip GetMusicClip(string trackName)
    {
        switch (trackName.ToLower())
        {
            case "mainmenu": return mainMenuMusic;
            case "day": return dayMusic;
            case "night": return nightMusic;
            case "combat": return combatMusic;
            case "gameover": return gameOverMusic;
            default:
                Debug.LogWarning($"Music track '{trackName}' not found");
                return null;
        }
    }

    // === AMBIENT SOUNDS ===

    public void PlayAmbient(AudioClip clip)
    {
        if (clip != null)
        {
            ambientSource.clip = clip;
            if (!ambientSource.isPlaying)
            {
                ambientSource.Play();
            }
        }
    }

    public void StopAmbient()
    {
        ambientSource.Stop();
    }

    // === SOUND EFFECTS ===

    public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource source = GetSFXSource();
        source.transform.position = position;
        source.clip = clip;
        source.volume = masterVolume * sfxVolume * volume;
        source.Play();

        activeSFXSources.Add(source);

        // Propagate sound to zombies
        PropagateSoundToZombies(position, volume);
    }

    public void PlaySFX(string sfxName, Vector3 position, float volume = 1f)
    {
        if (sfxLibrary.ContainsKey(sfxName))
        {
            PlaySFX(sfxLibrary[sfxName], position, volume);
        }
        else
        {
            Debug.LogWarning($"SFX '{sfxName}' not found in library");
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource source = GetSFXSource();
        source.spatialBlend = 0f; // 2D sound
        source.clip = clip;
        source.volume = masterVolume * sfxVolume * volume;
        source.Play();

        activeSFXSources.Add(source);
    }

    public void PlaySFX(string sfxName, float volume = 1f)
    {
        if (sfxLibrary.ContainsKey(sfxName))
        {
            PlaySFX(sfxLibrary[sfxName], volume);
        }
    }

    // === SOUND PROPAGATION ===

    public void PropagateSound(Vector3 position, float volume, float range = 0f)
    {
        if (range <= 0f)
        {
            range = maxSoundRange * volume;
        }

        PropagateSoundToZombies(position, volume, range);
    }

    private void PropagateSoundToZombies(Vector3 position, float volume, float range = 0f)
    {
        if (range <= 0f)
        {
            range = maxSoundRange * volume;
        }

        // Find all zombies in range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, range);

        foreach (Collider2D col in colliders)
        {
            // Check if it's a zombie
            ZombieAI zombie = col.GetComponent<ZombieAI>();
            if (zombie != null)
            {
                float distance = Vector3.Distance(position, zombie.transform.position);
                float soundIntensity = Mathf.Clamp01(1f - (distance / range));
                soundIntensity *= volume;

                // Alert the zombie to the sound
                zombie.HearSound(position, soundIntensity);
            }
        }
    }

    // === SFX OBJECT POOLING ===

    private void InitializeSFXPool()
    {
        if (sfxSourcePrefab == null)
        {
            // Create default prefab
            sfxSourcePrefab = new GameObject("SFXSource");
            AudioSource source = sfxSourcePrefab.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f; // 3D sound by default
            source.minDistance = 1f;
            source.maxDistance = 50f;
            source.rolloffMode = AudioRolloffMode.Linear;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewSFXSource();
        }
    }

    private AudioSource GetSFXSource()
    {
        if (sfxSourcePool.Count > 0)
        {
            AudioSource source = sfxSourcePool.Dequeue();
            source.gameObject.SetActive(true);
            return source;
        }
        else
        {
            return CreateNewSFXSource();
        }
    }

    private AudioSource CreateNewSFXSource()
    {
        GameObject obj = Instantiate(sfxSourcePrefab, transform);
        obj.name = $"SFXSource_{sfxSourcePool.Count + activeSFXSources.Count}";
        AudioSource source = obj.GetComponent<AudioSource>();
        obj.SetActive(false);
        sfxSourcePool.Enqueue(source);
        return source;
    }

    private void CleanupSFXSources()
    {
        for (int i = activeSFXSources.Count - 1; i >= 0; i--)
        {
            AudioSource source = activeSFXSources[i];
            if (source != null && !source.isPlaying)
            {
                source.gameObject.SetActive(false);
                sfxSourcePool.Enqueue(source);
                activeSFXSources.RemoveAt(i);
            }
        }
    }

    // === SFX LIBRARY ===

    private void BuildSFXLibrary()
    {
        // Add clips to library for easy access by name
        if (footstepClip != null) sfxLibrary["footstep"] = footstepClip;
        if (zombieGroanClip != null) sfxLibrary["zombie_groan"] = zombieGroanClip;
        if (doorOpenClip != null) sfxLibrary["door_open"] = doorOpenClip;
        if (itemPickupClip != null) sfxLibrary["item_pickup"] = itemPickupClip;
        if (craftingClip != null) sfxLibrary["crafting"] = craftingClip;
        if (hitClip != null) sfxLibrary["hit"] = hitClip;
        if (gunShotClip != null) sfxLibrary["gunshot"] = gunShotClip;

        // TODO: Load additional clips from Resources folder
        // AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio/SFX");
        // foreach (AudioClip clip in clips)
        // {
        //     sfxLibrary[clip.name.ToLower()] = clip;
        // }
    }

    // === EVENT HANDLERS ===

    private void OnDayStart()
    {
        PlayMusic(dayMusic, true);
        PlayAmbient(dayAmbient);
    }

    private void OnNightStart()
    {
        PlayMusic(nightMusic, true);
        PlayAmbient(nightAmbient);
    }

    // === VOLUME CONTROL ===

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
    }

    // === UTILITY METHODS ===

    public void PlayFootstep(Vector3 position)
    {
        PlaySFX("footstep", position, 0.3f);
    }

    public void PlayZombieGroan(Vector3 position)
    {
        PlaySFX("zombie_groan", position, 0.7f);
    }

    public void PlayGunshot(Vector3 position)
    {
        PlaySFX("gunshot", position, 1f);
        PropagateSound(position, 1f, maxSoundRange); // Gunshots attract zombies
    }

    public void PlayItemPickup()
    {
        PlaySFX("item_pickup", 0.5f);
    }

    public void PlayCrafting()
    {
        PlaySFX("crafting", 0.6f);
    }

    public void PlayDoorOpen(Vector3 position)
    {
        PlaySFX("door_open", position, 0.5f);
    }

    public void PlayHit(Vector3 position)
    {
        PlaySFX("hit", position, 0.7f);
    }
}
