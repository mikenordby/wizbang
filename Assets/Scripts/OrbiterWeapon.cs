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
        projectileCount = 1; // Start with 1 orbiter, upgrades add more
        projectileSpeed = 0f; // Not used for orbiters (they orbit, not fly)
        projectileSize = 1.5f; // Larger spinning blades
        
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
        
        // DIAGNOSTIC: Log projectile count calculation
        int upgradeBonus = upgrades[WeaponUpgrade.UpgradeType.ProjectileCount].GetProjectileBonus();
        int playerBonus = (player != null) ? player.BonusProjectiles : 0;
        DebugLog.Info($"[OrbiterWeapon.RecalculateStats] projectileCount={projectileCount}, upgradeBonus={upgradeBonus}, playerBonus={playerBonus}, TOTAL currentProjectileCount={currentProjectileCount}");
        
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
        DebugLog.Info($"[OrbiterWeapon.ApplyUpgrade] BEFORE: upgradeType={upgradeType}, currentProjectileCount={currentProjectileCount}");
        
        bool success = base.ApplyUpgrade(upgradeType);
        
        if (success)
        {
            DebugLog.Info($"[OrbiterWeapon.ApplyUpgrade] AFTER: Upgrade successful, currentProjectileCount={currentProjectileCount}");
            
            // ProjectileCount upgrade adds more orbiters
            if (upgradeType == WeaponUpgrade.UpgradeType.ProjectileCount)
            {
                DebugLog.Info($"[OrbiterWeapon] ✓ ProjectileCount upgraded! New count: {currentProjectileCount}");
            }
        }
        else
        {
            DebugLog.Warning($"[OrbiterWeapon.ApplyUpgrade] Upgrade FAILED for {upgradeType}");
        }
        
        return success;
    }
}
