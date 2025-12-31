using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject defining a weapon's identity, stats, and behavior.
/// Data-driven design: add new weapons by creating assets, not code.
/// </summary>
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Wizbang/Weapon Definition")]
public class WeaponDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique identifier (e.g., 'magic_missile')")]
    public string weaponId;

    [Tooltip("Display name shown in UI")]
    public string displayName = "New Weapon";

    [Tooltip("Description for tooltips")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("Icon for UI")]
    public Sprite icon;

    [Header("Base Stats")]
    [Tooltip("Base damage per hit/tick")]
    public float baseDamage = 10f;

    [Tooltip("Attacks per second")]
    public float baseFireRate = 1f;

    [Tooltip("Number of projectiles/instances per attack")]
    public int projectileCount = 1;

    [Tooltip("Number of enemies projectile passes through")]
    public int basePierce = 0;

    [Tooltip("Range multiplier (affects projectile lifetime/reach)")]
    public float baseRange = 1f;

    [Tooltip("Size multiplier for projectiles")]
    public float projectileSize = 1f;

    [Tooltip("Type of damage dealt")]
    public DamageType damageType = DamageType.Physical;

    [Header("Tags")]
    [Tooltip("Tags for synergy system (Fire, Ice, Lightning, etc.)")]
    public List<WeaponTag> tags = new List<WeaponTag>();

    [Header("Behavior")]
    [Tooltip("Type of attack behavior")]
    public WeaponBehaviorType behaviorType = WeaponBehaviorType.Projectile;

    [Header("Projectile Settings")]
    [Tooltip("Settings for Projectile behavior type")]
    public ProjectileSettings projectileSettings = new ProjectileSettings();

    [Header("AoE Settings")]
    [Tooltip("Settings for AoE behavior type")]
    public AoESettings aoeSettings = new AoESettings();

    [Header("Chain Settings")]
    [Tooltip("Settings for Chain behavior type")]
    public ChainSettings chainSettings = new ChainSettings();

    [Header("Beam Settings")]
    [Tooltip("Settings for Beam behavior type")]
    public BeamSettings beamSettings = new BeamSettings();

    [Header("Orbiter Settings")]
    [Tooltip("Settings for Orbiter behavior type")]
    public OrbiterSettings orbiterSettings = new OrbiterSettings();

    [Header("Visuals")]
    [Tooltip("Visual settings for projectiles")]
    public ProjectileVisuals visuals = new ProjectileVisuals();

    /// <summary>
    /// Get the behavior-specific settings based on behaviorType.
    /// </summary>
    public T GetSettings<T>() where T : class
    {
        return behaviorType switch
        {
            WeaponBehaviorType.Projectile => projectileSettings as T,
            WeaponBehaviorType.AoE => aoeSettings as T,
            WeaponBehaviorType.Chain => chainSettings as T,
            WeaponBehaviorType.Beam => beamSettings as T,
            WeaponBehaviorType.Orbiter => orbiterSettings as T,
            _ => null
        };
    }

    /// <summary>
    /// Validate the weapon definition on load.
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(weaponId))
        {
            weaponId = name.ToLowerInvariant().Replace(" ", "_");
        }
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = name;
        }
    }

    /// <summary>
    /// Create a runtime copy of this definition (for combinations).
    /// </summary>
    public WeaponDefinition CreateRuntimeCopy()
    {
        WeaponDefinition copy = CreateInstance<WeaponDefinition>();
        copy.weaponId = weaponId + "_copy";
        copy.displayName = displayName;
        copy.description = description;
        copy.icon = icon;
        copy.baseDamage = baseDamage;
        copy.baseFireRate = baseFireRate;
        copy.projectileCount = projectileCount;
        copy.basePierce = basePierce;
        copy.baseRange = baseRange;
        copy.projectileSize = projectileSize;
        copy.damageType = damageType;
        copy.tags = new List<WeaponTag>(tags);
        copy.behaviorType = behaviorType;
        copy.projectileSettings = projectileSettings;
        copy.aoeSettings = aoeSettings;
        copy.chainSettings = chainSettings;
        copy.beamSettings = beamSettings;
        copy.orbiterSettings = orbiterSettings;
        copy.visuals = visuals;
        return copy;
    }

    /// <summary>
    /// Create a combined weapon definition from two parent definitions.
    /// </summary>
    public static WeaponDefinition CreateCombined(
        WeaponDefinition parent1,
        WeaponDefinition parent2,
        string combinedName,
        float tierMultiplier = 1.5f)
    {
        WeaponDefinition combined = CreateInstance<WeaponDefinition>();

        combined.weaponId = combinedName.ToLowerInvariant().Replace(" ", "_");
        combined.displayName = combinedName;
        combined.description = $"Combined from {parent1.displayName} and {parent2.displayName}";

        // Stats: better of both × tier multiplier
        combined.baseDamage = Mathf.Max(parent1.baseDamage, parent2.baseDamage) * tierMultiplier;
        combined.baseFireRate = Mathf.Max(parent1.baseFireRate, parent2.baseFireRate) * tierMultiplier;
        combined.projectileCount = Mathf.Max(parent1.projectileCount, parent2.projectileCount);
        combined.basePierce = Mathf.Max(parent1.basePierce, parent2.basePierce);
        combined.baseRange = Mathf.Max(parent1.baseRange, parent2.baseRange);
        combined.projectileSize = Mathf.Max(parent1.projectileSize, parent2.projectileSize);

        // Union of tags (key feature!)
        combined.tags = new List<WeaponTag>(parent1.tags);
        foreach (var tag in parent2.tags)
        {
            if (!combined.tags.Contains(tag))
                combined.tags.Add(tag);
        }

        // Behavior: use parent1's behavior by default (can be overridden)
        combined.behaviorType = parent1.behaviorType;
        combined.projectileSettings = parent1.projectileSettings;
        combined.aoeSettings = parent1.aoeSettings;
        combined.chainSettings = parent1.chainSettings;
        combined.beamSettings = parent1.beamSettings;
        combined.orbiterSettings = parent1.orbiterSettings;
        combined.visuals = parent1.visuals;

        return combined;
    }
}
