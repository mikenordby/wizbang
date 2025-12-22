using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Tracks items collected by the player during a run.
/// Displays item icons in the HUD and manages item stacking.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxUniqueItems = 20;
    
    // Item storage: itemId -> (definition, count)
    private Dictionary<string, InventorySlot> items = new Dictionary<string, InventorySlot>();
    private List<string> itemOrder = new List<string>(); // Maintains pickup order for display
    
    public int UniqueItemCount => items.Count;
    public int TotalItemCount 
    {
        get 
        {
            int total = 0;
            foreach (var slot in items.Values)
                total += slot.count;
            return total;
        }
    }
    
    public event Action<ItemDefinition, int> OnItemAdded;
    
    // Suppress warning - event reserved for future item removal system
    #pragma warning disable 0067
    public event Action<ItemDefinition, int> OnItemRemoved;
    #pragma warning restore 0067
    
    /// <summary>
    /// Add an item to the inventory and apply its effects.
    /// </summary>
    public bool AddItem(ItemDefinition item)
    {
        if (item == null) return false;
        
        // Check if we already have this item
        if (items.TryGetValue(item.itemId, out var slot))
        {
            // Check stack limit
            if (slot.count >= item.maxStack)
            {
                DebugLog.Warning($"[PlayerInventory] Cannot add more {item.displayName} - stack full ({slot.count}/{item.maxStack})");
                return false;
            }
            
            // Add to stack
            slot.count++;
            items[item.itemId] = slot;
        }
        else
        {
            // Check unique item limit
            if (items.Count >= maxUniqueItems)
            {
                DebugLog.Warning($"[PlayerInventory] Cannot add {item.displayName} - inventory full ({items.Count}/{maxUniqueItems})");
                return false;
            }
            
            // New item
            items[item.itemId] = new InventorySlot { definition = item, count = 1 };
            itemOrder.Add(item.itemId);
        }
        
        // Apply item effects to player
        Player player = GetComponent<Player>();
        if (player != null)
        {
            item.ApplyToPlayer(player);
        }
        
        int newCount = items[item.itemId].count;
        OnItemAdded?.Invoke(item, newCount);
        
        DebugLog.Info($"[PlayerInventory] Added {ItemRarityUtils.GetColoredName(item.rarity, item.displayName)} (now have {newCount})");
        
        // Trigger game event
        GameEvents.TriggerItemCollected(item);
        
        return true;
    }
    
    /// <summary>
    /// Check if player has a specific item.
    /// </summary>
    public bool HasItem(string itemId)
    {
        return items.ContainsKey(itemId);
    }
    
    /// <summary>
    /// Get the count of a specific item.
    /// </summary>
    public int GetItemCount(string itemId)
    {
        return items.TryGetValue(itemId, out var slot) ? slot.count : 0;
    }
    
    /// <summary>
    /// Get all items in pickup order for display.
    /// </summary>
    public List<InventorySlot> GetAllItems()
    {
        List<InventorySlot> result = new List<InventorySlot>();
        foreach (string itemId in itemOrder)
        {
            if (items.TryGetValue(itemId, out var slot))
            {
                result.Add(slot);
            }
        }
        return result;
    }
    
    /// <summary>
    /// Clear all items (for new run).
    /// </summary>
    public void ClearInventory()
    {
        items.Clear();
        itemOrder.Clear();
        PlayerEffects.ResetAll();
        DebugLog.Info("[PlayerInventory] Inventory cleared for new run");
    }
    
    /// <summary>
    /// Get a debug string of all items.
    /// </summary>
    public string GetDebugString()
    {
        if (items.Count == 0) return "Empty inventory";
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"Inventory ({items.Count} unique, {TotalItemCount} total):");
        
        foreach (var slot in GetAllItems())
        {
            sb.AppendLine($"  [{slot.definition.rarity}] {slot.definition.displayName} x{slot.count}");
        }
        
        return sb.ToString();
    }
}

/// <summary>
/// Represents a slot in the player's inventory.
/// </summary>
[System.Serializable]
public struct InventorySlot
{
    public ItemDefinition definition;
    public int count;
}

