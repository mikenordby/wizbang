using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the player's active weapon loadout.
/// Uses data-driven GenericWeapon system with WeaponDefinition ScriptableObjects.
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
        DebugLog.Info($"[WeaponInventory] Started with {activeWeapons.Count} weapon(s)");
    }

    /// <summary>
    /// Add a new weapon to the inventory by weapon ID.
    /// Supports both new weapon IDs (e.g., "magic_missile") and legacy type names (e.g., "ProjectileWeapon").
    /// </summary>
    public bool AddWeapon(string weaponId)
    {
        DebugLog.Verbose($"[WeaponInventory] AddWeapon called with: {weaponId}");

        if (activeWeapons.Count >= maxWeaponSlots)
        {
            DebugLog.Warning($"[WeaponInventory] Cannot add weapon - inventory full ({maxWeaponSlots} slots)");
            return false;
        }

        var db = GameServices.WeaponDefinitionDatabase;
        if (db == null)
        {
            DebugLog.Error("[WeaponInventory] WeaponDefinitionDatabase not found!");
            return false;
        }

        // Try by ID first, then by legacy type name
        WeaponDefinition def = db.GetByID(weaponId) ?? db.GetByLegacyType(weaponId);
        if (def == null)
        {
            DebugLog.Error($"[WeaponInventory] Unknown weapon: {weaponId}");
            return false;
        }

        return AddWeaponFromDefinition(def);
    }

    /// <summary>
    /// Add a weapon from a WeaponDefinition.
    /// </summary>
    public bool AddWeaponFromDefinition(WeaponDefinition definition)
    {
        if (definition == null)
        {
            DebugLog.Warning("[WeaponInventory] Cannot add weapon: null definition");
            return false;
        }

        if (activeWeapons.Count >= maxWeaponSlots)
        {
            DebugLog.Warning($"[WeaponInventory] Cannot add weapon - inventory full ({maxWeaponSlots} slots)");
            return false;
        }

        // Check for duplicates by weapon ID
        Weapon existingWeapon = activeWeapons.Find(w =>
            w is GenericWeapon gw && gw.Definition?.weaponId == definition.weaponId);
        if (existingWeapon != null)
        {
            DebugLog.Warning($"[WeaponInventory] Already have {definition.displayName} - cannot add duplicates");
            return false;
        }

        // Create GenericWeapon
        GameObject weaponObj = new GameObject(definition.displayName);
        weaponObj.transform.SetParent(transform);

        GenericWeapon weapon = weaponObj.AddComponent<GenericWeapon>();
        weapon.Initialize(transform, player, definition);

        activeWeapons.Add(weapon);
        DebugLog.Info($"[WeaponInventory] Added {weapon.WeaponName} (slot {activeWeapons.Count}/{maxWeaponSlots})");

        // Trigger weapon added event
        GameEvents.TriggerWeaponAdded(weapon);

        // Recalculate synergies
        GameServices.SynergyManager?.RecalculateSynergies();

        return true;
    }

    /// <summary>
    /// Check if player has a weapon by ID or display name.
    /// </summary>
    public bool HasWeapon(string weaponId)
    {
        return activeWeapons.Find(w =>
            (w is GenericWeapon gw && gw.Definition?.weaponId == weaponId) ||
            w.WeaponName == weaponId) != null;
    }

    /// <summary>
    /// Get list of active weapons (returns copy).
    /// </summary>
    public List<Weapon> GetActiveWeapons()
    {
        return new List<Weapon>(activeWeapons);
    }

    /// <summary>
    /// Get list of weapons (direct reference, used by SynergyManager).
    /// </summary>
    public List<Weapon> GetWeapons()
    {
        return activeWeapons;
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

    /// <summary>
    /// Remove a weapon from the inventory (used for combinations).
    /// </summary>
    public bool RemoveWeapon(Weapon weapon)
    {
        if (weapon == null || !activeWeapons.Contains(weapon))
        {
            DebugLog.Warning("[WeaponInventory] Cannot remove weapon: not in inventory");
            return false;
        }

        activeWeapons.Remove(weapon);
        DebugLog.Info($"[WeaponInventory] Removed {weapon.WeaponName}");

        // Trigger weapon removed event
        GameEvents.TriggerWeaponRemoved(weapon.WeaponName);

        // Destroy weapon GameObject
        Destroy(weapon.gameObject);

        // Recalculate synergies
        GameServices.SynergyManager?.RecalculateSynergies();

        return true;
    }

    /// <summary>
    /// Add a weapon instance directly to the inventory (used for rollback).
    /// Does NOT create a new weapon, just adds existing one.
    /// </summary>
    public bool AddWeaponDirect(Weapon weapon)
    {
        if (weapon == null)
        {
            DebugLog.Warning("[WeaponInventory] Cannot add null weapon");
            return false;
        }

        if (activeWeapons.Count >= maxWeaponSlots)
        {
            DebugLog.Warning($"[WeaponInventory] Cannot add weapon - inventory full ({maxWeaponSlots} slots)");
            return false;
        }

        activeWeapons.Add(weapon);
        DebugLog.Info($"[WeaponInventory] Added {weapon.WeaponName} directly (slot {activeWeapons.Count}/{maxWeaponSlots})");

        // Trigger weapon added event
        GameEvents.TriggerWeaponAdded(weapon);

        // Recalculate synergies
        GameServices.SynergyManager?.RecalculateSynergies();

        return true;
    }

    /// <summary>
    /// Add a combined weapon with stat/tag inheritance from parent weapons.
    /// </summary>
    public bool AddWeaponWithInheritance(string weaponId, WeaponInheritanceData inheritanceData)
    {
        DebugLog.Info($"[WeaponInventory] AddWeaponWithInheritance called: {weaponId}");

        if (activeWeapons.Count >= maxWeaponSlots)
        {
            DebugLog.Warning($"[WeaponInventory] Cannot add weapon - inventory full ({maxWeaponSlots} slots)");
            return false;
        }

        var db = GameServices.WeaponDefinitionDatabase;
        if (db == null)
        {
            DebugLog.Error("[WeaponInventory] WeaponDefinitionDatabase not found!");
            return false;
        }

        WeaponDefinition def = db.GetByID(weaponId) ?? db.GetByLegacyType(weaponId);
        if (def == null)
        {
            DebugLog.Error($"[WeaponInventory] Unknown weapon: {weaponId}");
            return false;
        }

        return AddWeaponFromDefinitionWithInheritance(def, inheritanceData);
    }

    /// <summary>
    /// Add a combined weapon from a definition with inheritance data.
    /// </summary>
    public bool AddWeaponFromDefinitionWithInheritance(WeaponDefinition definition, WeaponInheritanceData inheritanceData)
    {
        if (definition == null)
        {
            DebugLog.Warning("[WeaponInventory] Cannot add weapon: null definition");
            return false;
        }

        if (activeWeapons.Count >= maxWeaponSlots)
        {
            DebugLog.Warning($"[WeaponInventory] Cannot add weapon - inventory full ({maxWeaponSlots} slots)");
            return false;
        }

        // Create GenericWeapon with inheritance
        GameObject weaponObj = new GameObject(definition.displayName);
        weaponObj.transform.SetParent(transform);

        GenericWeapon weapon = weaponObj.AddComponent<GenericWeapon>();
        weapon.InitializeWithInheritance(transform, player, definition, inheritanceData);

        activeWeapons.Add(weapon);
        DebugLog.Info($"[WeaponInventory] Added combined weapon {weapon.WeaponName} (slot {activeWeapons.Count}/{maxWeaponSlots})");

        // Trigger weapon added event
        GameEvents.TriggerWeaponAdded(weapon);

        // Recalculate synergies
        GameServices.SynergyManager?.RecalculateSynergies();

        return true;
    }
}
