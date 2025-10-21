using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

public class CraftingSystem : MonoBehaviour
{
    // Singleton instance
    public static CraftingSystem Instance { get; private set; }

    // === CRAFTING CONFIGURATION ===
    [Header("Crafting Settings")]
    [Tooltip("Enable crafting system")]
    public bool enableCrafting = true;
    [Tooltip("Crafting time multiplier")]
    [Range(0.1f, 2f)] public float craftingSpeedMultiplier = 1f;
    [Tooltip("Show crafting notifications")]
    public bool showNotifications = true;

    // === RECIPE DATABASE ===
    private Dictionary<string, CraftingRecipe> recipes = new Dictionary<string, CraftingRecipe>();
    private Dictionary<string, List<CraftingRecipe>> recipesByCategory = new Dictionary<string, List<CraftingRecipe>>();

    // === RUNTIME VARIABLES ===
    private CraftingRecipe currentlyCrafting = null;
    private float craftingProgress = 0f;
    private bool isCrafting = false;

    // Events
    [Header("Events")]
    public UnityEvent<CraftingRecipe> onCraftingStarted;
    public UnityEvent<CraftingRecipe> onCraftingCompleted;
    public UnityEvent<CraftingRecipe> onCraftingCancelled;
    public UnityEvent<string> onRecipeUnlocked;

    // Unlocked recipes
    private HashSet<string> unlockedRecipes = new HashSet<string>();

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
        // Initialize recipe database
        InitializeRecipes();

        // Unlock basic recipes
        UnlockBasicRecipes();

        Debug.Log($"CraftingSystem initialized with {recipes.Count} recipes");
    }

    void Update()
    {
        if (isCrafting)
        {
            UpdateCrafting();
        }
    }

    // === RECIPE DATABASE ===

    private void InitializeRecipes()
    {
        // WEAPONS
        AddRecipe("wooden_spear", "Wooden Spear", "Weapons", 5f,
            new Dictionary<string, int> { { "wood_plank", 2 }, { "rope", 1 } },
            "wooden_spear", 1, requiredSkill: "Carpentry", requiredLevel: 1);

        AddRecipe("reinforced_bat", "Reinforced Baseball Bat", "Weapons", 10f,
            new Dictionary<string, int> { { "baseball_bat", 1 }, { "nails", 10 }, { "duct_tape", 1 } },
            "reinforced_bat", 1, requiredSkill: "Carpentry", requiredLevel: 2);

        AddRecipe("molotov", "Molotov Cocktail", "Weapons", 3f,
            new Dictionary<string, int> { { "water_bottle", 1 }, { "cloth", 1 }, { "lighter", 1 } },
            "molotov", 1, requiredSkill: "Mechanics", requiredLevel: 1);

        AddRecipe("pipe_bomb", "Pipe Bomb", "Weapons", 15f,
            new Dictionary<string, int> { { "scrap_metal", 2 }, { "duct_tape", 1 }, { "nails", 20 } },
            "pipe_bomb", 1, requiredSkill: "Mechanics", requiredLevel: 5);

        // TOOLS
        AddRecipe("stone_axe", "Stone Axe", "Tools", 8f,
            new Dictionary<string, int> { { "wood_plank", 1 }, { "scrap_metal", 2 }, { "rope", 1 } },
            "stone_axe", 1, requiredSkill: "Carpentry", requiredLevel: 2);

        AddRecipe("lockpick", "Lockpick", "Tools", 5f,
            new Dictionary<string, int> { { "scrap_metal", 1 } },
            "lockpick", 3, requiredSkill: "Mechanics", requiredLevel: 2);

        AddRecipe("fishing_rod", "Fishing Rod", "Tools", 10f,
            new Dictionary<string, int> { { "wood_plank", 2 }, { "rope", 2 } },
            "fishing_rod", 1, requiredSkill: "Carpentry", requiredLevel: 3);

        // MEDICAL
        AddRecipe("herbal_medicine", "Herbal Medicine", "Medical", 15f,
            new Dictionary<string, int> { { "cloth", 2 } },
            "herbal_medicine", 2, requiredSkill: "Foraging", requiredLevel: 5);

        AddRecipe("improvised_bandage", "Improvised Bandage", "Medical", 5f,
            new Dictionary<string, int> { { "cloth", 3 } },
            "improvised_bandage", 5, requiredSkill: "Foraging", requiredLevel: 1);

        AddRecipe("splint", "Splint", "Medical", 8f,
            new Dictionary<string, int> { { "wood_plank", 2 }, { "cloth", 2 } },
            "splint", 1, requiredSkill: "Foraging", requiredLevel: 3);

        // FOOD
        AddRecipe("cooked_meat", "Cooked Meat", "Food", 10f,
            new Dictionary<string, int> { { "raw_meat", 1 } },
            "cooked_meat", 1, requiredSkill: "Cooking", requiredLevel: 1);

        AddRecipe("stew", "Stew", "Food", 20f,
            new Dictionary<string, int> { { "canned_soup", 1 }, { "water_bottle", 1 }, { "cooked_meat", 1 } },
            "stew", 1, requiredSkill: "Cooking", requiredLevel: 3);

        AddRecipe("sandwich", "Sandwich", "Food", 5f,
            new Dictionary<string, int> { { "bread", 2 }, { "cooked_meat", 1 } },
            "sandwich", 1, requiredSkill: "Cooking", requiredLevel: 1);

        AddRecipe("soup", "Soup", "Food", 15f,
            new Dictionary<string, int> { { "canned_beans", 1 }, { "water_bottle", 1 } },
            "soup", 1, requiredSkill: "Cooking", requiredLevel: 2);

        // BARRICADES
        AddRecipe("wooden_barricade", "Wooden Barricade", "Barricades", 20f,
            new Dictionary<string, int> { { "wood_plank", 5 }, { "nails", 20 } },
            "wooden_barricade", 1, requiredSkill: "Carpentry", requiredLevel: 1);

        AddRecipe("metal_barricade", "Metal Barricade", "Barricades", 30f,
            new Dictionary<string, int> { { "scrap_metal", 8 }, { "wood_plank", 3 } },
            "metal_barricade", 1, requiredSkill: "Carpentry", requiredLevel: 5);

        AddRecipe("spike_trap", "Spike Trap", "Barricades", 15f,
            new Dictionary<string, int> { { "wood_plank", 3 }, { "nails", 30 } },
            "spike_trap", 1, requiredSkill: "Carpentry", requiredLevel: 3);

        // STRUCTURES
        AddRecipe("campfire", "Campfire", "Structures", 10f,
            new Dictionary<string, int> { { "wood_plank", 5 } },
            "campfire", 1, requiredSkill: "Carpentry", requiredLevel: 1);

        AddRecipe("storage_box", "Storage Box", "Structures", 25f,
            new Dictionary<string, int> { { "wood_plank", 10 }, { "nails", 20 } },
            "storage_box", 1, requiredSkill: "Carpentry", requiredLevel: 4);

        AddRecipe("water_collector", "Water Collector", "Structures", 30f,
            new Dictionary<string, int> { { "scrap_metal", 5 }, { "wood_plank", 5 } },
            "water_collector", 1, requiredSkill: "Mechanics", requiredLevel: 5);

        AddRecipe("wall", "Wall", "Structures", 40f,
            new Dictionary<string, int> { { "wood_plank", 8 }, { "nails", 30 } },
            "wall", 1, requiredSkill: "Carpentry", requiredLevel: 5);

        AddRecipe("door", "Door", "Structures", 35f,
            new Dictionary<string, int> { { "wood_plank", 6 }, { "nails", 25 }, { "scrap_metal", 2 } },
            "door", 1, requiredSkill: "Carpentry", requiredLevel: 6);

        // AMMO
        AddRecipe("makeshift_arrows", "Makeshift Arrows", "Ammo", 10f,
            new Dictionary<string, int> { { "wood_plank", 1 }, { "scrap_metal", 1 } },
            "makeshift_arrows", 10, requiredSkill: "Carpentry", requiredLevel: 2);

        AddRecipe("shotgun_shells_craft", "Shotgun Shells (Craft)", "Ammo", 20f,
            new Dictionary<string, int> { { "scrap_metal", 2 }, { "nails", 10 } },
            "shotgun_shells", 5, requiredSkill: "Mechanics", requiredLevel: 7);

        // MISC
        AddRecipe("rope", "Rope", "Misc", 10f,
            new Dictionary<string, int> { { "cloth", 5 } },
            "rope", 1, requiredSkill: "Carpentry", requiredLevel: 1);

        AddRecipe("cloth_from_rags", "Cloth", "Misc", 5f,
            new Dictionary<string, int> { { "cloth", 2 } },
            "cloth", 5, requiredSkill: "Foraging", requiredLevel: 1);

        AddRecipe("repair_kit", "Repair Kit", "Misc", 15f,
            new Dictionary<string, int> { { "duct_tape", 2 }, { "scrap_metal", 2 } },
            "repair_kit", 1, requiredSkill: "Mechanics", requiredLevel: 3);

        Debug.Log($"Initialized {recipes.Count} crafting recipes");
    }

    private void AddRecipe(string id, string name, string category, float time,
                          Dictionary<string, int> ingredients, string outputItem, int outputCount,
                          string requiredSkill = "", int requiredLevel = 0)
    {
        CraftingRecipe recipe = new CraftingRecipe
        {
            recipeID = id,
            recipeName = name,
            category = category,
            craftingTime = time,
            ingredients = ingredients,
            outputItemID = outputItem,
            outputCount = outputCount,
            requiredSkill = requiredSkill,
            requiredSkillLevel = requiredLevel,
            isUnlocked = false
        };

        recipes[id] = recipe;

        // Add to category list
        if (!recipesByCategory.ContainsKey(category))
        {
            recipesByCategory[category] = new List<CraftingRecipe>();
        }
        recipesByCategory[category].Add(recipe);
    }

    private void UnlockBasicRecipes()
    {
        // Unlock all recipes with level 1 requirement
        foreach (var recipe in recipes.Values)
        {
            if (recipe.requiredSkillLevel <= 1)
            {
                UnlockRecipe(recipe.recipeID);
            }
        }
    }

    // === CRAFTING LOGIC ===

    public bool StartCrafting(string recipeID)
    {
        if (!enableCrafting || isCrafting)
        {
            Debug.LogWarning("Cannot start crafting: system disabled or already crafting");
            return false;
        }

        if (!recipes.ContainsKey(recipeID))
        {
            Debug.LogWarning($"Recipe {recipeID} not found");
            return false;
        }

        CraftingRecipe recipe = recipes[recipeID];

        // Check if recipe is unlocked
        if (!recipe.isUnlocked)
        {
            Debug.LogWarning($"Recipe {recipe.recipeName} is locked");
            return false;
        }

        // Check skill requirements
        if (!string.IsNullOrEmpty(recipe.requiredSkill))
        {
            if (SkillSystem.Instance != null)
            {
                int skillLevel = SkillSystem.Instance.GetSkillLevel(recipe.requiredSkill);
                if (skillLevel < recipe.requiredSkillLevel)
                {
                    Debug.LogWarning($"Recipe requires {recipe.requiredSkill} level {recipe.requiredSkillLevel}");
                    return false;
                }
            }
        }

        // Check if player has ingredients
        if (!HasRequiredIngredients(recipe))
        {
            Debug.LogWarning("Missing required ingredients");
            return false;
        }

        // Consume ingredients
        ConsumeIngredients(recipe);

        // Start crafting
        currentlyCrafting = recipe;
        craftingProgress = 0f;
        isCrafting = true;

        onCraftingStarted?.Invoke(recipe);

        Debug.Log($"Started crafting {recipe.recipeName}");
        return true;
    }

    private void UpdateCrafting()
    {
        if (currentlyCrafting == null)
        {
            isCrafting = false;
            return;
        }

        // Apply skill speed bonus
        float speedMultiplier = craftingSpeedMultiplier;

        if (SkillSystem.Instance != null && !string.IsNullOrEmpty(currentlyCrafting.requiredSkill))
        {
            float skillBonus = SkillSystem.Instance.GetCraftingSpeedBonus(currentlyCrafting.requiredSkill);
            speedMultiplier *= (1f + skillBonus);
        }

        // Update progress
        craftingProgress += Time.deltaTime * speedMultiplier;

        // Check if complete
        if (craftingProgress >= currentlyCrafting.craftingTime)
        {
            CompleteCrafting();
        }
    }

    private void CompleteCrafting()
    {
        if (currentlyCrafting == null)
            return;

        // Give output item to player
        if (PlayerInventory.Instance != null)
        {
            if (LootSpawner.Instance != null)
            {
                ItemDefinition itemDef = LootSpawner.Instance.GetItemDefinition(currentlyCrafting.outputItemID);
                if (itemDef != null)
                {
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

                    PlayerInventory.Instance.AddItem(item, currentlyCrafting.outputCount);
                }
            }
        }

        // Gain skill XP
        if (SkillSystem.Instance != null && !string.IsNullOrEmpty(currentlyCrafting.requiredSkill))
        {
            float xpGain = currentlyCrafting.craftingTime * 0.5f; // XP based on crafting time
            SkillSystem.Instance.GainXP(currentlyCrafting.requiredSkill, xpGain);
        }

        // Track statistics
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.IncrementStatistic("itemsCrafted", 1f);
        }

        // Play crafting sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCrafting();
        }

        onCraftingCompleted?.Invoke(currentlyCrafting);

        Debug.Log($"Completed crafting {currentlyCrafting.recipeName} x{currentlyCrafting.outputCount}");

        // Reset crafting state
        currentlyCrafting = null;
        craftingProgress = 0f;
        isCrafting = false;
    }

    public void CancelCrafting()
    {
        if (!isCrafting || currentlyCrafting == null)
            return;

        // Return ingredients (50% penalty)
        ReturnIngredients(currentlyCrafting, 0.5f);

        onCraftingCancelled?.Invoke(currentlyCrafting);

        Debug.Log($"Cancelled crafting {currentlyCrafting.recipeName}");

        currentlyCrafting = null;
        craftingProgress = 0f;
        isCrafting = false;
    }

    // === INGREDIENT MANAGEMENT ===

    private bool HasRequiredIngredients(CraftingRecipe recipe)
    {
        if (PlayerInventory.Instance == null)
            return false;

        foreach (var ingredient in recipe.ingredients)
        {
            if (!PlayerInventory.Instance.HasItem(ingredient.Key, ingredient.Value))
            {
                return false;
            }
        }

        return true;
    }

    private void ConsumeIngredients(CraftingRecipe recipe)
    {
        if (PlayerInventory.Instance == null)
            return;

        foreach (var ingredient in recipe.ingredients)
        {
            PlayerInventory.Instance.RemoveItem(
                new Item { itemID = ingredient.Key },
                ingredient.Value
            );
        }

        Debug.Log($"Consumed ingredients for {recipe.recipeName}");
    }

    private void ReturnIngredients(CraftingRecipe recipe, float returnPercentage)
    {
        if (PlayerInventory.Instance == null)
            return;

        foreach (var ingredient in recipe.ingredients)
        {
            int returnCount = Mathf.RoundToInt(ingredient.Value * returnPercentage);
            if (returnCount > 0)
            {
                // Get item definition
                if (LootSpawner.Instance != null)
                {
                    ItemDefinition itemDef = LootSpawner.Instance.GetItemDefinition(ingredient.Key);
                    if (itemDef != null)
                    {
                        Item item = new Item { itemID = itemDef.itemID };
                        PlayerInventory.Instance.AddItem(item, returnCount);
                    }
                }
            }
        }
    }

    // === RECIPE MANAGEMENT ===

    public void UnlockRecipe(string recipeID)
    {
        if (!recipes.ContainsKey(recipeID))
            return;

        if (unlockedRecipes.Contains(recipeID))
            return;

        recipes[recipeID].isUnlocked = true;
        unlockedRecipes.Add(recipeID);

        onRecipeUnlocked?.Invoke(recipeID);

        Debug.Log($"Unlocked recipe: {recipes[recipeID].recipeName}");
    }

    public void UnlockRecipesBySkill(string skillName, int skillLevel)
    {
        foreach (var recipe in recipes.Values)
        {
            if (recipe.requiredSkill == skillName && recipe.requiredSkillLevel <= skillLevel)
            {
                UnlockRecipe(recipe.recipeID);
            }
        }
    }

    public bool CanCraft(string recipeID)
    {
        if (!recipes.ContainsKey(recipeID))
            return false;

        CraftingRecipe recipe = recipes[recipeID];

        // Check if unlocked
        if (!recipe.isUnlocked)
            return false;

        // Check skill level
        if (!string.IsNullOrEmpty(recipe.requiredSkill) && SkillSystem.Instance != null)
        {
            int skillLevel = SkillSystem.Instance.GetSkillLevel(recipe.requiredSkill);
            if (skillLevel < recipe.requiredSkillLevel)
                return false;
        }

        // Check ingredients
        if (!HasRequiredIngredients(recipe))
            return false;

        return true;
    }

    // === QUERIES ===

    public CraftingRecipe GetRecipe(string recipeID)
    {
        return recipes.ContainsKey(recipeID) ? recipes[recipeID] : null;
    }

    public List<CraftingRecipe> GetAllRecipes()
    {
        return new List<CraftingRecipe>(recipes.Values);
    }

    public List<CraftingRecipe> GetRecipesByCategory(string category)
    {
        return recipesByCategory.ContainsKey(category)
            ? new List<CraftingRecipe>(recipesByCategory[category])
            : new List<CraftingRecipe>();
    }

    public List<CraftingRecipe> GetUnlockedRecipes()
    {
        return recipes.Values.Where(r => r.isUnlocked).ToList();
    }

    public List<CraftingRecipe> GetCraftableRecipes()
    {
        return recipes.Values.Where(r => CanCraft(r.recipeID)).ToList();
    }

    public List<string> GetAllCategories()
    {
        return new List<string>(recipesByCategory.Keys);
    }

    public bool IsCrafting() => isCrafting;
    public CraftingRecipe GetCurrentRecipe() => currentlyCrafting;
    public float GetCraftingProgress() => craftingProgress;
    public float GetCraftingProgressPercentage()
    {
        if (currentlyCrafting == null) return 0f;
        return craftingProgress / currentlyCrafting.craftingTime;
    }

    // === DEBUG ===

    [ContextMenu("Debug: Unlock All Recipes")]
    public void DebugUnlockAll()
    {
        foreach (var recipe in recipes.Keys)
        {
            UnlockRecipe(recipe);
        }
        Debug.Log("Unlocked all recipes");
    }

    [ContextMenu("Debug: Print Recipes")]
    public void DebugPrintRecipes()
    {
        Debug.Log($"=== CRAFTING RECIPES ({recipes.Count}) ===");

        foreach (var category in recipesByCategory.Keys)
        {
            Debug.Log($"\n{category}:");
            foreach (var recipe in recipesByCategory[category])
            {
                string status = recipe.isUnlocked ? "UNLOCKED" : "LOCKED";
                Debug.Log($"  - {recipe.recipeName} [{status}] ({recipe.craftingTime}s)");
            }
        }
    }
}

// === DATA CLASSES ===

[System.Serializable]
public class CraftingRecipe
{
    public string recipeID;
    public string recipeName;
    public string category;
    public float craftingTime; // In seconds
    public Dictionary<string, int> ingredients; // ItemID -> Count
    public string outputItemID;
    public int outputCount;
    public string requiredSkill;
    public int requiredSkillLevel;
    public bool isUnlocked;
}
