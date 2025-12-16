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
        // Starting weapon is now added by Player.InitializeWithCharacter() based on selected character
        // No longer adding default ProjectileWeapon here
        
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
        
        // Trigger weapon added event
        GameEvents.TriggerWeaponAdded(weapon);
        
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
        
        // Trigger weapon upgraded event
        GameEvents.TriggerWeaponUpgraded(weapon, weapon.WeaponLevel);
        
        return true;
    }
    
    /// <summary>
    /// Upgrade a weapon by type name.
    /// </summary>
    public bool UpgradeWeapon(string weaponType)
    {
        Weapon weapon = activeWeapons.Find(w => w.GetType().Name == weaponType);
        if (weapon == null)
        {
            DebugLog.Warning($"[WeaponInventory] Weapon type not found: {weaponType}");
            return false;
        }
        
        if (weapon.IsMaxLevel)
        {
            DebugLog.Warning($"[WeaponInventory] {weapon.WeaponName} is already max level");
            return false;
        }
        
        weapon.LevelUp();
        DebugLog.Info($"[WeaponInventory] Upgraded {weapon.WeaponName} to level {weapon.WeaponLevel}");
        return true;
    }
    
    /// <summary>
    /// Check if player has a weapon by type name.
    /// </summary>
    public bool HasWeapon(string weaponType)
    {
        return activeWeapons.Find(w => w.GetType().Name == weaponType) != null;
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
            "ProjectileWeapon" => weaponObj.AddComponent<ProjectileWeapon>(),
            "MagicMissile" => weaponObj.AddComponent<ProjectileWeapon>(),
            "OrbiterWeapon" => weaponObj.AddComponent<OrbiterWeapon>(),
            "BoomerangWeapon" => weaponObj.AddComponent<BoomerangWeapon>(),
            "RapidFireWeapon" => weaponObj.AddComponent<RapidFireWeapon>(),
            "LightningWeapon" => weaponObj.AddComponent<LightningWeapon>(),
            "PoisonWeapon" => weaponObj.AddComponent<PoisonWeapon>(),
            "LaserWeapon" => weaponObj.AddComponent<LaserWeapon>(),
            _ => null
        };
        
        if (weapon == null)
        {
            Destroy(weaponObj);
        }
        else
        {
            weapon.Initialize(transform, player);
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
