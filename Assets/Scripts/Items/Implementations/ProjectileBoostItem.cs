using UnityEngine;

/// <summary>
/// Projectile Boost (Common Item)
/// Adds +1 projectile to all weapons.
/// </summary>
[CreateAssetMenu(fileName = "ProjectileBoost", menuName = "Wizbang/Items/Common/Projectile Boost")]
public class ProjectileBoostItem : GlobalStatItem
{
    private void OnEnable()
    {
        // Set item identity
        itemName = "Projectile Boost";
        description = "Adds +1 projectile to all weapons.";
        rarity = ItemRarity.Common;
        effectType = ItemEffectType.Global;

        // Set stat bonuses
        bonusProjectiles = 1;
    }
}
