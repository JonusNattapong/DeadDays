using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    // Singleton instance
    public static SaveManager Instance { get; private set; }

    // Save file configuration
    [Header("Save Configuration")]
    [Tooltip("Name of the save file")]
    public string saveFileName = "savegame.dat";
    [Tooltip("Auto-save interval in seconds (0 = disabled)")]
    public float autoSaveInterval = 300f; // 5 minutes

    // Runtime variables
    private string saveFilePath;
    private float autoSaveTimer = 0f;
    private bool hasLoadedGame = false;

    // Events
    public delegate void SaveLoadHandler(bool success);
    public event SaveLoadHandler OnGameSaved;
    public event SaveLoadHandler OnGameLoaded;

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

        // Set save file path
        saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
        Debug.Log($"Save file path: {saveFilePath}");
    }

    void Update()
    {
        // Auto-save timer
        if (autoSaveInterval > 0f && GameManager.Instance != null)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                autoSaveTimer = 0f;
                AutoSave();
            }
        }
    }

    public bool SaveGame()
    {
        try
        {
            // Collect all game data
            GameData data = new GameData();

            // Time data
            if (TimeManager.Instance != null)
            {
                data.currentDay = TimeManager.Instance.GetCurrentDay();
                data.currentTimeOfDay = TimeManager.Instance.GetTimeOfDay();
            }

            // Player data
            if (PlayerController.Instance != null)
            {
                data.playerPosition = PlayerController.Instance.transform.position;
            }

            if (PlayerStats.Instance != null)
            {
                data.playerHealth = PlayerStats.Instance.health;
                data.playerHunger = PlayerStats.Instance.hunger;
                data.playerThirst = PlayerStats.Instance.thirst;
                data.playerFatigue = PlayerStats.Instance.fatigue;
                data.playerInfection = PlayerStats.Instance.infection;
                data.playerPanic = PlayerStats.Instance.panic;
                data.playerTemperature = PlayerStats.Instance.temperature;
            }

            // Inventory data
            if (PlayerInventory.Instance != null)
            {
                data.inventoryItems = PlayerInventory.Instance.GetInventoryData();
            }

            // Skills data
            if (SkillSystem.Instance != null)
            {
                data.skillLevels = SkillSystem.Instance.GetSkillData();
                data.skillXP = SkillSystem.Instance.GetSkillXPData();
            }

            // World data
            if (WorldGenerator.Instance != null)
            {
                data.worldSeed = WorldGenerator.Instance.GetWorldSeed();
                data.exploredAreas = WorldGenerator.Instance.GetExploredAreas();
            }

            // Base building data
            if (BuildingSystem.Instance != null)
            {
                data.placedStructures = BuildingSystem.Instance.GetPlacedStructures();
            }

            // Statistics
            data.zombiesKilled = GetStatistic("zombiesKilled");
            data.daysSurvived = data.currentDay - 1;
            data.distanceTraveled = GetStatistic("distanceTraveled");
            data.itemsCrafted = GetStatistic("itemsCrafted");

            // Metadata
            data.saveTime = DateTime.Now.ToString();
            data.gameVersion = Application.version;

            // Serialize to binary file
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(saveFilePath, FileMode.Create))
            {
                formatter.Serialize(stream, data);
            }

            Debug.Log($"Game saved successfully to {saveFilePath}");
            OnGameSaved?.Invoke(true);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
            OnGameSaved?.Invoke(false);
            return false;
        }
    }

    public bool LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning($"No save file found at {saveFilePath}");
            OnGameLoaded?.Invoke(false);
            return false;
        }

        try
        {
            // Deserialize from binary file
            GameData data;
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(saveFilePath, FileMode.Open))
            {
                data = formatter.Deserialize(stream) as GameData;
            }

            if (data == null)
            {
                Debug.LogError("Failed to deserialize save data");
                OnGameLoaded?.Invoke(false);
                return false;
            }

            // Apply loaded data
            ApplyLoadedData(data);

            hasLoadedGame = true;
            Debug.Log($"Game loaded successfully from {saveFilePath}");
            OnGameLoaded?.Invoke(true);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            OnGameLoaded?.Invoke(false);
            return false;
        }
    }

    private void ApplyLoadedData(GameData data)
    {
        // Time data
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetCurrentDay(data.currentDay);
            TimeManager.Instance.SetCurrentHour(data.currentTimeOfDay * 24f);
        }

        // Player data
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.transform.position = data.playerPosition;
        }

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.health = data.playerHealth;
            PlayerStats.Instance.hunger = data.playerHunger;
            PlayerStats.Instance.thirst = data.playerThirst;
            PlayerStats.Instance.fatigue = data.playerFatigue;
            PlayerStats.Instance.infection = data.playerInfection;
            PlayerStats.Instance.panic = data.playerPanic;
            PlayerStats.Instance.temperature = data.playerTemperature;
        }

        // Inventory data
        if (PlayerInventory.Instance != null && data.inventoryItems != null)
        {
            PlayerInventory.Instance.LoadInventoryData(data.inventoryItems);
        }

        // Skills data
        if (SkillSystem.Instance != null)
        {
            if (data.skillLevels != null)
                SkillSystem.Instance.LoadSkillData(data.skillLevels);
            if (data.skillXP != null)
                SkillSystem.Instance.LoadSkillXPData(data.skillXP);
        }

        // World data
        if (WorldGenerator.Instance != null)
        {
            if (data.worldSeed != 0)
                WorldGenerator.Instance.SetWorldSeed(data.worldSeed);
            if (data.exploredAreas != null)
                WorldGenerator.Instance.LoadExploredAreas(data.exploredAreas);
        }

        // Base building data
        if (BuildingSystem.Instance != null && data.placedStructures != null)
        {
            BuildingSystem.Instance.LoadPlacedStructures(data.placedStructures);
        }

        // Statistics
        SetStatistic("zombiesKilled", data.zombiesKilled);
        SetStatistic("distanceTraveled", data.distanceTraveled);
        SetStatistic("itemsCrafted", data.itemsCrafted);

        Debug.Log($"Loaded save from {data.saveTime} (Day {data.currentDay})");
    }

    public void NewGame()
    {
        // Delete existing save file
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Previous save deleted for new game");
        }

        hasLoadedGame = false;
        autoSaveTimer = 0f;

        // Reset all game systems to default
        // This will be handled by individual managers on scene load
    }

    public bool SaveExists()
    {
        return File.Exists(saveFilePath);
    }

    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log($"Save file deleted: {saveFilePath}");
        }
    }

    public GameData GetSaveInfo()
    {
        if (!File.Exists(saveFilePath))
            return null;

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(saveFilePath, FileMode.Open))
            {
                return formatter.Deserialize(stream) as GameData;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read save info: {e.Message}");
            return null;
        }
    }

    private void AutoSave()
    {
        if (GameManager.Instance != null && hasLoadedGame)
        {
            Debug.Log("Auto-saving...");
            SaveGame();
        }
    }

    // Statistics helpers (you could also create a separate StatisticsManager)
    private Dictionary<string, float> statistics = new Dictionary<string, float>();

    private float GetStatistic(string key)
    {
        return statistics.ContainsKey(key) ? statistics[key] : 0f;
    }

    private void SetStatistic(string key, float value)
    {
        statistics[key] = value;
    }

    public void IncrementStatistic(string key, float amount = 1f)
    {
        if (statistics.ContainsKey(key))
            statistics[key] += amount;
        else
            statistics[key] = amount;
    }
}

// Serializable data classes
[System.Serializable]
public class GameData
{
    // Metadata
    public string saveTime;
    public string gameVersion;

    // Time data
    public int currentDay;
    public float currentTimeOfDay;

    // Player data
    public Vector3 playerPosition;
    public float playerHealth;
    public float playerHunger;
    public float playerThirst;
    public float playerFatigue;
    public float playerInfection;
    public float playerPanic;
    public float playerTemperature;

    // Inventory data
    public List<InventoryItemData> inventoryItems;

    // Skills data
    public Dictionary<string, int> skillLevels;
    public Dictionary<string, float> skillXP;

    // World data
    public int worldSeed;
    public List<Vector2Int> exploredAreas;

    // Base building data
    public List<PlacedStructureData> placedStructures;

    // Statistics
    public int zombiesKilled;
    public int daysSurvived;
    public float distanceTraveled;
    public int itemsCrafted;
}

[System.Serializable]
public class InventoryItemData
{
    public string itemID;
    public int quantity;
    public float durability;
    public int slotIndex;
}

[System.Serializable]
public class PlacedStructureData
{
    public string structureID;
    public Vector3 position;
    public float rotation;
    public float health;
}
