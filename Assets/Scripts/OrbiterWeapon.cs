using UnityEngine;

/// <summary>
/// Orbiting blades weapon that circles the player.
/// Manages OrbiterManager and applies upgrades to orbiters.
/// </summary>
public class OrbiterWeapon : Weapon
{
    private OrbiterManager orbiterManager;
    
    protected override void Awake()
    {
        // Set weapon identity BEFORE base.Awake() so RecalculateStats() has correct base values
        weaponName = "Orbiting Blades";
        baseDamage = 15f;
        baseFireRate = 0f; // Always active, no firing
        projectileCount = 2; // Start with 2 orbiters
        
        base.Awake(); // This calls RecalculateStats()
        
        // Find or create OrbiterManager
        orbiterManager = FindAnyObjectByType<OrbiterManager>();
        if (orbiterManager == null)
        {
            GameObject managerObj = new GameObject("OrbiterManager");
            orbiterManager = managerObj.AddComponent<OrbiterManager>();
            orbiterManager.transform.SetParent(playerTransform);
            DebugLog.Info("[OrbiterWeapon] Created OrbiterManager");
        }
        
        // Initialize orbiters
        orbiterManager.SetOrbiterCount(projectileCount);
    }
    
    protected override void Fire()
    {
        // Orbiters attack continuously, no firing needed
    }
    
    /// <summary>
    /// Override RecalculateStats to update orbiter properties
    /// </summary>
    protected override void RecalculateStats()
    {
        base.RecalculateStats();
        
        // Update orbiter manager with new stats
        if (orbiterManager != null)
        {
            orbiterManager.SetOrbiterCount(currentProjectileCount);
            orbiterManager.SetDamage(currentDamage);
            orbiterManager.SetOrbitSpeed(1f + (currentFireRate * 0.2f)); // FireRate affects orbit speed
            orbiterManager.SetOrbitRadius(2f * currentRange); // Range affects orbit radius
        }
    }
    
    /// <summary>
    /// Override ApplyUpgrade to handle orbiter-specific upgrades
    /// </summary>
    public override bool ApplyUpgrade(WeaponUpgrade.UpgradeType upgradeType)
    {
        bool success = base.ApplyUpgrade(upgradeType);
        
        if (success)
        {
            // ProjectileCount upgrade adds more orbiters
            if (upgradeType == WeaponUpgrade.UpgradeType.ProjectileCount)
            {
                DebugLog.Info($"[OrbiterWeapon] Increased orbiter count to {currentProjectileCount}");
            }
        }
        
        return success;
    }
}
