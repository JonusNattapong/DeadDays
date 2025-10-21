using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class SkillSystem : MonoBehaviour
{
    // Singleton instance
    public static SkillSystem Instance { get; private set; }

    // === SKILL CONFIGURATION ===
    [Header("Skill Settings")]
    [Tooltip("Maximum skill level")]
    public int maxSkillLevel = 10;
    [Tooltip("Base XP required for level 1")]
    public float baseXPRequired = 100f;
    [Tooltip("XP multiplier per level")]
    public float xpMultiplier = 1.5f;

    // Skill data
    private Dictionary<string, SkillData> skills = new Dictionary<string, SkillData>();

    // Events
    [Header("Events")]
    public UnityEvent<string, int> onSkillLevelUp;
    public UnityEvent<string, float> onSkillXPGained;
    public UnityEvent<string> onSkillMaxed;

    // Skill names (from README)
    private readonly string[] skillNames = new string[]
    {
        "Carpentry",
        "Cooking",
        "Strength",
        "Fitness",
        "Aiming",
        "Mechanics",
        "Foraging"
    };

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
        // Initialize all skills
        InitializeSkills();
    }

    // === INITIALIZATION ===

    private void InitializeSkills()
    {
        foreach (string skillName in skillNames)
        {
            skills[skillName] = new SkillData
            {
                skillName = skillName,
                level = 1,
                currentXP = 0f,
                totalXP = 0f
            };
        }

        Debug.Log($"Initialized {skills.Count} skills");
    }

    // === XP MANAGEMENT ===

    public void GainXP(string skillName, float xpAmount)
    {
        if (!skills.ContainsKey(skillName))
        {
            Debug.LogWarning($"Skill '{skillName}' not found!");
            return;
        }

        SkillData skill = skills[skillName];

        // Check if already max level
        if (skill.level >= maxSkillLevel)
        {
            return;
        }

        // Add XP
        skill.currentXP += xpAmount;
        skill.totalXP += xpAmount;

        // Trigger event
        onSkillXPGained?.Invoke(skillName, xpAmount);

        // Check for level up
        float xpRequired = GetXPRequiredForNextLevel(skill.level);
        while (skill.currentXP >= xpRequired && skill.level < maxSkillLevel)
        {
            LevelUpSkill(skillName);
            xpRequired = GetXPRequiredForNextLevel(skill.level);
        }

        // Clamp XP if max level
        if (skill.level >= maxSkillLevel)
        {
            skill.currentXP = 0f;
        }
    }

    private void LevelUpSkill(string skillName)
    {
        if (!skills.ContainsKey(skillName))
            return;

        SkillData skill = skills[skillName];

        // Calculate XP required for this level
        float xpRequired = GetXPRequiredForNextLevel(skill.level);

        // Subtract used XP
        skill.currentXP -= xpRequired;

        // Increase level
        skill.level++;

        // Trigger events
        onSkillLevelUp?.Invoke(skillName, skill.level);

        Debug.Log($"LEVEL UP! {skillName} reached level {skill.level}");

        // Apply level-up bonuses
        ApplyLevelUpBonus(skillName, skill.level);

        // Check if maxed
        if (skill.level >= maxSkillLevel)
        {
            onSkillMaxed?.Invoke(skillName);
            Debug.Log($"SKILL MAXED! {skillName} reached max level {maxSkillLevel}");
        }
    }

    private void ApplyLevelUpBonus(string skillName, int newLevel)
    {
        // Apply skill-specific bonuses based on README definitions
        switch (skillName)
        {
            case "Carpentry":
                ApplyCarpentryBonus(newLevel);
                break;
            case "Cooking":
                ApplyCookingBonus(newLevel);
                break;
            case "Strength":
                ApplyStrengthBonus(newLevel);
                break;
            case "Fitness":
                ApplyFitnessBonus(newLevel);
                break;
            case "Aiming":
                ApplyAimingBonus(newLevel);
                break;
            case "Mechanics":
                ApplyMechanicsBonus(newLevel);
                break;
            case "Foraging":
                ApplyForagingBonus(newLevel);
                break;
        }
    }

    // === SKILL-SPECIFIC BONUSES (from README) ===

    private void ApplyCarpentryBonus(int level)
    {
        // Level 1: Board up windows
        // Level 5: Build walls and traps
        // Level 10: Build advanced furniture and bases
        if (level == 5)
        {
            Debug.Log("Carpentry Lv5: Unlocked walls and traps!");
        }
        else if (level == 10)
        {
            Debug.Log("Carpentry Lv10: Unlocked advanced structures!");
        }
    }

    private void ApplyCookingBonus(int level)
    {
        // Level 1: Boil water, grill meat
        // Level 5: Cook complex meals (+nutrition)
        // Level 10: Preserve food longer
        if (level == 5)
        {
            Debug.Log("Cooking Lv5: Unlocked complex recipes!");
        }
        else if (level == 10)
        {
            Debug.Log("Cooking Lv10: Food preservation improved!");
        }
    }

    private void ApplyStrengthBonus(int level)
    {
        // Level 1: Normal attack
        // Level 5: +50% damage, carry heavier items
        // Level 10: +100% damage, push zombie hordes
        if (PlayerCombat.Instance != null)
        {
            // Damage bonus applied via GetSkillModifier
            Debug.Log($"Strength Lv{level}: Damage increased!");
        }

        if (PlayerInventory.Instance != null && level >= 5)
        {
            // Increase weight capacity
            float bonusWeight = 10f * (level - 4);
            PlayerInventory.Instance.maxWeight += bonusWeight;
            Debug.Log($"Strength Lv{level}: Weight capacity +{bonusWeight}kg");
        }
    }

    private void ApplyFitnessBonus(int level)
    {
        // Level 1: Normal run
        // Level 5: Run farther, slower fatigue
        // Level 10: Run longest, jump higher
        if (PlayerController.Instance != null)
        {
            // Speed bonus applied via GetSkillModifier
            Debug.Log($"Fitness Lv{level}: Movement speed improved!");
        }

        if (PlayerStats.Instance != null && level >= 5)
        {
            // Reduce fatigue decay rate
            PlayerStats.Instance.fatigueDecayRate *= 0.9f;
            Debug.Log($"Fitness Lv{level}: Fatigue rate reduced!");
        }
    }

    private void ApplyAimingBonus(int level)
    {
        // Level 1: 30% accuracy
        // Level 5: 60% accuracy
        // Level 10: 90% accuracy + headshot chance
        if (PlayerCombat.Instance != null)
        {
            float accuracy = GetAimingAccuracy(level);
            Debug.Log($"Aiming Lv{level}: Accuracy now {accuracy * 100}%");
        }
    }

    private void ApplyMechanicsBonus(int level)
    {
        // Level 1: Repair tools
        // Level 5: Craft new tools
        // Level 10: Repair vehicles, craft machines
        if (level == 5)
        {
            Debug.Log("Mechanics Lv5: Unlocked tool crafting!");
        }
        else if (level == 10)
        {
            Debug.Log("Mechanics Lv10: Unlocked vehicle repair!");
        }
    }

    private void ApplyForagingBonus(int level)
    {
        // Level 1: Find vegetables, mushrooms
        // Level 5: Identify safe/poisonous plants
        // Level 10: Farm crops
        if (level == 5)
        {
            Debug.Log("Foraging Lv5: Can identify poisonous plants!");
        }
        else if (level == 10)
        {
            Debug.Log("Foraging Lv10: Unlocked farming!");
        }
    }

    // === XP CALCULATION ===

    public float GetXPRequiredForNextLevel(int currentLevel)
    {
        if (currentLevel >= maxSkillLevel)
            return 0f;

        // Formula: baseXP * (multiplier ^ (level - 1))
        return baseXPRequired * Mathf.Pow(xpMultiplier, currentLevel - 1);
    }

    public float GetTotalXPRequiredForLevel(int targetLevel)
    {
        if (targetLevel <= 1)
            return 0f;

        float totalXP = 0f;
        for (int level = 1; level < targetLevel; level++)
        {
            totalXP += GetXPRequiredForNextLevel(level);
        }
        return totalXP;
    }

    // === SKILL QUERIES ===

    public int GetSkillLevel(string skillName)
    {
        if (skills.ContainsKey(skillName))
        {
            return skills[skillName].level;
        }
        return 1;
    }

    public float GetSkillXP(string skillName)
    {
        if (skills.ContainsKey(skillName))
        {
            return skills[skillName].currentXP;
        }
        return 0f;
    }

    public float GetSkillTotalXP(string skillName)
    {
        if (skills.ContainsKey(skillName))
        {
            return skills[skillName].totalXP;
        }
        return 0f;
    }

    public float GetSkillProgress(string skillName)
    {
        if (!skills.ContainsKey(skillName))
            return 0f;

        SkillData skill = skills[skillName];
        if (skill.level >= maxSkillLevel)
            return 1f;

        float xpRequired = GetXPRequiredForNextLevel(skill.level);
        return skill.currentXP / xpRequired;
    }

    public bool IsSkillMaxed(string skillName)
    {
        if (skills.ContainsKey(skillName))
        {
            return skills[skillName].level >= maxSkillLevel;
        }
        return false;
    }

    // === SKILL MODIFIERS ===

    public float GetSkillModifier(string skillName)
    {
        int level = GetSkillLevel(skillName);
        return level - 1; // Returns 0-9 for levels 1-10
    }

    public float GetSkillMultiplier(string skillName, float baseMultiplier = 0.1f)
    {
        // Returns 1.0 to 1.9 (for 10% per level) at default
        int level = GetSkillLevel(skillName);
        return 1f + (level - 1) * baseMultiplier;
    }

    // === SPECIAL SKILL CALCULATIONS ===

    public float GetAimingAccuracy(int level)
    {
        // Level 1: 30%, Level 5: 60%, Level 10: 90%
        return 0.3f + (level - 1) * 0.067f; // ~6.7% per level
    }

    public float GetDamageBonus(string skillName)
    {
        int level = GetSkillLevel(skillName);

        switch (skillName)
        {
            case "Strength":
                // +10% per level, max +100% at level 10
                return (level - 1) * 0.1f;
            case "Aiming":
                // +5% per level for ranged
                return (level - 1) * 0.05f;
            default:
                return 0f;
        }
    }

    public float GetSpeedBonus(string skillName)
    {
        int level = GetSkillLevel(skillName);

        switch (skillName)
        {
            case "Fitness":
                // +5% per level, max +45% at level 10
                return (level - 1) * 0.05f;
            default:
                return 0f;
        }
    }

    public float GetCraftingSpeedBonus(string skillName)
    {
        int level = GetSkillLevel(skillName);

        switch (skillName)
        {
            case "Carpentry":
            case "Mechanics":
            case "Cooking":
                // +10% per level
                return (level - 1) * 0.1f;
            default:
                return 0f;
        }
    }

    public bool CanCraftItem(string itemID)
    {
        // Check skill requirements for crafting
        // TODO: Implement item database with skill requirements
        // For now, return true
        return true;
    }

    public bool CanBuildStructure(string structureID)
    {
        // Check carpentry level for building
        int carpentryLevel = GetSkillLevel("Carpentry");

        // Example structure requirements
        switch (structureID.ToLower())
        {
            case "barricade":
                return carpentryLevel >= 1;
            case "wall":
                return carpentryLevel >= 5;
            case "watchtower":
                return carpentryLevel >= 8;
            case "fortress":
                return carpentryLevel >= 10;
            default:
                return carpentryLevel >= 1;
        }
    }

    // === SKILL MANAGEMENT ===

    public void SetSkillLevel(string skillName, int level)
    {
        if (!skills.ContainsKey(skillName))
            return;

        level = Mathf.Clamp(level, 1, maxSkillLevel);
        skills[skillName].level = level;
        skills[skillName].currentXP = 0f;
        skills[skillName].totalXP = GetTotalXPRequiredForLevel(level);

        Debug.Log($"Set {skillName} to level {level}");
    }

    public void SetSkillXP(string skillName, float xp)
    {
        if (!skills.ContainsKey(skillName))
            return;

        skills[skillName].currentXP = xp;
    }

    public void ResetSkill(string skillName)
    {
        if (!skills.ContainsKey(skillName))
            return;

        skills[skillName].level = 1;
        skills[skillName].currentXP = 0f;
        skills[skillName].totalXP = 0f;

        Debug.Log($"Reset {skillName} to level 1");
    }

    public void ResetAllSkills()
    {
        foreach (var skillName in skillNames)
        {
            ResetSkill(skillName);
        }
        Debug.Log("Reset all skills to level 1");
    }

    // === SAVE/LOAD SUPPORT ===

    public Dictionary<string, int> GetSkillData()
    {
        Dictionary<string, int> data = new Dictionary<string, int>();
        foreach (var kvp in skills)
        {
            data[kvp.Key] = kvp.Value.level;
        }
        return data;
    }

    public Dictionary<string, float> GetSkillXPData()
    {
        Dictionary<string, float> data = new Dictionary<string, float>();
        foreach (var kvp in skills)
        {
            data[kvp.Key] = kvp.Value.currentXP;
        }
        return data;
    }

    public void LoadSkillData(Dictionary<string, int> data)
    {
        if (data == null)
            return;

        foreach (var kvp in data)
        {
            if (skills.ContainsKey(kvp.Key))
            {
                skills[kvp.Key].level = kvp.Value;
            }
        }

        Debug.Log("Skills loaded from save data");
    }

    public void LoadSkillXPData(Dictionary<string, float> data)
    {
        if (data == null)
            return;

        foreach (var kvp in data)
        {
            if (skills.ContainsKey(kvp.Key))
            {
                skills[kvp.Key].currentXP = kvp.Value;
            }
        }

        Debug.Log("Skill XP loaded from save data");
    }

    // === UTILITY ===

    public string[] GetAllSkillNames()
    {
        return skillNames;
    }

    public List<SkillData> GetAllSkills()
    {
        return new List<SkillData>(skills.Values);
    }

    public SkillData GetSkillData(string skillName)
    {
        if (skills.ContainsKey(skillName))
        {
            return skills[skillName];
        }
        return null;
    }

    // === DEBUG ===

    [ContextMenu("Debug: Print All Skills")]
    public void DebugPrintAllSkills()
    {
        Debug.Log("=== SKILL LEVELS ===");
        foreach (var kvp in skills)
        {
            SkillData skill = kvp.Value;
            float progress = GetSkillProgress(kvp.Key) * 100f;
            Debug.Log($"{kvp.Key}: Level {skill.level} ({progress:F1}% to next)");
        }
    }

    [ContextMenu("Debug: Add 50 XP to All Skills")]
    public void DebugAddXPToAll()
    {
        foreach (var skillName in skillNames)
        {
            GainXP(skillName, 50f);
        }
    }

    [ContextMenu("Debug: Max All Skills")]
    public void DebugMaxAllSkills()
    {
        foreach (var skillName in skillNames)
        {
            SetSkillLevel(skillName, maxSkillLevel);
        }
    }
}

// === SKILL DATA CLASS ===

[System.Serializable]
public class SkillData
{
    public string skillName;
    public int level;
    public float currentXP;
    public float totalXP;

    public SkillData()
    {
        level = 1;
        currentXP = 0f;
        totalXP = 0f;
    }
}
