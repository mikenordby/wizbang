using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Stat type enum for hero stat modifiers
/// </summary>
public enum StatType
{
    MaxHealth,
    MoveSpeed,
    Damage,
    AttackSpeed,
    CritChance,
    CritDamage,
    XPMagnetRange,
    PickupRadius,
    HealthRegen,
    DamageReduction,
    Luck,
    Pierce,
    ProjectileCount,
    Range,
    ProjectileSize,
    AOE,
    Lifesteal
}

/// <summary>
/// Composable stat modifier system for heroes.
/// Accumulates bonuses from upgrades, items, buffs, and debuffs.
/// Provides computed final values based on base stats + modifiers.
/// </summary>
public class HeroStats
{
    // Flat bonuses (e.g., +10 health, +0.5 move speed)
    private Dictionary<StatType, float> flatBonuses = new Dictionary<StatType, float>();
    
    // Percent bonuses (e.g., +20% damage = 0.2, stacks additively)
    private Dictionary<StatType, float> percentBonuses = new Dictionary<StatType, float>();
    
    /// <summary>
    /// Add a flat bonus to a stat (e.g., +10 health)
    /// </summary>
    public void AddFlatBonus(StatType stat, float amount)
    {
        if (!flatBonuses.ContainsKey(stat))
            flatBonuses[stat] = 0f;
        
        flatBonuses[stat] += amount;
        DebugLog.Info($"[HeroStats] Added flat bonus: {stat} +{amount} (total: {flatBonuses[stat]:F2})");
    }
    
    /// <summary>
    /// Add a percent bonus to a stat (e.g., +0.2 for +20% damage)
    /// Percent bonuses stack additively: +20% + +15% = +35%
    /// </summary>
    public void AddPercentBonus(StatType stat, float percent)
    {
        if (!percentBonuses.ContainsKey(stat))
            percentBonuses[stat] = 0f;
        
        percentBonuses[stat] += percent;
        DebugLog.Info($"[HeroStats] Added percent bonus: {stat} +{percent * 100:F0}% (total: {percentBonuses[stat] * 100:F0}%)");
    }
    
    /// <summary>
    /// Remove a flat bonus (e.g., when buff expires)
    /// </summary>
    public void RemoveFlatBonus(StatType stat, float amount)
    {
        if (flatBonuses.ContainsKey(stat))
        {
            flatBonuses[stat] -= amount;
            if (Mathf.Approximately(flatBonuses[stat], 0f))
                flatBonuses.Remove(stat);
        }
    }
    
    /// <summary>
    /// Remove a percent bonus (e.g., when buff expires)
    /// </summary>
    public void RemovePercentBonus(StatType stat, float percent)
    {
        if (percentBonuses.ContainsKey(stat))
        {
            percentBonuses[stat] -= percent;
            if (Mathf.Approximately(percentBonuses[stat], 0f))
                percentBonuses.Remove(stat);
        }
    }
    
    /// <summary>
    /// Get flat bonus for a stat
    /// </summary>
    public float GetFlatBonus(StatType stat)
    {
        return flatBonuses.GetValueOrDefault(stat, 0f);
    }
    
    /// <summary>
    /// Get percent bonus for a stat
    /// </summary>
    public float GetPercentBonus(StatType stat)
    {
        return percentBonuses.GetValueOrDefault(stat, 0f);
    }
    
    /// <summary>
    /// Calculate final stat value with all modifiers applied.
    /// Formula: (baseValue + flatBonus) * (1 + percentBonus)
    /// </summary>
    public float GetFinalValue(StatType stat, float baseValue)
    {
        float flat = flatBonuses.GetValueOrDefault(stat, 0f);
        float percent = percentBonuses.GetValueOrDefault(stat, 0f);
        
        float final = (baseValue + flat) * (1f + percent);
        
        // Special clamping for certain stats
        if (stat == StatType.CritChance || stat == StatType.Lifesteal)
            final = Mathf.Clamp01(final); // Cap crit chance and lifesteal at 100%
        else if (stat == StatType.DamageReduction)
            final = Mathf.Clamp(final, 0f, 0.9f); // Cap damage reduction at 90%
        else if (stat == StatType.MoveSpeed || stat == StatType.AttackSpeed)
            final = Mathf.Max(0.1f, final); // Prevent move/attack speed from going too low
        
        return final;
    }
    
    /// <summary>
    /// Clear all bonuses (e.g., on hero death/reset)
    /// </summary>
    public void ClearAll()
    {
        flatBonuses.Clear();
        percentBonuses.Clear();
        DebugLog.Info("[HeroStats] Cleared all stat bonuses");
    }
    
    /// <summary>
    /// Get a formatted string of all active bonuses for debugging
    /// </summary>
    public string GetDebugString()
    {
        var lines = new System.Text.StringBuilder();
        lines.AppendLine("=== Hero Stats ===");
        
        // Flat bonuses
        if (flatBonuses.Count > 0)
        {
            lines.AppendLine("Flat Bonuses:");
            foreach (var kvp in flatBonuses)
            {
                if (kvp.Value != 0f)
                    lines.AppendLine($"  {kvp.Key}: +{kvp.Value:F2}");
            }
        }
        
        // Percent bonuses
        if (percentBonuses.Count > 0)
        {
            lines.AppendLine("Percent Bonuses:");
            foreach (var kvp in percentBonuses)
            {
                if (kvp.Value != 0f)
                    lines.AppendLine($"  {kvp.Key}: +{kvp.Value * 100:F0}%");
            }
        }
        
        if (flatBonuses.Count == 0 && percentBonuses.Count == 0)
            lines.AppendLine("  (No bonuses)");
        
        return lines.ToString();
    }
}

