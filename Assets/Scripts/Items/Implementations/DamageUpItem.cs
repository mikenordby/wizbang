using UnityEngine;

/// <summary>
/// Damage Up (Common Item)
/// Provides a flat +10% damage bonus to all weapons.
/// </summary>
[CreateAssetMenu(fileName = "DamageUp", menuName = "Wizbang/Items/Common/Damage Up")]
public class DamageUpItem : GlobalStatItem
{
    private void OnEnable()
    {
        // Set item identity
        itemName = "Damage Up";
        description = "Increases damage of all weapons by 10%.";
        rarity = ItemRarity.Common;
        effectType = ItemEffectType.Global;

        // Set stat bonuses
        damageBonus = 0.10f; // +10%
    }
}
