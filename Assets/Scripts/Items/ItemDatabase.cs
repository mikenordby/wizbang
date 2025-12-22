using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central database for all items. Loads items from Resources/Items folder.
/// Provides methods to query items by rarity, ID, and random selection.
/// </summary>
public class ItemDatabase : MonoBehaviour
{
    private static ItemDatabase instance;
    public static ItemDatabase Instance => instance;
    
    [Header("Item Loading")]
    [SerializeField] private string itemResourcePath = "Items";
    
    private List<ItemDefinition> allItems = new List<ItemDefinition>();
    private Dictionary<string, ItemDefinition> itemsById = new Dictionary<string, ItemDefinition>();
    private Dictionary<ItemRarity, List<ItemDefinition>> itemsByRarity = new Dictionary<ItemRarity, List<ItemDefinition>>();
    
    public List<ItemDefinition> AllItems => allItems;
    public int ItemCount => allItems.Count;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        LoadAllItems();
    }
    
    /// <summary>
    /// Load all ItemDefinition assets from Resources.
    /// Falls back to creating default items if none found.
    /// </summary>
    private void LoadAllItems()
    {
        allItems.Clear();
        itemsById.Clear();
        itemsByRarity.Clear();
        
        // Initialize rarity lists
        foreach (ItemRarity rarity in System.Enum.GetValues(typeof(ItemRarity)))
        {
            itemsByRarity[rarity] = new List<ItemDefinition>();
        }
        
        // Load from Resources
        ItemDefinition[] loadedItems = Resources.LoadAll<ItemDefinition>(itemResourcePath);
        
        foreach (var item in loadedItems)
        {
            RegisterItem(item);
        }
        
        // If no items loaded, create default set
        if (allItems.Count == 0)
        {
            DebugLog.Info("[ItemDatabase] No items found in Resources, creating default item set");
            CreateDefaultItems();
        }
        
        DebugLog.Info($"[ItemDatabase] Loaded {allItems.Count} items: " +
            $"{itemsByRarity[ItemRarity.Common].Count} Common, " +
            $"{itemsByRarity[ItemRarity.Rare].Count} Rare, " +
            $"{itemsByRarity[ItemRarity.Exotic].Count} Exotic, " +
            $"{itemsByRarity[ItemRarity.Legendary].Count} Legendary, " +
            $"{itemsByRarity[ItemRarity.Supreme].Count} Supreme");
    }
    
    private void RegisterItem(ItemDefinition item)
    {
        allItems.Add(item);
        itemsById[item.itemId] = item;
        itemsByRarity[item.rarity].Add(item);
    }
    
    /// <summary>
    /// Create default items programmatically (used when no assets found).
    /// Uses ItemLibrary for comprehensive item definitions.
    /// </summary>
    private void CreateDefaultItems()
    {
        ItemDefinition[] defaultItems = ItemLibrary.CreateDefaultItems();
        foreach (var item in defaultItems)
        {
            RegisterItem(item);
        }
    }
    
    /// <summary>
    /// Get an item by its unique ID.
    /// </summary>
    public ItemDefinition GetItem(string itemId)
    {
        if (itemsById.TryGetValue(itemId, out var item))
            return item;
        
        DebugLog.Warning($"[ItemDatabase] Item not found: {itemId}");
        return null;
    }
    
    /// <summary>
    /// Get all items of a specific rarity.
    /// </summary>
    public List<ItemDefinition> GetItemsByRarity(ItemRarity rarity)
    {
        return itemsByRarity.TryGetValue(rarity, out var items) ? items : new List<ItemDefinition>();
    }
    
    /// <summary>
    /// Get a random item weighted by rarity drop chances.
    /// </summary>
    public ItemDefinition GetRandomItem()
    {
        if (allItems.Count == 0)
        {
            DebugLog.Warning("[ItemDatabase] No items loaded!");
            return null;
        }
        
        // Calculate total weight
        float totalWeight = 0f;
        foreach (var item in allItems)
        {
            totalWeight += ItemRarityUtils.GetDropWeight(item.rarity);
        }
        
        // Roll
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        
        foreach (var item in allItems)
        {
            cumulative += ItemRarityUtils.GetDropWeight(item.rarity);
            if (roll <= cumulative)
            {
                return item;
            }
        }
        
        // Fallback (shouldn't reach here)
        return allItems[Random.Range(0, allItems.Count)];
    }
    
    /// <summary>
    /// Get a random item of a specific rarity.
    /// </summary>
    public ItemDefinition GetRandomItemOfRarity(ItemRarity rarity)
    {
        var items = GetItemsByRarity(rarity);
        if (items.Count == 0)
        {
            DebugLog.Warning($"[ItemDatabase] No items of rarity {rarity}");
            return null;
        }
        
        return items[Random.Range(0, items.Count)];
    }
    
    /// <summary>
    /// Get a random rarity based on drop weights.
    /// </summary>
    public ItemRarity GetRandomRarity()
    {
        float totalWeight = 0f;
        foreach (ItemRarity rarity in System.Enum.GetValues(typeof(ItemRarity)))
        {
            if (itemsByRarity[rarity].Count > 0) // Only consider rarities that have items
            {
                totalWeight += ItemRarityUtils.GetDropWeight(rarity);
            }
        }
        
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        
        foreach (ItemRarity rarity in System.Enum.GetValues(typeof(ItemRarity)))
        {
            if (itemsByRarity[rarity].Count == 0) continue;
            
            cumulative += ItemRarityUtils.GetDropWeight(rarity);
            if (roll <= cumulative)
            {
                return rarity;
            }
        }
        
        return ItemRarity.Common;
    }
}

