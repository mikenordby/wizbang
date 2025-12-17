using UnityEngine;

/// <summary>
/// Player character stats, progression, and state management
/// </summary>
public class Player : MonoBehaviour, ICollidable
{
    [Header("Character")]
    private CharacterData characterData;
    private bool isInitialized = false;
    
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
    
    // ICollidable implementation
    public Vector3 Position => transform.position;
    public float CollisionRadius => 0.4f; // Wizard body: 128px sprite at 128 PPU = ~0.8 diameter
    public bool IsActive => true; // Player always active
    public CollisionLayer Layer => CollisionLayer.Player;
    
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
    
    [Tooltip("Projectile visual and hitbox size multiplier")]
    [SerializeField] private float projectileSizeMultiplier = 1f;
    
    [Header("Runtime Stats")]
    [SerializeField] private int enemiesKilled = 0;
    [SerializeField] private float damageDealt = 0f;
    [SerializeField] private float damageTaken = 0f;
    
    // Component references
    private DirectionalSpriteController directionController;
    
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
    public float ProjectileSizeMultiplier => projectileSizeMultiplier;
    
    public int EnemiesKilled => enemiesKilled;
    public float DamageDealt => damageDealt;
    public float DamageTaken => damageTaken;
    
    void Start()
    {
        // Initialization is deferred until InitializeWithCharacter() is called by CharacterSelectionUI
        // This allows character selection before game starts
    }
    
    /// <summary>
    /// Initialize player with selected character data.
    /// Called by CharacterSelectionUI after character selection.
    /// </summary>
    public void InitializeWithCharacter(CharacterData character)
    {
        if (isInitialized)
        {
            DebugLog.Warning("[Player] Already initialized, ignoring duplicate initialization");
            return;
        }
        
        characterData = character;
        
        // Apply base stats from character
        moveSpeedMultiplier = character.moveSpeedModifier;
        damageMultiplier = character.damageModifier;
        attackSpeedMultiplier = character.attackSpeedModifier;
        critChance = character.startingCritChance;
        critDamage = character.critDamageModifier;
        xpMagnetRange = character.xpMagnetRange;
        pickupRadius = character.pickupRadius;
        healthRegen = character.healthRegen;
        
        // Initialize health with character's base health
        Health health = GetComponent<Health>();
        if (health == null)
            health = gameObject.AddComponent<Health>();
        health.Initialize(character.baseMaxHealth);
        
        // Update PlayerMovement with character's move speed
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.SetMoveSpeedModifier(character.moveSpeedModifier);
        }
        
        // Set up SpriteRenderer first
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();
        
        sr.color = character.characterColor;
        sr.sortingOrder = 10; // Above ground and projectiles
        
        // Set up DirectionalSpriteController (8-directional sprites)
        directionController = GetComponent<DirectionalSpriteController>();
        if (directionController == null)
            directionController = gameObject.AddComponent<DirectionalSpriteController>();
        
        // Set entity type and let controller load sprites
        DebugLog.Info($"[Player] Setting sprite type: '{character.spriteType}'");
        directionController.SetEntityType(character.spriteType);
        
        // Set up AnimatedSpriteController for walking animation
        AnimatedSpriteController animController = GetComponent<AnimatedSpriteController>();
        if (animController == null)
            animController = gameObject.AddComponent<AnimatedSpriteController>();
        
        animController.SetEntityType(character.spriteType);
        animController.SetAnimation("walking");
        animController.LoadAllAnimations();
        DebugLog.Info($"[Player] AnimatedSpriteController configured for {character.spriteType}/walking");
        
        // Scale adjustment: PixelLab wizard is 96×96@PPU32 = 3 units, procedural is 128×128@PPU128 = 1 unit
        // To maintain similar visual size: scale by PPU ratio
        float ppu = sr.sprite != null ? sr.sprite.pixelsPerUnit : 32f; // Default to 32 for PixelLab
        float scaleMultiplier = ppu <= 32f ? 1.0f : 2.0f; // PixelLab sprites now at proper scale
        transform.localScale = Vector3.one * scaleMultiplier * character.characterScale;
        
        if (sr.sprite != null)
        {
            DebugLog.Info($"[Player] ✓ {character.characterName} sprite: {sr.sprite.name}, Size: {sr.sprite.texture.width}x{sr.sprite.texture.height}px, PPU={ppu}, Scale={scaleMultiplier}");
            DebugLog.Info($"[Player] SpriteRenderer: enabled={sr.enabled}, color={sr.color}, sortingLayer={sr.sortingLayerName}, sortingOrder={sr.sortingOrder}");
            DebugLog.Info($"[Player] GameObject: active={gameObject.activeSelf}, position={transform.position}, scale={transform.localScale}, layer={gameObject.layer}");
        }
        else
        {
            DebugLog.Error($"[Player] ✗ SpriteRenderer.sprite is NULL! Failed to load {character.spriteType}");
        }
        
        // Add starting weapon
        WeaponInventory weaponInventory = GetComponent<WeaponInventory>();
        if (weaponInventory == null)
            weaponInventory = gameObject.AddComponent<WeaponInventory>();
        
        weaponInventory.AddWeapon(character.startingWeaponType);
        
        CalculateXPToNextLevel();
        levelUpUI = GameServices.LevelUpUI;
        
        // Subscribe to game events
        GameEvents.OnEnemyKilled += OnEnemyKilledHandler;
        
        isInitialized = true;
        
        DebugLog.Info($"Player.InitializeWithCharacter: {character.characterName} ready! Level {currentLevel}, XP {currentXP}/{xpToNextLevel}");
    }
    
    /// <summary>
    /// Load character sprite from PixelLab assets
    /// </summary>
    private Sprite LoadCharacterSprite(string spriteType)
    {
        // All characters now use PixelLab 8-directional sprites
        // Load the south-facing sprite as default
        return SpriteLoader.LoadCharacterSprite(spriteType, "south");
    }
    
    private Sprite LoadCharacterSpriteFallback(string spriteType)
    {
        // Fallback for legacy code - try south direction
        switch (spriteType.ToLower())
        {
            case "wizard":
            case "playerwizard":
                return SpriteLoader.LoadCharacterSprite("PlayerWizard", "south");
            case "knight":
                return SpriteLoader.LoadCharacterSprite("knight", "south");
            default:
                DebugLog.Warning($"[Player] Unknown sprite type '{spriteType}', defaulting to wizard");
                return SpriteLoader.LoadCharacterSprite("PlayerWizard", "south");
        }
    }
    
    /// <summary>
    /// Add XP and check for level up
    /// </summary>
    public void AddXP(int amount)
    {
        currentXP += amount;
        DebugLog.Info($"Player.AddXP: Gained {amount} XP, total={currentXP}/{xpToNextLevel}");
        
        // Trigger XP gained event
        GameEvents.TriggerXPGained(amount, currentXP);
        
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
        
        DebugLog.Info($"Player.LevelUp: LEVEL UP! Now level {currentLevel}, XP={currentXP}/{xpToNextLevel}");
        
        // Trigger level up event (UI will subscribe to this)
        GameEvents.TriggerPlayerLevelUp(currentLevel);
        
        if (levelUpUI != null)
        {
            levelUpUI.ShowUI(currentLevel);
        }
    }
    
    /// <summary>
    /// Calculate XP needed for next level using formula: 3 * level (easy leveling for testing)
    /// Level 2=3, Level 3=6, Level 4=9, Level 5=12, etc.
    /// </summary>
    private void CalculateXPToNextLevel()
    {
        xpToNextLevel = 3 * currentLevel;
        DebugLog.Info($"Player.CalculateXP: Level {currentLevel} requires {xpToNextLevel} XP for next level");
    }
    
    /// <summary>
    /// Event handler for enemy deaths (increments kill count)
    /// </summary>
    private void OnEnemyKilledHandler(Enemy enemy)
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
    /// Cleanup event subscriptions on destroy
    /// </summary>
    private void OnDestroy()
    {
        GameEvents.OnEnemyKilled -= OnEnemyKilledHandler;
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
                DebugLog.Warning($"Player.ModifyStat: Unknown stat '{statName}'");
                break;
        }
        
        DebugLog.Info($"Player.ModifyStat: {statName} modified by {value} ({(isMultiplier ? "multiply" : "add")})");
    }
    
    // ===== Stat Upgrade Methods (for UpgradeChoiceGenerator) =====
    
    public void AddDamageMultiplier(float amount) 
    { 
        damageMultiplier += amount;
        DebugLog.Info($"[Player] Damage: {damageMultiplier:F2}x");
    }
    
    public void AddAttackSpeedMultiplier(float amount) 
    { 
        attackSpeedMultiplier += amount;
        DebugLog.Info($"[Player] Attack Speed: {attackSpeedMultiplier:F2}x");
    }
    
    public void AddCritChance(float amount) 
    { 
        critChance = Mathf.Clamp01(critChance + amount);
        DebugLog.Info($"[Player] Crit Chance: {critChance * 100:F1}%");
    }
    
    public void AddCritDamage(float amount) 
    { 
        critDamage += amount;
        DebugLog.Info($"[Player] Crit Damage: {critDamage:F2}x");
    }
    
    public void AddMaxHealth(float amount)
    {
        var health = GetComponent<Health>();
        if (health != null)
        {
            health.IncreaseMaxHealth(amount);
            DebugLog.Info($"[Player] Max Health: {health.MaxHealth}");
        }
    }
    
    public void AddHealthRegen(float amount) 
    { 
        healthRegen += amount;
        DebugLog.Info($"[Player] Health Regen: {healthRegen:F1}/s");
    }
    
    public void AddMoveSpeedMultiplier(float amount) 
    { 
        moveSpeedMultiplier += amount;
        DebugLog.Info($"[Player] Move Speed: {moveSpeedMultiplier:F2}x");
    }
    
    public void AddPickupRadius(float multiplier) 
    { 
        pickupRadius *= (1f + multiplier);
        xpMagnetRange *= (1f + multiplier);
        DebugLog.Info($"[Player] Pickup Radius: {pickupRadius:F1}, Magnet: {xpMagnetRange:F1}");
    }
    
    public void AddDamageReduction(float amount) 
    { 
        damageReduction = Mathf.Clamp01(damageReduction + amount);
        DebugLog.Info($"[Player] Damage Reduction: {damageReduction * 100:F1}%");
    }
}
