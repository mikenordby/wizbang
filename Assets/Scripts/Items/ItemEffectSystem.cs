using UnityEngine;

/// <summary>
/// Handles special item effects that go beyond simple stat modifications.
/// Implements unique behaviors for exotic, legendary, and supreme items.
/// </summary>
public static class ItemEffectSystem
{
    /// <summary>
    /// Apply a special effect to the player.
    /// </summary>
    /// <param name="player">The player to affect</param>
    /// <param name="effectId">The effect identifier</param>
    /// <param name="value">The effect value (interpretation depends on effect)</param>
    public static void ApplyEffect(Player player, string effectId, float value)
    {
        if (player == null || string.IsNullOrEmpty(effectId)) return;
        
        switch (effectId.ToLower())
        {
            case "lifesteal":
                // Lifesteal: Heal for a percentage of damage dealt
                ApplyLifesteal(player, value);
                break;
                
            case "thorns":
                // Thorns: Reflect damage back to attackers
                ApplyThorns(player, value);
                break;
                
            case "lucky_drops":
                // Lucky Drops: Increased XP and item drop chance
                ApplyLuckyDrops(player, value);
                break;
                
            case "berserker":
                // Berserker: Damage increases as health decreases
                ApplyBerserker(player, value);
                break;
                
            case "magnet_master":
                // Magnet Master: XP orbs are auto-collected screen-wide
                ApplyMagnetMaster(player, value);
                break;
                
            case "bullet_time":
                // Bullet Time: Slow motion when taking damage
                ApplyBulletTime(player, value);
                break;
                
            case "double_shot":
                // Double Shot: Chance to fire twice
                ApplyDoubleShot(player, value);
                break;
                
            case "phoenix":
                // Phoenix: Revive once per run with full health
                ApplyPhoenix(player, value);
                break;
                
            case "overkill":
                // Overkill: Excess damage chains to nearby enemies
                ApplyOverkill(player, value);
                break;
                
            case "god_slayer":
                // God Slayer: Massive damage boost against bosses
                ApplyGodSlayer(player, value);
                break;
                
            default:
                DebugLog.Warning($"[ItemEffectSystem] Unknown effect: {effectId}");
                break;
        }
    }
    
    // ===== Effect Implementations =====
    // These set flags/values that other systems check during gameplay
    
    private static void ApplyLifesteal(Player player, float value)
    {
        PlayerEffects.LifestealPercent += value;
        DebugLog.Info($"[ItemEffectSystem] Lifesteal: {PlayerEffects.LifestealPercent * 100:F0}% (+{value * 100:F0}%)");
    }
    
    private static void ApplyThorns(Player player, float value)
    {
        PlayerEffects.ThornsDamage += value;
        DebugLog.Info($"[ItemEffectSystem] Thorns: {PlayerEffects.ThornsDamage:F0} damage (+{value:F0})");
    }
    
    private static void ApplyLuckyDrops(Player player, float value)
    {
        PlayerEffects.LuckyDropsMultiplier += value;
        DebugLog.Info($"[ItemEffectSystem] Lucky Drops: {PlayerEffects.LuckyDropsMultiplier * 100:F0}% (+{value * 100:F0}%)");
    }
    
    private static void ApplyBerserker(Player player, float value)
    {
        PlayerEffects.BerserkerEnabled = true;
        PlayerEffects.BerserkerMaxBonus += value;
        DebugLog.Info($"[ItemEffectSystem] Berserker enabled: up to +{PlayerEffects.BerserkerMaxBonus * 100:F0}% damage at low HP");
    }
    
    private static void ApplyMagnetMaster(Player player, float value)
    {
        PlayerEffects.MagnetMasterRange = value;
        DebugLog.Info($"[ItemEffectSystem] Magnet Master: XP collection range = {value:F0} units");
    }
    
    private static void ApplyBulletTime(Player player, float value)
    {
        PlayerEffects.BulletTimeSlowFactor = value;
        DebugLog.Info($"[ItemEffectSystem] Bullet Time: {value:F0}x slowdown on hit");
    }
    
    private static void ApplyDoubleShot(Player player, float value)
    {
        PlayerEffects.DoubleShotChance += value;
        DebugLog.Info($"[ItemEffectSystem] Double Shot: {PlayerEffects.DoubleShotChance * 100:F0}% chance (+{value * 100:F0}%)");
    }
    
    private static void ApplyPhoenix(Player player, float value)
    {
        PlayerEffects.PhoenixRevivesRemaining += (int)value;
        DebugLog.Info($"[ItemEffectSystem] Phoenix: {PlayerEffects.PhoenixRevivesRemaining} revives available");
    }
    
    private static void ApplyOverkill(Player player, float value)
    {
        PlayerEffects.OverkillChainPercent = value;
        DebugLog.Info($"[ItemEffectSystem] Overkill: {value * 100:F0}% of excess damage chains");
    }
    
    private static void ApplyGodSlayer(Player player, float value)
    {
        PlayerEffects.GodSlayerBonusDamage = value;
        DebugLog.Info($"[ItemEffectSystem] God Slayer: +{value * 100:F0}% damage vs bosses");
    }
}

/// <summary>
/// Static class holding player effect states.
/// These are checked by combat systems during gameplay.
/// Reset when starting a new run.
/// </summary>
public static class PlayerEffects
{
    // Lifesteal
    public static float LifestealPercent = 0f;
    
    // Thorns
    public static float ThornsDamage = 0f;
    
    // Lucky Drops
    public static float LuckyDropsMultiplier = 1f;
    
    // Berserker
    public static bool BerserkerEnabled = false;
    public static float BerserkerMaxBonus = 0f;
    
    // Magnet Master
    public static float MagnetMasterRange = 0f;
    
    // Bullet Time
    public static float BulletTimeSlowFactor = 1f;
    
    // Double Shot
    public static float DoubleShotChance = 0f;
    
    // Phoenix
    public static int PhoenixRevivesRemaining = 0;
    
    // Overkill
    public static float OverkillChainPercent = 0f;
    
    // God Slayer
    public static float GodSlayerBonusDamage = 0f;
    
    /// <summary>
    /// Reset all effects for a new run.
    /// </summary>
    public static void ResetAll()
    {
        LifestealPercent = 0f;
        ThornsDamage = 0f;
        LuckyDropsMultiplier = 1f;
        BerserkerEnabled = false;
        BerserkerMaxBonus = 0f;
        MagnetMasterRange = 0f;
        BulletTimeSlowFactor = 1f;
        DoubleShotChance = 0f;
        PhoenixRevivesRemaining = 0;
        OverkillChainPercent = 0f;
        GodSlayerBonusDamage = 0f;
        
        DebugLog.Info("[PlayerEffects] All effects reset for new run");
    }
    
    /// <summary>
    /// Get berserker damage multiplier based on current HP ratio.
    /// </summary>
    public static float GetBerserkerMultiplier(float currentHP, float maxHP)
    {
        if (!BerserkerEnabled || maxHP <= 0) return 1f;
        
        float hpPercent = currentHP / maxHP;
        // At 100% HP = 1.0x damage
        // At 0% HP = (1 + BerserkerMaxBonus)x damage
        float bonus = (1f - hpPercent) * BerserkerMaxBonus;
        return 1f + bonus;
    }
}

