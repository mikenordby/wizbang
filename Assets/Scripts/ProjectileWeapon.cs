using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Auto-aim projectile weapon that shoots at nearest enemy.
/// Inherits fire rate and damage from Weapon base class.
/// Implements IWeaponCollisionHandler for self-managed collision detection.
/// </summary>
public class ProjectileWeapon : Weapon, IWeaponCollisionHandler
{
    private ProjectilePool projectilePool;
    private EnemyPool enemyPool;
    
    protected override void Awake()
    {
        // Set weapon identity BEFORE base.Awake() so RecalculateStats() has correct base values
        weaponName = "Magic Missile";
        baseDamage = 10f;
        baseFireRate = 1f;
        projectileSize = 1.2f; // Standard projectile size
        
        base.Awake(); // This calls RecalculateStats()
        
        // Find required pools
        projectilePool = GameServices.ProjectilePool;
        enemyPool = GameServices.EnemyPool;
        
        if (projectilePool == null)
            DebugLog.Warning("[ProjectileWeapon] ProjectilePool not found");
        if (enemyPool == null)
            DebugLog.Warning("[ProjectileWeapon] EnemyPool not found");
    }
    
    protected override void Fire()
    {
        if (projectilePool == null || enemyPool == null || playerTransform == null)
            return;
        
        // Find nearest enemy
        Enemy nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null)
            return; // No targets
        
        Vector3 baseDirection = (nearestEnemy.transform.position - playerTransform.position).normalized;
        
        // Fire multiple projectiles with spread
        for (int i = 0; i < currentProjectileCount; i++)
        {
            // Calculate spread angle for this projectile
            float angleOffset = 0f;
            if (currentProjectileCount > 1)
            {
                float spreadAngle = 15f; // Degrees of spread between projectiles
                float totalSpread = spreadAngle * (currentProjectileCount - 1);
                angleOffset = -totalSpread / 2f + (spreadAngle * i);
            }
            
            // Rotate direction by angle offset
            Quaternion rotation = Quaternion.Euler(0, 0, angleOffset);
            Vector3 direction = rotation * baseDirection;
            
            // Spawn projectile
            Projectile projectile = projectilePool.GetProjectile();
            if (projectile != null)
            {
                float finalSize = currentProjectileSize * (player != null ? player.ProjectileSizeMultiplier : 1f);
                DebugLog.Info($"[ProjectileWeapon.Fire] Setting projectile stats: damage={currentDamage:F1} pierce={currentPierce} size={finalSize:F2}");
                projectile.SetStats(currentDamage, currentPierce, DamageType.Physical, finalSize);
                projectile.ActivateStraight(playerTransform.position, direction);
            }
            else
            {
                DebugLog.Warning($"[ProjectileWeapon.Fire] Failed to get projectile from pool!");
            }
        }
    }
    
    private Enemy FindNearestEnemy()
    {
        if (enemyPool == null) return null;
        
        var enemies = enemyPool.GetActiveEnemies();
        Enemy nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.IsActive) continue;
            
            float distance = Vector3.Distance(playerTransform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }
        
        return nearest;
    }
    
    #region IWeaponCollisionHandler Implementation
    
    /// <summary>
    /// Check collisions for projectiles from this weapon.
    /// Note: All ProjectileWeapon and RapidFireWeapon share the same pool,
    /// so we check ALL projectiles, not just ones from this specific weapon.
    /// </summary>
    public void CheckCollisions(SpatialHashGrid grid, EnemyPool enemyPool)
    {
        if (grid == null || enemyPool == null || projectilePool == null) return;
        
        // Get all active projectiles (shared pool)
        List<Projectile> activeProjectiles = new List<Projectile>(projectilePool.GetActiveProjectiles());
        
        foreach (var projectile in activeProjectiles)
        {
            if (!projectile.IsActive) continue;
            
            // Query spatial grid for nearby enemies
            var nearbyEntities = grid.Query(
                projectile.Position,
                projectile.CollisionRadius,
                CollisionLayer.Enemy
            );
            
            foreach (var entity in nearbyEntities)
            {
                if (entity is Enemy enemy && enemy.gameObject.activeInHierarchy)
                {
                    float distance = UnityEngine.Vector3.Distance(projectile.Position, enemy.Position);
                    float combinedRadius = projectile.CollisionRadius + enemy.CollisionRadius;
                    
                    if (distance < combinedRadius)
                    {
                        int enemyID = enemy.gameObject.GetInstanceID();
                        if (projectile.RegisterHit(enemyID))
                        {
                            Health enemyHealth = enemy.GetComponent<Health>();
                            if (enemyHealth != null)
                            {
                                DamageContext context = new DamageContext
                                {
                                    baseDamage = projectile.Damage,
                                    player = GameServices.Player,
                                    enemy = enemy,
                                    damageType = projectile.DamageType
                                };
                                
                                DamageResult result = DamageCalculator.Instance.CalculateDamage(context);
                                enemyHealth.TakeDamage(result.finalDamage);
                                
                                DamageNumberPool damagePool = GameServices.DamageNumberPool;
                                if (damagePool != null)
                                {
                                    if (result.isCritical)
                                        damagePool.ShowCriticalDamage(enemy.Position, result.finalDamage);
                                    else
                                        damagePool.ShowDamage(enemy.Position, result.finalDamage);
                                }
                            }
                            
                            if (projectile.EnemiesHit > projectile.Pierce)
                            {
                                projectile.Deactivate();
                                break;
                            }
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
