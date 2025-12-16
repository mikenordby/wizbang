using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a single upgrade choice shown during level-up
/// </summary>
public class UpgradeChoice
{
    public enum ChoiceType
    {
        NewWeapon,
        WeaponUpgrade,
        PlayerStat
    }
    
    public ChoiceType Type { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string IconName { get; set; } // For future sprite icons
    
    // For weapon choices
    public string WeaponType { get; set; }
    
    // For stat upgrades
    public PlayerStatType StatType { get; set; }
    
    public UpgradeChoice(ChoiceType type, string displayName, string description)
    {
        Type = type;
        DisplayName = displayName;
        Description = description;
    }
}

/// <summary>
/// Player stat types for upgrades
/// </summary>
public enum PlayerStatType
{
    Damage,
    AttackSpeed,
    CritChance,
    CritDamage,
    MaxHealth,
    HealthRegen,
    MoveSpeed,
    PickupRadius,
    DamageReduction
}

/// <summary>
/// Generates random upgrade choices for level-up screen
/// </summary>
public class UpgradeChoiceGenerator : MonoBehaviour
{
    [Header("Choice Settings")]
    [SerializeField] private int choicesPerLevelUp = 3;
    [SerializeField] private bool allowDuplicates = false;
    
    [Header("Weighting (1.0 = equal chance)")]
    [SerializeField] private float newWeaponWeight = 1.0f;
    [SerializeField] private float weaponUpgradeWeight = 1.0f;
    [SerializeField] private float playerStatWeight = 1.0f;
    
    [Header("Reroll Settings")]
    [SerializeField] private int maxRerolls = 1;
    private int rerollsUsed = 0;
    
    private WeaponInventory weaponInventory;
    private Player player;
    
    // All available weapon types
    private static readonly string[] AllWeaponTypes = new string[]
    {
        "ProjectileWeapon",
        "OrbiterWeapon",
        "BoomerangWeapon",
        "RapidFireWeapon",
        "LightningWeapon",
        "PoisonWeapon",
        "LaserWeapon"
    };
    
    // Weapon display names and descriptions
    private static readonly Dictionary<string, (string name, string desc)> WeaponInfo = new Dictionary<string, (string, string)>
    {
        { "ProjectileWeapon", ("Magic Missile", "Straight-firing magical projectiles") },
        { "OrbiterWeapon", ("Orbiting Blades", "Knives that circle around you") },
        { "BoomerangWeapon", ("Boomerang", "Returns after hitting enemies") },
        { "RapidFireWeapon", ("Rapid Fire", "High fire rate, lower damage") },
        { "LightningWeapon", ("Chain Lightning", "Arcs between nearby enemies") },
        { "PoisonWeapon", ("Poison Cloud", "Area damage over time") },
        { "LaserWeapon", ("Piercing Laser", "Continuous beam through all enemies") }
    };
    
    // Player stat info
    private static readonly Dictionary<PlayerStatType, (string name, string desc, float value)> StatInfo = new Dictionary<PlayerStatType, (string, string, float)>
    {
        { PlayerStatType.Damage, ("Might", "+10% damage to all weapons", 0.1f) },
        { PlayerStatType.AttackSpeed, ("Haste", "+10% attack speed", 0.1f) },
        { PlayerStatType.CritChance, ("Critical Strike", "+5% critical hit chance", 0.05f) },
        { PlayerStatType.CritDamage, ("Critical Power", "+25% critical damage", 0.25f) },
        { PlayerStatType.MaxHealth, ("Vitality", "+20 maximum health", 20f) },
        { PlayerStatType.HealthRegen, ("Regeneration", "+1 health per second", 1f) },
        { PlayerStatType.MoveSpeed, ("Swiftness", "+10% movement speed", 0.1f) },
        { PlayerStatType.PickupRadius, ("Magnet", "+50% pickup radius", 0.5f) },
        { PlayerStatType.DamageReduction, ("Armor", "+5% damage reduction", 0.05f) }
    };
    
    private void Awake()
    {
        weaponInventory = GetComponent<WeaponInventory>();
        player = GetComponent<Player>();
    }
    
    /// <summary>
    /// Generate random upgrade choices for level-up
    /// </summary>
    public List<UpgradeChoice> GenerateChoices()
    {
        List<UpgradeChoice> choices = new List<UpgradeChoice>();
        List<UpgradeChoice> pool = BuildChoicePool();
        
        if (pool.Count == 0)
        {
            DebugLog.Warning("[UpgradeChoiceGenerator] No choices available!");
            return choices;
        }
        
        // Generate unique choices
        int attemptsLeft = 100; // Prevent infinite loop
        while (choices.Count < choicesPerLevelUp && pool.Count > 0 && attemptsLeft > 0)
        {
            UpgradeChoice choice = SelectWeightedChoice(pool);
            
            if (allowDuplicates || !IsDuplicateChoice(choice, choices))
            {
                choices.Add(choice);
                if (!allowDuplicates)
                    pool.Remove(choice); // Remove to prevent duplicates
            }
            
            attemptsLeft--;
        }
        
        DebugLog.Info($"[UpgradeChoiceGenerator] Generated {choices.Count} choices");
        return choices;
    }
    
    /// <summary>
    /// Build the pool of all possible choices
    /// </summary>
    private List<UpgradeChoice> BuildChoicePool()
    {
        List<UpgradeChoice> pool = new List<UpgradeChoice>();
        
        // Add new weapon choices (if inventory not full)
        if (weaponInventory.HasSpace())
        {
            foreach (string weaponType in AllWeaponTypes)
            {
                // Only offer weapons player doesn't have
                if (!weaponInventory.HasWeapon(weaponType))
                {
                    var info = WeaponInfo[weaponType];
                    var choice = new UpgradeChoice(
                        UpgradeChoice.ChoiceType.NewWeapon,
                        info.name,
                        info.desc
                    );
                    choice.WeaponType = weaponType;
                    
                    // Apply weighting (add multiple times for higher chance)
                    int weight = Mathf.RoundToInt(newWeaponWeight * 10);
                    for (int i = 0; i < weight; i++)
                        pool.Add(choice);
                }
            }
        }
        
        // Add weapon upgrade choices (for owned weapons below max level)
        var weapons = weaponInventory.GetActiveWeapons();
        foreach (var weapon in weapons)
        {
            if (!weapon.IsMaxLevel)
            {
                var choice = new UpgradeChoice(
                    UpgradeChoice.ChoiceType.WeaponUpgrade,
                    $"Upgrade {weapon.WeaponName}",
                    $"Level {weapon.WeaponLevel} → {weapon.WeaponLevel + 1}"
                );
                choice.WeaponType = weapon.GetType().Name;
                
                int weight = Mathf.RoundToInt(weaponUpgradeWeight * 10);
                for (int i = 0; i < weight; i++)
                    pool.Add(choice);
            }
        }
        
        // Add player stat upgrade choices
        foreach (var statType in System.Enum.GetValues(typeof(PlayerStatType)).Cast<PlayerStatType>())
        {
            var info = StatInfo[statType];
            var choice = new UpgradeChoice(
                UpgradeChoice.ChoiceType.PlayerStat,
                info.name,
                info.desc
            );
            choice.StatType = statType;
            
            int weight = Mathf.RoundToInt(playerStatWeight * 10);
            for (int i = 0; i < weight; i++)
                pool.Add(choice);
        }
        
        return pool;
    }
    
    /// <summary>
    /// Select a random choice from pool (weighted)
    /// </summary>
    private UpgradeChoice SelectWeightedChoice(List<UpgradeChoice> pool)
    {
        if (pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }
    
    /// <summary>
    /// Check if choice is duplicate
    /// </summary>
    private bool IsDuplicateChoice(UpgradeChoice newChoice, List<UpgradeChoice> existing)
    {
        foreach (var choice in existing)
        {
            if (choice.Type != newChoice.Type) continue;
            
            if (choice.Type == UpgradeChoice.ChoiceType.NewWeapon || 
                choice.Type == UpgradeChoice.ChoiceType.WeaponUpgrade)
            {
                if (choice.WeaponType == newChoice.WeaponType)
                    return true;
            }
            else if (choice.Type == UpgradeChoice.ChoiceType.PlayerStat)
            {
                if (choice.StatType == newChoice.StatType)
                    return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Apply the chosen upgrade
    /// </summary>
    public void ApplyChoice(UpgradeChoice choice)
    {
        if (choice == null) return;
        
        switch (choice.Type)
        {
            case UpgradeChoice.ChoiceType.NewWeapon:
                weaponInventory.AddWeapon(choice.WeaponType);
                DebugLog.Info($"[Upgrade] Added new weapon: {choice.DisplayName}");
                break;
                
            case UpgradeChoice.ChoiceType.WeaponUpgrade:
                weaponInventory.UpgradeWeapon(choice.WeaponType);
                DebugLog.Info($"[Upgrade] Upgraded weapon: {choice.DisplayName}");
                break;
                
            case UpgradeChoice.ChoiceType.PlayerStat:
                ApplyStatUpgrade(choice.StatType);
                DebugLog.Info($"[Upgrade] Upgraded stat: {choice.DisplayName}");
                break;
        }
    }
    
    /// <summary>
    /// Apply a player stat upgrade
    /// </summary>
    private void ApplyStatUpgrade(PlayerStatType statType)
    {
        if (player == null) return;
        
        var info = StatInfo[statType];
        float value = info.value;
        
        switch (statType)
        {
            case PlayerStatType.Damage:
                player.AddDamageMultiplier(value);
                break;
            case PlayerStatType.AttackSpeed:
                player.AddAttackSpeedMultiplier(value);
                break;
            case PlayerStatType.CritChance:
                player.AddCritChance(value);
                break;
            case PlayerStatType.CritDamage:
                player.AddCritDamage(value);
                break;
            case PlayerStatType.MaxHealth:
                player.AddMaxHealth(value);
                break;
            case PlayerStatType.HealthRegen:
                player.AddHealthRegen(value);
                break;
            case PlayerStatType.MoveSpeed:
                player.AddMoveSpeedMultiplier(value);
                break;
            case PlayerStatType.PickupRadius:
                player.AddPickupRadius(value);
                break;
            case PlayerStatType.DamageReduction:
                player.AddDamageReduction(value);
                break;
        }
    }
    
    /// <summary>
    /// Check if reroll is available
    /// </summary>
    public bool CanReroll()
    {
        return rerollsUsed < maxRerolls;
    }
    
    /// <summary>
    /// Use a reroll
    /// </summary>
    public void UseReroll()
    {
        rerollsUsed++;
        DebugLog.Info($"[UpgradeChoiceGenerator] Rerolls used: {rerollsUsed}/{maxRerolls}");
    }
    
    /// <summary>
    /// Reset rerolls (new game)
    /// </summary>
    public void ResetRerolls()
    {
        rerollsUsed = 0;
    }
    
    /// <summary>
    /// Get remaining rerolls
    /// </summary>
    public int GetRemainingRerolls()
    {
        return maxRerolls - rerollsUsed;
    }
}
