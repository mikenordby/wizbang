using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the player's active weapon loadout.
/// Handles adding, upgrading, and removing weapons.
/// </summary>
public class WeaponInventory : MonoBehaviour
{
    [Header("Weapon Slots")]
    [SerializeField] private int maxWeaponSlots = 6;
    [SerializeField] private List<Weapon> activeWeapons = new List<Weapon>();
    
    private Player player;
    
    private void Awake()
    {
        player = GetComponent<Player>();
    }
    
    private void Start()
    {
        // Start with Magic Missile only
        AddWeapon("MagicMissile");
        
        DebugLog.Info($"[WeaponInventory] Started with {activeWeapons.Count} weapon(s)");
    }
    
    /// <summary>
    /// Add a new weapon to the inventory.
    /// </summary>
    public bool AddWeapon(string weaponType)
    {
        if (activeWeapons.Count >= maxWeaponSlots)
        {
            DebugLog.Warning($"[WeaponInventory] Cannot add weapon - inventory full ({maxWeaponSlots} slots)");
            return false;
        }
        
        // Check if weapon already exists
        Weapon existingWeapon = activeWeapons.Find(w => w.GetType().Name == weaponType);
        if (existingWeapon != null)
        {
            // Upgrade existing weapon instead
            existingWeapon.LevelUp();
            DebugLog.Info($"[WeaponInventory] Upgraded existing {weaponType}");
            return true;
        }
        
        // Create new weapon dynamically
        Weapon weapon = CreateWeapon(weaponType);
        if (weapon == null)
        {
            DebugLog.Warning($"[WeaponInventory] Unknown weapon type: {weaponType}");
            return false;
        }
        
        activeWeapons.Add(weapon);
        DebugLog.Info($"[WeaponInventory] Added {weapon.WeaponName} (slot {activeWeapons.Count}/{maxWeaponSlots})");
        return true;
    }
    
    /// <summary>
    /// Upgrade a weapon by index.
    /// </summary>
    public bool UpgradeWeapon(int index)
    {
        if (index < 0 || index >= activeWeapons.Count)
        {
            DebugLog.Warning($"[WeaponInventory] Invalid weapon index: {index}");
            return false;
        }
        
        Weapon weapon = activeWeapons[index];
        if (weapon.IsMaxLevel)
        {
            DebugLog.Warning($"[WeaponInventory] {weapon.WeaponName} is already max level");
            return false;
        }
        
        weapon.LevelUp();
        return true;
    }
    
    /// <summary>
    /// Upgrade a weapon by name.
    /// </summary>
    public bool UpgradeWeapon(string weaponName)
    {
        Weapon weapon = activeWeapons.Find(w => w.WeaponName == weaponName);
        if (weapon == null)
        {
            DebugLog.Warning($"[WeaponInventory] Weapon not found: {weaponName}");
            return false;
        }
        
        if (weapon.IsMaxLevel)
        {
            DebugLog.Warning($"[WeaponInventory] {weaponName} is already max level");
            return false;
        }
        
        weapon.LevelUp();
        return true;
    }
    
    /// <summary>
    /// Create weapon dynamically by type.
    /// </summary>
    private Weapon CreateWeapon(string weaponType)
    {
        GameObject weaponObj = new GameObject(weaponType);
        weaponObj.transform.SetParent(transform);
        
        Weapon weapon = weaponType switch
        {
            "MagicMissile" => weaponObj.AddComponent<ProjectileWeapon>(),
            "OrbiterWeapon" => weaponObj.AddComponent<OrbiterWeapon>(),
            "BoomerangWeapon" => weaponObj.AddComponent<BoomerangWeapon>(),
            "RapidFireWeapon" => weaponObj.AddComponent<RapidFireWeapon>(),
            _ => null
        };
        
        if (weapon == null)
        {
            Destroy(weaponObj);
        }
        
        return weapon;
    }
    
    /// <summary>
    /// Get list of active weapons.
    /// </summary>
    public List<Weapon> GetActiveWeapons()
    {
        return new List<Weapon>(activeWeapons);
    }
    
    /// <summary>
    /// Check if inventory has space for new weapon.
    /// </summary>
    public bool HasSpace()
    {
        return activeWeapons.Count < maxWeaponSlots;
    }
    
    /// <summary>
    /// Get number of active weapons.
    /// </summary>
    public int GetWeaponCount()
    {
        return activeWeapons.Count;
    }
}
