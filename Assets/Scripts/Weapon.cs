using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Abstract base class for all weapons.
/// Handles fire rate, damage, leveling, and common weapon logic.
/// Power scaling comes from weapon levels, items, combinations, and player multipliers.
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    /// <summary>
    /// Global fire rate multiplier applied to all weapons (0.8 = 20% slower for slower-paced gameplay)
    /// </summary>
    public static float GlobalFireRateMultiplier = 0.8f;

    /// <summary>
    /// Maximum weapon level (1-3)
    /// </summary>
    public const int MaxLevel = 3;

    /// <summary>
    /// Event fired when a weapon levels up. Args: (Weapon weapon, int newLevel)
    /// </summary>
    public static event System.Action<Weapon, int> OnWeaponLevelUp;

    [Header("Weapon Level")]
    [SerializeField] protected int level = 1;

    [Header("Weapon Stats")]
    [SerializeField] protected string weaponName = "Weapon";
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected float baseFireRate = 1f; // Shots per second
    [SerializeField] protected int projectileCount = 1;
    [SerializeField] protected int basePierce = 0;
    [SerializeField] protected float baseRange = 1f; // Multiplier for projectile lifetime
    
    [Header("Projectile Properties")]
    [Tooltip("Visual and collision size multiplier (1.0 = normal)")]
    [SerializeField] protected float projectileSize = 1f;

    [Tooltip("Type of damage (affects future elemental interactions)")]
    [SerializeField] protected DamageType damageType = DamageType.Physical;

    [Header("Weapon Tags")]
    [Tooltip("Tags for synergy bonuses (matching tags across weapons = damage multipliers)")]
    [SerializeField] protected List<WeaponTag> weaponTags = new List<WeaponTag>();

    // Calculated stats
    protected int currentProjectileCount;
    protected float currentDamage;
    protected float currentFireRate;
    protected int currentPierce;
    protected float currentRange;
    protected float currentProjectileSize;
    
    [Header("References")]
    [SerializeField] protected Transform playerTransform;
    
    protected float timeSinceLastFire;
    
    // Player stat multipliers
    protected Player player;
    
    protected virtual void Awake()
    {
        player = GetComponentInParent<Player>();
        if (playerTransform == null)
            playerTransform = transform.parent;

        RecalculateStats();
    }
    
    protected virtual void Start()
    {
        // Override in subclasses for initialization after Awake
    }
    
    /// <summary>
    /// Initialize weapon with player reference (called by WeaponInventory)
    /// </summary>
    public virtual void Initialize(Transform playerTransform, Player player)
    {
        this.playerTransform = playerTransform;
        this.player = player;
        RecalculateStats();
    }

    /// <summary>
    /// Initialize combined weapon with inheritance data from parent weapons.
    /// Combined weapons should override this to apply inherited stats.
    /// </summary>
    public virtual void InitializeWithInheritance(Transform playerTransform, Player player, WeaponInheritanceData inheritanceData)
    {
        this.playerTransform = playerTransform;
        this.player = player;

        // Apply inherited stats (combined weapons can override to customize behavior)
        ApplyInheritedStats(inheritanceData);

        RecalculateStats();

        DebugLog.Info($"[Weapon] {weaponName} initialized with inheritance: damage={baseDamage:F1}, fireRate={baseFireRate:F1}, tags={string.Join(",", weaponTags)}");
    }

    /// <summary>
    /// Apply inherited stats from parent weapons.
    /// Combined weapons can override this for custom stat inheritance behavior.
    /// </summary>
    protected virtual void ApplyInheritedStats(WeaponInheritanceData inheritanceData)
    {
        if (inheritanceData == null)
        {
            DebugLog.Warning($"[Weapon] {weaponName}: No inheritance data provided");
            return;
        }

        // Inherit stats (better of both × tier multiplier)
        baseDamage = inheritanceData.GetInheritedDamage();
        baseFireRate = inheritanceData.GetInheritedFireRate();
        projectileCount = inheritanceData.GetInheritedProjectileCount();
        basePierce = inheritanceData.GetInheritedPierce();
        baseRange = inheritanceData.GetInheritedRange();
        projectileSize = inheritanceData.GetInheritedProjectileSize();

        // Inherit tags (union of both parent tags) - THIS IS THE KEY!
        weaponTags = inheritanceData.GetInheritedTags();

        DebugLog.Info($"[Weapon.ApplyInheritedStats] {weaponName}: Inherited {weaponTags.Count} tags from parents");
    }
    
    /// <summary>
    /// Auto-registers this weapon with CollisionManager if it implements IWeaponCollisionHandler.
    /// Call this from derived weapon's Awake() after base.Awake().
    /// </summary>
    protected void RegisterWithCollisionManager()
    {
        // Only register if weapon implements the collision interface
        if (this is IWeaponCollisionHandler handler)
        {
            CollisionManager collisionMgr = FindAnyObjectByType<CollisionManager>();
            if (collisionMgr != null)
            {
                collisionMgr.RegisterWeapon(handler);
                DebugLog.Info($"[{weaponName}] Auto-registered with CollisionManager");
            }
            else
            {
                DebugLog.Error($"[{weaponName}] CollisionManager not found - collisions will NOT work!");
            }
        }
    }
    
    protected virtual void Update()
    {
        // ONLY fire weapons during gameplay phase
        if (GamePhaseManager.CurrentPhase != GamePhase.Gameplay) return;
        if (GameState.IsPaused) return;

        timeSinceLastFire += Time.deltaTime;

        if (timeSinceLastFire >= 1f / currentFireRate)
        {
            Fire();
            timeSinceLastFire = 0f;
        }
    }
    
    /// <summary>
    /// Fire the weapon. Override in subclasses.
    /// </summary>
    protected abstract void Fire();

    /// <summary>
    /// Get the damage multiplier from weapon level.
    /// +20% per level above 1 (Level 2 = 1.2x, Level 5 = 1.8x)
    /// </summary>
    protected virtual float GetLevelDamageMultiplier()
    {
        return 1f + (level - 1) * 0.2f;
    }

    /// <summary>
    /// Get the fire rate multiplier from weapon level.
    /// +10% per level above 1 (Level 2 = 1.1x, Level 5 = 1.4x)
    /// </summary>
    protected virtual float GetLevelFireRateMultiplier()
    {
        return 1f + (level - 1) * 0.1f;
    }

    /// <summary>
    /// Get bonus projectiles from weapon level milestones.
    /// +1 projectile at max level (level 3)
    /// </summary>
    protected virtual int GetLevelBonusProjectiles()
    {
        return level >= MaxLevel ? 1 : 0;
    }

    /// <summary>
    /// Get bonus pierce from weapon level milestones.
    /// Currently no pierce bonus (reserved for future items/upgrades)
    /// </summary>
    protected virtual int GetLevelBonusPierce()
    {
        return 0;
    }

    /// <summary>
    /// Recalculate weapon stats based on level, player multipliers, and items.
    /// Public so SynergyManager and ItemEffects can trigger recalculation.
    /// </summary>
    public virtual void RecalculateStats()
    {
        // Damage: base × level multiplier × player multiplier
        currentDamage = baseDamage * GetLevelDamageMultiplier();
        if (player != null)
            currentDamage *= player.DamageMultiplier;

        // Apply synergy bonuses (TODO: Remove this in future - tags should enable mechanics, not damage)
        SynergyManager synergyManager = GameServices.SynergyManager;
        if (synergyManager != null)
        {
            float synergyMultiplier = 1f;
            foreach (WeaponTag tag in weaponTags)
            {
                synergyMultiplier += synergyManager.GetSynergyMultiplier(tag);
            }
            currentDamage *= synergyMultiplier;

            if (synergyMultiplier > 1f)
            {
                DebugLog.Info($"[Weapon.Synergy] {weaponName} synergy multiplier: {synergyMultiplier:F2}x", "Weapon");
            }
        }

        DebugLog.Info($"[Weapon.RecalculateStats] {weaponName} Lv.{level}: baseDamage={baseDamage:F1}, levelMult={GetLevelDamageMultiplier():F2}, playerMult={player?.DamageMultiplier:F2} → currentDamage={currentDamage:F1}", "Weapon");

        // Fire rate: base × level multiplier × player attack speed × global multiplier
        currentFireRate = baseFireRate * GetLevelFireRateMultiplier() * GlobalFireRateMultiplier;
        if (player != null)
            currentFireRate *= player.AttackSpeedMultiplier;

        // Projectile count: base + level bonus + player bonus
        currentProjectileCount = projectileCount + GetLevelBonusProjectiles();
        if (player != null)
            currentProjectileCount += player.BonusProjectiles;

        // Pierce: base + level bonus (items will modify this in the future)
        currentPierce = basePierce + GetLevelBonusPierce();

        // Range: base only (items will modify this in the future)
        currentRange = baseRange;

        // Projectile Size: base only (items will modify this in the future)
        currentProjectileSize = projectileSize;
    }
    
    // Public getters
    public string WeaponName => weaponName;
    public float Damage => currentDamage;
    public float FireRate => currentFireRate;
    public int ProjectileCount => currentProjectileCount;
    public int Pierce => currentPierce;
    public float Range => currentRange;
    public float ProjectileSize => currentProjectileSize;
    public List<WeaponTag> GetTags() => weaponTags;

    // Level-related properties
    public int Level => level;
    public bool CanLevelUp => level < MaxLevel;
    public bool CanCombine => level >= 2;

    /// <summary>
    /// Level up this weapon. Recalculates stats and fires event.
    /// </summary>
    public virtual void LevelUp()
    {
        if (!CanLevelUp)
        {
            DebugLog.Warning($"[Weapon] {weaponName} is already at max level ({MaxLevel})");
            return;
        }

        level++;
        RecalculateStats();
        OnWeaponLevelUp?.Invoke(this, level);

        DebugLog.Info($"[Weapon] {weaponName} leveled up to level {level}!");
    }

    /// <summary>
    /// Check if weapon has a specific tag.
    /// Used by item effects system for tag-based mechanics.
    /// </summary>
    public bool HasTag(WeaponTag tag) => weaponTags.Contains(tag);

    #region Targeting Utilities

    /// <summary>
    /// Find the nearest enemy within range using spatial grid query.
    /// </summary>
    /// <param name="position">Origin position for distance calculation</param>
    /// <param name="maxRange">Maximum range to search (required)</param>
    /// <param name="exclude">Optional enemy to exclude from results</param>
    /// <returns>Nearest enemy or null if none found</returns>
    protected Enemy FindNearestEnemy(Vector3 position, float maxRange, Enemy exclude = null)
    {
        CollisionManager collisionMgr = GameServices.CollisionManager;
        if (collisionMgr == null)
        {
            DebugLog.Error("[Weapon] CollisionManager is NULL! Cannot find enemies.");
            return null;
        }

        var nearbyEntities = collisionMgr.QueryNearbyEnemies(position, maxRange);

        Enemy nearest = null;
        float nearestDistance = maxRange;

        foreach (var entity in nearbyEntities)
        {
            if (!(entity is Enemy enemy) || !enemy.IsActive) continue;
            if (exclude != null && enemy == exclude) continue;

            float distance = Vector3.Distance(position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Find nearest enemy from player position.
    /// </summary>
    protected Enemy FindNearestEnemy(float maxRange)
    {
        if (playerTransform == null) return null;
        return FindNearestEnemy(playerTransform.position, maxRange);
    }

    #endregion
}
