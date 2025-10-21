using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LootSpawner : MonoBehaviour
{
    // Singleton instance
    public static LootSpawner Instance { get; private set; }

    // === LOOT CONFIGURATION ===
    [Header("Spawn Settings")]
    [Tooltip("Enable loot spawning")]
    public bool enableLootSpawning = true;
    [Tooltip("Global loot multiplier")]
    [Range(0f, 2f)] public float lootMultiplier = 1f;
    [Tooltip("Item prefab for spawning")]
    public GameObject itemPickupPrefab;

    [Header("Loot Tables")]
    [Tooltip("Enable dynamic loot tables")]
    public bool useDynamicLootTables = true;

    [Header("Container Settings")]
    [Tooltip("Container prefabs")]
    public GameObject[] containerPrefabs;
    [Tooltip("Containers per building")]
    public int containersPerBuilding = 3;

    // === LOOT TABLES ===
    private Dictionary<RoomType, LootTable> roomLootTables = new Dictionary<RoomType, LootTable>();
    private Dictionary<BuildingType, LootTable> buildingLootTables = new Dictionary<BuildingType, LootTable>();
    private Dictionary<string, ItemDefinition> itemDatabase = new Dictionary<string, ItemDefinition>();

    // === RUNTIME VARIABLES ===
    private System.Random random;
    private List<LootContainer> spawnedContainers = new List<LootContainer>();
    private int totalItemsSpawned = 0;

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

        // Initialize item database
        InitializeItemDatabase();

        // Initialize loot tables
        InitializeLootTables();

        Debug.Log("LootSpawner initialized");
    }

    // === ITEM DATABASE ===

    private void InitializeItemDatabase()
    {
        // WEAPONS
        AddItem("baseball_bat", "Baseball Bat", ItemType.Weapon, 2f, 10, 1, 15f, 1f, 1.5f);
        AddItem("kitchen_knife", "Kitchen Knife", ItemType.Weapon, 0.5f, 5, 1, 10f, 1.2f, 1f);
        AddItem("crowbar", "Crowbar", ItemType.Weapon, 3f, 15, 1, 18f, 1f, 1.5f);
        AddItem("axe", "Axe", ItemType.Weapon, 4f, 20, 1, 25f, 0.8f, 2f);
        AddItem("pistol", "Pistol", ItemType.Weapon, 1.5f, 50, 1, 30f, 2f, 10f);
        AddItem("shotgun", "Shotgun", ItemType.Weapon, 4f, 100, 1, 50f, 1f, 8f);
        AddItem("rifle", "Rifle", ItemType.Weapon, 5f, 150, 1, 40f, 1.5f, 15f);

        // AMMO
        AddItem("pistol_ammo", "Pistol Ammo", ItemType.Material, 0.5f, 10, 50, rarity: 0.6f);
        AddItem("shotgun_shells", "Shotgun Shells", ItemType.Material, 0.8f, 15, 25, rarity: 0.4f);
        AddItem("rifle_ammo", "Rifle Ammo", ItemType.Material, 0.6f, 12, 40, rarity: 0.5f);

        // FOOD
        AddItem("canned_beans", "Canned Beans", ItemType.Food, 0.5f, 5, 10, foodValue: 30f, nutrition: 0.7f);
        AddItem("canned_soup", "Canned Soup", ItemType.Food, 0.5f, 5, 10, foodValue: 35f, nutrition: 0.8f);
        AddItem("bread", "Bread", ItemType.Food, 0.3f, 3, 5, foodValue: 20f, nutrition: 0.5f);
        AddItem("chips", "Chips", ItemType.Food, 0.2f, 2, 5, foodValue: 15f, nutrition: 0.3f);
        AddItem("chocolate", "Chocolate", ItemType.Food, 0.1f, 3, 10, foodValue: 10f, nutrition: 0.4f);
        AddItem("mre", "MRE", ItemType.Food, 1f, 20, 5, foodValue: 50f, nutrition: 1f, rarity: 0.3f);

        // DRINKS
        AddItem("water_bottle", "Water Bottle", ItemType.Drink, 0.5f, 3, 10, thirst: 40f);
        AddItem("soda", "Soda", ItemType.Drink, 0.5f, 2, 10, thirst: 25f);
        AddItem("energy_drink", "Energy Drink", ItemType.Drink, 0.3f, 5, 5, thirst: 20f, rarity: 0.5f);
        AddItem("juice", "Juice", ItemType.Drink, 0.5f, 4, 8, thirst: 35f);

        // MEDICINE
        AddItem("bandage", "Bandage", ItemType.Medicine, 0.1f, 10, 5, medicine: "bandage");
        AddItem("first_aid_kit", "First Aid Kit", ItemType.Medicine, 0.8f, 30, 3, medicine: "first_aid", rarity: 0.4f);
        AddItem("antibiotics", "Antibiotics", ItemType.Medicine, 0.2f, 25, 5, medicine: "antibiotics", rarity: 0.3f);
        AddItem("painkillers", "Painkillers", ItemType.Medicine, 0.1f, 15, 10, medicine: "painkillers");
        AddItem("antidote", "Antidote", ItemType.Medicine, 0.3f, 40, 3, medicine: "antidote", rarity: 0.2f);

        // TOOLS
        AddItem("flashlight", "Flashlight", ItemType.Tool, 0.5f, 15, 1);
        AddItem("rope", "Rope", ItemType.Material, 1f, 10, 5);
        AddItem("duct_tape", "Duct Tape", ItemType.Material, 0.3f, 8, 3);
        AddItem("nails", "Nails", ItemType.Material, 0.5f, 5, 50);
        AddItem("wood_plank", "Wood Plank", ItemType.Material, 2f, 3, 10);
        AddItem("scrap_metal", "Scrap Metal", ItemType.Material, 3f, 5, 8);
        AddItem("cloth", "Cloth", ItemType.Material, 0.2f, 2, 20);

        // ARMOR
        AddItem("leather_jacket", "Leather Jacket", ItemType.Armor, 2f, 50, 1, defense: 10f, rarity: 0.5f);
        AddItem("kevlar_vest", "Kevlar Vest", ItemType.Armor, 5f, 150, 1, defense: 30f, rarity: 0.2f);
        AddItem("helmet", "Helmet", ItemType.Armor, 1.5f, 40, 1, defense: 15f, rarity: 0.4f);

        // BACKPACKS
        AddItem("small_backpack", "Small Backpack", ItemType.Other, 1f, 20, 1, weightBonus: 10f);
        AddItem("large_backpack", "Large Backpack", ItemType.Other, 2f, 40, 1, weightBonus: 20f, rarity: 0.4f);
        AddItem("military_backpack", "Military Backpack", ItemType.Other, 3f, 60, 1, weightBonus: 30f, rarity: 0.2f);

        // MISC
        AddItem("battery", "Battery", ItemType.Material, 0.2f, 5, 10);
        AddItem("lighter", "Lighter", ItemType.Tool, 0.1f, 5, 5);
        AddItem("matches", "Matches", ItemType.Material, 0.1f, 2, 20);
        AddItem("radio", "Radio", ItemType.Tool, 1f, 30, 1, rarity: 0.3f);

        Debug.Log($"Item database initialized: {itemDatabase.Count} items");
    }

    private void AddItem(string id, string name, ItemType type, float weight, int value, int maxStack,
                        float damage = 0f, float attackSpeed = 1f, float range = 1f,
                        float foodValue = 0f, float nutrition = 1f, float thirst = 0f,
                        string medicine = "", float defense = 0f, float weightBonus = 0f,
                        float rarity = 1f)
    {
        ItemDefinition item = new ItemDefinition
        {
            itemID = id,
            itemName = name,
            itemType = type,
            weight = weight,
            value = value,
            maxStackSize = maxStack,
            damage = damage,
            attackSpeed = attackSpeed,
            range = range,
            foodValue = foodValue,
            nutritionQuality = nutrition,
            thirstValue = thirst,
            medicineType = medicine,
            defense = defense,
            weightCapacityBonus = weightBonus,
            rarity = rarity,
            isStackable = maxStack > 1
        };

        itemDatabase[id] = item;
    }

    // === LOOT TABLES ===

    private void InitializeLootTables()
    {
        // ROOM LOOT TABLES

        // Kitchen
        roomLootTables[RoomType.Kitchen] = new LootTable
        {
            items = new List<LootEntry>
            {
                new LootEntry { itemID = "canned_beans", chance = 0.6f, minCount = 1, maxCount = 3 },
                new LootEntry { itemID = "canned_soup", chance = 0.5f, minCount = 1, maxCount = 2 },
                new LootEntry { itemID = "water_bottle", chance = 0.7f, minCount = 1, maxCount = 4 },
                new LootEntry { itemID = "bread", chance = 0.4f, minCount = 1, maxCount = 2 },
                new LootEntry { itemID = "kitchen_knife", chance = 0.3f, minCount = 1, maxCount = 1 },
                new LootEntry { itemID = "chips", chance = 0.5f, minCount = 1, maxCount = 3 },
            },
            minItems = 2,
            maxItems = 5
        };

        // Bedroom
        roomLootTables[RoomType.Bedroom] = new LootTable
        {
            items = new List<LootEntry>
            {
                new LootEntry { itemID = "bandage", chance = 0.4f, minCount = 1, maxCount = 3 },
                new LootEntry { itemID = "painkillers", chance = 0.3f, minCount = 1, maxCount = 2 },
                new LootEntry { itemID = "cloth", chance = 0.6f, minCount = 2, maxCount = 5 },
                new LootEntry { itemID = "flashlight", chance = 0.3f, minCount = 1, maxCount = 1 },
                new LootEntry { itemID = "small_backpack", chance = 0.2f, minCount = 1, maxCount = 1 },
            },
            minItems = 1,
            maxItems = 3
        };

        // Bathroom
        roomLootTables[RoomType.Bathroom] = new LootTable
        {
            items = new List<LootEntry>
            {
                new LootEntry { itemID = "bandage", chance = 0.7f, minCount = 1, maxCount = 5 },
                new LootEntry { itemID = "painkillers", chance = 0.5f, minCount = 1, maxCount = 3 },
                new LootEntry { itemID = "first_aid_kit", chance = 0.3f, minCount = 1, maxCount = 1 },
                new LootEntry { itemID = "cloth", chance = 0.4f, minCount = 1, maxCount = 3 },
            },
            minItems = 1,
            maxItems = 3
        };

        // Storage
        roomLootTables[RoomType.Storage] = new LootTable
        {
            items = new List<LootEntry>
            {
                new LootEntry { itemID = "wood_plank", chance = 0.7f, minCount = 3, maxCount = 10 },
                new LootEntry { itemID = "nails", chance = 0.6f, minCount = 10, maxCount = 30 },
                new LootEntry { itemID = "rope", chance = 0.5f, minCount = 1, maxCount = 3 },
                new LootEntry { itemID = "duct_tape", chance = 0.4f, minCount = 1, maxCount = 2 },
                new LootEntry { itemID = "scrap_metal", chance = 0.5f, minCount = 2, maxCount = 8 },
                new LootEntry { itemID = "battery", chance = 0.4f, minCount = 2, maxCount = 6 },
            },
            minItems = 3,
            maxItems = 6
        };

        // Shop
        roomLootTables[RoomType.Shop] = new LootTable
        {
            items = new List<LootEntry>
            {
                new LootEntry { itemID = "canned_beans", chance = 0.8f, minCount = 2, maxCount = 5 },
                new LootEntry { itemID = "canned_soup", chance = 0.7f, minCount = 2, maxCount = 4 },
                new LootEntry { itemID = "water_bottle", chance = 0.9f, minCount = 3, maxCount = 8 },
                new LootEntry { itemID = "soda", chance = 0.7f, minCount = 2, maxCount = 6 },
                new LootEntry { itemID = "chips", chance = 0.8f, minCount = 2, maxCount = 5 },
                new LootEntry { itemID = "chocolate", chance = 0.6f, minCount = 2, maxCount = 8 },
                new LootEntry { itemID = "bandage", chance = 0.5f, minCount = 1, maxCount = 3 },
                new LootEntry { itemID = "flashlight", chance = 0.4f, minCount = 1, maxCount = 2 },
                new LootEntry { itemID = "battery", chance = 0.5f, minCount = 2, maxCount = 6 },
            },
            minItems = 5,
            maxItems = 10
        };

        // Medical
        roomLootTables[RoomType.Medical] = new LootTable
        {
            items = new List<LootEntry>
            {
                new LootEntry { itemID = "bandage", chance = 0.9f, minCount = 3, maxCount = 10 },
                new LootEntry { itemID = "first_aid_kit", chance = 0.7f, minCount = 1, maxCount = 3 },
                new LootEntry { itemID = "antibiotics", chance = 0.6f, minCount = 1, maxCount = 4 },
                new LootEntry { itemID = "painkillers", chance = 0.8f, minCount = 2, maxCount = 6 },
                new LootEntry { itemID = "antidote", chance = 0.4f, minCount = 1, maxCount = 2 },
            },
            minItems = 3,
            maxItems = 6
        };

        // Armory
        roomLootTables[RoomType.Armory] = new LootTable
        {
            items = new List<LootEntry>
            {
                new LootEntry { itemID = "pistol", chance = 0.8f, minCount = 1, maxCount = 2 },
                new LootEntry { itemID = "shotgun", chance = 0.5f, minCount = 1, maxCount = 1 },
                new LootEntry { itemID = "rifle", chance = 0.4f, minCount = 1, maxCount = 1 },
                new LootEntry { itemID = "pistol_ammo", chance = 0.9f, minCount = 10, maxCount = 50 },
                new LootEntry { itemID = "shotgun_shells", chance = 0.7f, minCount = 5, maxCount = 25 },
                new LootEntry { itemID = "rifle_ammo", chance = 0.6f, minCount = 10, maxCount = 40 },
                new LootEntry { itemID = "kevlar_vest", chance = 0.4f, minCount = 1, maxCount = 1 },
                new LootEntry { itemID = "helmet", chance = 0.5f, minCount = 1, maxCount = 2 },
                new LootEntry { itemID = "military_backpack", chance = 0.3f, minCount = 1, maxCount = 1 },
            },
            minItems = 4,
            maxItems = 8
        };

        // Office
        roomLootTables[RoomType.Office] = new LootTable
        {
            items = new List<LootEntry>
            {
                new LootEntry { itemID = "bandage", chance = 0.3f, minCount = 1, maxCount = 2 },
                new LootEntry { itemID = "water_bottle", chance = 0.4f, minCount = 1, maxCount = 2 },
                new LootEntry { itemID = "flashlight", chance = 0.3f, minCount = 1, maxCount = 1 },
                new LootEntry { itemID = "battery", chance = 0.4f, minCount = 1, maxCount = 4 },
                new LootEntry { itemID = "small_backpack", chance = 0.2f, minCount = 1, maxCount = 1 },
            },
            minItems = 1,
            maxItems = 3
        };

        // BUILDING LOOT TABLES (additional items beyond rooms)

        buildingLootTables[BuildingType.PoliceStation] = new LootTable
        {
            items = new List<LootEntry>
            {
                new LootEntry { itemID = "pistol", chance = 0.6f, minCount = 1, maxCount = 2 },
                new LootEntry { itemID = "shotgun", chance = 0.4f, minCount = 1, maxCount = 1 },
                new LootEntry { itemID = "pistol_ammo", chance = 0.8f, minCount = 10, maxCount = 30 },
                new LootEntry { itemID = "kevlar_vest", chance = 0.3f, minCount = 1, maxCount = 1 },
            },
            minItems = 2,
            maxItems = 4
        };

        buildingLootTables[BuildingType.Hospital] = new LootTable
        {
            items = new List<LootEntry>
            {
                new LootEntry { itemID = "first_aid_kit", chance = 0.8f, minCount = 2, maxCount = 4 },
                new LootEntry { itemID = "antibiotics", chance = 0.7f, minCount = 2, maxCount = 5 },
                new LootEntry { itemID = "antidote", chance = 0.5f, minCount = 1, maxCount = 3 },
            },
            minItems = 2,
            maxItems = 5
        };

        buildingLootTables[BuildingType.Warehouse] = new LootTable
        {
            items = new List<LootEntry>
            {
                new LootEntry { itemID = "canned_beans", chance = 0.9f, minCount = 5, maxCount = 15 },
                new LootEntry { itemID = "water_bottle", chance = 0.9f, minCount = 5, maxCount = 20 },
                new LootEntry { itemID = "wood_plank", chance = 0.8f, minCount = 5, maxCount = 15 },
                new LootEntry { itemID = "scrap_metal", chance = 0.7f, minCount = 3, maxCount = 10 },
            },
            minItems = 3,
            maxItems = 8
        };

        Debug.Log("Loot tables initialized");
    }

    // === LOOT SPAWNING ===

    public void SpawnLootInBuilding(BuildingData building)
    {
        if (!enableLootSpawning || building == null)
            return;

        // Spawn loot in rooms
        foreach (RoomData room in building.rooms)
        {
            if (room.hasLoot && !room.isLooted)
            {
                SpawnLootInRoom(room, building);
            }
        }

        // Spawn building-specific loot
        if (buildingLootTables.ContainsKey(building.type))
        {
            SpawnBuildingLoot(building);
        }

        Debug.Log($"Spawned loot in {building.type} at {building.position}");
    }

    private void SpawnLootInRoom(RoomData room, BuildingData building)
    {
        if (!roomLootTables.ContainsKey(room.roomType))
            return;

        LootTable lootTable = roomLootTables[room.roomType];
        Vector2Int roomWorldPos = building.position + room.position;

        // Determine number of items
        int itemCount = random.Next(lootTable.minItems, lootTable.maxItems + 1);
        itemCount = Mathf.RoundToInt(itemCount * lootMultiplier * room.lootQuality);

        // Spawn items
        for (int i = 0; i < itemCount; i++)
        {
            LootEntry selectedEntry = SelectRandomLootEntry(lootTable);
            if (selectedEntry != null && itemDatabase.ContainsKey(selectedEntry.itemID))
            {
                int count = random.Next(selectedEntry.minCount, selectedEntry.maxCount + 1);
                Vector3 spawnPos = GetRandomPositionInRoom(roomWorldPos, room.size);
                SpawnItem(selectedEntry.itemID, count, spawnPos);
            }
        }
    }

    private void SpawnBuildingLoot(BuildingData building)
    {
        LootTable lootTable = buildingLootTables[building.type];

        int itemCount = random.Next(lootTable.minItems, lootTable.maxItems + 1);

        for (int i = 0; i < itemCount; i++)
        {
            LootEntry selectedEntry = SelectRandomLootEntry(lootTable);
            if (selectedEntry != null && itemDatabase.ContainsKey(selectedEntry.itemID))
            {
                int count = random.Next(selectedEntry.minCount, selectedEntry.maxCount + 1);

                // Spawn in random room
                if (building.rooms.Count > 0)
                {
                    RoomData randomRoom = building.rooms[random.Next(building.rooms.Count)];
                    Vector2Int roomWorldPos = building.position + randomRoom.position;
                    Vector3 spawnPos = GetRandomPositionInRoom(roomWorldPos, randomRoom.size);
                    SpawnItem(selectedEntry.itemID, count, spawnPos);
                }
            }
        }
    }

    private LootEntry SelectRandomLootEntry(LootTable lootTable)
    {
        // Weighted random selection
        float totalWeight = lootTable.items.Sum(entry => entry.chance);
        float randomValue = (float)random.NextDouble() * totalWeight;
        float cumulative = 0f;

        foreach (LootEntry entry in lootTable.items)
        {
            cumulative += entry.chance;
            if (randomValue <= cumulative)
            {
                // Additional rarity check
                if (itemDatabase.ContainsKey(entry.itemID))
                {
                    float rarity = itemDatabase[entry.itemID].rarity;
                    if (random.NextDouble() <= rarity)
                    {
                        return entry;
                    }
                }
            }
        }

        return lootTable.items.Count > 0 ? lootTable.items[0] : null;
    }

    private void SpawnItem(string itemID, int count, Vector3 position)
    {
        if (!itemDatabase.ContainsKey(itemID))
        {
            Debug.LogWarning($"Item {itemID} not found in database");
            return;
        }

        ItemDefinition itemDef = itemDatabase[itemID];

        // Create Item from definition
        Item item = new Item
        {
            itemID = itemDef.itemID,
            itemName = itemDef.itemName,
            itemType = itemDef.itemType,
            weight = itemDef.weight,
            value = itemDef.value,
            isStackable = itemDef.isStackable,
            maxStackSize = itemDef.maxStackSize,
            durability = itemDef.maxDurability,
            maxDurability = itemDef.maxDurability,
            damage = itemDef.damage,
            attackSpeed = itemDef.attackSpeed,
            range = itemDef.range,
            foodValue = itemDef.foodValue,
            nutritionQuality = itemDef.nutritionQuality,
            thirstValue = itemDef.thirstValue,
            medicineType = itemDef.medicineType,
            defense = itemDef.defense,
            weightCapacityBonus = itemDef.weightCapacityBonus
        };

        // Spawn pickup in world
        if (itemPickupPrefab != null)
        {
            GameObject pickupObj = Instantiate(itemPickupPrefab, position, Quaternion.identity);
            ItemPickup pickup = pickupObj.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.item = item;
                pickup.quantity = count;
            }
        }

        totalItemsSpawned++;
    }

    private Vector3 GetRandomPositionInRoom(Vector2Int roomWorldPos, Vector2Int roomSize)
    {
        float x = roomWorldPos.x + random.Next(1, roomSize.x - 1) + (float)random.NextDouble();
        float y = roomWorldPos.y + random.Next(1, roomSize.y - 1) + (float)random.NextDouble();
        return new Vector3(x, y, 0);
    }

    // === CONTAINER SPAWNING ===

    public void SpawnContainersInBuilding(BuildingData building)
    {
        if (containerPrefabs == null || containerPrefabs.Length == 0)
            return;

        int containerCount = Mathf.Min(containersPerBuilding, building.rooms.Count);

        for (int i = 0; i < containerCount; i++)
        {
            if (building.rooms.Count == 0) break;

            RoomData room = building.rooms[random.Next(building.rooms.Count)];
            Vector2Int roomWorldPos = building.position + room.position;
            Vector3 spawnPos = GetRandomPositionInRoom(roomWorldPos, room.size);

            GameObject containerPrefab = containerPrefabs[random.Next(containerPrefabs.Length)];
            GameObject container = Instantiate(containerPrefab, spawnPos, Quaternion.identity);

            LootContainer lootContainer = container.GetComponent<LootContainer>();
            if (lootContainer != null)
            {
                lootContainer.roomType = room.roomType;
                lootContainer.isLooted = false;
                spawnedContainers.Add(lootContainer);
            }
        }
    }

    // === PUBLIC METHODS ===

    public ItemDefinition GetItemDefinition(string itemID)
    {
        return itemDatabase.ContainsKey(itemID) ? itemDatabase[itemID] : null;
    }

    public List<string> GetAllItemIDs()
    {
        return new List<string>(itemDatabase.Keys);
    }

    public int GetTotalItemsSpawned()
    {
        return totalItemsSpawned;
    }

    // === DEBUG ===

    [ContextMenu("Debug: Print Item Database")]
    public void DebugPrintDatabase()
    {
        Debug.Log($"=== ITEM DATABASE ({itemDatabase.Count} items) ===");

        Dictionary<ItemType, int> counts = new Dictionary<ItemType, int>();
        foreach (var item in itemDatabase.Values)
        {
            if (!counts.ContainsKey(item.itemType))
                counts[item.itemType] = 0;
            counts[item.itemType]++;
        }

        foreach (var kvp in counts)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value} items");
        }
    }

    [ContextMenu("Debug: Print Loot Tables")]
    public void DebugPrintLootTables()
    {
        Debug.Log($"=== LOOT TABLES ===");
        Debug.Log($"Room tables: {roomLootTables.Count}");
        Debug.Log($"Building tables: {buildingLootTables.Count}");
    }
}

// === DATA CLASSES ===

[System.Serializable]
public class ItemDefinition
{
    public string itemID;
    public string itemName;
    public ItemType itemType;
    public float weight;
    public int value;
    public bool isStackable;
    public int maxStackSize;
    public float maxDurability = 100f;
    public float rarity = 1f; // 0-1, lower = rarer

    // Weapon stats
    public float damage;
    public float attackSpeed = 1f;
    public float range = 1f;

    // Food/Drink stats
    public float foodValue;
    public float nutritionQuality = 1f;
    public float thirstValue;

    // Medicine stats
    public string medicineType;

    // Armor stats
    public float defense;

    // Backpack stats
    public float weightCapacityBonus;
}

[System.Serializable]
public class LootTable
{
    public List<LootEntry> items = new List<LootEntry>();
    public int minItems = 1;
    public int maxItems = 5;
}

[System.Serializable]
public class LootEntry
{
    public string itemID;
    public float chance = 1f; // Weight for selection
    public int minCount = 1;
    public int maxCount = 1;
}

[System.Serializable]
public class LootContainer
{
    public RoomType roomType;
    public bool isLooted;
    public List<Item> items = new List<Item>();
}
