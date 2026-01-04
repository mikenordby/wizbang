using UnityEngine;

/// <summary>
/// Critical Strike (Rare Item)
/// Provides +10% crit chance and +50% crit damage.
/// </summary>
[CreateAssetMenu(fileName = "CriticalStrike", menuName = "Wizbang/Items/Rare/Critical Strike")]
public class CriticalStrikeItem : GlobalStatItem
{
    private void OnEnable()
    {
        // Set item identity
        itemName = "Critical Strike";
        description = "Gain +10% critical hit chance and +50% critical damage.";
        rarity = ItemRarity.Rare;
        effectType = ItemEffectType.Global;

        // Set stat bonuses
        critChanceBonus = 0.10f; // +10% crit chance
        critDamageBonus = 0.50f; // +50% crit damage (adds to base 2.0x = 2.5x total)
    }
}
