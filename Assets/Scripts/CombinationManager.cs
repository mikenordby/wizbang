using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages weapon combination recipes and validates possible combinations.
/// Discovers available combinations based on player's current weapons.
/// </summary>
public class CombinationManager : MonoBehaviour
{
    [Header("Combination Database")]
    [SerializeField] private List<WeaponCombination> availableCombinations = new List<WeaponCombination>();

    [Header("Unlocks")]
    [Tooltip("Combinations unlocked through metaprogression")]
    private HashSet<string> unlockedCombinations = new HashSet<string>();

    private WeaponInventory weaponInventory;

    void Awake()
    {
        // Register with GameServices
        GameServices.RegisterCombinationManager(this);

        // Load all WeaponCombination assets from Resources
        LoadCombinationDatabase();
    }

    void Start()
    {
        weaponInventory = GameServices.Player?.GetComponent<WeaponInventory>();
        if (weaponInventory == null)
        {
            DebugLog.Warning("[CombinationManager] WeaponInventory not found");
        }
    }

    /// <summary>
    /// Load all WeaponCombination ScriptableObjects from Resources/Combinations/
    /// </summary>
    private void LoadCombinationDatabase()
    {
        WeaponCombination[] combinations = Resources.LoadAll<WeaponCombination>("Combinations");
        if (combinations.Length == 0)
        {
            DebugLog.Warning("[CombinationManager] No combinations found in Resources/Combinations/");
        }
        else
        {
            availableCombinations.AddRange(combinations);
            DebugLog.Info($"[CombinationManager] Loaded {combinations.Length} combinations");
        }
    }

    /// <summary>
    /// Find all valid combinations the player can make with current weapons.
    /// Returns list of (combination, weapon1, weapon2) tuples.
    /// </summary>
    public List<(WeaponCombination combination, Weapon weapon1, Weapon weapon2)> GetAvailableCombinations()
    {
        List<(WeaponCombination, Weapon, Weapon)> validCombinations = new List<(WeaponCombination, Weapon, Weapon)>();

        if (weaponInventory == null)
            return validCombinations;

        List<Weapon> weapons = weaponInventory.GetWeapons();

        // Check all pairs of weapons
        for (int i = 0; i < weapons.Count; i++)
        {
            for (int j = i + 1; j < weapons.Count; j++)
            {
                Weapon weaponA = weapons[i];
                Weapon weaponB = weapons[j];

                // Get weapon type names (e.g., "ProjectileWeapon")
                string typeA = weaponA.GetType().Name;
                string typeB = weaponB.GetType().Name;

                // Check all combination recipes
                foreach (WeaponCombination combo in availableCombinations)
                {
                    // Check if this combination is unlocked (for metaprogression)
                    if (!IsCombinationUnlocked(combo.combinationName))
                        continue;

                    // Check if weapon types match
                    if (!combo.MatchesWeapons(typeA, typeB))
                        continue;

                    // Check level requirements
                    int levelA = weaponA.WeaponLevel;
                    int levelB = weaponB.WeaponLevel;
                    if (!combo.MeetsLevelRequirements(typeA, levelA, typeB, levelB))
                        continue;

                    // Valid combination found
                    validCombinations.Add((combo, weaponA, weaponB));
                    DebugLog.Info($"[CombinationManager] Found valid combination: {weaponA.WeaponName} + {weaponB.WeaponName} = {combo.combinationName}");
                }
            }
        }

        return validCombinations;
    }

    /// <summary>
    /// Check if a specific combination recipe is unlocked.
    /// For now, all combinations are unlocked by default.
    /// </summary>
    public bool IsCombinationUnlocked(string combinationName)
    {
        // TODO: Implement metaprogression unlock system
        // For MVP, all combinations are unlocked
        return true;
    }

    /// <summary>
    /// Unlock a combination (for metaprogression).
    /// </summary>
    public void UnlockCombination(string combinationName)
    {
        if (unlockedCombinations.Add(combinationName))
        {
            DebugLog.Info($"[CombinationManager] Unlocked combination: {combinationName}");
        }
    }

    /// <summary>
    /// Perform weapon combination.
    /// Removes both parent weapons and adds the combined weapon.
    /// </summary>
    public bool CombineWeapons(WeaponCombination combination, Weapon weapon1, Weapon weapon2)
    {
        if (weaponInventory == null)
        {
            DebugLog.Error("[CombinationManager] Cannot combine: WeaponInventory not found");
            return false;
        }

        // Verify weapons still exist and are valid
        List<Weapon> currentWeapons = weaponInventory.GetWeapons();
        if (!currentWeapons.Contains(weapon1) || !currentWeapons.Contains(weapon2))
        {
            DebugLog.Warning("[CombinationManager] Cannot combine: One or both weapons no longer in inventory");
            return false;
        }

        // Remove parent weapons
        weaponInventory.RemoveWeapon(weapon1);
        weaponInventory.RemoveWeapon(weapon2);

        DebugLog.Info($"[CombinationManager] Removed {weapon1.WeaponName} and {weapon2.WeaponName}");

        // Add combined weapon
        bool success = weaponInventory.AddWeapon(combination.resultWeaponType);

        if (success)
        {
            DebugLog.Info($"[CombinationManager] Successfully combined into {combination.combinationName}");

            // Trigger combination event (for achievements, etc.)
            GameEvents.TriggerWeaponCombined(combination.combinationName);
        }
        else
        {
            DebugLog.Error($"[CombinationManager] Failed to create combined weapon: {combination.resultWeaponType}");

            // CRITICAL: Re-add the original weapons if combination failed
            weaponInventory.AddWeaponDirect(weapon1);
            weaponInventory.AddWeaponDirect(weapon2);
            DebugLog.Warning("[CombinationManager] Rolled back: Re-added original weapons");
        }

        return success;
    }

    /// <summary>
    /// Get combination recipe by name.
    /// </summary>
    public WeaponCombination GetCombinationByName(string combinationName)
    {
        return availableCombinations.FirstOrDefault(c => c.combinationName == combinationName);
    }

    /// <summary>
    /// Get all combination recipes (for UI, etc.).
    /// </summary>
    public List<WeaponCombination> GetAllCombinations()
    {
        return new List<WeaponCombination>(availableCombinations);
    }
}
