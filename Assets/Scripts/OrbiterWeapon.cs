using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Orbiting blades weapon that circles the player.
/// Manages OrbiterManager and applies upgrades to orbiters.
/// Implements IWeaponCollisionHandler for self-managed collision detection.
/// </summary>
public class OrbiterWeapon : Weapon, IWeaponCollisionHandler
{
    private OrbiterManager orbiterManager;
    
    protected override void Awake()
    {
        // Set weapon identity BEFORE base.Awake() so RecalculateStats() has correct base values
        weaponName = "Orbiting Blades";
        baseDamage = 15f;
        baseFireRate = 0f; // Always active, no firing
        projectileCount = 1; // Start with 1 orbiter (knight gets bonus from character data)
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
        
        RegisterWithCollisionManager();
        
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
            orbiterManager.SetOrbitSpeed(8f + (currentFireRate * 0.5f)); // Base 8 rad/sec, faster with upgrades
            orbiterManager.SetOrbitRadius(2f * currentRange); // Range affects orbit radius
            
            float finalSize = currentProjectileSize * (player != null ? player.ProjectileSizeMultiplier : 1f);
            orbiterManager.SetSize(finalSize);
            
            // Hit cooldown inversely proportional to fire rate (higher fire rate = faster hits)
            // baseFireRate=1.0 → 1.0s cooldown, baseFireRate=2.0 → 0.5s cooldown
            float hitCooldown = currentFireRate > 0 ? 1f / currentFireRate : 1f;
            orbiterManager.SetHitCooldown(hitCooldown);
            
            DebugLog.Info($"[OrbiterWeapon.RecalculateStats] fireRate={currentFireRate:F2} → hitCooldown={hitCooldown:F2}s");
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
    
    #region IWeaponCollisionHandler Implementation
    
    /// <summary>
    /// Check collisions for all active orbiters.
    /// </summary>
    public void CheckCollisions(SpatialHashGrid grid, EnemyPool enemyPool)
    {
        if (grid == null || enemyPool == null || orbiterManager == null) return;
        
        List<OrbiterProjectile> activeOrbiters = orbiterManager.GetActiveOrbiters();
        if (activeOrbiters == null || activeOrbiters.Count == 0) return;
        
        foreach (var orbiter in activeOrbiters)
        {
            if (orbiter == null || !orbiter.IsActive) continue;
            
            // Query spatial grid for nearby enemies
            var nearbyEntities = grid.Query(
                orbiter.Position,
                orbiter.CollisionRadius,
                CollisionLayer.Enemy
            );
            
            foreach (var entity in nearbyEntities)
            {
                if (entity is Enemy enemy && enemy.gameObject.activeInHierarchy && enemy.IsActive)
                {
                    // Check if this orbiter can hit this enemy (not on cooldown)
                    int enemyInstanceID = enemy.GetInstanceID();
                    if (!orbiter.CanHitEnemy(enemyInstanceID))
                        continue;
                    
                    float distance = UnityEngine.Vector3.Distance(orbiter.Position, enemy.Position);
                    float combinedRadius = orbiter.CollisionRadius + enemy.CollisionRadius;
                    
                    if (distance < combinedRadius)
                    {
                        Health enemyHealth = enemy.GetComponent<Health>();
                        if (enemyHealth != null)
                        {
                            // Calculate damage with crits
                            DamageContext context = new DamageContext
                            {
                                baseDamage = orbiter.Damage,
                                player = GameServices.Player,
                                enemy = enemy,
                                damageType = DamageType.Physical
                            };
                            
                            DamageResult result = DamageCalculator.Instance.CalculateDamage(context);
                            enemyHealth.TakeDamage(result.finalDamage);
                            
                            // Lifesteal healing (chance to heal 1 HP on hit)
                            if (player != null && player.LifestealChance > 0f && Random.value < player.LifestealChance)
                            {
                                Health playerHealth = player.GetComponent<Health>();
                                if (playerHealth != null && playerHealth.CurrentHealth < playerHealth.MaxHealth)
                                {
                                    playerHealth.Heal(1f);
                                    DebugLog.Info($"[OrbiterWeapon] 💚 Lifesteal! Healed 1 HP (chance={player.LifestealChance*100:F0}%)");
                                }
                            }
                            
                            // Show damage number
                            DamageNumberPool damagePool = GameServices.DamageNumberPool;
                            if (damagePool != null)
                            {
                                if (result.isCritical)
                                    damagePool.ShowCriticalDamage(enemy.Position, result.finalDamage);
                                else
                                    damagePool.ShowDamage(enemy.Position, result.finalDamage);
                            }
                            
                            // Register hit with cooldown
                            orbiter.RegisterHit(enemyInstanceID);
                            DebugLog.Verbose($"[OrbiterWeapon] Hit {enemy.name} for {result.finalDamage:F1} damage");
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Whether this weapon is active and should check collisions
    /// </summary>
    bool IWeaponCollisionHandler.IsActive => gameObject.activeInHierarchy && enabled;
    
    #endregion
}
