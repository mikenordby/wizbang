using UnityEngine;
using System;
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
        PlayerStat,
        WeaponCombination
    }

    public ChoiceType Type { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string IconName { get; set; }

    // For weapon choices
    public string WeaponType { get; set; }

    // For stat upgrades
    public PlayerStatType StatType { get; set; }

    // For weapon upgrades
    public Weapon WeaponToUpgrade { get; set; }

    // For weapon combinations
    public WeaponCombination CombinationRecipe { get; set; }
    public Weapon Weapon1 { get; set; }
    public Weapon Weapon2 { get; set; }

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
    DamageReduction,
    Lifesteal
}

/// <summary>
/// Generates random upgrade choices for level-up screen.
/// Uses data-driven WeaponDefinition system.
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
    [SerializeField] private float combinationWeight = 1.5f;

    [Header("Reroll Settings")]
    [SerializeField] private int maxRerolls = 20; // Set to 20 for debugging
    private int rerollsUsed = 0;

    private WeaponInventory weaponInventory;
    private Player player;

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
        { PlayerStatType.DamageReduction, ("Armor", "+5% damage reduction", 0.05f) },
        { PlayerStatType.Lifesteal, ("Lifesteal", "5% chance to heal 1 HP on hit", 0.05f) }
    };

    private void Awake()
    {
        weaponInventory = GetComponent<WeaponInventory>();
        player = GetComponent<Player>();

        if (weaponInventory == null && GameServices.Player != null)
        {
            weaponInventory = GameServices.Player.GetComponent<WeaponInventory>();
        }

        if (player == null)
        {
            player = GameServices.Player;
        }
    }

    private void EnsureReferences()
    {
        if (weaponInventory == null)
        {
            if (GameServices.Player != null)
            {
                weaponInventory = GameServices.Player.GetComponent<WeaponInventory>();
            }
            if (weaponInventory == null)
            {
                DebugLog.Error("[UpgradeChoiceGenerator] WeaponInventory is NULL!");
            }
        }

        if (player == null)
        {
            player = GameServices.Player;
        }
    }

    /// <summary>
    /// Generate random upgrade choices for level-up
    /// </summary>
    public List<UpgradeChoice> GenerateChoices()
    {
        EnsureReferences();

        List<UpgradeChoice> choices = new List<UpgradeChoice>();
        List<UpgradeChoice> pool = BuildChoicePool();

        if (pool.Count == 0)
        {
            DebugLog.Warning("[UpgradeChoiceGenerator] No choices available!");
            return choices;
        }

        DebugLog.Info($"[UpgradeChoiceGenerator] Pool size: {pool.Count} choices available");

        int attemptsLeft = 100;
        while (choices.Count < choicesPerLevelUp && pool.Count > 0 && attemptsLeft > 0)
        {
            UpgradeChoice choice = SelectWeightedChoice(pool);

            if (allowDuplicates || !IsDuplicateChoice(choice, choices))
            {
                choices.Add(choice);
                DebugLog.Info($"[UpgradeChoiceGenerator] Selected: {choice.DisplayName} ({choice.Type})");
                if (!allowDuplicates)
                    pool.Remove(choice);
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

        // Get owned weapon names
        HashSet<string> ownedWeaponNames = new HashSet<string>();

        if (weaponInventory != null)
        {
            var weapons = weaponInventory.GetWeapons();
            foreach (var weapon in weapons)
            {
                if (weapon != null)
                {
                    ownedWeaponNames.Add(weapon.WeaponName);
                    DebugLog.Verbose($"[UpgradeChoiceGenerator] Player owns: '{weapon.WeaponName}'");
                }
            }
        }

        // Add new weapon choices from WeaponDefinitionDatabase
        if (weaponInventory != null && weaponInventory.HasSpace())
        {
            var db = GameServices.WeaponDefinitionDatabase;
            if (db != null)
            {
                var basicWeapons = db.GetBasicWeapons();
                foreach (var weaponDef in basicWeapons)
                {
                    // Skip if player already owns this weapon
                    if (ownedWeaponNames.Contains(weaponDef.displayName))
                        continue;

                    var choice = new UpgradeChoice(
                        UpgradeChoice.ChoiceType.NewWeapon,
                        weaponDef.displayName,
                        weaponDef.description
                    );
                    choice.WeaponType = weaponDef.weaponId;
                    pool.Add(choice);
                }
            }
        }

        // Add weapon upgrade choices for weapons that can level up
        if (weaponInventory != null)
        {
            var weapons = weaponInventory.GetWeapons();
            foreach (var weapon in weapons)
            {
                if (weapon != null && weapon.CanLevelUp)
                {
                    int nextLevel = weapon.Level + 1;
                    string bonusText = GetLevelBonusDescription(weapon, nextLevel);

                    var choice = new UpgradeChoice(
                        UpgradeChoice.ChoiceType.WeaponUpgrade,
                        $"{weapon.WeaponName} Lv.{nextLevel}",
                        $"Level up {weapon.WeaponName}\n+20% damage, +10% attack speed{bonusText}"
                    );
                    choice.WeaponToUpgrade = weapon;
                    pool.Add(choice);
                }
            }
        }

        // Add weapon combination choices
        CombinationManager combinationManager = GameServices.CombinationManager;
        if (combinationManager != null)
        {
            var availableCombinations = combinationManager.GetAvailableCombinations();
            foreach (var (combo, weapon1, weapon2) in availableCombinations)
            {
                var choice = new UpgradeChoice(
                    UpgradeChoice.ChoiceType.WeaponCombination,
                    combo.combinationName,
                    $"Combine {weapon1.WeaponName} + {weapon2.WeaponName}\n{combo.description}"
                );
                choice.CombinationRecipe = combo;
                choice.Weapon1 = weapon1;
                choice.Weapon2 = weapon2;
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
            pool.Add(choice);
        }

        DebugLog.Info($"[UpgradeChoiceGenerator] Total pool size: {pool.Count} choices");
        return pool;
    }

    private UpgradeChoice SelectWeightedChoice(List<UpgradeChoice> pool)
    {
        if (pool.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var choice in pool)
        {
            totalWeight += GetChoiceWeight(choice);
        }

        // Use System.Random with time-based seed instead of UnityEngine.Random
        // Unity's Random can produce deterministic results when game is paused
        var sysRandom = new System.Random(Environment.TickCount);
        float randomPoint = (float)(sysRandom.NextDouble() * totalWeight);
        float currentWeight = 0f;

        foreach (var choice in pool)
        {
            currentWeight += GetChoiceWeight(choice);
            if (randomPoint <= currentWeight)
            {
                return choice;
            }
        }

        return pool[pool.Count - 1];
    }

    private float GetChoiceWeight(UpgradeChoice choice)
    {
        return choice.Type switch
        {
            UpgradeChoice.ChoiceType.NewWeapon => newWeaponWeight,
            UpgradeChoice.ChoiceType.WeaponUpgrade => weaponUpgradeWeight,
            UpgradeChoice.ChoiceType.PlayerStat => playerStatWeight,
            UpgradeChoice.ChoiceType.WeaponCombination => combinationWeight,
            _ => 1f
        };
    }

    private bool IsDuplicateChoice(UpgradeChoice newChoice, List<UpgradeChoice> existing)
    {
        foreach (var choice in existing)
        {
            if (choice.Type != newChoice.Type) continue;

            if (choice.Type == UpgradeChoice.ChoiceType.NewWeapon)
            {
                if (choice.WeaponType == newChoice.WeaponType)
                    return true;
            }
            else if (choice.Type == UpgradeChoice.ChoiceType.WeaponUpgrade)
            {
                if (choice.WeaponToUpgrade != null && newChoice.WeaponToUpgrade != null &&
                    choice.WeaponToUpgrade == newChoice.WeaponToUpgrade)
                    return true;
            }
            else if (choice.Type == UpgradeChoice.ChoiceType.PlayerStat)
            {
                if (choice.StatType == newChoice.StatType)
                    return true;
            }
            else if (choice.Type == UpgradeChoice.ChoiceType.WeaponCombination)
            {
                if (choice.CombinationRecipe != null && newChoice.CombinationRecipe != null &&
                    choice.CombinationRecipe.combinationName == newChoice.CombinationRecipe.combinationName)
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
                if (choice.WeaponToUpgrade != null && choice.WeaponToUpgrade.CanLevelUp)
                {
                    choice.WeaponToUpgrade.LevelUp();
                    DebugLog.Info($"[Upgrade] Leveled up weapon: {choice.WeaponToUpgrade.WeaponName} to Lv.{choice.WeaponToUpgrade.Level}");
                }
                else
                {
                    DebugLog.Warning("[Upgrade] Weapon upgrade failed - weapon is null or already max level");
                }
                break;

            case UpgradeChoice.ChoiceType.PlayerStat:
                ApplyStatUpgrade(choice.StatType);
                DebugLog.Info($"[Upgrade] Upgraded stat: {choice.DisplayName}");
                break;

            case UpgradeChoice.ChoiceType.WeaponCombination:
                CombinationManager combinationManager = GameServices.CombinationManager;
                if (combinationManager != null && choice.CombinationRecipe != null)
                {
                    bool success = combinationManager.CombineWeapons(
                        choice.CombinationRecipe,
                        choice.Weapon1,
                        choice.Weapon2
                    );
                    if (success)
                    {
                        DebugLog.Info($"[Upgrade] Combined weapons: {choice.DisplayName}");
                    }
                    else
                    {
                        DebugLog.Error($"[Upgrade] Failed to combine weapons: {choice.DisplayName}");
                    }
                }
                break;
        }
    }

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
            case PlayerStatType.Lifesteal:
                player.AddLifestealChance(value);
                break;
        }
    }

    public bool CanReroll() => rerollsUsed < maxRerolls;
    public void UseReroll() => rerollsUsed++;
    public void ResetRerolls() => rerollsUsed = 0;
    public int GetRemainingRerolls() => maxRerolls - rerollsUsed;

    /// <summary>
    /// Get description of milestone bonus for reaching a level
    /// </summary>
    private string GetLevelBonusDescription(Weapon weapon, int targetLevel)
    {
        string bonus = "";

        if (targetLevel == 2)
        {
            bonus = "\n<color=#FFD700>★ Unlocks combinations!</color>";
        }
        else if (targetLevel == 3)
        {
            bonus = "\n<color=#00BFFF>+1 projectile</color>\n<color=#FF6600>MAX LEVEL</color>";
        }

        return bonus;
    }
}
