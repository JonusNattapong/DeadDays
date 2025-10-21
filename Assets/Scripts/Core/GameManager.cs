using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Game state enum
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    // Current game state
    private GameState currentState = GameState.MainMenu;

    // References to other managers
    public TimeManager timeManager;
    public SaveManager saveManager;
    public AudioManager audioManager;
    // TODO: Add references to other managers like PlayerStats, HUDManager, etc.

    // Events for state changes
    public delegate void StateChangeHandler(GameState newState);
    public event StateChangeHandler OnStateChanged;

    void Awake()
    {
        // Ensure singleton pattern
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

        // Initialize references if not set in inspector
        if (timeManager == null) timeManager = FindObjectOfType<TimeManager>();
        if (saveManager == null) saveManager = FindObjectOfType<SaveManager>();
        if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
    }

    void Start()
    {
        // Start in MainMenu state
        SetState(GameState.MainMenu);
    }

    void Update()
    {
        // Handle global inputs
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing)
            {
                SetState(GameState.Paused);
            }
            else if (currentState == GameState.Paused)
            {
                SetState(GameState.Playing);
            }
        }

        // State-specific updates
        switch (currentState)
        {
            case GameState.Playing:
                // Normal game updates (e.g., check for game over conditions)
                CheckGameOverConditions();
                break;
            // Other states can have logic here
        }
    }

    public void SetState(GameState newState)
    {
        if (currentState == newState) return;

        // Exit current state
        switch (currentState)
        {
            case GameState.Playing:
                Time.timeScale = 0f;
                break;
            case GameState.Paused:
                // Hide pause menu
                break;
            case GameState.GameOver:
                // Hide game over screen
                break;
        }

        currentState = newState;

        // Enter new state
        switch (newState)
        {
            case GameState.MainMenu:
                SceneManager.LoadScene("MainMenu");
                Time.timeScale = 1f;
                audioManager?.PlayMusic(/* Main menu music clip */ null); // TODO: Assign clip
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                // Show HUD
                audioManager?.PlayMusic(/* Gameplay music clip */ null); // TODO: Assign clip
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                // Show pause menu
                audioManager?.PlaySFX(/* Pause sound */ null); // TODO: Assign clip
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                // Show game over screen
                audioManager?.PlayMusic(/* Game over music clip */ null); // TODO: Assign clip
                break;
        }

        // Notify listeners
        OnStateChanged?.Invoke(newState);
    }

    private void CheckGameOverConditions()
    {
        // TODO: Integrate with PlayerStats
        // if (PlayerStats.Instance.health <= 0)
        // {
        //     SetState(GameState.GameOver);
        // }
    }

    public void StartNewGame()
    {
        // Reset all managers
        timeManager?.ResetTime();
        saveManager?.NewGame();
        // TODO: Reset player stats, inventory, world, etc.

        SceneManager.LoadScene("Game");
        SetState(GameState.Playing);
    }

    public void LoadGame()
    {
        if (saveManager?.LoadGame() ?? false)
        {
            SceneManager.LoadScene("Game");
            SetState(GameState.Playing);
        }
        else
        {
            Debug.LogWarning("No save file to load.");
        }
    }

    public void SaveGame()
    {
        saveManager?.SaveGame();
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // TODO: Add methods for difficulty scaling, milestone checks, etc. as per README
}
