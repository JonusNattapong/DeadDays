using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class BuildingGenerator : MonoBehaviour
{
    // Singleton instance
    public static BuildingGenerator Instance { get; private set; }

    // === BUILDING CONFIGURATION ===
    [Header("Building Settings")]
    [Tooltip("Minimum rooms per building")]
    public int minRooms = 1;
    [Tooltip("Maximum rooms per building")]
    public int maxRooms = 4;
    [Tooltip("Room size range")]
    public Vector2Int minRoomSize = new Vector2Int(3, 3);
    public Vector2Int maxRoomSize = new Vector2Int(8, 8);

    [Header("Building Probabilities")]
    [Range(0f, 1f)] public float houseProbability = 0.5f;
    [Range(0f, 1f)] public float storeProbability = 0.2f;
    [Range(0f, 1f)] public float warehouseProbability = 0.15f;
    [Range(0f, 1f)] public float hospitalProbability = 0.05f;
    [Range(0f, 1f)] public float policeStationProbability = 0.05f;
    [Range(0f, 1f)] public float schoolProbability = 0.05f;

    [Header("Furniture Settings")]
    [Tooltip("Enable furniture generation")]
    public bool generateFurniture = true;
    [Tooltip("Furniture density per room")]
    [Range(0f, 1f)] public float furnitureDensity = 0.3f;

    [Header("Tilemap References")]
    public Tilemap wallTilemap;
    public Tilemap floorTilemap;
    public Tilemap furnitureTilemap;

    [Header("Tile References")]
    public TileBase wallTile;
    public TileBase doorTile;
    public TileBase windowTile;
    public TileBase floorTile;
    public TileBase carpetTile;
    public TileBase[] furnitureTiles;

    // === RUNTIME VARIABLES ===
    private System.Random random;
    private List<BuildingData> generatedBuildings = new List<BuildingData>();
    private Dictionary<BuildingType, BuildingTemplate> buildingTemplates = new Dictionary<BuildingType, BuildingTemplate>();

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

        // Initialize building templates
        InitializeBuildingTemplates();

        Debug.Log("BuildingGenerator initialized");
    }

    // === BUILDING TEMPLATES ===

    private void InitializeBuildingTemplates()
    {
        // House template
        buildingTemplates[BuildingType.House] = new BuildingTemplate
        {
            minSize = new Vector2Int(6, 6),
            maxSize = new Vector2Int(10, 10),
            requiredRooms = new List<RoomType> { RoomType.LivingRoom, RoomType.Bedroom, RoomType.Kitchen },
            optionalRooms = new List<RoomType> { RoomType.Bathroom, RoomType.Bedroom },
            hasWindows = true,
            windowChance = 0.7f
        };

        // Store template
        buildingTemplates[BuildingType.Store] = new BuildingTemplate
        {
            minSize = new Vector2Int(8, 8),
            maxSize = new Vector2Int(12, 12),
            requiredRooms = new List<RoomType> { RoomType.Shop, RoomType.Storage },
            optionalRooms = new List<RoomType> { RoomType.Office },
            hasWindows = true,
            windowChance = 0.5f
        };

        // Warehouse template
        buildingTemplates[BuildingType.Warehouse] = new BuildingTemplate
        {
            minSize = new Vector2Int(12, 12),
            maxSize = new Vector2Int(20, 20),
            requiredRooms = new List<RoomType> { RoomType.Storage, RoomType.Storage },
            optionalRooms = new List<RoomType> { RoomType.Office },
            hasWindows = false,
            windowChance = 0.2f
        };

        // Hospital template
        buildingTemplates[BuildingType.Hospital] = new BuildingTemplate
        {
            minSize = new Vector2Int(15, 15),
            maxSize = new Vector2Int(25, 25),
            requiredRooms = new List<RoomType> { RoomType.Medical, RoomType.Medical, RoomType.Office },
            optionalRooms = new List<RoomType> { RoomType.Storage, RoomType.Bathroom },
            hasWindows = true,
            windowChance = 0.6f
        };

        // Police Station template
        buildingTemplates[BuildingType.PoliceStation] = new BuildingTemplate
        {
            minSize = new Vector2Int(10, 10),
            maxSize = new Vector2Int(15, 15),
            requiredRooms = new List<RoomType> { RoomType.Office, RoomType.Armory, RoomType.Jail },
            optionalRooms = new List<RoomType> { RoomType.Storage },
            hasWindows = true,
            windowChance = 0.4f
        };

        // School template
        buildingTemplates[BuildingType.School] = new BuildingTemplate
        {
            minSize = new Vector2Int(12, 12),
            maxSize = new Vector2Int(20, 20),
            requiredRooms = new List<RoomType> { RoomType.Classroom, RoomType.Classroom, RoomType.Office },
            optionalRooms = new List<RoomType> { RoomType.Cafeteria, RoomType.Bathroom },
            hasWindows = true,
            windowChance = 0.8f
        };
    }

    // === BUILDING GENERATION ===

    public BuildingData GenerateBuilding(Vector2Int position, BuildingType? forcedType = null)
    {
        // Determine building type
        BuildingType buildingType;
        if (forcedType.HasValue)
        {
            buildingType = forcedType.Value;
        }
        else
        {
            buildingType = SelectRandomBuildingType();
        }

        // Get template
        if (!buildingTemplates.ContainsKey(buildingType))
        {
            buildingType = BuildingType.House; // Fallback
        }

        BuildingTemplate template = buildingTemplates[buildingType];

        // Generate size
        int width = random.Next(template.minSize.x, template.maxSize.x + 1);
        int height = random.Next(template.minSize.y, template.maxSize.y + 1);

        // Create building data
        BuildingData building = new BuildingData
        {
            buildingID = System.Guid.NewGuid().ToString(),
            position = position,
            size = new Vector2Int(width, height),
            type = buildingType,
            rooms = new List<RoomData>(),
            isLooted = false,
            hasZombies = random.NextDouble() < 0.3f // 30% chance of zombies
        };

        // Generate rooms
        GenerateRooms(building, template);

        // Generate structure
        GenerateStructure(building, template);

        // Generate furniture
        if (generateFurniture)
        {
            GenerateFurniture(building);
        }

        generatedBuildings.Add(building);

        Debug.Log($"Generated {buildingType} at {position} (Size: {width}x{height}, Rooms: {building.rooms.Count})");

        return building;
    }

    private BuildingType SelectRandomBuildingType()
    {
        float total = houseProbability + storeProbability + warehouseProbability +
                     hospitalProbability + policeStationProbability + schoolProbability;

        float random = (float)this.random.NextDouble() * total;
        float cumulative = 0f;

        cumulative += houseProbability;
        if (random < cumulative) return BuildingType.House;

        cumulative += storeProbability;
        if (random < cumulative) return BuildingType.Store;

        cumulative += warehouseProbability;
        if (random < cumulative) return BuildingType.Warehouse;

        cumulative += hospitalProbability;
        if (random < cumulative) return BuildingType.Hospital;

        cumulative += policeStationProbability;
        if (random < cumulative) return BuildingType.PoliceStation;

        return BuildingType.School;
    }

    // === ROOM GENERATION ===

    private void GenerateRooms(BuildingData building, BuildingTemplate template)
    {
        // Add required rooms
        foreach (RoomType roomType in template.requiredRooms)
        {
            AddRoom(building, roomType);
        }

        // Add random optional rooms
        int optionalRoomCount = random.Next(0, template.optionalRooms.Count + 1);
        for (int i = 0; i < optionalRoomCount && i < template.optionalRooms.Count; i++)
        {
            if (random.NextDouble() < 0.5f)
            {
                AddRoom(building, template.optionalRooms[i]);
            }
        }

        // Assign room positions (simple grid layout)
        LayoutRooms(building);
    }

    private void AddRoom(BuildingData building, RoomType roomType)
    {
        RoomData room = new RoomData
        {
            roomType = roomType,
            size = new Vector2Int(
                random.Next(minRoomSize.x, maxRoomSize.x + 1),
                random.Next(minRoomSize.y, maxRoomSize.y + 1)
            ),
            hasLoot = random.NextDouble() < 0.6f, // 60% chance of loot
            lootQuality = (float)random.NextDouble()
        };

        building.rooms.Add(room);
    }

    private void LayoutRooms(BuildingData building)
    {
        // Simple horizontal or vertical layout
        bool isHorizontal = random.NextDouble() < 0.5f;

        int currentX = 1;
        int currentY = 1;

        foreach (RoomData room in building.rooms)
        {
            // Clamp room size to fit building
            room.size.x = Mathf.Min(room.size.x, building.size.x - 2);
            room.size.y = Mathf.Min(room.size.y, building.size.y - 2);

            room.position = new Vector2Int(currentX, currentY);

            if (isHorizontal)
            {
                currentX += room.size.x;
                if (currentX >= building.size.x - 1)
                {
                    currentX = 1;
                    currentY += maxRoomSize.y;
                }
            }
            else
            {
                currentY += room.size.y;
                if (currentY >= building.size.y - 1)
                {
                    currentY = 1;
                    currentX += maxRoomSize.x;
                }
            }
        }
    }

    // === STRUCTURE GENERATION ===

    private void GenerateStructure(BuildingData building, BuildingTemplate template)
    {
        if (wallTilemap == null || floorTilemap == null)
        {
            Debug.LogWarning("Tilemaps not assigned!");
            return;
        }

        Vector2Int pos = building.position;
        Vector2Int size = building.size;

        // Generate floor
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(pos.x + x, pos.y + y, 0);
                floorTilemap.SetTile(tilePos, floorTile);
            }
        }

        // Generate walls (perimeter)
        for (int x = 0; x < size.x; x++)
        {
            Vector3Int bottomWall = new Vector3Int(pos.x + x, pos.y, 0);
            Vector3Int topWall = new Vector3Int(pos.x + x, pos.y + size.y - 1, 0);
            wallTilemap.SetTile(bottomWall, wallTile);
            wallTilemap.SetTile(topWall, wallTile);
        }

        for (int y = 0; y < size.y; y++)
        {
            Vector3Int leftWall = new Vector3Int(pos.x, pos.y + y, 0);
            Vector3Int rightWall = new Vector3Int(pos.x + size.x - 1, pos.y + y, 0);
            wallTilemap.SetTile(leftWall, wallTile);
            wallTilemap.SetTile(rightWall, wallTile);
        }

        // Generate doors
        GenerateDoors(building);

        // Generate windows
        if (template.hasWindows && windowTile != null)
        {
            GenerateWindows(building, template.windowChance);
        }

        // Generate interior walls (between rooms)
        GenerateInteriorWalls(building);
    }

    private void GenerateDoors(BuildingData building)
    {
        if (doorTile == null) return;

        Vector2Int pos = building.position;
        Vector2Int size = building.size;

        // Main entrance (random side)
        int doorSide = random.Next(4);
        Vector3Int doorPos;

        switch (doorSide)
        {
            case 0: // Bottom
                doorPos = new Vector3Int(pos.x + size.x / 2, pos.y, 0);
                break;
            case 1: // Top
                doorPos = new Vector3Int(pos.x + size.x / 2, pos.y + size.y - 1, 0);
                break;
            case 2: // Left
                doorPos = new Vector3Int(pos.x, pos.y + size.y / 2, 0);
                break;
            default: // Right
                doorPos = new Vector3Int(pos.x + size.x - 1, pos.y + size.y / 2, 0);
                break;
        }

        wallTilemap.SetTile(doorPos, doorTile);
        building.entrancePosition = new Vector2Int(doorPos.x, doorPos.y);
    }

    private void GenerateWindows(BuildingData building, float windowChance)
    {
        Vector2Int pos = building.position;
        Vector2Int size = building.size;

        // Windows on walls (skip corners and door positions)
        for (int x = 2; x < size.x - 2; x += 2)
        {
            if (random.NextDouble() < windowChance)
            {
                Vector3Int windowPos = new Vector3Int(pos.x + x, pos.y, 0);
                if (wallTilemap.GetTile(windowPos) == wallTile)
                {
                    wallTilemap.SetTile(windowPos, windowTile);
                }
            }

            if (random.NextDouble() < windowChance)
            {
                Vector3Int windowPos = new Vector3Int(pos.x + x, pos.y + size.y - 1, 0);
                if (wallTilemap.GetTile(windowPos) == wallTile)
                {
                    wallTilemap.SetTile(windowPos, windowTile);
                }
            }
        }

        for (int y = 2; y < size.y - 2; y += 2)
        {
            if (random.NextDouble() < windowChance)
            {
                Vector3Int windowPos = new Vector3Int(pos.x, pos.y + y, 0);
                if (wallTilemap.GetTile(windowPos) == wallTile)
                {
                    wallTilemap.SetTile(windowPos, windowTile);
                }
            }

            if (random.NextDouble() < windowChance)
            {
                Vector3Int windowPos = new Vector3Int(pos.x + size.x - 1, pos.y + y, 0);
                if (wallTilemap.GetTile(windowPos) == wallTile)
                {
                    wallTilemap.SetTile(windowPos, windowTile);
                }
            }
        }
    }

    private void GenerateInteriorWalls(BuildingData building)
    {
        // Generate walls between rooms
        foreach (RoomData room in building.rooms)
        {
            Vector2Int roomWorldPos = building.position + room.position;

            // Room perimeter (interior walls)
            for (int x = 0; x < room.size.x; x++)
            {
                Vector3Int bottomWall = new Vector3Int(roomWorldPos.x + x, roomWorldPos.y, 0);
                Vector3Int topWall = new Vector3Int(roomWorldPos.x + x, roomWorldPos.y + room.size.y - 1, 0);

                // Only place if not already a door
                if (wallTilemap.GetTile(bottomWall) == null)
                    wallTilemap.SetTile(bottomWall, wallTile);
                if (wallTilemap.GetTile(topWall) == null)
                    wallTilemap.SetTile(topWall, wallTile);
            }

            for (int y = 0; y < room.size.y; y++)
            {
                Vector3Int leftWall = new Vector3Int(roomWorldPos.x, roomWorldPos.y + y, 0);
                Vector3Int rightWall = new Vector3Int(roomWorldPos.x + room.size.x - 1, roomWorldPos.y + y, 0);

                if (wallTilemap.GetTile(leftWall) == null)
                    wallTilemap.SetTile(leftWall, wallTile);
                if (wallTilemap.GetTile(rightWall) == null)
                    wallTilemap.SetTile(rightWall, wallTile);
            }

            // Interior door (random side)
            int doorSide = random.Next(4);
            Vector3Int doorPos;

            switch (doorSide)
            {
                case 0:
                    doorPos = new Vector3Int(roomWorldPos.x + room.size.x / 2, roomWorldPos.y, 0);
                    break;
                case 1:
                    doorPos = new Vector3Int(roomWorldPos.x + room.size.x / 2, roomWorldPos.y + room.size.y - 1, 0);
                    break;
                case 2:
                    doorPos = new Vector3Int(roomWorldPos.x, roomWorldPos.y + room.size.y / 2, 0);
                    break;
                default:
                    doorPos = new Vector3Int(roomWorldPos.x + room.size.x - 1, roomWorldPos.y + room.size.y / 2, 0);
                    break;
            }

            if (doorTile != null)
            {
                wallTilemap.SetTile(doorPos, doorTile);
            }
        }
    }

    // === FURNITURE GENERATION ===

    private void GenerateFurniture(BuildingData building)
    {
        if (furnitureTilemap == null || furnitureTiles == null || furnitureTiles.Length == 0)
            return;

        foreach (RoomData room in building.rooms)
        {
            int furnitureCount = Mathf.RoundToInt(room.size.x * room.size.y * furnitureDensity);

            for (int i = 0; i < furnitureCount; i++)
            {
                // Random position within room
                Vector2Int roomWorldPos = building.position + room.position;
                int x = random.Next(1, room.size.x - 1);
                int y = random.Next(1, room.size.y - 1);

                Vector3Int furniturePos = new Vector3Int(roomWorldPos.x + x, roomWorldPos.y + y, 0);

                // Check if position is empty
                if (furnitureTilemap.GetTile(furniturePos) == null)
                {
                    TileBase furnitureTile = furnitureTiles[random.Next(furnitureTiles.Length)];
                    furnitureTilemap.SetTile(furniturePos, furnitureTile);
                }
            }
        }
    }

    // === PUBLIC METHODS ===

    public void GenerateBuildings(WorldGenerator worldGenerator)
    {
        // This can be called by WorldGenerator to generate all buildings
        // For now, buildings are generated as part of WorldGenerator chunks
        Debug.Log("BuildingGenerator: Batch generation requested");
    }

    public BuildingData GetBuildingAt(Vector2Int position)
    {
        foreach (BuildingData building in generatedBuildings)
        {
            if (IsPositionInBuilding(position, building))
            {
                return building;
            }
        }
        return null;
    }

    public bool IsPositionInBuilding(Vector2Int position, BuildingData building)
    {
        return position.x >= building.position.x &&
               position.x < building.position.x + building.size.x &&
               position.y >= building.position.y &&
               position.y < building.position.y + building.size.y;
    }

    public RoomData GetRoomAt(Vector2Int position, BuildingData building)
    {
        foreach (RoomData room in building.rooms)
        {
            Vector2Int roomWorldPos = building.position + room.position;

            if (position.x >= roomWorldPos.x &&
                position.x < roomWorldPos.x + room.size.x &&
                position.y >= roomWorldPos.y &&
                position.y < roomWorldPos.y + room.size.y)
            {
                return room;
            }
        }
        return null;
    }

    public List<BuildingData> GetAllBuildings()
    {
        return new List<BuildingData>(generatedBuildings);
    }

    // === DEBUG ===

    [ContextMenu("Debug: Print Building Count")]
    public void DebugPrintCount()
    {
        Debug.Log($"Total buildings generated: {generatedBuildings.Count}");

        Dictionary<BuildingType, int> counts = new Dictionary<BuildingType, int>();
        foreach (BuildingData building in generatedBuildings)
        {
            if (!counts.ContainsKey(building.type))
                counts[building.type] = 0;
            counts[building.type]++;
        }

        foreach (var kvp in counts)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
    }
}

// === DATA CLASSES ===

[System.Serializable]
public class BuildingData
{
    public string buildingID;
    public Vector2Int position;
    public Vector2Int size;
    public BuildingType type;
    public List<RoomData> rooms;
    public Vector2Int entrancePosition;
    public bool isLooted;
    public bool hasZombies;
    public int zombieCount;
}

[System.Serializable]
public class RoomData
{
    public RoomType roomType;
    public Vector2Int position; // Relative to building
    public Vector2Int size;
    public bool hasLoot;
    public float lootQuality; // 0-1
    public bool isLooted;
}

[System.Serializable]
public class BuildingTemplate
{
    public Vector2Int minSize;
    public Vector2Int maxSize;
    public List<RoomType> requiredRooms;
    public List<RoomType> optionalRooms;
    public bool hasWindows;
    public float windowChance;
}

// === ENUMS ===

public enum RoomType
{
    LivingRoom,
    Bedroom,
    Kitchen,
    Bathroom,
    Office,
    Storage,
    Shop,
    Medical,
    Armory,
    Jail,
    Classroom,
    Cafeteria
}
