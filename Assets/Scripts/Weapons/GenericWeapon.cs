using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Data-driven weapon implementation.
/// Reads stats and behavior from WeaponDefinition ScriptableObject.
/// Delegates attack logic to IWeaponBehavior strategies.
/// </summary>
public class GenericWeapon : Weapon
{
    [Header("Data-Driven")]
    [SerializeField] private WeaponDefinition definition;

    private IWeaponBehavior behavior;
    private WeaponContext context;

    /// <summary>
    /// The weapon definition this weapon uses.
    /// </summary>
    public WeaponDefinition Definition => definition;

    protected override void Awake()
    {
        if (definition != null)
        {
            ApplyDefinition(definition);
        }

        base.Awake();

        // Initialize behavior after base.Awake() sets up player reference
        if (behavior != null && context != null)
        {
            context.PlayerTransform = playerTransform;
            context.Player = player;
            context.WeaponMonoBehaviour = this;
            behavior.Initialize(context);
        }
    }

    /// <summary>
    /// Set or change the weapon definition at runtime.
    /// </summary>
    public void SetDefinition(WeaponDefinition def)
    {
        // Cleanup old behavior
        behavior?.Cleanup();

        definition = def;
        ApplyDefinition(def);

        // Initialize new behavior
        if (behavior != null && context != null)
        {
            context.PlayerTransform = playerTransform;
            context.Player = player;
            context.WeaponMonoBehaviour = this;
            behavior.Initialize(context);
        }

        RecalculateStats();

        DebugLog.Info($"[GenericWeapon] Definition set to: {def.displayName}");
    }

    private void ApplyDefinition(WeaponDefinition def)
    {
        if (def == null)
        {
            DebugLog.Error("[GenericWeapon] Cannot apply null definition");
            return;
        }

        // Apply identity
        weaponName = def.displayName;
        baseDamage = def.baseDamage;
        baseFireRate = def.baseFireRate;
        projectileCount = def.projectileCount;
        basePierce = def.basePierce;
        baseRange = def.baseRange;
        projectileSize = def.projectileSize;
        damageType = def.damageType;
        weaponTags = new List<WeaponTag>(def.tags);

        // Create behavior strategy
        behavior = WeaponBehaviorFactory.Create(def.behaviorType);

        // Create context for behavior
        context = new WeaponContext
        {
            Definition = def,
            PlayerTransform = playerTransform,
            Player = player,
            WeaponMonoBehaviour = this
        };
    }

    protected override void Fire()
    {
        if (behavior == null || context == null)
        {
            DebugLog.Warning("[GenericWeapon] Cannot fire - no behavior configured");
            return;
        }

        UpdateContext();
        behavior.Execute(context);
    }

    protected override void Update()
    {
        base.Update();

        // Let behavior do per-frame updates (for beams, orbiters, etc.)
        if (behavior != null && context != null)
        {
            UpdateContext();
            behavior.OnUpdate(context);
        }
    }

    private void UpdateContext()
    {
        if (context == null) return;

        context.CurrentDamage = currentDamage;
        context.CurrentFireRate = currentFireRate;
        context.CurrentProjectileCount = currentProjectileCount;
        context.CurrentRange = currentRange;
        context.CurrentProjectileSize = currentProjectileSize;
        context.CurrentPierce = currentPierce;
        context.WeaponLevel = level;
        context.PlayerTransform = playerTransform;
        context.Player = player;
    }

    public override void RecalculateStats()
    {
        base.RecalculateStats();

        // Notify behavior of stat changes
        if (behavior != null && context != null)
        {
            UpdateContext();
            behavior.OnStatsChanged(context);
        }
    }

    /// <summary>
    /// Initialize with a definition programmatically.
    /// Used by WeaponInventory when spawning data-driven weapons.
    /// </summary>
    public void Initialize(Transform playerTransform, Player player, WeaponDefinition def)
    {
        this.playerTransform = playerTransform;
        this.player = player;

        // Cleanup old behavior if any
        behavior?.Cleanup();

        definition = def;
        ApplyDefinition(def);

        // Initialize behavior
        if (behavior != null && context != null)
        {
            context.PlayerTransform = playerTransform;
            context.Player = player;
            context.WeaponMonoBehaviour = this;
            behavior.Initialize(context);
        }

        RecalculateStats();

        DebugLog.Info($"[GenericWeapon] Initialized with definition: {def.displayName}");
    }

    /// <summary>
    /// Initialize combined weapon with inheritance from parent weapons.
    /// </summary>
    public void InitializeWithInheritance(
        Transform playerTransform,
        Player player,
        WeaponDefinition def,
        WeaponInheritanceData inheritanceData)
    {
        this.playerTransform = playerTransform;
        this.player = player;

        // Cleanup old behavior if any
        behavior?.Cleanup();

        // Create modified definition with inherited stats
        if (inheritanceData != null)
        {
            definition = def.CreateRuntimeCopy();
            definition.baseDamage = inheritanceData.GetInheritedDamage();
            definition.baseFireRate = inheritanceData.GetInheritedFireRate();
            definition.projectileCount = inheritanceData.GetInheritedProjectileCount();
            definition.basePierce = inheritanceData.GetInheritedPierce();
            definition.baseRange = inheritanceData.GetInheritedRange();
            definition.projectileSize = inheritanceData.GetInheritedProjectileSize();
            definition.tags = inheritanceData.GetInheritedTags();
        }
        else
        {
            definition = def;
        }

        ApplyDefinition(definition);

        // Initialize behavior
        if (behavior != null && context != null)
        {
            context.PlayerTransform = playerTransform;
            context.Player = player;
            context.WeaponMonoBehaviour = this;
            behavior.Initialize(context);
        }

        RecalculateStats();

        DebugLog.Info($"[GenericWeapon] Initialized combined weapon: {definition.displayName} with {definition.tags.Count} tags");
    }

    private void OnDestroy()
    {
        behavior?.Cleanup();
    }

    private void OnDisable()
    {
        // Behaviors might need to hide visuals when disabled
        // but keep state for re-enable
    }
}
