using UnityEngine;

/// <summary>
/// Centralized damage calculation system
/// Calculates final damage only on collision (zero per-frame overhead)
/// Handles player stats, crits, damage types, and future status effects
/// </summary>
public class DamageCalculator : MonoBehaviour
{
    public static DamageCalculator Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    /// <summary>
    /// Calculate final damage when projectile hits enemy
    /// Only called on collision - zero overhead otherwise
    /// </summary>
    public DamageResult CalculateDamage(DamageContext context)
    {
        float damage = context.baseDamage;
        bool isCrit = false;
        
        DebugLog.Info($"[DamageCalculator] Input: baseDamage={context.baseDamage:F1}, playerMult={context.player?.DamageMultiplier:F2}, critChance={context.player?.CritChance:F2}", "Damage");
        
        // 1. Apply player damage multiplier
        if (context.player != null)
        {
            damage *= context.player.DamageMultiplier;
        }
        
        // 2. Critical hit check
        if (context.player != null && Random.value < context.player.CritChance)
        {
            damage *= context.player.CritDamage;
            isCrit = true;
            DebugLog.Verbose($"[DamageCalculator] CRITICAL HIT! Damage: {context.baseDamage:F1} → {damage:F1}");
        }
        
        DebugLog.Info($"[DamageCalculator] Output: finalDamage={damage:F1}, isCrit={isCrit}", "Damage");
        
        // Future: Apply weapon-specific multipliers
        // if (context.damageType == DamageType.Fire && context.enemy.IsBurning)
        //     damage *= 1.5f;
        
        // Future: Apply enemy status modifiers
        // if (context.enemy.HasStatus(StatusEffect.Vulnerable))
        //     damage *= 1.25f;
        
        return new DamageResult
        {
            finalDamage = damage,
            isCritical = isCrit,
            damageType = context.damageType
        };
    }
}

/// <summary>
/// Input context for damage calculation
/// </summary>
public struct DamageContext
{
    public float baseDamage;        // From projectile
    public Player player;           // Source of damage
    public Enemy enemy;             // Target of damage (future: for status effects)
    public DamageType damageType;   // Physical, Fire, Poison, etc.
}

/// <summary>
/// Result of damage calculation
/// </summary>
public struct DamageResult
{
    public float finalDamage;
    public bool isCritical;
    public DamageType damageType;
}

/// <summary>
/// Types of damage for elemental effects and resistances
/// </summary>
public enum DamageType
{
    Physical,   // Default for most weapons
    Fire,       // Future: Applies burn, bonus vs burning enemies
    Poison,     // Future: Applies poison DoT
    Ice,        // Future: Applies slow
    Lightning,  // Future: Chain damage to nearby enemies
    Arcane      // Magical damage (Magic Missile, etc.)
}
