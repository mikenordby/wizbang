using UnityEngine;

/// <summary>
/// ScriptableObject defining a playable hero's immutable identity and base stats.
/// This is the "what the hero IS" - never changes during gameplay.
/// Create instances: Assets > Create > Wizbang > Hero Definition
/// </summary>
[CreateAssetMenu(fileName = "NewHero", menuName = "Wizbang/Hero Definition", order = 0)]
public class HeroDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique identifier for this hero (e.g., 'wizard', 'knight')")]
    public string heroId = "wizard";
    
    [Tooltip("Display name shown in UI")]
    public string displayName = "Wizard";
    
    [TextArea(2, 4)]
    [Tooltip("Hero description for character selection screen")]
    public string description = "Balanced character with standard stats";
    
    [Header("Starting Loadout")]
    [Tooltip("Weapon type this hero starts with (e.g., 'ProjectileWeapon', 'OrbiterWeapon')")]
    public string startingWeaponType = "ProjectileWeapon";
    
    [Header("Base Stats (Immutable)")]
    [Tooltip("Base max health (before modifiers)")]
    public float baseMaxHealth = 100f;
    
    [Tooltip("Base movement speed (1.0 = default, before modifiers)")]
    public float baseMoveSpeed = 1f;
    
    [Tooltip("Base damage multiplier (1.0 = default, before bonuses)")]
    public float baseDamage = 1f;
    
    [Tooltip("Base attack speed multiplier (1.0 = default, before bonuses)")]
    public float baseAttackSpeed = 1f;
    
    [Tooltip("Starting crit chance (0-1, before bonuses)")]
    public float baseCritChance = 0f;
    
    [Tooltip("Base crit damage multiplier (2.0 = 200% damage on crit)")]
    public float baseCritDamage = 2f;
    
    [Header("Utility Stats (Immutable)")]
    [Tooltip("Base XP magnet range")]
    public float baseXPMagnetRange = 2f;
    
    [Tooltip("Base pickup radius")]
    public float basePickupRadius = 0.5f;
    
    [Tooltip("Base health regeneration per second")]
    public float baseHealthRegen = 0f;
    
    [Header("Visual (Immutable)")]
    [Tooltip("Hero sprite color tint")]
    public Color characterColor = Color.white;
    
    [Tooltip("Visual scale (1.0 = 2 world units)")]
    public float visualScale = 1f;
    
    [Tooltip("Sprite type for loading assets: 'wizard', 'knight', 'rogue', etc.")]
    public string spriteType = "wizard";
    
    [Header("Hero-Specific Abilities (Future)")]
    [Tooltip("Passive ability unique to this hero")]
    public string passiveAbilityId = "";
    
    [Tooltip("Starting perk for this hero")]
    public string startingPerkId = "";
}

