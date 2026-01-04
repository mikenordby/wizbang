using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages and applies item effects to weapons and player.
/// Handles tag-based, slot-specific, global, and fundamental item effects.
/// </summary>
public class ItemEffectsManager : MonoBehaviour
{
    private Player player;
    private WeaponInventory weaponInventory;

    // Track active items for refresh/recalculation
    private List<Item> activeItems = new List<Item>();

    private void Awake()
    {
        player = GetComponent<Player>();
        weaponInventory = GetComponent<WeaponInventory>();

        // Register with GameServices
        GameServices.RegisterItemEffectsManager(this);

        DebugLog.Info("[ItemEffectsManager] Initialized");
    }

    /// <summary>
    /// Apply an item effect to the player.
    /// Items are immediately active and permanent (for now).
    /// </summary>
    public void ApplyItem(Item item)
    {
        if (item == null)
        {
            DebugLog.Warning("[ItemEffectsManager] Cannot apply null item");
            return;
        }

        DebugLog.Info($"[ItemEffectsManager] Applying item: {item.ItemName} ({item.Rarity})");

        // Apply the item's effect
        item.ApplyEffect(player);

        // Track active item
        activeItems.Add(item);

        // Refresh all weapon stats (item might affect them)
        RefreshAllWeaponStats();

        DebugLog.Info($"[ItemEffectsManager] Item applied: {item.ItemName}. Total items: {activeItems.Count}");
    }

    /// <summary>
    /// Remove an item effect (for testing or special mechanics).
    /// </summary>
    public void RemoveItem(Item item)
    {
        if (item == null || !activeItems.Contains(item))
        {
            DebugLog.Warning("[ItemEffectsManager] Cannot remove item: not active");
            return;
        }

        item.RemoveEffect(player);
        activeItems.Remove(item);

        // Refresh all weapon stats
        RefreshAllWeaponStats();

        DebugLog.Info($"[ItemEffectsManager] Item removed: {item.ItemName}. Total items: {activeItems.Count}");
    }

    /// <summary>
    /// Refresh all weapon stats.
    /// Call this when items are added/removed or weapons change.
    /// </summary>
    public void RefreshAllWeaponStats()
    {
        if (weaponInventory == null) return;

        List<Weapon> weapons = weaponInventory.GetWeapons();
        foreach (Weapon weapon in weapons)
        {
            weapon.RecalculateStats();
        }

        DebugLog.Verbose($"[ItemEffectsManager] Refreshed {weapons.Count} weapon stats");
    }

    /// <summary>
    /// Get all active items.
    /// </summary>
    public List<Item> GetActiveItems()
    {
        return new List<Item>(activeItems);
    }

    /// <summary>
    /// Get count of active items.
    /// </summary>
    public int GetItemCount()
    {
        return activeItems.Count;
    }

    /// <summary>
    /// Check if player has a specific item.
    /// </summary>
    public bool HasItem(string itemName)
    {
        return activeItems.Exists(i => i.ItemName == itemName);
    }

    /// <summary>
    /// Get all items of a specific rarity.
    /// </summary>
    public List<Item> GetItemsByRarity(ItemRarity rarity)
    {
        return activeItems.FindAll(i => i.Rarity == rarity);
    }

    /// <summary>
    /// Get all items of a specific effect type.
    /// </summary>
    public List<Item> GetItemsByEffectType(ItemEffectType effectType)
    {
        return activeItems.FindAll(i => i.EffectType == effectType);
    }

    /// <summary>
    /// Called when a weapon is added to inventory.
    /// Re-apply item effects to new weapon.
    /// </summary>
    public void OnWeaponAdded(Weapon weapon)
    {
        // Weapon stats will be recalculated automatically
        // Tag-based and slot-specific effects handled in RecalculateStats
        DebugLog.Verbose($"[ItemEffectsManager] Weapon added: {weapon.WeaponName}");
    }

    /// <summary>
    /// Called when a weapon is removed from inventory.
    /// </summary>
    public void OnWeaponRemoved(string weaponName)
    {
        DebugLog.Verbose($"[ItemEffectsManager] Weapon removed: {weaponName}");
    }

    // Debug: Log all active items
    public void LogActiveItems()
    {
        DebugLog.Info($"[ItemEffectsManager] Active Items ({activeItems.Count}):");
        foreach (Item item in activeItems)
        {
            DebugLog.Info($"  - {item.ItemName} ({item.Rarity}, {item.EffectType})");
        }
    }
}
