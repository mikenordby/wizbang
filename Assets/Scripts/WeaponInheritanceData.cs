using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Data class for passing parent weapon stats to combined weapons.
/// Used to calculate inherited stats (better of both × tier multiplier).
/// </summary>
[System.Serializable]
public class WeaponInheritanceData
{
    // Parent weapon stats
    public float parent1Damage;
    public float parent1FireRate;
    public int parent1ProjectileCount;
    public int parent1Pierce;
    public float parent1Range;
    public float parent1ProjectileSize;
    public List<WeaponTag> parent1Tags;

    public float parent2Damage;
    public float parent2FireRate;
    public int parent2ProjectileCount;
    public int parent2Pierce;
    public float parent2Range;
    public float parent2ProjectileSize;
    public List<WeaponTag> parent2Tags;

    // Tier multiplier (1.5 for Tier 1, 2.25 for Tier 2)
    public float tierMultiplier = 1.5f;

    /// <summary>
    /// Create inheritance data from two parent weapons.
    /// </summary>
    public static WeaponInheritanceData FromParents(Weapon weapon1, Weapon weapon2, float tierMultiplier = 1.5f)
    {
        return new WeaponInheritanceData
        {
            parent1Damage = weapon1.Damage,
            parent1FireRate = weapon1.FireRate,
            parent1ProjectileCount = weapon1.ProjectileCount,
            parent1Pierce = weapon1.Pierce,
            parent1Range = weapon1.Range,
            parent1ProjectileSize = weapon1.ProjectileSize,
            parent1Tags = new List<WeaponTag>(weapon1.GetTags()),

            parent2Damage = weapon2.Damage,
            parent2FireRate = weapon2.FireRate,
            parent2ProjectileCount = weapon2.ProjectileCount,
            parent2Pierce = weapon2.Pierce,
            parent2Range = weapon2.Range,
            parent2ProjectileSize = weapon2.ProjectileSize,
            parent2Tags = new List<WeaponTag>(weapon2.GetTags()),

            tierMultiplier = tierMultiplier
        };
    }

    /// <summary>
    /// Calculate inherited base damage (better of both × tier multiplier).
    /// </summary>
    public float GetInheritedDamage()
    {
        return Mathf.Max(parent1Damage, parent2Damage) * tierMultiplier;
    }

    /// <summary>
    /// Calculate inherited fire rate (better of both × tier multiplier).
    /// </summary>
    public float GetInheritedFireRate()
    {
        return Mathf.Max(parent1FireRate, parent2FireRate) * tierMultiplier;
    }

    /// <summary>
    /// Calculate inherited projectile count (better of both, no multiplier for counts).
    /// </summary>
    public int GetInheritedProjectileCount()
    {
        return Mathf.Max(parent1ProjectileCount, parent2ProjectileCount);
    }

    /// <summary>
    /// Calculate inherited pierce (better of both).
    /// </summary>
    public int GetInheritedPierce()
    {
        return Mathf.Max(parent1Pierce, parent2Pierce);
    }

    /// <summary>
    /// Calculate inherited range (better of both × tier multiplier).
    /// </summary>
    public float GetInheritedRange()
    {
        return Mathf.Max(parent1Range, parent2Range) * tierMultiplier;
    }

    /// <summary>
    /// Calculate inherited projectile size (better of both × tier multiplier).
    /// </summary>
    public float GetInheritedProjectileSize()
    {
        return Mathf.Max(parent1ProjectileSize, parent2ProjectileSize) * tierMultiplier;
    }

    /// <summary>
    /// Get combined tags (union of both parent tag sets).
    /// This is THE key to the synergy system - combined weapons get MORE synergy bonuses!
    /// </summary>
    public List<WeaponTag> GetInheritedTags()
    {
        // Union of both tag lists (no duplicates)
        return parent1Tags.Union(parent2Tags).ToList();
    }
}
