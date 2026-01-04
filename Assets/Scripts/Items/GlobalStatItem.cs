using UnityEngine;

/// <summary>
/// Base class for items that provide global stat bonuses.
/// Affects all weapons equally (e.g., +10% attack speed, +15% damage).
/// </summary>
public abstract class GlobalStatItem : Item
{
    [Header("Global Stat Bonuses")]
    [Tooltip("Damage multiplier bonus (e.g., 0.10 = +10% damage)")]
    [SerializeField] protected float damageBonus = 0f;

    [Tooltip("Attack speed multiplier bonus (e.g., 0.15 = +15% attack speed)")]
    [SerializeField] protected float attackSpeedBonus = 0f;

    [Tooltip("Flat bonus projectiles (e.g., 1 = +1 projectile to all weapons)")]
    [SerializeField] protected int bonusProjectiles = 0;

    [Tooltip("Critical strike chance bonus (e.g., 0.10 = +10% crit chance)")]
    [SerializeField] protected float critChanceBonus = 0f;

    [Tooltip("Critical damage multiplier bonus (e.g., 0.50 = +50% crit damage)")]
    [SerializeField] protected float critDamageBonus = 0f;

    public override void ApplyEffect(Player player)
    {
        if (player == null)
        {
            DebugLog.Warning($"[{itemName}] Cannot apply effect - player is null");
            return;
        }

        // Apply bonuses to player global multipliers
        if (damageBonus != 0f)
        {
            player.GlobalDamageMultiplier += damageBonus;
            DebugLog.Info($"[{itemName}] Applied +{damageBonus * 100f:F0}% damage. New multiplier: {player.GlobalDamageMultiplier:F2}x");
        }

        if (attackSpeedBonus != 0f)
        {
            player.GlobalAttackSpeedMultiplier += attackSpeedBonus;
            DebugLog.Info($"[{itemName}] Applied +{attackSpeedBonus * 100f:F0}% attack speed. New multiplier: {player.GlobalAttackSpeedMultiplier:F2}x");
        }

        if (bonusProjectiles != 0)
        {
            player.AddBonusProjectiles(bonusProjectiles);
            DebugLog.Info($"[{itemName}] Applied +{bonusProjectiles} projectiles. New total: {player.BonusProjectiles}");
        }

        if (critChanceBonus != 0f)
        {
            player.GlobalCritChance += critChanceBonus;
            DebugLog.Info($"[{itemName}] Applied +{critChanceBonus * 100f:F0}% crit chance. New chance: {player.GlobalCritChance * 100f:F1}%");
        }

        if (critDamageBonus != 0f)
        {
            player.GlobalCritDamage += critDamageBonus;
            DebugLog.Info($"[{itemName}] Applied +{critDamageBonus * 100f:F0}% crit damage. New multiplier: {player.GlobalCritDamage:F2}x");
        }
    }

    public override void RemoveEffect(Player player)
    {
        if (player == null) return;

        // Remove bonuses
        player.GlobalDamageMultiplier -= damageBonus;
        player.GlobalAttackSpeedMultiplier -= attackSpeedBonus;
        if (bonusProjectiles != 0)
        {
            player.RemoveBonusProjectiles(bonusProjectiles);
        }
        player.GlobalCritChance -= critChanceBonus;
        player.GlobalCritDamage -= critDamageBonus;

        DebugLog.Info($"[{itemName}] Removed all bonuses");
    }

    public override string GetDetailedDescription()
    {
        string details = description + "\n\n";

        if (damageBonus != 0f)
            details += $"• +{damageBonus * 100f:F0}% Damage\n";
        if (attackSpeedBonus != 0f)
            details += $"• +{attackSpeedBonus * 100f:F0}% Attack Speed\n";
        if (bonusProjectiles != 0)
            details += $"• +{bonusProjectiles} Projectiles\n";
        if (critChanceBonus != 0f)
            details += $"• +{critChanceBonus * 100f:F0}% Crit Chance\n";
        if (critDamageBonus != 0f)
            details += $"• +{critDamageBonus * 100f:F0}% Crit Damage\n";

        return details.TrimEnd();
    }
}
