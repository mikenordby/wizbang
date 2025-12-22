using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject defining a weapon combination recipe.
/// Specifies which 2 weapons combine into a new weapon.
/// </summary>
[CreateAssetMenu(fileName = "NewWeaponCombination", menuName = "Wizbang/Weapon Combination")]
public class WeaponCombination : ScriptableObject
{
    [Header("Combination Recipe")]
    [Tooltip("First weapon type required (e.g., 'ProjectileWeapon')")]
    public string weapon1Type;

    [Tooltip("Second weapon type required (e.g., 'FireRingWeapon')")]
    public string weapon2Type;

    [Header("Result")]
    [Tooltip("Resulting combined weapon type (e.g., 'CombinedWeapon')")]
    public string resultWeaponType;

    [Tooltip("Display name for the combination (e.g., 'Super Weapon')")]
    public string combinationName;

    [Tooltip("Description of the combined weapon")]
    [TextArea(3, 5)]
    public string description;

    [Header("Requirements")]
    [Tooltip("Minimum level required for weapon1 (0 = any level)")]
    public int weapon1MinLevel = 0;

    [Tooltip("Minimum level required for weapon2 (0 = any level)")]
    public int weapon2MinLevel = 0;

    [Header("Tags")]
    [Tooltip("Tags inherited from parents plus any new tags")]
    public List<WeaponTag> resultTags = new List<WeaponTag>();

    /// <summary>
    /// Check if two weapon types match this combination recipe.
    /// Order doesn't matter (A+B = B+A).
    /// </summary>
    public bool MatchesWeapons(string typeA, string typeB)
    {
        return (typeA == weapon1Type && typeB == weapon2Type) ||
               (typeA == weapon2Type && typeB == weapon1Type);
    }

    /// <summary>
    /// Check if weapon levels meet minimum requirements.
    /// </summary>
    public bool MeetsLevelRequirements(string typeA, int levelA, string typeB, int levelB)
    {
        // Check both orderings
        if (typeA == weapon1Type && typeB == weapon2Type)
        {
            return levelA >= weapon1MinLevel && levelB >= weapon2MinLevel;
        }
        else if (typeA == weapon2Type && typeB == weapon1Type)
        {
            return levelA >= weapon2MinLevel && levelB >= weapon1MinLevel;
        }

        return false;
    }
}
