using UnityEngine;

/// <summary>
/// ScriptableObject defining a playable character's stats and starting configuration.
/// Create instances: Assets > Create > Wizbang > Character Data
/// </summary>
[CreateAssetMenu(fileName = "NewCharacter", menuName = "Wizbang/Character Data", order = 0)]
public class CharacterData : ScriptableObject
{
    [Header("Identity")]
    public string characterName = "Wizard";
    
    [TextArea(2, 4)]
    public string description = "Balanced character with standard stats";
    
    [Header("Starting Weapon")]
    [Tooltip("Weapon type this character starts with (e.g., 'ProjectileWeapon', 'OrbiterWeapon')")]
    public string startingWeaponType = "ProjectileWeapon";
    
    [Header("Base Stats")]
    [Tooltip("Starting max health")]
    public float baseMaxHealth = 100f;
    
    [Tooltip("Movement speed modifier (1.0 = default)")]
    public float moveSpeedModifier = 1f;
    
    [Tooltip("Damage multiplier (1.0 = default)")]
    public float damageModifier = 1f;
    
    [Tooltip("Attack speed multiplier (1.0 = default)")]
    public float attackSpeedModifier = 1f;
    
    [Tooltip("Starting crit chance (0-1)")]
    public float startingCritChance = 0f;
    
    [Tooltip("Crit damage multiplier")]
    public float critDamageModifier = 2f;
    
    [Header("Utility Stats")]
    [Tooltip("XP magnet range")]
    public float xpMagnetRange = 2f;
    
    [Tooltip("Pickup radius")]
    public float pickupRadius = 0.5f;
    
    [Tooltip("Health regeneration per second")]
    public float healthRegen = 0f;
    
    [Header("Visual")]
    [Tooltip("Character sprite color tint")]
    public Color characterColor = Color.white;
    
    [Tooltip("Character scale (1.0 = 2 world units)")]
    public float characterScale = 1f;
    
    [Tooltip("Sprite type: 'wizard', 'knight', 'rogue', etc.")]
    public string spriteType = "wizard";
}
