using UnityEngine;

/// <summary>
/// ScriptableObject defining an item's properties and effects.
/// Create instances: Assets > Create > Wizbang > Item Definition
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Wizbang/Item Definition", order = 1)]
public class ItemDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique identifier for this item (e.g., 'speed_boots', 'vampiric_fang')")]
    public string itemId = "new_item";
    
    [Tooltip("Display name shown in UI")]
    public string displayName = "New Item";
    
    [TextArea(2, 4)]
    [Tooltip("Item description explaining its effects")]
    public string description = "An item with mysterious powers.";
    
    [Tooltip("Item rarity tier")]
    public ItemRarity rarity = ItemRarity.Common;
    
    [Header("Visual")]
    [Tooltip("Sprite type for loading from Resources (e.g., 'Items/speed_boots')")]
    public string spriteType = "Items/default";
    
    [Tooltip("Item icon color tint (usually white for no tint)")]
    public Color iconTint = Color.white;
    
    [Header("Stack Settings")]
    [Tooltip("Can this item stack with itself?")]
    public bool isStackable = true;
    
    [Tooltip("Maximum stack size (1 = unique item)")]
    public int maxStack = 99;
    
    [Header("Stat Modifiers")]
    [Tooltip("Flat bonus to max health")]
    public float bonusMaxHealth = 0f;
    
    [Tooltip("Flat bonus to health regen (HP/sec)")]
    public float bonusHealthRegen = 0f;
    
    [Tooltip("Percent bonus to damage (0.1 = +10%)")]
    public float bonusDamagePercent = 0f;
    
    [Tooltip("Percent bonus to attack speed (0.1 = +10%)")]
    public float bonusAttackSpeedPercent = 0f;
    
    [Tooltip("Percent bonus to move speed (0.1 = +10%)")]
    public float bonusMoveSpeedPercent = 0f;
    
    [Tooltip("Flat bonus to crit chance (0.05 = +5%)")]
    public float bonusCritChance = 0f;
    
    [Tooltip("Flat bonus to crit damage multiplier (0.5 = +50% crit damage)")]
    public float bonusCritDamage = 0f;
    
    [Tooltip("Percent bonus to pickup/magnet radius (0.1 = +10%)")]
    public float bonusPickupRadiusPercent = 0f;
    
    [Tooltip("Percent bonus to projectile size (0.1 = +10%)")]
    public float bonusProjectileSizePercent = 0f;
    
    [Tooltip("Flat bonus to pierce")]
    public int bonusPierce = 0;
    
    [Tooltip("Flat bonus to projectile count")]
    public int bonusProjectileCount = 0;
    
    [Header("Special Effects")]
    [Tooltip("Special effect ID (for unique item behaviors)")]
    public string specialEffectId = "";
    
    [Tooltip("Special effect value (interpretation depends on effect)")]
    public float specialEffectValue = 0f;
    
    /// <summary>
    /// Apply this item's stat bonuses to the player.
    /// </summary>
    public void ApplyToPlayer(Player player)
    {
        if (player == null) return;
        
        // Apply stat bonuses
        if (bonusMaxHealth != 0f) player.AddMaxHealth(bonusMaxHealth);
        if (bonusHealthRegen != 0f) player.AddHealthRegen(bonusHealthRegen);
        if (bonusDamagePercent != 0f) player.AddDamageMultiplier(bonusDamagePercent);
        if (bonusAttackSpeedPercent != 0f) player.AddAttackSpeedMultiplier(bonusAttackSpeedPercent);
        if (bonusMoveSpeedPercent != 0f) player.AddMoveSpeedMultiplier(bonusMoveSpeedPercent);
        if (bonusCritChance != 0f) player.AddCritChance(bonusCritChance);
        if (bonusCritDamage != 0f) player.AddCritDamage(bonusCritDamage);
        if (bonusPickupRadiusPercent != 0f) player.AddPickupRadius(bonusPickupRadiusPercent);
        
        // Apply special effects via the ItemEffectSystem
        if (!string.IsNullOrEmpty(specialEffectId))
        {
            ItemEffectSystem.ApplyEffect(player, specialEffectId, specialEffectValue);
        }
        
        DebugLog.Info($"[ItemDefinition] Applied '{displayName}' to player");
    }
    
    /// <summary>
    /// Get a formatted description with stat values.
    /// </summary>
    public string GetFormattedDescription()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(description);
        sb.AppendLine();
        
        if (bonusMaxHealth != 0f) sb.AppendLine($"+{bonusMaxHealth:F0} Max Health");
        if (bonusHealthRegen != 0f) sb.AppendLine($"+{bonusHealthRegen:F1} HP/sec");
        if (bonusDamagePercent != 0f) sb.AppendLine($"+{bonusDamagePercent * 100:F0}% Damage");
        if (bonusAttackSpeedPercent != 0f) sb.AppendLine($"+{bonusAttackSpeedPercent * 100:F0}% Attack Speed");
        if (bonusMoveSpeedPercent != 0f) sb.AppendLine($"+{bonusMoveSpeedPercent * 100:F0}% Move Speed");
        if (bonusCritChance != 0f) sb.AppendLine($"+{bonusCritChance * 100:F0}% Crit Chance");
        if (bonusCritDamage != 0f) sb.AppendLine($"+{bonusCritDamage * 100:F0}% Crit Damage");
        if (bonusPickupRadiusPercent != 0f) sb.AppendLine($"+{bonusPickupRadiusPercent * 100:F0}% Pickup Radius");
        if (bonusPierce != 0) sb.AppendLine($"+{bonusPierce} Pierce");
        if (bonusProjectileCount != 0) sb.AppendLine($"+{bonusProjectileCount} Projectiles");
        
        return sb.ToString().TrimEnd();
    }
}

