using UnityEngine;

/// <summary>
/// Player character runtime state and progression.
/// Separated from HeroDefinition (identity) for cleaner architecture.
/// Uses HeroStats for composable stat modifiers from upgrades/items/buffs.
/// </summary>
public class Player : MonoBehaviour, ICollidable
{
    [Header("Hero Identity (Immutable)")]
    [SerializeField] private HeroDefinition heroDefinition; // What the hero IS
    private bool isInitialized = false;
    
    [Header("Runtime Stat Modifiers")]
    private HeroStats stats; // Accumulates bonuses from upgrades/items/buffs
    
    [Header("Level & XP")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    private int xpToNextLevel = 100;
    private LevelUpUI levelUpUI;
    
    [Header("Weapon Bonuses")]
    [SerializeField] private int maxWeaponSlots = 4;
    [SerializeField] private int bonusPierce = 0;
    [SerializeField] private int bonusProjectiles = 0;
    
    [Header("Runtime Statistics")]
    [SerializeField] private int enemiesKilled = 0;
    [SerializeField] private float damageDealt = 0f;
    [SerializeField] private float damageTaken = 0f;
    
    // Component references (none needed - components are accessed via GetComponent when needed)
    
    // ICollidable implementation
    public Vector3 Position => transform.position;
    public float CollisionRadius => 0.4f;
    public bool IsActive => true;
    public CollisionLayer Layer => CollisionLayer.Player;
    
    // Properties - Compute final values using base + modifiers
    public int CurrentLevel => currentLevel;
    public int CurrentXP => currentXP;
    public int XPToNextLevel => xpToNextLevel;
    public float XPProgress => (float)currentXP / xpToNextLevel;
    
    // Combat stats (base from HeroDefinition + modifiers from HeroStats)
    public float DamageMultiplier => heroDefinition != null && stats != null 
        ? stats.GetFinalValue(StatType.Damage, heroDefinition.baseDamage) 
        : 1f;
    
    public float AttackSpeedMultiplier => heroDefinition != null && stats != null 
        ? stats.GetFinalValue(StatType.AttackSpeed, heroDefinition.baseAttackSpeed) 
        : 1f;
    
    public float ProjectileSpeedMultiplier => 1f; // TODO: Add to HeroDefinition if needed
    
    public float CritChance => heroDefinition != null && stats != null 
        ? stats.GetFinalValue(StatType.CritChance, heroDefinition.baseCritChance) 
        : 0f;
    
    public float CritDamage => heroDefinition != null && stats != null 
        ? stats.GetFinalValue(StatType.CritDamage, heroDefinition.baseCritDamage) 
        : 2f;
    
    // Defensive stats
    public float DamageReduction => stats != null 
        ? stats.GetFinalValue(StatType.DamageReduction, 0f) 
        : 0f;
    
    public float MaxHealthMultiplier => 1f; // Kept for Health component compatibility
    
    public float HealthRegen => heroDefinition != null && stats != null 
        ? stats.GetFinalValue(StatType.HealthRegen, heroDefinition.baseHealthRegen) 
        : 0f;
    
    // Utility stats
    public float MoveSpeedMultiplier => heroDefinition != null && stats != null 
        ? stats.GetFinalValue(StatType.MoveSpeed, heroDefinition.baseMoveSpeed) 
        : 1f;
    
    public float XPMagnetRange => heroDefinition != null && stats != null 
        ? stats.GetFinalValue(StatType.XPMagnetRange, heroDefinition.baseXPMagnetRange) 
        : 2f;
    
    public float PickupRadius => heroDefinition != null && stats != null 
        ? stats.GetFinalValue(StatType.PickupRadius, heroDefinition.basePickupRadius) 
        : 0.5f;
    
    public float Luck => stats != null 
        ? stats.GetFinalValue(StatType.Luck, 0f) 
        : 0f;
    
    public float LifestealChance => stats != null 
        ? stats.GetFinalValue(StatType.Lifesteal, 0f) 
        : 0f;
    
    // Weapon stats
    public int MaxWeaponSlots => maxWeaponSlots;
    public int BonusPierce => bonusPierce + (int)stats?.GetFlatBonus(StatType.Pierce);
    public int BonusProjectiles => bonusProjectiles + (int)stats?.GetFlatBonus(StatType.ProjectileCount);
    public float AOEMultiplier => stats != null ? stats.GetFinalValue(StatType.AOE, 1f) : 1f;
    public float RangeMultiplier => stats != null ? stats.GetFinalValue(StatType.Range, 1f) : 1f;
    public float ProjectileSizeMultiplier => stats != null ? stats.GetFinalValue(StatType.ProjectileSize, 1f) : 1f;
    
    // Statistics
    public int EnemiesKilled => enemiesKilled;
    public float DamageDealt => damageDealt;
    public float DamageTaken => damageTaken;
    
    // Hero identity accessor
    public HeroDefinition Hero => heroDefinition;
    
    void Start()
    {
        // Initialization is deferred until InitializeWithHero() is called by CharacterSelectionUI
        // This allows character selection before game starts
    }
    
    /// <summary>
    /// Initialize player with selected hero.
    /// Called by CharacterSelectionUI after hero selection.
    /// </summary>
    public void InitializeWithHero(HeroDefinition hero)
    {
        if (isInitialized)
        {
            DebugLog.Warning("[Player] Already initialized, ignoring duplicate initialization");
            return;
        }
        
        heroDefinition = hero;
        
        // Initialize stat modifier system
        stats = new HeroStats();
        
        // NOTE: We don't copy stats - properties compute them on demand from heroDefinition + stats
        
        // Initialize health with hero's base health and regen
        Health health = GetComponent<Health>();
        if (health == null)
            health = gameObject.AddComponent<Health>();
        health.Initialize(hero.baseMaxHealth);
        health.SetRegenRate(hero.baseHealthRegen);
        
        // Update PlayerMovement with hero's move speed
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.SetMoveSpeedModifier(hero.baseMoveSpeed);
        }
        
        // Set up SpriteRenderer first
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();
        
        sr.color = hero.characterColor;
        sr.sortingOrder = 10; // Above ground and projectiles
        
        // Set up AnimatedSpriteController for walking animation (replaces DirectionalSpriteController)
        AnimatedSpriteController animController = GetComponent<AnimatedSpriteController>();
        if (animController == null)
            animController = gameObject.AddComponent<AnimatedSpriteController>();
        
        // Use "Playerwizard" to match Heroes/wizard path structure
        animController.SetEntityType("Player" + hero.spriteType);
        animController.SetAnimation("walking-8-frames");
        animController.LoadAllAnimations();
        DebugLog.Info($"[Player] AnimatedSpriteController configured for Heroes/{hero.spriteType}/walking-8-frames");
        
        // IMPORTANT: Recalculate scale AFTER animated sprites are loaded to ensure correct PPU
        // We need to wait a frame for the sprite to be properly assigned by AnimatedSpriteController
        StartCoroutine(RecalculateScaleNextFrame(sr, hero.visualScale));
        
        DebugLog.Info($"[Player] SpriteRenderer: enabled={sr.enabled}, color={sr.color}, sortingLayer={sr.sortingLayerName}, sortingOrder={sr.sortingOrder}");
        DebugLog.Info($"[Player] GameObject: active={gameObject.activeSelf}, position={transform.position}, layer={gameObject.layer}");
        
        // Add starting weapon
        WeaponInventory weaponInventory = GetComponent<WeaponInventory>();
        if (weaponInventory == null)
            weaponInventory = gameObject.AddComponent<WeaponInventory>();
        
        weaponInventory.AddWeapon(hero.startingWeaponType);
        
        // Add item inventory for collecting items
        PlayerInventory itemInventory = GetComponent<PlayerInventory>();
        if (itemInventory == null)
            itemInventory = gameObject.AddComponent<PlayerInventory>();
        
        CalculateXPToNextLevel();
        levelUpUI = GameServices.LevelUpUI;
        
        // Subscribe to game events
        GameEvents.OnEnemyKilled += OnEnemyKilledHandler;
        
        isInitialized = true;
        
        DebugLog.Info($"Player.InitializeWithHero: {hero.displayName} ready! Level {currentLevel}, XP {currentXP}/{xpToNextLevel}");
    }
    
    /// <summary>
    /// Recalculate and apply scale after sprites are loaded.
    /// Called as coroutine to ensure sprites are fully initialized.
    /// </summary>
    private System.Collections.IEnumerator RecalculateScaleNextFrame(SpriteRenderer sr, float visualScale)
    {
        // Wait for AnimatedSpriteController to fully load and set the sprite
        yield return new WaitForSeconds(0.1f); // Give more time for animation frames to load
        
        // Now calculate scale based on the ACTUAL loaded sprite
        float ppu = sr.sprite != null ? sr.sprite.pixelsPerUnit : 32f;
        
        DebugLog.Info($"[Player] Sprite PPU detected: {ppu}, sprite={(sr.sprite != null ? sr.sprite.name : "null")}");
        
        // Force larger scale for wizard - AnimatedSpriteController frames are at PPU=100
        // We want the wizard to be visibly larger on screen
        float scaleMultiplier = 2.0f; // Make wizard 2x larger than default
        
        transform.localScale = Vector3.one * scaleMultiplier * visualScale;
        
        DebugLog.Info($"[Player] Scale applied: scaleMultiplier={scaleMultiplier:F2}x, visualScale={visualScale:F2}, final={transform.localScale.x:F2}");
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
    /// Modify a stat using the new HeroStats system (for upgrades/power-ups)
    /// </summary>
    public void ModifyStat(string statName, float value, bool isMultiplier = false)
    {
        if (stats == null)
        {
            DebugLog.Error("[Player] Cannot modify stat - HeroStats not initialized");
            return;
        }
        
        StatType? statType = statName.ToLower() switch
        {
            "damage" or "damagemultiplier" => StatType.Damage,
            "attackspeed" => StatType.AttackSpeed,
            "movespeed" => StatType.MoveSpeed,
            "critchance" => StatType.CritChance,
            "critdamage" => StatType.CritDamage,
            "xprange" or "magnetrange" => StatType.XPMagnetRange,
            "luck" => StatType.Luck,
            "aoe" => StatType.AOE,
            "range" => StatType.Range,
            "health" or "maxhealth" => StatType.MaxHealth,
            "healthregen" => StatType.HealthRegen,
            "damagereduction" => StatType.DamageReduction,
            _ => null
        };
        
        if (statType == null)
        {
            DebugLog.Warning($"[Player] Unknown stat '{statName}'");
            return;
        }
        
        if (isMultiplier)
        {
            // Convert multiplier to percent bonus (e.g., 1.2x = +0.2 = +20%)
            stats.AddPercentBonus(statType.Value, value - 1f);
        }
        else
        {
            stats.AddFlatBonus(statType.Value, value);
        }
        
        DebugLog.Info($"[Player] Modified {statName}: {(isMultiplier ? $"{value:F2}x" : $"+{value:F2}")}");
    }
    
    // ===== Stat Upgrade Methods (for UpgradeChoiceGenerator) =====
    // These now use HeroStats for composable modifiers
    
    public void AddDamageMultiplier(float amount) 
    { 
        if (stats != null)
        {
            stats.AddPercentBonus(StatType.Damage, amount);
            DebugLog.Info($"[Player] Damage: {DamageMultiplier:F2}x (+{amount * 100:F0}%)");
        }
    }
    
    public void AddAttackSpeedMultiplier(float amount) 
    { 
        if (stats != null)
        {
            stats.AddPercentBonus(StatType.AttackSpeed, amount);
            DebugLog.Info($"[Player] Attack Speed: {AttackSpeedMultiplier:F2}x (+{amount * 100:F0}%)");
        }
    }
    
    public void AddCritChance(float amount) 
    { 
        if (stats != null)
        {
            stats.AddFlatBonus(StatType.CritChance, amount);
            DebugLog.Info($"[Player] Crit Chance: {CritChance * 100:F1}% (+{amount * 100:F0}%)");
        }
    }
    
    public void AddCritDamage(float amount) 
    { 
        if (stats != null)
        {
            stats.AddFlatBonus(StatType.CritDamage, amount);
            DebugLog.Info($"[Player] Crit Damage: {CritDamage:F2}x (+{amount:F2})");
        }
    }
    
    public void AddMaxHealth(float amount)
    {
        var health = GetComponent<Health>();
        if (health != null)
        {
            health.IncreaseMaxHealth(amount);
            DebugLog.Info($"[Player] Max Health: {health.MaxHealth} (+{amount:F0})");
        }
    }
    
    public void AddHealthRegen(float amount) 
    { 
        if (stats != null)
        {
            stats.AddFlatBonus(StatType.HealthRegen, amount);
            
            // Update the Health component's regen rate
            var health = GetComponent<Health>();
            if (health != null)
            {
                health.AddRegenRate(amount);
            }
            
            DebugLog.Info($"[Player] Health Regen: {HealthRegen:F1}/s (+{amount:F1})");
        }
    }
    
    public void AddMoveSpeedMultiplier(float amount) 
    { 
        if (stats != null)
        {
            stats.AddPercentBonus(StatType.MoveSpeed, amount);
            DebugLog.Info($"[Player] Move Speed: {MoveSpeedMultiplier:F2}x (+{amount * 100:F0}%)");
        }
    }
    
    public void AddPickupRadius(float multiplier) 
    { 
        if (stats != null)
        {
            stats.AddPercentBonus(StatType.PickupRadius, multiplier);
            stats.AddPercentBonus(StatType.XPMagnetRange, multiplier);
            DebugLog.Info($"[Player] Pickup Radius: {PickupRadius:F1}, Magnet: {XPMagnetRange:F1} (+{multiplier * 100:F0}%)");
        }
    }
    
    public void AddDamageReduction(float amount) 
    { 
        if (stats != null)
        {
            stats.AddFlatBonus(StatType.DamageReduction, amount);
            DebugLog.Info($"[Player] Damage Reduction: {DamageReduction * 100:F1}% (+{amount * 100:F0}%)");
        }
    }
    
    public void AddLifestealChance(float amount) 
    { 
        if (stats != null)
        {
            stats.AddFlatBonus(StatType.Lifesteal, amount);
            DebugLog.Info($"[Player] Lifesteal Chance: {LifestealChance * 100:F1}% (+{amount * 100:F0}%)");
        }
    }
    
    /// <summary>
    /// Get debug string of all active stat bonuses
    /// </summary>
    public string GetStatsDebugString()
    {
        return stats != null ? stats.GetDebugString() : "Stats not initialized";
    }
}
