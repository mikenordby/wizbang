using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Abstract base class for all weapons.
/// Handles fire rate, damage, and common weapon logic with individual upgrade tracking.
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] protected string weaponName = "Weapon";
    [SerializeField] protected int weaponLevel = 1;
    [SerializeField] protected int maxWeaponLevel = 8;
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
    
    // Individual upgrade tracking
    protected Dictionary<WeaponUpgrade.UpgradeType, WeaponUpgrade> upgrades = new Dictionary<WeaponUpgrade.UpgradeType, WeaponUpgrade>();
    
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
        
        // Initialize upgrade dictionary
        upgrades[WeaponUpgrade.UpgradeType.Damage] = new WeaponUpgrade(WeaponUpgrade.UpgradeType.Damage, 10);
        upgrades[WeaponUpgrade.UpgradeType.ProjectileCount] = new WeaponUpgrade(WeaponUpgrade.UpgradeType.ProjectileCount, 5);
        upgrades[WeaponUpgrade.UpgradeType.FireRate] = new WeaponUpgrade(WeaponUpgrade.UpgradeType.FireRate, 8);
        upgrades[WeaponUpgrade.UpgradeType.Pierce] = new WeaponUpgrade(WeaponUpgrade.UpgradeType.Pierce, 5);
        upgrades[WeaponUpgrade.UpgradeType.Range] = new WeaponUpgrade(WeaponUpgrade.UpgradeType.Range, 5);
        upgrades[WeaponUpgrade.UpgradeType.ProjectileSize] = new WeaponUpgrade(WeaponUpgrade.UpgradeType.ProjectileSize, 5);
            
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
        // Debug logging - only log occasionally
        if (Time.frameCount % 120 == 0) // Every 2 seconds
        {
            DebugLog.Verbose($"[Weapon.Update] Called on {GetType().Name}, phase={GamePhaseManager.CurrentPhase}, paused={GameState.IsPaused}, timeSinceLastFire={timeSinceLastFire:F2}");
        }
        
        // ONLY fire weapons during gameplay phase
        if (GamePhaseManager.CurrentPhase != GamePhase.Gameplay)
        {
            if (Time.frameCount % 60 == 0) // Log once per second
            {
                DebugLog.Warning($"[Weapon] Cannot fire - current phase is {GamePhaseManager.CurrentPhase}, need Gameplay");
            }
            return;
        }
        
        if (GameState.IsPaused) return;
        
        timeSinceLastFire += Time.deltaTime;
        
        if (timeSinceLastFire >= 1f / currentFireRate)
        {
            DebugLog.Verbose($"[Weapon] Firing {GetType().Name}, timeSinceLastFire={timeSinceLastFire:F2}, fireRate={currentFireRate:F2}");
            Fire();
            timeSinceLastFire = 0f;
        }
    }
    
    /// <summary>
    /// Fire the weapon. Override in subclasses.
    /// </summary>
    protected abstract void Fire();
    
    /// <summary>
    /// Apply a specific upgrade to this weapon
    /// </summary>
    public virtual bool ApplyUpgrade(WeaponUpgrade.UpgradeType upgradeType)
    {
        if (!upgrades.ContainsKey(upgradeType))
        {
            DebugLog.Warning($"[Weapon] {weaponName} does not have upgrade type: {upgradeType}");
            return false;
        }
        
        WeaponUpgrade upgrade = upgrades[upgradeType];
        if (!upgrade.CanUpgrade)
        {
            DebugLog.Warning($"[Weapon] {weaponName} upgrade {upgradeType} is already maxed");
            return false;
        }
        
        upgrade.currentLevel++;
        weaponLevel++; // Overall weapon level increases
        RecalculateStats();
        
        DebugLog.Info($"[Weapon] {weaponName} upgraded {upgradeType} to level {upgrade.currentLevel} (weapon level {weaponLevel})");
        return true;
    }
    
    /// <summary>
    /// Get all available upgrades for this weapon
    /// </summary>
    public List<WeaponUpgrade> GetAvailableUpgrades()
    {
        List<WeaponUpgrade> available = new List<WeaponUpgrade>();
        foreach (var upgrade in upgrades.Values)
        {
            if (upgrade.CanUpgrade)
                available.Add(upgrade);
        }
        return available;
    }
    
    /// <summary>
    /// Level up the weapon, improving its stats (legacy method - now use ApplyUpgrade)
    /// </summary>
    public virtual void LevelUp()
    {
        weaponLevel++;
        ApplyLevelUpgrades();
        RecalculateStats();
        DebugLog.Info($"[Weapon] {weaponName} leveled up to {weaponLevel}");
    }
    
    /// <summary>
    /// Apply level-specific upgrades (override in subclasses for custom scaling)
    /// </summary>
    protected virtual void ApplyLevelUpgrades()
    {
        // Base implementation - subclasses override for custom behavior
    }
    
    /// <summary>
    /// Recalculate weapon stats based on level and player multipliers.
    /// </summary>
    protected virtual void RecalculateStats()
    {
        // Damage: base + upgrade bonus + player multiplier
        float damageUpgradeBonus = upgrades[WeaponUpgrade.UpgradeType.Damage].GetDamageBonus();
        currentDamage = baseDamage * (1f + damageUpgradeBonus / 100f);
        if (player != null)
            currentDamage *= player.DamageMultiplier;
        
        DebugLog.Info($"[Weapon.RecalculateStats] {weaponName}: baseDamage={baseDamage:F1}, upgradeBonus={damageUpgradeBonus:F1}%, playerMult={player?.DamageMultiplier:F2} → currentDamage={currentDamage:F1}", "Weapon");
        
        // Fire rate: base + upgrade bonus + player attack speed
        float fireRateUpgradeBonus = upgrades[WeaponUpgrade.UpgradeType.FireRate].GetFireRateBonus();
        currentFireRate = baseFireRate * (1f + fireRateUpgradeBonus / 100f);
        if (player != null)
            currentFireRate *= player.AttackSpeedMultiplier;
        
        // Projectile count: base + upgrade + player bonus
        currentProjectileCount = projectileCount + upgrades[WeaponUpgrade.UpgradeType.ProjectileCount].GetProjectileBonus();
        if (player != null)
            currentProjectileCount += player.BonusProjectiles;
        
        // Pierce: base + upgrade
        currentPierce = basePierce + upgrades[WeaponUpgrade.UpgradeType.Pierce].GetPierceBonus();
        
        // Range: base + upgrade bonus
        float rangeUpgradeBonus = upgrades[WeaponUpgrade.UpgradeType.Range].GetRangeBonus();
        currentRange = baseRange * (1f + rangeUpgradeBonus / 100f);
        
        // Projectile Size: base + upgrade bonus
        float sizeUpgradeBonus = upgrades[WeaponUpgrade.UpgradeType.ProjectileSize].GetSizeBonus();
        currentProjectileSize = projectileSize * (1f + sizeUpgradeBonus / 100f);
    }
    
    // Public getters
    public string WeaponName => weaponName;
    public int WeaponLevel => weaponLevel;
    public int MaxWeaponLevel => maxWeaponLevel;
    public bool IsMaxLevel => weaponLevel >= maxWeaponLevel;
    public float Damage => currentDamage;
    public float FireRate => currentFireRate;
    public int ProjectileCount => currentProjectileCount;
    public int Pierce => currentPierce;
    public float Range => currentRange;
    public float ProjectileSize => currentProjectileSize;
    public Dictionary<WeaponUpgrade.UpgradeType, WeaponUpgrade> Upgrades => upgrades;
}
