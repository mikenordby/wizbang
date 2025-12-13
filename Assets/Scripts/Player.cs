using UnityEngine;

/// <summary>
/// Player character stats, progression, and state management
/// </summary>
public class Player : MonoBehaviour
{
    [Header("Level & XP")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    private int xpToNextLevel = 100;
    private LevelUpUI levelUpUI;
    
    [Header("Combat Stats")]
    [Tooltip("Global damage multiplier for all weapons")]
    [SerializeField] private float damageMultiplier = 1f;
    
    [Tooltip("Attack speed multiplier for all weapons")]
    [SerializeField] private float attackSpeedMultiplier = 1f;
    
    [Tooltip("Projectile speed multiplier")]
    [SerializeField] private float projectileSpeedMultiplier = 1f;
    
    [Tooltip("Critical hit chance (0-1)")]
    [SerializeField] private float critChance = 0f;
    
    [Tooltip("Critical hit damage multiplier")]
    [SerializeField] private float critDamage = 2f;
    
    [Header("Defensive Stats")]
    [Tooltip("Damage reduction percentage (0-1)")]
    [SerializeField] private float damageReduction = 0f;
    
    [Tooltip("Max health modifier")]
    [SerializeField] private float maxHealthMultiplier = 1f;
    
    [Tooltip("Health regeneration per second")]
    [SerializeField] private float healthRegen = 0f;
    
    [Header("Utility Stats")]
    [Tooltip("Movement speed multiplier")]
    [SerializeField] private float moveSpeedMultiplier = 1f;
    
    [Tooltip("XP collection radius")]
    [SerializeField] private float xpMagnetRange = 2f;
    
    [Tooltip("Pickup radius for items")]
    [SerializeField] private float pickupRadius = 0.5f;
    
    [Tooltip("Luck stat (affects drop rates, crits, etc)")]
    [SerializeField] private float luck = 0f;
    
    [Header("Weapon Stats")]
    [Tooltip("Max weapon slots available")]
    [SerializeField] private int maxWeaponSlots = 4;
    
    [Tooltip("Projectile pierce count bonus")]
    [SerializeField] private int bonusPierce = 0;
    
    [Tooltip("Additional projectile count")]
    [SerializeField] private int bonusProjectiles = 0;
    
    [Tooltip("Area of effect size multiplier")]
    [SerializeField] private float aoeMultiplier = 1f;
    
    [Tooltip("Projectile duration/range multiplier")]
    [SerializeField] private float rangeMultiplier = 1f;
    
    [Header("Runtime Stats")]
    [SerializeField] private int enemiesKilled = 0;
    [SerializeField] private float damageDealt = 0f;
    [SerializeField] private float damageTaken = 0f;
    
    // Properties for easy access
    public int CurrentLevel => currentLevel;
    public int CurrentXP => currentXP;
    public int XPToNextLevel => xpToNextLevel;
    public float XPProgress => (float)currentXP / xpToNextLevel;
    
    public float DamageMultiplier => damageMultiplier;
    public float AttackSpeedMultiplier => attackSpeedMultiplier;
    public float ProjectileSpeedMultiplier => projectileSpeedMultiplier;
    public float CritChance => critChance;
    public float CritDamage => critDamage;
    
    public float DamageReduction => damageReduction;
    public float MaxHealthMultiplier => maxHealthMultiplier;
    public float HealthRegen => healthRegen;
    
    public float MoveSpeedMultiplier => moveSpeedMultiplier;
    public float XPMagnetRange => xpMagnetRange;
    public float PickupRadius => pickupRadius;
    public float Luck => luck;
    
    public int MaxWeaponSlots => maxWeaponSlots;
    public int BonusPierce => bonusPierce;
    public int BonusProjectiles => bonusProjectiles;
    public float AOEMultiplier => aoeMultiplier;
    public float RangeMultiplier => rangeMultiplier;
    
    public int EnemiesKilled => enemiesKilled;
    public float DamageDealt => damageDealt;
    public float DamageTaken => damageTaken;
    
    void Start()
    {
        CalculateXPToNextLevel();
        levelUpUI = FindAnyObjectByType<LevelUpUI>();
        
        // Create wizard sprite
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateWizardSprite();
        
        Debug.Log($"Player.Start: Level {currentLevel}, XP {currentXP}/{xpToNextLevel}");
    }
    
    /// <summary>
    /// Add XP and check for level up
    /// </summary>
    public void AddXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"Player.AddXP: Gained {amount} XP, total={currentXP}/{xpToNextLevel}");
        
        // Check for level up
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }
    
    /// <summary>
    /// Level up the player
    /// </summary>
    private void LevelUp()
    {
        currentLevel++;
        currentXP -= xpToNextLevel;
        CalculateXPToNextLevel();
        
        Debug.Log($"Player.LevelUp: LEVEL UP! Now level {currentLevel}, XP={currentXP}/{xpToNextLevel}");
        
        if (levelUpUI != null)
        {
            levelUpUI.ShowUI(currentLevel);
        }
    }
    
    /// <summary>
    /// Calculate XP needed for next level using formula: 5 * level
    /// </summary>
    private void CalculateXPToNextLevel()
    {
        xpToNextLevel = 5 * currentLevel;
    }
    
    /// <summary>
    /// Increment enemy kill count
    /// </summary>
    public void OnEnemyKilled()
    {
        enemiesKilled++;
    }
    
    /// <summary>
    /// Track damage dealt by player
    /// </summary>
    public void OnDamageDealt(float amount)
    {
        damageDealt += amount;
    }
    
    /// <summary>
    /// Track damage taken by player
    /// </summary>
    public void OnDamageTaken(float amount)
    {
        damageTaken += amount;
    }
    
    /// <summary>
    /// Modify a stat (for upgrades/power-ups)
    /// </summary>
    public void ModifyStat(string statName, float value, bool isMultiplier = false)
    {
        switch (statName.ToLower())
        {
            case "damage":
            case "damagemultiplier":
                damageMultiplier = isMultiplier ? damageMultiplier * value : damageMultiplier + value;
                break;
            case "attackspeed":
                attackSpeedMultiplier = isMultiplier ? attackSpeedMultiplier * value : attackSpeedMultiplier + value;
                break;
            case "movespeed":
                moveSpeedMultiplier = isMultiplier ? moveSpeedMultiplier * value : moveSpeedMultiplier + value;
                break;
            case "critchance":
                critChance = isMultiplier ? critChance * value : Mathf.Clamp01(critChance + value);
                break;
            case "critdamage":
                critDamage = isMultiplier ? critDamage * value : critDamage + value;
                break;
            case "xprange":
            case "magnetrange":
                xpMagnetRange = isMultiplier ? xpMagnetRange * value : xpMagnetRange + value;
                break;
            case "luck":
                luck = isMultiplier ? luck * value : luck + value;
                break;
            case "aoe":
                aoeMultiplier = isMultiplier ? aoeMultiplier * value : aoeMultiplier + value;
                break;
            case "range":
                rangeMultiplier = isMultiplier ? rangeMultiplier * value : rangeMultiplier + value;
                break;
            default:
                Debug.LogWarning($"Player.ModifyStat: Unknown stat '{statName}'");
                break;
        }
        
        Debug.Log($"Player.ModifyStat: {statName} modified by {value} ({(isMultiplier ? "multiply" : "add")})");
    }
}
