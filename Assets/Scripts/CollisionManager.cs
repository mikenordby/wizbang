using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Efficient collision detection manager using spatial hash grid.
/// Handles projectile-enemy and player-enemy collisions.
/// Uses spatial partitioning to reduce O(n²) checks to O(n).
/// </summary>
public class CollisionManager : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private OrbiterManager orbiterManager;
    [SerializeField] private float playerCollisionRadius = 0.4f; // Match wizard body size
    
    [Header("Spatial Hash Grid Settings")]
    [Tooltip("Cell size for spatial partitioning (should be ~2x max collision radius)")]
    [SerializeField] private float gridCellSize = 2.0f;
    
    [Tooltip("Show grid debug info in Scene view")]
    [SerializeField] private bool showGridDebug = false;
    
    private float lastPlayerDamageTime = -999f;
    private float playerDamageCooldown = 0.5f;
    
    private bool gameOver = false;
    private Health playerHealth;
    private SpatialHashGrid spatialGrid;

    
    public bool IsGameOver => gameOver;
    
    private void Start()
    {
        // Initialize spatial hash grid
        spatialGrid = new SpatialHashGrid(gridCellSize);
        DebugLog.Info($"[CollisionManager] Initialized spatial hash grid with cell size {gridCellSize}");
        
        // Auto-find OrbiterManager if not assigned
        if (orbiterManager == null)
        {
            orbiterManager = GetComponent<OrbiterManager>();
            if (orbiterManager != null)
            {
                DebugLog.Info("[CollisionManager] Auto-found OrbiterManager on same GameObject");
            }
        }
        
        DebugLog.Info($"[CollisionManager] Starting - orbiterManager={orbiterManager != null}, enemyPool={enemyPool != null}, projectilePool={projectilePool != null}");
        
        if (playerTransform != null)
        {
            playerHealth = playerTransform.GetComponent<Health>();
            if (playerHealth == null)
            {
                playerHealth = playerTransform.gameObject.AddComponent<Health>();
                playerHealth.Initialize(30f); // 3 hits at 10 damage each
            }
            
            // Add physics components for player collision and knockback
            Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.bodyType = RigidbodyType2D.Dynamic;
                playerRb.gravityScale = 0f;
                playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
                playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                playerRb.mass = 1f;
            }
            
            CircleCollider2D playerCollider = playerTransform.GetComponent<CircleCollider2D>();
            if (playerCollider == null)
            {
                playerCollider = playerTransform.gameObject.AddComponent<CircleCollider2D>();
                playerCollider.radius = playerCollisionRadius;
                playerCollider.isTrigger = false;
            }
        }
    }

    
    private void Update()
    {
        if (gameOver || GameState.IsPaused) return;
        
        // Populate spatial hash grid with all entities
        PopulateSpatialGrid();
        
        // Perform collision checks using grid
        CheckProjectileEnemyCollisions();
        CheckOrbiterEnemyCollisions();
        CheckPlayerEnemyCollisions();
    }
    
    /// <summary>
    /// Populate spatial hash grid with all collidable entities
    /// </summary>
    private void PopulateSpatialGrid()
    {
        spatialGrid.Clear();
        
        // Insert all enemies (using cached active list - no GC)
        if (enemyPool != null)
        {
            List<Enemy> activeEnemies = enemyPool.GetActiveEnemies();
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy && enemy.IsActive)
                {
                    spatialGrid.Insert(enemy);
                }
            }
        }
        
        // Note: We don't insert projectiles/orbiters into grid because they query enemies
        // Only enemies need to be in the grid for efficient nearest-neighbor queries
    }
    
    /// <summary>
    /// Check all active projectiles against nearby enemies using spatial hash grid
    /// </summary>
    private void CheckProjectileEnemyCollisions()
    {
        if (projectilePool == null || enemyPool == null) return;
        
        // Create a copy to avoid collection modified exception when Deactivate() removes from list
        List<Projectile> activeProjectiles = new List<Projectile>(projectilePool.GetActiveProjectiles());
        
        foreach (var projectile in activeProjectiles)
        {
            if (!projectile.IsActive)
            {
                DebugLog.Verbose($"[CheckProjectileCollisions] Skipping inactive projectile");
                continue;
            }
            
            DebugLog.Verbose($"[CheckProjectileCollisions] Checking projectile at ({projectile.Position.x:F2},{projectile.Position.y:F2}) damage={projectile.Damage:F1} pierce={projectile.Pierce} hits={projectile.EnemiesHit}");
            
            // Query spatial grid for nearby enemies
            var nearbyEntities = spatialGrid.Query(
                projectile.Position, 
                projectile.CollisionRadius, 
                CollisionLayer.Enemy
            );
            
            foreach (var entity in nearbyEntities)
            {
                if (entity is Enemy enemy && enemy.gameObject.activeInHierarchy)
                {
                    float distance = Vector3.Distance(projectile.Position, enemy.Position);
                    float combinedRadius = projectile.CollisionRadius + enemy.CollisionRadius;
                    
                    DebugLog.Verbose($"[CheckProjectileCollisions] Distance to {enemy.name}: {distance:F3} vs combinedRadius: {combinedRadius:F3}");
                    
                    if (distance < combinedRadius)
                    {
                        // Check if projectile has already hit this enemy (prevents double-hits on consecutive frames)
                        int enemyID = enemy.gameObject.GetInstanceID();
                        if (projectile.RegisterHit(enemyID))
                        {
                            // RegisterHit returns false if already hit this enemy
                            // Only apply damage if this is a new hit
                            Health enemyHealth = enemy.GetComponent<Health>();
                            if (enemyHealth != null)
                            {
                                // Calculate damage with crits and player multipliers
                                DamageContext context = new DamageContext
                                {
                                    baseDamage = projectile.Damage,
                                    player = GameServices.Player,
                                    enemy = enemy,
                                    damageType = projectile.DamageType
                                };
                                
                                DamageResult result = DamageCalculator.Instance.CalculateDamage(context);
                                
                                float healthBefore = enemyHealth.CurrentHealth;
                                bool died = enemyHealth.TakeDamage(result.finalDamage);
                                float healthAfter = enemyHealth.CurrentHealth;
                                
                                string critText = result.isCritical ? " CRIT!" : "";
                                DebugLog.Info($"[PROJECTILE HIT] {enemy.name} - Damage={result.finalDamage:F1}{critText} HP: {healthBefore:F1}→{healthAfter:F1} Died={died} Pierce={projectile.Pierce} Hits={projectile.EnemiesHit}");
                                
                                // Show damage number (gold for crits)
                                DamageNumberPool damagePool = GameServices.DamageNumberPool;
                                if (damagePool != null)
                                {
                                    if (result.isCritical)
                                        damagePool.ShowCriticalDamage(enemy.Position, result.finalDamage);
                                    else
                                        damagePool.ShowDamage(enemy.Position, result.finalDamage);
                                }
                            }
                            
                            // RegisterHit returns true if projectile should be deactivated (pierce exhausted)
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
    /// Check all active orbiters against nearby enemies using spatial hash grid
    /// </summary>
    private void CheckOrbiterEnemyCollisions()
    {
        if (orbiterManager == null || enemyPool == null) return;
        
        List<OrbiterProjectile> activeOrbiters = orbiterManager.GetActiveOrbiters();
        if (activeOrbiters == null || activeOrbiters.Count == 0) return;
        
        foreach (var orbiter in activeOrbiters)
        {
            if (orbiter == null || !orbiter.IsActive) continue;
            
            // Query spatial grid for nearby enemies
            var nearbyEntities = spatialGrid.Query(
                orbiter.Position,
                orbiter.CollisionRadius,
                CollisionLayer.Enemy
            );
            
            foreach (var entity in nearbyEntities)
            {
                if (entity is Enemy enemy && enemy.gameObject.activeInHierarchy && enemy.IsActive)
                {
                    float distance = Vector3.Distance(orbiter.Position, enemy.Position);
                    float combinedRadius = orbiter.CollisionRadius + enemy.CollisionRadius;
                    
                    if (distance < combinedRadius)
                    {
                        Health enemyHealth = enemy.GetComponent<Health>();
                        if (enemyHealth != null && enemyHealth.IsAlive)
                        {
                            DebugLog.Verbose($"[ORBITER HIT] Orbiter collided with enemy! Distance={distance:F3}, CombinedRadius={combinedRadius:F3}");
                            
                            // Calculate damage with crits and player multipliers
                            DamageContext context = new DamageContext
                            {
                                baseDamage = orbiter.Damage,
                                player = GameServices.Player,
                                enemy = enemy,
                                damageType = orbiter.DamageType
                            };
                            
                            DamageResult result = DamageCalculator.Instance.CalculateDamage(context);
                            
                            float healthBefore = enemyHealth.CurrentHealth;
                            bool died = enemyHealth.TakeDamage(result.finalDamage);
                            float healthAfter = enemyHealth.CurrentHealth;
                            
                            string critText = result.isCritical ? " CRIT!" : "";
                            DebugLog.Info($"[ORBITER HIT] {enemy.name} - Damage={result.finalDamage:F1}{critText} HP: {healthBefore:F1}→{healthAfter:F1} Died={died}");
                            
                            // Show damage number (gold for crits)
                            DamageNumberPool damagePool = GameServices.DamageNumberPool;
                            if (damagePool != null)
                            {
                                if (result.isCritical)
                                    damagePool.ShowCriticalDamage(enemy.Position, result.finalDamage);
                                else
                                    damagePool.ShowDamage(enemy.Position, result.finalDamage);
                            }
                        }
                        
                        DebugLog.Verbose($"[ORBITER HIT] Deactivating orbiter for 2 seconds");
                        orbiter.Deactivate();
                        break;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Check if player touched any nearby enemy with cooldown-based damage and knockback
    /// </summary>
    private void CheckPlayerEnemyCollisions()
    {
        if (playerTransform == null || enemyPool == null || playerHealth == null) return;
        
        // Skip if level-up UI is showing
        LevelUpUI levelUpUI = GameServices.LevelUpUI;
        if (levelUpUI != null && levelUpUI.IsShowingUI) return;
        
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        
        // Query spatial grid for nearby enemies
        var nearbyEntities = spatialGrid.Query(
            playerTransform.position,
            playerCollisionRadius,
            CollisionLayer.Enemy
        );
        
        foreach (var entity in nearbyEntities)
        {
            if (entity is Enemy enemy && enemy.gameObject.activeInHierarchy && enemy.IsActive)
            {
                float distance = Vector3.Distance(playerTransform.position, enemy.Position);
                float combinedRadius = playerCollisionRadius + enemy.CollisionRadius;
                
                if (distance < combinedRadius)
                {
                    // Only apply damage if cooldown elapsed
                    if (Time.time - lastPlayerDamageTime >= playerDamageCooldown)
                    {
                        float damage = enemy.ContactDamage;
                        bool died = playerHealth.TakeDamage(damage);
                        lastPlayerDamageTime = Time.time;
                        
                        DebugLog.Info($"Player hit by {enemy.name}! Took {damage} damage, HP: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
                        
                        if (died)
                        {
                            DebugLog.Info("Player DIED!");
                            gameOver = true;
                            Time.timeScale = 0f;
                            return;
                        }
                        
                        // Apply knockback to player
                        if (playerRb != null)
                        {
                            Vector2 knockbackDir = (playerTransform.position - enemy.Position).normalized;
                            playerRb.AddForce(knockbackDir * 250f, ForceMode2D.Impulse);
                            DebugLog.Verbose($"Player knocked back from enemy at {enemy.Position}");
                        }
                    }
                    
                    break; // Only process one enemy collision per frame
                }
            }
        }
    }
    
    /// <summary>
    /// Display game over UI and spatial grid debug info
    /// </summary>
    private void OnGUI()
    {
        if (gameOver)
        {
            // Center screen text
            GUIStyle style = new GUIStyle();
            style.fontSize = 48;
            style.normal.textColor = Color.red;
            style.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "GAME OVER", style);
            
            // Restart instructions
            style.fontSize = 24;
            GUI.Label(new Rect(0, 100, Screen.width, Screen.height), "Press R to Restart", style);
        }
        
        // Show spatial grid stats in top-left
        if (showGridDebug && spatialGrid != null)
        {
            GUIStyle debugStyle = new GUIStyle();
            debugStyle.fontSize = 12;
            debugStyle.normal.textColor = Color.green;
            GUI.Label(new Rect(10, 10, 400, 20), spatialGrid.GetDebugStats(), debugStyle);
        }
    }
    
    /// <summary>
    /// Draw spatial grid in Scene view for debugging
    /// </summary>
    private void OnDrawGizmos()
    {
        if (showGridDebug && spatialGrid != null)
        {
            spatialGrid.DrawGizmos();
        }
    }
}