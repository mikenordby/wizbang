using UnityEngine;

/// <summary>
/// Item rarity tiers with associated colors and drop chances.
/// Higher rarity = more powerful effects.
/// </summary>
public enum ItemRarity
{
    Common,     // White - basic stat boosts
    Rare,       // Blue - better stats or minor unique effects
    Exotic,     // Purple - strong unique effects
    Legendary,  // Orange - very powerful effects
    Supreme     // Red/Gold - game-changing effects
}

/// <summary>
/// Utility class for rarity-related operations.
/// </summary>
public static class ItemRarityUtils
{
    /// <summary>
    /// Get the display color for this rarity.
    /// </summary>
    public static Color GetColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => new Color(0.9f, 0.9f, 0.9f),       // White
            ItemRarity.Rare => new Color(0.3f, 0.5f, 1f),           // Blue
            ItemRarity.Exotic => new Color(0.7f, 0.3f, 0.9f),       // Purple
            ItemRarity.Legendary => new Color(1f, 0.6f, 0.1f),      // Orange
            ItemRarity.Supreme => new Color(1f, 0.2f, 0.2f),        // Red
            _ => Color.white
        };
    }
    
    /// <summary>
    /// Get the glow/outline color for dropped items.
    /// </summary>
    public static Color GetGlowColor(ItemRarity rarity)
    {
        Color baseColor = GetColor(rarity);
        return new Color(baseColor.r, baseColor.g, baseColor.b, 0.5f);
    }
    
    /// <summary>
    /// Get the relative drop weight (higher = more common).
    /// </summary>
    public static float GetDropWeight(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => 50f,
            ItemRarity.Rare => 25f,
            ItemRarity.Exotic => 15f,
            ItemRarity.Legendary => 8f,
            ItemRarity.Supreme => 2f,
            _ => 50f
        };
    }
    
    /// <summary>
    /// Get display name with color tag for rich text.
    /// </summary>
    public static string GetColoredName(ItemRarity rarity, string itemName)
    {
        Color color = GetColor(rarity);
        string hex = ColorUtility.ToHtmlStringRGB(color);
        return $"<color=#{hex}>{itemName}</color>";
    }
}

