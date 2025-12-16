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
    [Tooltip("Base movement speed of projectiles")]
    [SerializeField] protected float projectileSpeed = 8f;
    
    [Tooltip("Visual and collision size multiplier (1.0 = normal)")]
    [SerializeField] protected float projectileSize = 1f;
    
    [Tooltip("Type of damage (affects future elemental interactions)")]
    [SerializeField] protected DamageType damageType = DamageType.Physical;
    
    [Header("Advanced Properties")]
    [Tooltip("Spread angle in degrees for multiple projectiles (0 = parallel)")]
    [SerializeField] protected float spreadAngle = 0f;
    
    [Tooltip("Enable homing behavior (targets nearest enemy)")]
    [SerializeField] protected bool homing = false;
    
    // Individual upgrade tracking
    protected Dictionary<WeaponUpgrade.UpgradeType, WeaponUpgrade> upgrades = new Dictionary<WeaponUpgrade.UpgradeType, WeaponUpgrade>();
    
    // Calculated stats
    protected int currentProjectileCount;
    protected float currentDamage;
    protected float currentFireRate;
    protected int currentPierce;
    protected float currentRange;
    
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
    
    protected virtual void Update()
    {
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
        
        DebugLog.Info($"[Weapon.RecalculateStats] {weaponName}: baseDamage={baseDamage:F1}, upgradeBonus={damageUpgradeBonus:F1}%, playerMult={player?.DamageMultiplier:F2} → currentDamage={currentDamage:F1}");
        
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
    public Dictionary<WeaponUpgrade.UpgradeType, WeaponUpgrade> Upgrades => upgrades;
}
