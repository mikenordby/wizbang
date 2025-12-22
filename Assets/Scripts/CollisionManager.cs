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
    
    // New weapon registration system
    private List<IWeaponCollisionHandler> registeredWeapons = new List<IWeaponCollisionHandler>();
    
    [Header("Spatial Hash Grid Settings")]
    [Tooltip("Cell size for spatial partitioning (should be ~2x max collision radius)")]
    [SerializeField] private float gridCellSize = 2.0f;
    
    [Tooltip("Show grid debug info in Scene view")]
    [SerializeField] private bool showGridDebug = false;
    
    private float lastPlayerDamageTime = -999f;
    private float playerDamageCooldown = 0.1f; // Reduced from 0.5s - much shorter i-frames
    
    private bool gameOver = false;
    private Health playerHealth;
    private SpatialHashGrid spatialGrid;

    
    public bool IsGameOver => gameOver;
    
    /// <summary>
    /// Query enemies within a radius using spatial hash grid.
    /// Much more efficient than checking all active enemies.
    /// </summary>
    public List<ICollidable> QueryNearbyEnemies(Vector3 position, float radius)
    {
        if (spatialGrid == null)
        {
            return new List<ICollidable>();
        }
        return spatialGrid.Query(position, radius, CollisionLayer.Enemy);
    }
    
    /// <summary>
    /// Register a weapon that handles its own collision detection.
    /// Registered weapons will have CheckCollisions() called each frame.
    /// </summary>
    public void RegisterWeapon(IWeaponCollisionHandler weapon)
    {
        if (weapon != null && !registeredWeapons.Contains(weapon))
        {
            registeredWeapons.Add(weapon);
            DebugLog.Info($"[CollisionManager] Registered weapon: {weapon.GetType().Name}", "Collision");
        }
    }
    
    /// <summary>
    /// Unregister a weapon from collision detection.
    /// </summary>
    public void UnregisterWeapon(IWeaponCollisionHandler weapon)
    {
        if (weapon != null && registeredWeapons.Contains(weapon))
        {
            registeredWeapons.Remove(weapon);
            DebugLog.Info($"[CollisionManager] Unregistered weapon: {weapon.GetType().Name}", "Collision");
        }
    }
    
    private void Start()
    {
        // Initialize spatial hash grid
        spatialGrid = new SpatialHashGrid(gridCellSize);
        DebugLog.Info($"[CollisionManager] Initialized spatial hash grid with cell size {gridCellSize}", "Collision");
        
        // Validate pool references
        if (projectilePool == null)
        {
            DebugLog.Error("[CollisionManager] ProjectilePool not assigned! Collision detection will not work.", "Collision");
        }
        else
        {
            DebugLog.Info("[CollisionManager] ProjectilePool reference found", "Collision");
        }
        
        if (enemyPool == null)
        {
            DebugLog.Error("[CollisionManager] EnemyPool not assigned! Collision detection will not work.", "Collision");
        }
        else
        {
            DebugLog.Info("[CollisionManager] EnemyPool reference found", "Collision");
        }
        
        // Auto-find OrbiterManager if not assigned
        if (orbiterManager == null)
        {
            orbiterManager = GetComponent<OrbiterManager>();
            if (orbiterManager != null)
            {
                DebugLog.Info("[CollisionManager] Auto-found OrbiterManager on same GameObject", "Collision");
            }
        }
        
        // Auto-register all weapons that implement IWeaponCollisionHandler
        if (playerTransform != null)
        {
            // Register OrbiterWeapon (still uses IWeaponCollisionHandler)
            OrbiterWeapon orbiterWeapon = playerTransform.GetComponent<OrbiterWeapon>();
            if (orbiterWeapon != null)
            {
                RegisterWeapon(orbiterWeapon);
            }
            
            // Register FireRingWeapon (uses IWeaponCollisionHandler)
            FireRingWeapon fireRingWeapon = playerTransform.GetComponent<FireRingWeapon>();
            if (fireRingWeapon != null)
            {
                RegisterWeapon(fireRingWeapon);
            }
            
            // NOTE: ProjectileWeapon and RapidFireWeapon no longer register here
            // They now use centralized collision detection via ProcessProjectileCollisions()
            
            DebugLog.Info($"[CollisionManager] Registered {registeredWeapons.Count} IWeaponCollisionHandler weapons (orbiters, fire rings, etc.)", "Collision");
        }
        
        DebugLog.Verbose($"[CollisionManager] Starting - orbiterManager={orbiterManager != null}, enemyPool={enemyPool != null}, projectilePool={projectilePool != null}");
        
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
        // ONLY run collisions during gameplay phase
        if (GamePhaseManager.CurrentPhase != GamePhase.Gameplay) return;
        
        if (gameOver || GameState.IsPaused) return;
        
        // Log collision manager is running
        if (Time.frameCount % 120 == 0)
        {
            DebugLog.Verbose($"[CollisionManager.Update] Running, registeredWeapons={registeredWeapons.Count}");
        }
        
        // Safety check - spatial grid must be initialized
        if (spatialGrid == null)
        {
            spatialGrid = new SpatialHashGrid(gridCellSize);
        }
        
        // Dynamic weapon registration (catches weapons added after Start)
        // NOTE: ProjectileWeapon no longer needs registration - centralized collision in ProcessProjectileCollisions()
        // Only register weapons that still implement IWeaponCollisionHandler (boomerangs, orbiters, etc.)
        
        // Populate spatial hash grid with all entities
        PopulateSpatialGrid();
        
        // Perform collision checks using grid
        // NEW: Centralized projectile collision processing (eliminates duplication)
        ProcessProjectileCollisions();
        
        // LEGACY: Check collisions for registered weapons (orbiters, boomerangs, etc.)
        // Standard projectiles now use ProcessProjectileCollisions() above
        CheckRegisteredWeaponCollisions();
        
        // Player collision still handled by CollisionManager (not weapon-specific)
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
    /// Centralized projectile collision processor - eliminates code duplication across weapons.
    /// Handles collision detection for all projectiles from the ProjectilePool.
    /// </summary>
    private void ProcessProjectileCollisions()
    {
        if (projectilePool == null)
        {
            if (Time.frameCount % 60 == 0)
                DebugLog.Warning("[CollisionManager] ProjectilePool is null! Collision detection disabled.");
            return;
        }
        
        if (enemyPool == null)
        {
            if (Time.frameCount % 60 == 0)
                DebugLog.Warning("[CollisionManager] EnemyPool is null! Collision detection disabled.");
            return;
        }
        
        if (spatialGrid == null)
        {
            if (Time.frameCount % 60 == 0)
                DebugLog.Warning("[CollisionManager] SpatialGrid is null! Collision detection disabled.");
            return;
        }
        
        // Get all active projectiles from the shared pool
        List<Projectile> activeProjectiles = projectilePool.GetActiveProjectiles();
        
        if (activeProjectiles == null)
        {
            if (Time.frameCount % 60 == 0)
                DebugLog.Warning("[CollisionManager] GetActiveProjectiles returned null!");
            return;
        }
        
        if (Time.frameCount % 120 == 0 && activeProjectiles.Count > 0)
        {
            DebugLog.Info($"[CollisionManager] Processing {activeProjectiles.Count} active projectiles");
        }
        
        int totalHits = 0;
        
        // Iterate backwards to safely handle projectiles being deactivated mid-loop
        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            var projectile = activeProjectiles[i];
            if (projectile == null || !projectile.IsActive) continue;
            
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
                    float distance = UnityEngine.Vector3.Distance(projectile.Position, enemy.Position);
                    float combinedRadius = projectile.CollisionRadius + enemy.CollisionRadius;
                    
                    if (distance < combinedRadius)
                    {
                        int enemyID = enemy.gameObject.GetInstanceID();
                        
                        // Register hit on projectile (handles pierce logic)
                        if (projectile.RegisterHit(enemyID))
                        {
                            totalHits++;
                            
                            // Apply damage
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
                                bool died = enemyHealth.TakeDamage(result.finalDamage);
                                
                                DebugLog.Verbose($"[CollisionManager] Projectile hit {enemy.name}: {result.finalDamage:F1} damage, died={died}");
                                
                                // Lifesteal healing (chance to heal 1 HP on hit)
                                Player player = GameServices.Player;
                                if (player != null && player.LifestealChance > 0f && Random.value < player.LifestealChance)
                                {
                                    Health playerHealth = player.GetComponent<Health>();
                                    if (playerHealth != null && playerHealth.CurrentHealth < playerHealth.MaxHealth)
                                    {
                                        playerHealth.Heal(1f);
                                        DebugLog.Info($"[CollisionManager] 💚 Lifesteal! Healed 1 HP (chance={player.LifestealChance*100:F0}%)");
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
                            }

                            // Invoke hit callback if set (for explosions, special effects, etc.)
                            if (projectile is BaseProjectile baseProj && baseProj.OnHitCallback != null)
                            {
                                baseProj.OnHitCallback.Invoke(baseProj, enemy.Position);
                            }

                            // Deactivate if exceeded pierce limit
                            if (projectile.EnemiesHit > projectile.Pierce)
                            {
                                projectile.Deactivate();
                                break; // Move to next projectile
                            }
                        }
                    }
                }
            }
        }
        
        if (Time.frameCount % 120 == 0 && totalHits > 0)
        {
            DebugLog.Verbose($"[CollisionManager] Projectile collisions: {totalHits} hits");
        }
    }
    
    /// <summary>
    /// Check collisions for all registered weapons using their own logic.
    /// LEGACY: Kept for weapons that don't use the standard projectile pool (orbiters, boomerangs, etc.)
    /// Most weapons now use centralized collision via ProcessProjectileCollisions().
    /// </summary>
    private void CheckRegisteredWeaponCollisions()
    {
        // No warning needed - it's normal to have 0 registered weapons if only using projectile-based weapons
        if (registeredWeapons.Count == 0) return;
        
        foreach (var weapon in registeredWeapons)
        {
            if (weapon != null && weapon.IsActive)
            {
                weapon.CheckCollisions(spatialGrid, enemyPool);
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
                        bool playerDied = playerHealth.TakeDamage(damage);
                        lastPlayerDamageTime = Time.time;

                        DebugLog.Verbose($"Player hit by {enemy.name}! Took {damage} damage, HP: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}", "Collision");

                        // Show red damage number above player
                        DamageNumberPool damagePool = GameServices.DamageNumberPool;
                        if (damagePool != null)
                        {
                            damagePool.ShowPlayerDamage(playerTransform.position + Vector3.up * 0.5f, damage);
                        }
                        
                        // Kill the enemy that hit the player (balances reduced i-frames)
                        Health enemyHealth = enemy.GetComponent<Health>();
                        if (enemyHealth != null)
                        {
                            enemyHealth.TakeDamage(999999f); // Instant kill
                            DebugLog.Verbose($"Enemy {enemy.name} killed after hitting player (suicide attack)", "Collision");
                        }
                        
                        if (playerDied)
                        {
                            DebugLog.Info("Player DIED!", "Player");
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