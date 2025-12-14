using UnityEngine;

/// <summary>
/// Represents a single upgrade that can be applied to a weapon
/// </summary>
[System.Serializable]
public class WeaponUpgrade
{
    public enum UpgradeType
    {
        Damage,          // Increase base damage
        ProjectileCount, // Add projectiles
        FireRate,        // Reduce cooldown (increase fire rate)
        Pierce,          // Projectiles pierce through enemies
        Range            // Increase projectile lifetime/range
    }
    
    public UpgradeType type;
    public int currentLevel;
    public int maxLevel;
    
    public WeaponUpgrade(UpgradeType upgradeType, int max = 5)
    {
        type = upgradeType;
        currentLevel = 0;
        maxLevel = max;
    }
    
    public bool CanUpgrade => currentLevel < maxLevel;
    
    public string GetDescription()
    {
        switch (type)
        {
            case UpgradeType.Damage:
                return $"+{GetDamageBonus():F0}% Damage (Lv {currentLevel}/{maxLevel})";
            case UpgradeType.ProjectileCount:
                return $"+{currentLevel} Projectiles (Lv {currentLevel}/{maxLevel})";
            case UpgradeType.FireRate:
                return $"+{GetFireRateBonus():F0}% Fire Rate (Lv {currentLevel}/{maxLevel})";
            case UpgradeType.Pierce:
                return $"+{currentLevel} Pierce (Lv {currentLevel}/{maxLevel})";
            case UpgradeType.Range:
                return $"+{GetRangeBonus():F0}% Range (Lv {currentLevel}/{maxLevel})";
            default:
                return "Unknown Upgrade";
        }
    }
    
    public string GetNextLevelPreview()
    {
        if (!CanUpgrade) return "MAX";
        
        switch (type)
        {
            case UpgradeType.Damage:
                return $"+25% Damage";
            case UpgradeType.ProjectileCount:
                return $"+1 Projectile";
            case UpgradeType.FireRate:
                return $"+20% Fire Rate";
            case UpgradeType.Pierce:
                return $"+1 Pierce";
            case UpgradeType.Range:
                return $"+30% Range";
            default:
                return "Unknown";
        }
    }
    
    // Calculate actual bonus values
    public float GetDamageBonus() => currentLevel * 25f; // 25% per level
    public int GetProjectileBonus() => currentLevel;
    public float GetFireRateBonus() => currentLevel * 20f; // 20% per level
    public int GetPierceBonus() => currentLevel;
    public float GetRangeBonus() => currentLevel * 30f; // 30% per level
}
