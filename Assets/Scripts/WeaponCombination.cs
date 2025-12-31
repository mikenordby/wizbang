using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject defining a weapon combination recipe.
/// Specifies which 2 weapons combine into a new weapon.
/// Supports both legacy class-based weapons and new data-driven weapons.
/// </summary>
[CreateAssetMenu(fileName = "NewWeaponCombination", menuName = "Wizbang/Weapon Combination")]
public class WeaponCombination : ScriptableObject
{
    [Header("Combination Recipe")]
    [Tooltip("First weapon type/ID required")]
    public string weapon1Type;

    [Tooltip("Second weapon type/ID required")]
    public string weapon2Type;

    [Header("Result")]
    [Tooltip("Resulting combined weapon type/ID")]
    public string resultWeaponType;

    [Tooltip("Display name for the combination")]
    public string combinationName;

    [Tooltip("Description of the combined weapon")]
    [TextArea(3, 5)]
    public string description;

    [Header("Combination Tier")]
    [Tooltip("Tier multiplier for stat inheritance (1.5 for Tier 1, 2.25 for Tier 2)")]
    public float tierMultiplier = 1.5f;

    [Header("Data-Driven Mode")]
    [Tooltip("Result weapon definition (optional - if set, uses this instead of resultWeaponType)")]
    public WeaponDefinition resultDefinition;

    // NOTE: Tags are NOT manually configured!
    // Combined weapons automatically inherit ALL tags from both parents (union).
    // Example: FireRing [Fire, Area] + Poison [Poison, Area] = Poisoned Flames [Fire, Area, Poison]

    /// <summary>
    /// Check if two weapon types match this combination recipe.
    /// Supports both legacy type names and new weapon IDs.
    /// Order doesn't matter (A+B = B+A).
    /// </summary>
    public bool MatchesWeapons(string typeA, string typeB)
    {
        return (MatchesType(typeA, weapon1Type) && MatchesType(typeB, weapon2Type)) ||
               (MatchesType(typeA, weapon2Type) && MatchesType(typeB, weapon1Type));
    }

    /// <summary>
    /// Check if two weapons match this combination recipe (data-driven).
    /// </summary>
    public bool MatchesWeapons(Weapon weaponA, Weapon weaponB)
    {
        string typeA = GetWeaponIdentifier(weaponA);
        string typeB = GetWeaponIdentifier(weaponB);
        return MatchesWeapons(typeA, typeB);
    }

    /// <summary>
    /// Get weapon identifier (ID for GenericWeapon, class name for legacy).
    /// </summary>
    private string GetWeaponIdentifier(Weapon weapon)
    {
        if (weapon is GenericWeapon gw && gw.Definition != null)
        {
            return gw.Definition.weaponId;
        }
        return weapon.GetType().Name;
    }

    /// <summary>
    /// Check if a weapon type/ID matches an expected value.
    /// Handles both legacy class names and new weapon IDs.
    /// </summary>
    private bool MatchesType(string actual, string expected)
    {
        if (actual == expected) return true;

        // Handle legacy type -> ID mapping
        var db = GameServices.WeaponDefinitionDatabase;
        if (db != null)
        {
            var def = db.GetByLegacyType(actual);
            if (def != null && def.weaponId == expected) return true;

            def = db.GetByLegacyType(expected);
            if (def != null && def.weaponId == actual) return true;
        }

        return false;
    }

    /// <summary>
    /// Get the result weapon definition for this combination.
    /// Creates dynamically if no explicit definition is set.
    /// </summary>
    public WeaponDefinition GetResultDefinition()
    {
        if (resultDefinition != null)
            return resultDefinition;

        var db = GameServices.WeaponDefinitionDatabase;
        if (db != null)
        {
            return db.GetByID(resultWeaponType) ?? db.GetByLegacyType(resultWeaponType);
        }

        return null;
    }
}
