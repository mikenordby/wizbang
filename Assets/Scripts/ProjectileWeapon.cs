using UnityEngine;

/// <summary>
/// Auto-aim projectile weapon that shoots at nearest enemy.
/// Inherits fire rate and damage from Weapon base class.
/// </summary>
public class ProjectileWeapon : Weapon
{
    [Header("Projectile Settings")]
    [SerializeField] private float spreadAngle = 15f; // Degrees between projectiles when count > 1
    
    private ProjectilePool projectilePool;
    private EnemyPool enemyPool;
    
    protected override void Awake()
    {
        // Set weapon identity BEFORE base.Awake() so RecalculateStats() has correct base values
        weaponName = "Magic Missile";
        baseDamage = 10f;
        baseFireRate = 1f;
        
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
                DebugLog.Info($"[ProjectileWeapon.Fire] Setting projectile stats: damage={currentDamage:F1} pierce={currentPierce}");
                projectile.SetStats(currentDamage, currentPierce);
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
}
