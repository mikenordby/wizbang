using UnityEngine;

/// <summary>
/// Rapid-fire pistol weapon that shoots at nearest enemy at high speed.
/// Low damage, high fire rate - spray and pray!
/// Implements IWeaponCollisionHandler for self-managed collision detection.
/// </summary>
public class RapidFireWeapon : Weapon, IWeaponCollisionHandler
{
    [Header("Rapid Fire Settings")]
    [Tooltip("Spread angle in degrees for bullet spread")]
    [SerializeField] private float spreadAngle = 8f;
    
    private ProjectilePool projectilePool;
    
    protected override void Awake()
    {
        // Set weapon identity BEFORE base.Awake()
        weaponName = "Rapid Fire Pistol";
        baseDamage = 5f; // LOW damage per shot
        baseFireRate = 4f; // MUCH faster fire rate (4 shots per second)
        projectileCount = 1;
        basePierce = 0; // No pierce by default
        baseRange = 0.8f; // Shorter range
        projectileSize = 0.7f; // Smaller bullets
        
        base.Awake();
        
        projectilePool = GameServices.ProjectilePool;
        
        RegisterWithCollisionManager();
        
        DebugLog.Info("[RapidFireWeapon] Initialized - spray and pray!");
    }
    
    protected override void Fire()
    {
        if (projectilePool == null) return;
        
        // Find nearest enemy for auto-aim
        Transform nearestEnemy = FindNearestEnemy();
        Vector3 baseDirection;
        
        if (nearestEnemy != null)
        {
            baseDirection = (nearestEnemy.position - playerTransform.position).normalized;
        }
        else
        {
            // No enemy, shoot in random direction
            float randomAngle = Random.Range(0f, 360f);
            float rad = randomAngle * Mathf.Deg2Rad;
            baseDirection = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        }
        
        // Spawn bullets with slight spread
        float angleStep = currentProjectileCount > 1 ? spreadAngle / (currentProjectileCount - 1) : 0f;
        float startAngle = -spreadAngle / 2f;
        
        for (int i = 0; i < currentProjectileCount; i++)
        {
            Projectile bullet = projectilePool.GetProjectile();
            if (bullet == null) continue;
            
            // Apply spread
            float angleOffset = startAngle + (angleStep * i);
            float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
            float finalAngle = (baseAngle + angleOffset) * Mathf.Deg2Rad;
            Vector3 shootDir = new Vector3(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle), 0f);
            
            // Spawn bullet
            bullet.transform.position = playerTransform.position;
            bullet.ActivateStraight(playerTransform.position, shootDir);
            
            float finalSize = currentProjectileSize * (player != null ? player.ProjectileSizeMultiplier : 1f);
            bullet.SetStats(currentDamage, currentPierce, DamageType.Physical, finalSize);
            
            // Make bullets yellow
            SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = SpriteLoader.LoadProjectileSprite("fireball");
                sr.color = Color.yellow;
            }
        }
        
        DebugLog.Verbose($"[RapidFireWeapon] Fired {currentProjectileCount} bullet(s)");
    }
    
    private Transform FindNearestEnemy()
    {
        EnemyPool enemyPool = GameServices.EnemyPool;
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
        
        return nearest != null ? nearest.transform : null;
    }
    
    #region IWeaponCollisionHandler Implementation
    
    /// <summary>
    /// RapidFireWeapon shares the projectile pool with ProjectileWeapon.
    /// We register but don't do collision checks here to avoid duplicate checks.
    /// ProjectileWeapon handles collision detection for the shared pool.
    /// </summary>
    public void CheckCollisions(SpatialHashGrid grid, EnemyPool enemyPool)
    {
        // No-op: Collision detection handled by ProjectileWeapon for shared pool
    }
    
    /// <summary>
    /// Whether this weapon is active and should check collisions
    /// </summary>
    bool IWeaponCollisionHandler.IsActive => false; // Disable to avoid duplicate checks
    
    #endregion
}
