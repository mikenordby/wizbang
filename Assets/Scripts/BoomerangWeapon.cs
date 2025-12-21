using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Boomerang weapon that throws projectiles in an arc that return to player.
/// Boomerangs damage enemies on the way out AND back.
/// Implements IWeaponCollisionHandler for self-managed collision detection.
/// </summary>
public class BoomerangWeapon : Weapon, IWeaponCollisionHandler
{
    [Header("Boomerang Settings")]
    private List<BoomerangProjectile> boomerangPool = new List<BoomerangProjectile>();
    private int poolSize = 20;
    
    protected override void Awake()
    {
        // Set weapon identity BEFORE base.Awake()
        weaponName = "Boomerang";
        baseDamage = 12f;
        baseFireRate = 0.8f; // Slightly slower than Magic Missile
        projectileCount = 1;
        basePierce = 5; // Can hit multiple enemies on the way out AND back
        baseRange = 1.2f;
        projectileSize = 1.5f; // Larger boomerangs
        
        base.Awake();
        
        // Create boomerang pool
        CreateBoomerangPool();
        
        RegisterWithCollisionManager();
        
        DebugLog.Info("[BoomerangWeapon] Initialized with arc throw pattern");
    }
    
    private void CreateBoomerangPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject boomerangObj = new GameObject($"Boomerang_{i}");
            boomerangObj.transform.SetParent(transform);
            boomerangObj.transform.localScale = Vector3.one * 0.8f;
            
            // Add sprite
            SpriteRenderer sr = boomerangObj.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteLoader.LoadBoomerangSprite();
            sr.sortingOrder = 5;
            
            // Add collider
            CircleCollider2D collider = boomerangObj.AddComponent<CircleCollider2D>();
            collider.radius = 0.3f; // Match boomerang blade arc
            collider.isTrigger = true;
            
            // Add boomerang component
            BoomerangProjectile boomerang = boomerangObj.AddComponent<BoomerangProjectile>();
            boomerangObj.SetActive(false);
            
            boomerangPool.Add(boomerang);
        }
        
        DebugLog.Info($"[BoomerangWeapon] Created pool of {poolSize} boomerangs");
    }
    
    private BoomerangProjectile GetBoomerang()
    {
        foreach (var boomerang in boomerangPool)
        {
            if (!boomerang.IsActive)
            {
                boomerang.gameObject.SetActive(true);
                return boomerang;
            }
        }
        
        DebugLog.Warning("[BoomerangWeapon] Pool exhausted!");
        return null;
    }
    
    protected override void Fire()
    {
        // Get spread angle based on projectile count
        float spreadAngle = currentProjectileCount > 1 ? 30f : 0f;
        float angleStep = currentProjectileCount > 1 ? spreadAngle / (currentProjectileCount - 1) : 0f;
        float startAngle = -spreadAngle / 2f;
        
        for (int i = 0; i < currentProjectileCount; i++)
        {
            BoomerangProjectile boomerang = GetBoomerang();
            if (boomerang == null) continue;
            
            // Calculate throw direction with spread
            float angle = startAngle + (angleStep * i);
            Vector3 targetDir = FindTargetDirection(angle);
            
            // Activate boomerang with arc motion
            boomerang.ActivateArc(playerTransform.position, targetDir, playerTransform);
            float finalSize = currentProjectileSize * (player != null ? player.ProjectileSizeMultiplier : 1f);
            boomerang.SetStats(currentDamage, currentPierce, DamageType.Physical, finalSize);
        }
        
        DebugLog.Verbose($"[BoomerangWeapon] Fired {currentProjectileCount} boomerang(s)");
    }
    
    private Vector3 FindTargetDirection(float angleOffset)
    {
        // Find nearest enemy
        Transform nearestEnemy = FindNearestEnemy();
        
        if (nearestEnemy != null)
        {
            Vector3 toEnemy = (nearestEnemy.position - playerTransform.position).normalized;
            // Apply angle offset
            float angle = Mathf.Atan2(toEnemy.y, toEnemy.x) * Mathf.Rad2Deg + angleOffset;
            float radians = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f);
        }
        
        // No enemy, throw in random direction with offset
        float randomAngle = Random.Range(0f, 360f) + angleOffset;
        float randomRadians = randomAngle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(randomRadians), Mathf.Sin(randomRadians), 0f);
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
    
    /// <summary>
    /// Get all active boomerangs for collision detection
    /// </summary>
    public List<BoomerangProjectile> GetActiveBoomerangs()
    {
        List<BoomerangProjectile> active = new List<BoomerangProjectile>();
        foreach (var boomerang in boomerangPool)
        {
            if (boomerang != null && boomerang.IsActive)
            {
                active.Add(boomerang);
            }
        }
        return active;
    }    
    #region IWeaponCollisionHandler Implementation
    
    /// <summary>
    /// Check collisions for all active boomerangs.
    /// Boomerangs damage enemies on both outbound and return trips.
    /// </summary>
    public void CheckCollisions(SpatialHashGrid grid, EnemyPool enemyPool)
    {
        if (grid == null || enemyPool == null) return;
        
        var activeBoomerangs = GetActiveBoomerangs();
        if (activeBoomerangs.Count == 0) return;
        
        foreach (var boomerang in activeBoomerangs)
        {
            if (!boomerang.IsActive) continue;
            
            // Query spatial grid for nearby enemies
            var nearbyEntities = grid.Query(
                boomerang.Position,
                boomerang.CollisionRadius,
                CollisionLayer.Enemy
            );
            
            foreach (var entity in nearbyEntities)
            {
                if (entity is Enemy enemy && enemy.gameObject.activeInHierarchy)
                {
                    float distance = UnityEngine.Vector3.Distance(boomerang.Position, enemy.Position);
                    float combinedRadius = boomerang.CollisionRadius + enemy.CollisionRadius;
                    
                    if (distance < combinedRadius)
                    {
                        // Check if boomerang has already hit this enemy
                        int enemyID = enemy.gameObject.GetInstanceID();
                        bool shouldDeactivate = boomerang.RegisterHit(enemyID);
                        
                        if (!shouldDeactivate) // RegisterHit returns false if already hit - new hit occurred
                        {
                            Health enemyHealth = enemy.GetComponent<Health>();
                            if (enemyHealth != null)
                            {
                                // Calculate damage with crits
                                DamageContext context = new DamageContext
                                {
                                    baseDamage = boomerang.Damage,
                                    player = GameServices.Player,
                                    enemy = enemy,
                                    damageType = boomerang.DamageType
                                };
                                
                                DamageResult result = DamageCalculator.Instance.CalculateDamage(context);
                                enemyHealth.TakeDamage(result.finalDamage);
                                
                                // Show damage number
                                DamageNumberPool damagePool = GameServices.DamageNumberPool;
                                if (damagePool != null)
                                {
                                    if (result.isCritical)
                                        damagePool.ShowCriticalDamage(enemy.Position, result.finalDamage);
                                    else
                                        damagePool.ShowDamage(enemy.Position, result.finalDamage);
                                }
                                
                                DebugLog.Verbose($"[BoomerangWeapon] Hit {enemy.name} for {result.finalDamage:F1} damage (crit={result.isCritical})");
                            }
                        }
                        
                        // Check if boomerang should deactivate after exceeding pierce
                        if (boomerang.EnemiesHit > boomerang.Pierce)
                        {
                            boomerang.Deactivate();
                            DebugLog.Verbose($"[BoomerangWeapon] Deactivated after hitting {boomerang.EnemiesHit} enemies (pierce={boomerang.Pierce})");
                            break; // Stop checking this boomerang
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
