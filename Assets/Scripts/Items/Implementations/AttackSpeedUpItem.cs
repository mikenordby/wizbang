using UnityEngine;

/// <summary>
/// Attack Speed Up (Common Item)
/// Provides a flat +15% attack speed bonus to all weapons.
/// </summary>
[CreateAssetMenu(fileName = "AttackSpeedUp", menuName = "Wizbang/Items/Common/Attack Speed Up")]
public class AttackSpeedUpItem : GlobalStatItem
{
    private void OnEnable()
    {
        // Set item identity
        itemName = "Attack Speed Up";
        description = "Increases attack speed of all weapons by 15%.";
        rarity = ItemRarity.Common;
        effectType = ItemEffectType.Global;

        // Set stat bonuses
        attackSpeedBonus = 0.15f; // +15%
    }
}
