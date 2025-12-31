using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Database for managing all WeaponDefinition assets.
/// Loads definitions from Resources/Weapons/ at runtime.
/// Provides lookup by ID, behavior type, and tags.
/// </summary>
public class WeaponDefinitionDatabase : MonoBehaviour
{
    [Header("Runtime Database")]
    [SerializeField] private List<WeaponDefinition> allDefinitions = new List<WeaponDefinition>();

    private Dictionary<string, WeaponDefinition> definitionsByID = new Dictionary<string, WeaponDefinition>();
    private Dictionary<string, WeaponDefinition> definitionsByLegacyType = new Dictionary<string, WeaponDefinition>();

    private static WeaponDefinitionDatabase instance;
    public static WeaponDefinitionDatabase Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        LoadAllDefinitions();
        GameServices.RegisterWeaponDefinitionDatabase(this);
    }

    /// <summary>
    /// Load all WeaponDefinition assets from Resources/Weapons/
    /// Falls back to DefaultWeaponDefinitions if no assets found.
    /// </summary>
    private void LoadAllDefinitions()
    {
        WeaponDefinition[] loaded = Resources.LoadAll<WeaponDefinition>("Weapons");

        allDefinitions.Clear();
        definitionsByID.Clear();
        definitionsByLegacyType.Clear();

        if (loaded.Length > 0)
        {
            // Load from Resources
            foreach (var def in loaded)
            {
                RegisterDefinition(def);
            }
            DebugLog.Info($"[WeaponDB] Loaded {allDefinitions.Count} weapon definitions from Resources");
        }
        else
        {
            // Fall back to default definitions
            DebugLog.Info("[WeaponDB] No weapon definitions in Resources/Weapons/, using defaults");
            var defaults = DefaultWeaponDefinitions.CreateAll();
            foreach (var def in defaults)
            {
                RegisterDefinition(def);
            }
            DebugLog.Info($"[WeaponDB] Created {allDefinitions.Count} default weapon definitions");
        }

        // Set up legacy type mappings for backward compatibility
        SetupLegacyMappings();
    }

    /// <summary>
    /// Register a single definition into the database.
    /// </summary>
    private void RegisterDefinition(WeaponDefinition def)
    {
        if (def == null) return;

        allDefinitions.Add(def);

        // Index by ID
        if (!string.IsNullOrEmpty(def.weaponId))
        {
            if (definitionsByID.ContainsKey(def.weaponId))
            {
                DebugLog.Warning($"[WeaponDB] Duplicate weapon ID: {def.weaponId}");
            }
            else
            {
                definitionsByID[def.weaponId] = def;
            }
        }
    }

    /// <summary>
    /// Map old class names to new weapon IDs for backward compatibility.
    /// </summary>
    private void SetupLegacyMappings()
    {
        // Map old weapon type names to weapon IDs
        var legacyMappings = new Dictionary<string, string>
        {
            // Tier 1 Basic Weapons
            { "ProjectileWeapon", "magic_missile" },
            { "MagicMissile", "magic_missile" },
            { "RapidFireWeapon", "rapid_fire" },
            { "OrbiterWeapon", "orbiting_blades" },
            { "LightningWeapon", "chain_lightning" },
            { "PoisonWeapon", "poison_cloud" },
            { "FlameBolt", "flame_bolt" },
            { "HeatAura", "heat_aura" },
            { "LightBeam", "light_beam" },
            // Tier 2 Combined Weapons
            { "CircleOfFire", "circle_of_fire" },
            { "FireRingWeapon", "circle_of_fire" },
            { "LaserBeam", "laser_beam" },
            { "LaserWeapon", "laser_beam" },
            { "PoisonedFlames", "poisoned_flames" },
            { "PoisonedFlamesWeapon", "poisoned_flames" },
            { "StormShield", "storm_shield" },
            { "ArcaneGatling", "arcane_gatling" },
            { "ToxicBarrage", "toxic_barrage" },
            { "FlameChain", "flame_chain" },
            { "FrozenStorm", "frozen_storm" },
            { "BladeBlaze", "blade_blaze" },
            { "ArcaneAura", "arcane_aura" }
        };

        foreach (var mapping in legacyMappings)
        {
            if (definitionsByID.TryGetValue(mapping.Value, out var def))
            {
                definitionsByLegacyType[mapping.Key] = def;
            }
        }
    }

    /// <summary>
    /// Get weapon definition by unique ID.
    /// </summary>
    public WeaponDefinition GetByID(string weaponId)
    {
        if (string.IsNullOrEmpty(weaponId)) return null;

        if (definitionsByID.TryGetValue(weaponId, out var def))
        {
            return def;
        }

        // Not a warning - legacy weapons don't have definitions, fallback to code-based is expected
        DebugLog.Verbose($"[WeaponDB] Weapon ID not found: {weaponId} (will try legacy fallback)");
        return null;
    }

    /// <summary>
    /// Get weapon definition by legacy type name (for backward compatibility).
    /// </summary>
    public WeaponDefinition GetByLegacyType(string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return null;

        if (definitionsByLegacyType.TryGetValue(typeName, out var def))
        {
            return def;
        }

        return null;
    }

    /// <summary>
    /// Get all weapon definitions.
    /// </summary>
    public List<WeaponDefinition> GetAll()
    {
        return new List<WeaponDefinition>(allDefinitions);
    }

    // Tier 2 combined weapon IDs (not available as starting weapons)
    private static readonly HashSet<string> Tier2WeaponIds = new HashSet<string>
    {
        "circle_of_fire",
        "laser_beam",
        "poisoned_flames",
        "storm_shield",
        "arcane_gatling",
        "toxic_barrage",
        "flame_chain",
        "frozen_storm",
        "blade_blaze",
        "arcane_aura"
    };

    /// <summary>
    /// Get all basic (Tier 1) weapon definitions.
    /// Excludes Tier 2 combined weapons.
    /// </summary>
    public List<WeaponDefinition> GetBasicWeapons()
    {
        return allDefinitions.Where(d => !Tier2WeaponIds.Contains(d.weaponId)).ToList();
    }

    /// <summary>
    /// Check if a weapon is a Tier 2 combined weapon.
    /// </summary>
    public bool IsTier2Weapon(string weaponId)
    {
        return Tier2WeaponIds.Contains(weaponId);
    }

    /// <summary>
    /// Get weapons by behavior type.
    /// </summary>
    public List<WeaponDefinition> GetByBehaviorType(WeaponBehaviorType type)
    {
        return allDefinitions.Where(d => d.behaviorType == type).ToList();
    }

    /// <summary>
    /// Get weapons that have a specific tag.
    /// </summary>
    public List<WeaponDefinition> GetByTag(WeaponTag tag)
    {
        return allDefinitions.Where(d => d.tags.Contains(tag)).ToList();
    }

    /// <summary>
    /// Check if a weapon definition exists.
    /// </summary>
    public bool HasDefinition(string weaponId)
    {
        return definitionsByID.ContainsKey(weaponId);
    }

    /// <summary>
    /// Register a runtime-created definition (for combinations).
    /// </summary>
    public void RegisterRuntimeDefinition(WeaponDefinition def)
    {
        if (def == null || string.IsNullOrEmpty(def.weaponId)) return;

        if (!allDefinitions.Contains(def))
        {
            allDefinitions.Add(def);
        }

        definitionsByID[def.weaponId] = def;
        DebugLog.Info($"[WeaponDB] Registered runtime definition: {def.weaponId}");
    }
}
