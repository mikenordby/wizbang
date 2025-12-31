using UnityEngine;

/// <summary>
/// Base class for all items in the game.
/// Items provide passive effects that modify player stats, weapon behavior, or game rules.
/// Unlike weapons, items don't have levels - they're acquired and immediately active.
/// </summary>
public abstract class Item : ScriptableObject
{
    [Header("Item Identity")]
    [SerializeField] protected string itemName = "Item";

    [SerializeField]
    [TextArea(2, 4)]
    protected string description = "Does something cool.";

    [SerializeField] protected ItemRarity rarity = ItemRarity.Common;

    [SerializeField] protected Sprite icon;

    [Header("Item Type")]
    [Tooltip("How this item affects weapons/player")]
    [SerializeField] protected ItemEffectType effectType = ItemEffectType.Global;

    // Public accessors
    public string ItemName => itemName;
    public string Description => description;
    public ItemRarity Rarity => rarity;
    public Sprite Icon => icon;
    public ItemEffectType EffectType => effectType;

    /// <summary>
    /// Apply this item's effect to the player.
    /// Called once when the item is acquired.
    /// </summary>
    public abstract void ApplyEffect(Player player);

    /// <summary>
    /// Remove this item's effect (for testing or special mechanics).
    /// Most items won't need to implement this.
    /// </summary>
    public virtual void RemoveEffect(Player player)
    {
        DebugLog.Warning($"[Item] {itemName}: RemoveEffect not implemented");
    }

    /// <summary>
    /// Get detailed effect description for UI tooltips.
    /// Override to provide dynamic descriptions based on current state.
    /// </summary>
    public virtual string GetDetailedDescription()
    {
        return description;
    }
}

/// <summary>
/// How the item affects weapons/player.
/// Used for UI organization and filtering.
/// </summary>
public enum ItemEffectType
{
    Global,         // Affects all weapons (e.g., +10% attack speed)
    TagBased,       // Affects weapons with specific tags (e.g., all [Fire] weapons burn)
    SlotSpecific,   // Affects a specific weapon slot (e.g., Slot 1 fires twice)
    Fundamental     // Changes core game rules (e.g., Glass Cannon)
}

/*
Rarity Tiers (from ItemRarity.cs):
- Common: Basic stat boosts (50% drop weight)
- Rare: Better stats or minor unique effects (25% drop weight)
- Exotic: Strong unique effects (15% drop weight)
- Legendary: Very powerful effects (8% drop weight)
- Supreme: Game-changing effects (2% drop weight)
*/
