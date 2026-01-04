using UnityEngine;

/// <summary>
/// Shared utility for damage type color mapping.
/// Eliminates duplicate GetColorForDamageType methods across weapon behaviors.
/// </summary>
public static class DamageTypeColors
{
    /// <summary>
    /// Get color for damage type (used for projectiles, beams, effects).
    /// Full opacity version for solid visuals.
    /// </summary>
    public static Color GetColor(DamageType type)
    {
        return type switch
        {
            DamageType.Fire => new Color(1f, 0.4f, 0.1f, 1f),
            DamageType.Ice => new Color(0.4f, 0.7f, 1f, 1f),
            DamageType.Lightning => new Color(0.6f, 0.6f, 1f, 1f),
            DamageType.Poison => new Color(0.3f, 0.9f, 0.2f, 1f),
            DamageType.Arcane => new Color(0.9f, 0.3f, 0.9f, 1f),
            _ => new Color(1f, 0.2f, 0.2f, 1f)
        };
    }

    /// <summary>
    /// Get color for damage type with custom alpha.
    /// Used for AoE effects, trails, and semi-transparent visuals.
    /// </summary>
    public static Color GetColor(DamageType type, float alpha)
    {
        Color c = GetColor(type);
        c.a = alpha;
        return c;
    }

    /// <summary>
    /// Get faded end color (50% alpha) for trails and gradients.
    /// </summary>
    public static Color GetFadedColor(DamageType type)
    {
        Color c = GetColor(type);
        c.a = 0.5f;
        return c;
    }
}
