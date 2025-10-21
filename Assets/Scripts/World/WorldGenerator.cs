using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    // Singleton instance
    public static WorldGenerator Instance { get; private set; }

    // === WORLD CONFIGURATION ===
    [Header("World Settings")]
    [Tooltip("World seed (0 = random)")]
    public int worldSeed = 0;
    [Tooltip("World size in chunks")]
    public int worldSizeInChunks = 10;
    [Tooltip("Chunk size in tiles")]
    public int chunkSize = 32;
    [Tooltip("Use procedural generation")]
    public bool useProceduralGeneration = true;

    [Header("Tilemap References")]
    public Tilemap groundTilemap;
    public Tilemap wallTilemap;
    public Tilemap decorationTilemap;

    [Header("Tile References")]
    public TileBase grassTile;
    public TileBase dirtTile;
    public TileBase roadTile;
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase doorTile;

    [Header("Generation Settings")]
    [Range(0f, 1f)] public float forestDensity = 0.3f;
    [Range(0f, 1f)] public float roadDensity = 0.1f;
    [Range(0f, 1f)] public float buildingDensity = 0.15f;
    public int minBuildingSize = 4;
    public int maxBuildingSize = 12;

    [Header("Noise Settings")]
    public float noiseScale = 0.1f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    // === RUNTIME VARIABLES ===
    private Dictionary<Vector2Int, Chunk> loadedChunks = new Dictionary<Vector2Int, Chunk>();
    private List<Vector2Int> exploredAreas = new List<Vector2Int>();
    private System.Random random;
    private int actualSeed;
    private Vector2Int playerChunk;

    // Buildings list
    private List<Building> generatedBuildings = new List<Building>();

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
        // Initialize seed
        if (worldSeed == 0)
        {
            actualSeed = Random.Range(1, 999999);
        }
        else
        {
            actualSeed = worldSeed;
        }

        random = new System.Random(actualSeed);

        Debug.Log($"WorldGenerator initialized with seed: {actualSeed}");

        // Generate initial world
        if (useProceduralGeneration)
        {
            GenerateInitialWorld();
        }
    }

    void Update()
    {
        // Track player position and load/unload chunks
        if (PlayerController.Instance != null)
        {
            Vector2Int currentChunk = GetChunkPosition(PlayerController.Instance.transform.position);

            if (currentChunk != playerChunk)
            {
                playerChunk = currentChunk;
                UpdateLoadedChunks(currentChunk);
            }
        }
    }

    // === WORLD GENERATION ===

    private void GenerateInitialWorld()
    {
        // Generate starting area (3x3 chunks around origin)
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int chunkPos = new Vector2Int(x, y);
                GenerateChunk(chunkPos);
            }
        }

        Debug.Log("Initial world generated");
    }

    private void GenerateChunk(Vector2Int chunkPosition)
    {
        if (loadedChunks.ContainsKey(chunkPosition))
        {
            return; // Already generated
        }

        Chunk chunk = new Chunk
        {
            position = chunkPosition,
            tiles = new TileType[chunkSize, chunkSize],
            isGenerated = false
        };

        // Generate terrain
        GenerateTerrain(chunk);

        // Generate roads
        GenerateRoads(chunk);

        // Generate buildings
        GenerateBuildings(chunk);

        chunk.isGenerated = true;
        loadedChunks[chunkPosition] = chunk;

        // Apply to tilemap
        ApplyChunkToTilemap(chunk);

        Debug.Log($"Generated chunk at {chunkPosition}");
    }

    private void GenerateTerrain(Chunk chunk)
    {
        Vector2Int chunkWorldPos = chunk.position * chunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector2Int worldPos = chunkWorldPos + new Vector2Int(x, y);

                // Use Perlin noise for terrain variation
                float noiseValue = PerlinNoise(worldPos.x, worldPos.y);

                // Determine tile type
                if (noiseValue < 0.3f)
                {
                    chunk.tiles[x, y] = TileType.Dirt;
                }
                else if (noiseValue < 0.7f)
                {
                    chunk.tiles[x, y] = TileType.Grass;
                }
                else
                {
                    chunk.tiles[x, y] = TileType.Grass; // Could be forest
                }
            }
        }
    }

    private void GenerateRoads(Chunk chunk)
    {
        // Simple road generation (horizontal and vertical roads)
        bool hasHorizontalRoad = random.NextDouble() < roadDensity;
        bool hasVerticalRoad = random.NextDouble() < roadDensity;

        if (hasHorizontalRoad)
        {
            int roadY = chunkSize / 2;
            for (int x = 0; x < chunkSize; x++)
            {
                chunk.tiles[x, roadY] = TileType.Road;
                chunk.tiles[x, roadY + 1] = TileType.Road;
            }
        }

        if (hasVerticalRoad)
        {
            int roadX = chunkSize / 2;
            for (int y = 0; y < chunkSize; y++)
            {
                chunk.tiles[roadX, y] = TileType.Road;
                chunk.tiles[roadX + 1, y] = TileType.Road;
            }
        }
    }

    private void GenerateBuildings(Chunk chunk)
    {
        // Random building placement
        int buildingCount = Mathf.RoundToInt(buildingDensity * 10);

        for (int i = 0; i < buildingCount; i++)
        {
            int width = random.Next(minBuildingSize, maxBuildingSize + 1);
            int height = random.Next(minBuildingSize, maxBuildingSize + 1);

            int x = random.Next(2, chunkSize - width - 2);
            int y = random.Next(2, chunkSize - height - 2);

            // Check if area is clear
            bool canPlace = true;
            for (int bx = x; bx < x + width; bx++)
            {
                for (int by = y; by < y + height; by++)
                {
                    if (chunk.tiles[bx, by] == TileType.Road || chunk.tiles[bx, by] == TileType.Building)
                    {
                        canPlace = false;
                        break;
                    }
                }
                if (!canPlace) break;
            }

            if (canPlace)
            {
                PlaceBuilding(chunk, x, y, width, height);
            }
        }
    }

    private void PlaceBuilding(Chunk chunk, int x, int y, int width, int height)
    {
        // Floor
        for (int bx = x; bx < x + width; bx++)
        {
            for (int by = y; by < y + height; by++)
            {
                chunk.tiles[bx, by] = TileType.Floor;
            }
        }

        // Walls
        for (int bx = x; bx < x + width; bx++)
        {
            chunk.tiles[bx, y] = TileType.Wall; // Bottom wall
            chunk.tiles[bx, y + height - 1] = TileType.Wall; // Top wall
        }

        for (int by = y; by < y + height; by++)
        {
            chunk.tiles[x, by] = TileType.Wall; // Left wall
            chunk.tiles[x + width - 1, by] = TileType.Wall; // Right wall
        }

        // Door (random side)
        int doorSide = random.Next(4);
        switch (doorSide)
        {
            case 0: // Bottom
                chunk.tiles[x + width / 2, y] = TileType.Door;
                break;
            case 1: // Top
                chunk.tiles[x + width / 2, y + height - 1] = TileType.Door;
                break;
            case 2: // Left
                chunk.tiles[x, y + height / 2] = TileType.Door;
                break;
            case 3: // Right
                chunk.tiles[x + width - 1, y + height / 2] = TileType.Door;
                break;
        }

        // Create building data
        Vector2Int worldPos = chunk.position * chunkSize + new Vector2Int(x, y);
        Building building = new Building
        {
            position = worldPos,
            width = width,
            height = height,
            buildingType = (BuildingType)random.Next(0, 4)
        };

        generatedBuildings.Add(building);
    }

    private void ApplyChunkToTilemap(Chunk chunk)
    {
        if (groundTilemap == null || wallTilemap == null)
        {
            Debug.LogWarning("Tilemaps not assigned!");
            return;
        }

        Vector2Int chunkWorldPos = chunk.position * chunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePos = new Vector3Int(chunkWorldPos.x + x, chunkWorldPos.y + y, 0);
                TileType tileType = chunk.tiles[x, y];

                TileBase tile = GetTileForType(tileType);

                switch (tileType)
                {
                    case TileType.Wall:
                    case TileType.Door:
                        wallTilemap.SetTile(tilePos, tile);
                        groundTilemap.SetTile(tilePos, floorTile);
                        break;
                    default:
                        groundTilemap.SetTile(tilePos, tile);
                        break;
                }
            }
        }
    }

    private TileBase GetTileForType(TileType type)
    {
        switch (type)
        {
            case TileType.Grass:
                return grassTile;
            case TileType.Dirt:
                return dirtTile;
            case TileType.Road:
                return roadTile;
            case TileType.Floor:
                return floorTile;
            case TileType.Wall:
                return wallTile;
            case TileType.Door:
                return doorTile;
            default:
                return grassTile;
        }
    }

    // === CHUNK MANAGEMENT ===

    private void UpdateLoadedChunks(Vector2Int playerChunk)
    {
        // Load chunks in render distance (e.g., 2 chunks radius)
        int renderDistance = 2;

        // Load new chunks
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                Vector2Int chunkPos = playerChunk + new Vector2Int(x, y);
                if (!loadedChunks.ContainsKey(chunkPos))
                {
                    GenerateChunk(chunkPos);
                }
            }
        }

        // Unload distant chunks (optional for memory management)
        List<Vector2Int> chunksToUnload = new List<Vector2Int>();
        foreach (var kvp in loadedChunks)
        {
            Vector2Int chunkPos = kvp.Key;
            int distance = Mathf.Max(Mathf.Abs(chunkPos.x - playerChunk.x), Mathf.Abs(chunkPos.y - playerChunk.y));

            if (distance > renderDistance + 1)
            {
                chunksToUnload.Add(chunkPos);
            }
        }

        foreach (Vector2Int chunkPos in chunksToUnload)
        {
            UnloadChunk(chunkPos);
        }
    }

    private void UnloadChunk(Vector2Int chunkPosition)
    {
        if (loadedChunks.ContainsKey(chunkPosition))
        {
            // Clear tilemap tiles for this chunk
            Vector2Int chunkWorldPos = chunkPosition * chunkSize;
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    Vector3Int tilePos = new Vector3Int(chunkWorldPos.x + x, chunkWorldPos.y + y, 0);
                    if (groundTilemap != null) groundTilemap.SetTile(tilePos, null);
                    if (wallTilemap != null) wallTilemap.SetTile(tilePos, null);
                }
            }

            loadedChunks.Remove(chunkPosition);
            Debug.Log($"Unloaded chunk at {chunkPosition}");
        }
    }

    private Vector2Int GetChunkPosition(Vector3 worldPosition)
    {
        int chunkX = Mathf.FloorToInt(worldPosition.x / chunkSize);
        int chunkY = Mathf.FloorToInt(worldPosition.y / chunkSize);
        return new Vector2Int(chunkX, chunkY);
    }

    // === NOISE GENERATION ===

    private float PerlinNoise(float x, float y)
    {
        float total = 0f;
        float frequency = noiseScale;
        float amplitude = 1f;
        float maxValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x + actualSeed) * frequency;
            float sampleY = (y + actualSeed) * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
            total += perlinValue * amplitude;

            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / maxValue;
    }

    // === PUBLIC METHODS ===

    public int GetWorldSeed() => actualSeed;

    public void SetWorldSeed(int seed)
    {
        actualSeed = seed;
        random = new System.Random(seed);
    }

    public List<Vector2Int> GetExploredAreas() => new List<Vector2Int>(exploredAreas);

    public void LoadExploredAreas(List<Vector2Int> areas)
    {
        if (areas != null)
        {
            exploredAreas = new List<Vector2Int>(areas);
        }
    }

    public Building GetNearestBuilding(Vector3 position)
    {
        Building nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Building building in generatedBuildings)
        {
            Vector3 buildingPos = new Vector3(building.position.x, building.position.y, 0);
            float distance = Vector3.Distance(position, buildingPos);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = building;
            }
        }

        return nearest;
    }

    public List<Building> GetBuildingsInRadius(Vector3 position, float radius)
    {
        List<Building> buildings = new List<Building>();

        foreach (Building building in generatedBuildings)
        {
            Vector3 buildingPos = new Vector3(building.position.x, building.position.y, 0);
            float distance = Vector3.Distance(position, buildingPos);

            if (distance <= radius)
            {
                buildings.Add(building);
            }
        }

        return buildings;
    }

    // === DEBUG ===

    [ContextMenu("Debug: Regenerate World")]
    public void DebugRegenerateWorld()
    {
        // Clear all chunks
        List<Vector2Int> allChunks = new List<Vector2Int>(loadedChunks.Keys);
        foreach (Vector2Int chunk in allChunks)
        {
            UnloadChunk(chunk);
        }

        generatedBuildings.Clear();

        // Regenerate
        GenerateInitialWorld();
    }

    [ContextMenu("Debug: Print World Info")]
    public void DebugPrintInfo()
    {
        Debug.Log("=== WORLD GENERATOR INFO ===");
        Debug.Log($"Seed: {actualSeed}");
        Debug.Log($"Loaded Chunks: {loadedChunks.Count}");
        Debug.Log($"Generated Buildings: {generatedBuildings.Count}");
        Debug.Log($"Player Chunk: {playerChunk}");
    }
}

// === DATA CLASSES ===

[System.Serializable]
public class Chunk
{
    public Vector2Int position;
    public TileType[,] tiles;
    public bool isGenerated;
}

[System.Serializable]
public class Building
{
    public Vector2Int position;
    public int width;
    public int height;
    public BuildingType buildingType;
}

public enum TileType
{
    Grass,
    Dirt,
    Road,
    Floor,
    Wall,
    Door,
    Water,
    Forest,
    Building
}

public enum BuildingType
{
    House,
    Store,
    Warehouse,
    Hospital
}
