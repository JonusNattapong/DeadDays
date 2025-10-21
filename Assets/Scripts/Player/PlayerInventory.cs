using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

public class PlayerInventory : MonoBehaviour
{
    // Singleton instance
    public static PlayerInventory Instance { get; private set; }

    // === INVENTORY CONFIGURATION ===
    [Header("Inventory Configuration")]
    [Tooltip("Maximum number of inventory slots")]
    public int maxInventorySlots = 20;
    [Tooltip("Maximum weight capacity in kg")]
    public float maxWeight = 50f;
    [Tooltip("Quick access slots (hotbar)")]
    public int quickSlots = 6;

    // === EQUIPMENT SLOTS ===
    [Header("Equipment Slots")]
    public Item equippedWeapon;
    public Item equippedBackpack;
    public Item equippedArmor;
    public Item equippedAccessory;

    // Inventory data
    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private float currentWeight = 0f;
    private int selectedQuickSlot = 0;

    // Events
    [Header("Events")]
    public UnityEvent<Item> onItemAdded;
    public UnityEvent<Item> onItemRemoved;
    public UnityEvent onInventoryChanged;
    public UnityEvent<int> onQuickSlotChanged;
    public UnityEvent onInventoryFull;
    public UnityEvent onWeightExceeded;

    void Awake()
    {
        // Ensure singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Initialize inventory slots
        InitializeInventory();
    }

    void Update()
    {
        // Handle quick slot selection (1-6 keys)
        HandleQuickSlotInput();

        // Handle item usage
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseSelectedItem();
        }

        // Handle item dropping
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropSelectedItem();
        }
    }

    // === INITIALIZATION ===

    private void InitializeInventory()
    {
        inventorySlots.Clear();
        for (int i = 0; i < maxInventorySlots; i++)
        {
            inventorySlots.Add(new InventorySlot(i));
        }
        currentWeight = 0f;
        Debug.Log($"Inventory initialized with {maxInventorySlots} slots");
    }

    // === ITEM MANAGEMENT ===

    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null)
        {
            Debug.LogWarning("Attempted to add null item to inventory");
            return false;
        }

        // Check weight capacity
        float itemWeight = item.weight * quantity;
        if (currentWeight + itemWeight > maxWeight)
        {
            Debug.LogWarning($"Cannot add {item.itemName}: weight limit exceeded");
            onWeightExceeded?.Invoke();
            return false;
        }

        // Check if item is stackable
        if (item.isStackable)
        {
            // Try to add to existing stack
            foreach (var slot in inventorySlots)
            {
                if (slot.item != null && slot.item.itemID == item.itemID && slot.quantity < item.maxStackSize)
                {
                    int spaceInStack = item.maxStackSize - slot.quantity;
                    int amountToAdd = Mathf.Min(quantity, spaceInStack);
                    slot.quantity += amountToAdd;
                    quantity -= amountToAdd;
                    currentWeight += item.weight * amountToAdd;

                    if (quantity == 0)
                    {
                        onItemAdded?.Invoke(item);
                        onInventoryChanged?.Invoke();
                        Debug.Log($"Added {amountToAdd}x {item.itemName} to existing stack");
                        return true;
                    }
                }
            }
        }

        // Add to new slot(s)
        while (quantity > 0)
        {
            InventorySlot emptySlot = GetEmptySlot();
            if (emptySlot == null)
            {
                Debug.LogWarning($"Inventory full! Cannot add {item.itemName}");
                onInventoryFull?.Invoke();
                return false;
            }

            int amountToAdd = item.isStackable ? Mathf.Min(quantity, item.maxStackSize) : 1;
            emptySlot.item = item;
            emptySlot.quantity = amountToAdd;
            emptySlot.durability = item.durability;
            quantity -= amountToAdd;
            currentWeight += item.weight * amountToAdd;

            Debug.Log($"Added {amountToAdd}x {item.itemName} to slot {emptySlot.slotIndex}");
        }

        onItemAdded?.Invoke(item);
        onInventoryChanged?.Invoke();

        // Play pickup sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayItemPickup();
        }

        return true;
    }

    public bool RemoveItem(Item item, int quantity = 1)
    {
        if (item == null) return false;

        int remainingToRemove = quantity;

        // Find and remove from slots
        for (int i = inventorySlots.Count - 1; i >= 0; i--)
        {
            var slot = inventorySlots[i];
            if (slot.item != null && slot.item.itemID == item.itemID)
            {
                int amountToRemove = Mathf.Min(remainingToRemove, slot.quantity);
                slot.quantity -= amountToRemove;
                remainingToRemove -= amountToRemove;
                currentWeight -= item.weight * amountToRemove;

                if (slot.quantity <= 0)
                {
                    slot.Clear();
                }

                if (remainingToRemove <= 0)
                {
                    break;
                }
            }
        }

        if (remainingToRemove < quantity)
        {
            onItemRemoved?.Invoke(item);
            onInventoryChanged?.Invoke();
            Debug.Log($"Removed {quantity - remainingToRemove}x {item.itemName}");
            return true;
        }

        return false;
    }

    public bool RemoveItemFromSlot(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count)
            return false;

        var slot = inventorySlots[slotIndex];
        if (slot.item == null) return false;

        Item item = slot.item;
        int amountToRemove = Mathf.Min(quantity, slot.quantity);
        slot.quantity -= amountToRemove;
        currentWeight -= item.weight * amountToRemove;

        if (slot.quantity <= 0)
        {
            slot.Clear();
        }

        onItemRemoved?.Invoke(item);
        onInventoryChanged?.Invoke();
        return true;
    }

    public bool HasItem(string itemID, int quantity = 1)
    {
        int count = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot.item != null && slot.item.itemID == itemID)
            {
                count += slot.quantity;
                if (count >= quantity)
                    return true;
            }
        }
        return false;
    }

    public int GetItemCount(string itemID)
    {
        int count = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot.item != null && slot.item.itemID == itemID)
            {
                count += slot.quantity;
            }
        }
        return count;
    }

    public Item GetItemInSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Count)
        {
            return inventorySlots[slotIndex].item;
        }
        return null;
    }

    public InventorySlot GetSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Count)
        {
            return inventorySlots[slotIndex];
        }
        return null;
    }

    public List<InventorySlot> GetAllSlots()
    {
        return inventorySlots;
    }

    private InventorySlot GetEmptySlot()
    {
        return inventorySlots.FirstOrDefault(slot => slot.item == null);
    }

    // === ITEM USAGE ===

    public void UseItem(Item item)
    {
        if (item == null) return;

        switch (item.itemType)
        {
            case ItemType.Food:
                ConsumeFood(item);
                break;
            case ItemType.Drink:
                ConsumeDrink(item);
                break;
            case ItemType.Medicine:
                UseMedicine(item);
                break;
            case ItemType.Tool:
                UseTool(item);
                break;
            case ItemType.Weapon:
                EquipWeapon(item);
                break;
            case ItemType.Armor:
                EquipArmor(item);
                break;
            case ItemType.Consumable:
                ConsumeItem(item);
                break;
            default:
                Debug.Log($"Cannot use item: {item.itemName}");
                break;
        }
    }

    public void UseItemInSlot(int slotIndex)
    {
        Item item = GetItemInSlot(slotIndex);
        if (item != null)
        {
            UseItem(item);
        }
    }

    public void UseSelectedItem()
    {
        UseItemInSlot(selectedQuickSlot);
    }

    private void ConsumeFood(Item item)
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.Eat(item.foodValue, item.nutritionQuality);
        }
        RemoveItem(item, 1);
        Debug.Log($"Consumed {item.itemName}");
    }

    private void ConsumeDrink(Item item)
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.Drink(item.thirstValue);
        }
        RemoveItem(item, 1);
        Debug.Log($"Drank {item.itemName}");
    }

    private void UseMedicine(Item item)
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ApplyMedicine(item.medicineType);
        }
        RemoveItem(item, 1);
        Debug.Log($"Used medicine: {item.itemName}");
    }

    private void UseTool(Item item)
    {
        // Tools are equipped to use
        equippedWeapon = item;
        Debug.Log($"Equipped tool: {item.itemName}");
    }

    private void ConsumeItem(Item item)
    {
        // Generic consumable
        RemoveItem(item, 1);
        Debug.Log($"Used {item.itemName}");
    }

    // === EQUIPMENT ===

    public void EquipWeapon(Item weapon)
    {
        if (weapon == null) return;

        // Unequip current weapon
        if (equippedWeapon != null)
        {
            // Return to inventory if different
            if (equippedWeapon.itemID != weapon.itemID)
            {
                // Already in inventory, just unequip
            }
        }

        equippedWeapon = weapon;
        Debug.Log($"Equipped weapon: {weapon.itemName}");
        onInventoryChanged?.Invoke();
    }

    public void UnequipWeapon()
    {
        equippedWeapon = null;
        onInventoryChanged?.Invoke();
    }

    public void EquipArmor(Item armor)
    {
        if (armor == null || armor.itemType != ItemType.Armor) return;

        equippedArmor = armor;
        Debug.Log($"Equipped armor: {armor.itemName}");
        onInventoryChanged?.Invoke();
    }

    public void EquipBackpack(Item backpack)
    {
        if (backpack == null) return;

        equippedBackpack = backpack;
        // Increase inventory capacity
        maxWeight += backpack.weightCapacityBonus;
        Debug.Log($"Equipped backpack: {backpack.itemName}. Weight capacity: {maxWeight}");
        onInventoryChanged?.Invoke();
    }

    // === ITEM DROPPING ===

    public void DropItem(Item item, int quantity = 1)
    {
        if (RemoveItem(item, quantity))
        {
            // Spawn item in world
            SpawnItemInWorld(item, quantity);
            Debug.Log($"Dropped {quantity}x {item.itemName}");
        }
    }

    public void DropItemFromSlot(int slotIndex)
    {
        var slot = GetSlot(slotIndex);
        if (slot != null && slot.item != null)
        {
            DropItem(slot.item, slot.quantity);
        }
    }

    public void DropSelectedItem()
    {
        DropItemFromSlot(selectedQuickSlot);
    }

    private void SpawnItemInWorld(Item item, int quantity)
    {
        // TODO: Implement item spawning in world
        // Create a GameObject with the item sprite and pickup script
        Vector3 dropPosition = transform.position + transform.right * 1.5f;

        // For now, just log
        Debug.Log($"Item {item.itemName} x{quantity} would spawn at {dropPosition}");

        // In full implementation:
        // GameObject itemObj = Instantiate(itemPrefab, dropPosition, Quaternion.identity);
        // ItemPickup pickup = itemObj.GetComponent<ItemPickup>();
        // pickup.SetItem(item, quantity);
    }

    // === QUICK SLOTS (HOTBAR) ===

    private void HandleQuickSlotInput()
    {
        // Number keys 1-6 for quick slot selection
        for (int i = 0; i < quickSlots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetSelectedQuickSlot(i);
            }
        }

        // Mouse wheel for cycling
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            CycleQuickSlot(1);
        }
        else if (scroll < 0f)
        {
            CycleQuickSlot(-1);
        }
    }

    public void SetSelectedQuickSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < quickSlots)
        {
            selectedQuickSlot = slotIndex;
            onQuickSlotChanged?.Invoke(selectedQuickSlot);
            Debug.Log($"Selected quick slot {selectedQuickSlot}");
        }
    }

    public void CycleQuickSlot(int direction)
    {
        selectedQuickSlot += direction;
        if (selectedQuickSlot < 0)
            selectedQuickSlot = quickSlots - 1;
        else if (selectedQuickSlot >= quickSlots)
            selectedQuickSlot = 0;

        onQuickSlotChanged?.Invoke(selectedQuickSlot);
    }

    public int GetSelectedQuickSlot()
    {
        return selectedQuickSlot;
    }

    // === INVENTORY QUERIES ===

    public bool IsFull()
    {
        return GetEmptySlot() == null;
    }

    public bool IsOverweight()
    {
        return currentWeight > maxWeight;
    }

    public float GetCurrentWeight()
    {
        return currentWeight;
    }

    public float GetWeightPercentage()
    {
        return currentWeight / maxWeight;
    }

    public int GetEmptySlotCount()
    {
        return inventorySlots.Count(slot => slot.item == null);
    }

    public int GetUsedSlotCount()
    {
        return inventorySlots.Count(slot => slot.item != null);
    }

    // === SAVE/LOAD SUPPORT ===

    public List<InventoryItemData> GetInventoryData()
    {
        List<InventoryItemData> data = new List<InventoryItemData>();
        foreach (var slot in inventorySlots)
        {
            if (slot.item != null)
            {
                data.Add(new InventoryItemData
                {
                    itemID = slot.item.itemID,
                    quantity = slot.quantity,
                    durability = slot.durability,
                    slotIndex = slot.slotIndex
                });
            }
        }
        return data;
    }

    public void LoadInventoryData(List<InventoryItemData> data)
    {
        // Clear current inventory
        InitializeInventory();

        // Load items
        foreach (var itemData in data)
        {
            // TODO: Get item from item database
            // Item item = ItemDatabase.GetItem(itemData.itemID);
            // if (item != null)
            // {
            //     var slot = inventorySlots[itemData.slotIndex];
            //     slot.item = item;
            //     slot.quantity = itemData.quantity;
            //     slot.durability = itemData.durability;
            //     currentWeight += item.weight * itemData.quantity;
            // }
        }

        onInventoryChanged?.Invoke();
        Debug.Log("Inventory loaded from save data");
    }

    // === UTILITY ===

    public void SortInventory()
    {
        // Sort by item type, then by name
        var items = new List<(Item item, int quantity, float durability)>();

        foreach (var slot in inventorySlots)
        {
            if (slot.item != null)
            {
                items.Add((slot.item, slot.quantity, slot.durability));
                slot.Clear();
            }
        }

        items = items.OrderBy(x => x.item.itemType)
                     .ThenBy(x => x.item.itemName)
                     .ToList();

        int slotIndex = 0;
        foreach (var itemData in items)
        {
            var slot = inventorySlots[slotIndex];
            slot.item = itemData.item;
            slot.quantity = itemData.quantity;
            slot.durability = itemData.durability;
            slotIndex++;
        }

        onInventoryChanged?.Invoke();
        Debug.Log("Inventory sorted");
    }

    public void ClearInventory()
    {
        InitializeInventory();
        onInventoryChanged?.Invoke();
        Debug.Log("Inventory cleared");
    }
}

// === INVENTORY SLOT CLASS ===

[System.Serializable]
public class InventorySlot
{
    public int slotIndex;
    public Item item;
    public int quantity;
    public float durability;

    public InventorySlot(int index)
    {
        slotIndex = index;
        item = null;
        quantity = 0;
        durability = 100f;
    }

    public void Clear()
    {
        item = null;
        quantity = 0;
        durability = 100f;
    }

    public bool IsEmpty()
    {
        return item == null;
    }
}

// === ITEM CLASS ===
// This is a basic item class - you may want to create a more detailed one

[System.Serializable]
public class Item
{
    public string itemID;
    public string itemName;
    public string description;
    public ItemType itemType;
    public Sprite icon;
    public float weight;
    public int value;

    // Stackable properties
    public bool isStackable;
    public int maxStackSize = 1;

    // Durability
    public float durability = 100f;
    public float maxDurability = 100f;

    // Food properties
    public float foodValue;
    public float nutritionQuality;

    // Drink properties
    public float thirstValue;

    // Medicine properties
    public string medicineType;

    // Weapon properties
    public float damage;
    public float attackSpeed;
    public float range;

    // Armor properties
    public float defense;

    // Backpack properties
    public float weightCapacityBonus;
}

public enum ItemType
{
    Food,
    Drink,
    Medicine,
    Tool,
    Weapon,
    Armor,
    Material,
    Consumable,
    Key,
    Quest,
    Other
}
