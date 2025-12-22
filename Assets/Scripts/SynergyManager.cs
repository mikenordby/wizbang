using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages weapon tag synergies.
/// When multiple weapons share the same tag, they receive damage bonuses.
/// Example: 3 [Fire] weapons = +100% fire damage to all [Fire] weapons
/// </summary>
public class SynergyManager : MonoBehaviour
{
    private WeaponInventory weaponInventory;

    // Cache of active synergies
    private Dictionary<WeaponTag, int> tagCounts = new Dictionary<WeaponTag, int>();
    private Dictionary<WeaponTag, float> synergyBonuses = new Dictionary<WeaponTag, float>();

    // Synergy rules (updated 2025-12-22)
    private const float SYNERGY_BONUS_PER_TAG = 0.25f; // 25% per matching tag
    // 2 weapons with same tag = +25% damage
    // 3 weapons = +50% damage
    // 4 weapons = +75% damage
    // 5 weapons = +100% damage

    void Awake()
    {
        // Register with GameServices
        GameServices.RegisterSynergyManager(this);
    }

    void Start()
    {
        // Get weapon inventory from player
        weaponInventory = GameServices.Player?.GetComponent<WeaponInventory>();

        if (weaponInventory == null)
        {
            DebugLog.Error("[SynergyManager] WeaponInventory not found! Synergies will NOT work!");
        }
    }

    /// <summary>
    /// Recalculate all synergies based on current weapons
    /// Called when weapons are added/removed/upgraded
    /// </summary>
    public void RecalculateSynergies()
    {
        if (weaponInventory == null) return;

        // Clear previous
        tagCounts.Clear();
        synergyBonuses.Clear();

        // Count tags from all active weapons
        List<Weapon> weapons = weaponInventory.GetWeapons();
        foreach (Weapon weapon in weapons)
        {
            foreach (WeaponTag tag in weapon.GetTags())
            {
                if (!tagCounts.ContainsKey(tag))
                    tagCounts[tag] = 0;
                tagCounts[tag]++;
            }
        }

        // Calculate bonuses (only if 2+ weapons share tag)
        foreach (var kvp in tagCounts)
        {
            if (kvp.Value >= 2)
            {
                // Bonus = 25% * (count - 1)
                // 2 weapons = +25%, 3 weapons = +50%, 4 weapons = +75%, 5 weapons = +100%
                synergyBonuses[kvp.Key] = SYNERGY_BONUS_PER_TAG * (kvp.Value - 1);

                DebugLog.Info($"[SynergyManager] [{kvp.Key}] x{kvp.Value} = +{(synergyBonuses[kvp.Key] * 100f):F0}% damage");
            }
        }

        // Notify all weapons to recalculate stats
        foreach (Weapon weapon in weapons)
        {
            weapon.RecalculateStats();
        }

        DebugLog.Info($"[SynergyManager] Recalculated synergies: {synergyBonuses.Count} active synergies");
    }

    /// <summary>
    /// Get the synergy multiplier for a specific tag
    /// Returns 0 if no synergy active (caller should add to 1.0 for final multiplier)
    /// </summary>
    public float GetSynergyMultiplier(WeaponTag tag)
    {
        return synergyBonuses.ContainsKey(tag) ? synergyBonuses[tag] : 0f;
    }

    /// <summary>
    /// Get all active synergies (for UI display)
    /// </summary>
    public Dictionary<WeaponTag, float> GetActiveSynergies()
    {
        return new Dictionary<WeaponTag, float>(synergyBonuses);
    }

    /// <summary>
    /// Get tag counts (for debugging)
    /// </summary>
    public Dictionary<WeaponTag, int> GetTagCounts()
    {
        return new Dictionary<WeaponTag, int>(tagCounts);
    }
}
