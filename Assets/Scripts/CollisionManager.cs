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
    
    // Legacy direct reference for backward compatibility
    private BoomerangWeapon boomerangWeapon;
    
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
            // Register BoomerangWeapon
            boomerangWeapon = playerTransform.GetComponent<BoomerangWeapon>();
            if (boomerangWeapon != null)
            {
                RegisterWeapon(boomerangWeapon);
            }
            
            // Register OrbiterWeapon
            OrbiterWeapon orbiterWeapon = playerTransform.GetComponent<OrbiterWeapon>();
            if (orbiterWeapon != null)
            {
                RegisterWeapon(orbiterWeapon);
            }
            
            // Register ProjectileWeapon (handles shared projectile pool)
            ProjectileWeapon projectileWeapon = playerTransform.GetComponent<ProjectileWeapon>();
            if (projectileWeapon != null)
            {
                RegisterWeapon(projectileWeapon);
            }
            
            // Register RapidFireWeapon (shares pool, but registration is needed for consistency)
            RapidFireWeapon rapidFireWeapon = playerTransform.GetComponent<RapidFireWeapon>();
            if (rapidFireWeapon != null)
            {
                RegisterWeapon(rapidFireWeapon);
            }
            
            DebugLog.Info($"[CollisionManager] Registered {registeredWeapons.Count} weapons for collision detection", "Collision");
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
        if (playerTransform != null && registeredWeapons.Count == 0)
        {
            ProjectileWeapon projectileWeapon = playerTransform.GetComponent<ProjectileWeapon>();
            if (projectileWeapon != null)
            {
                RegisterWeapon(projectileWeapon);
                DebugLog.Info("[CollisionManager] Late-registered ProjectileWeapon in Update", "Collision");
            }
            else
            {
                DebugLog.Warning("[CollisionManager] No ProjectileWeapon found on player!");
            }
        }
        
        // Populate spatial hash grid with all entities
        PopulateSpatialGrid();
        
        // Perform collision checks using grid
        // Check collisions for all registered weapons (new pattern - replaces old hard-coded methods)
        if (Time.frameCount % 120 == 0 && registeredWeapons.Count > 0)
        {
            DebugLog.Verbose($"[CollisionManager] About to CheckRegisteredWeaponCollisions, count={registeredWeapons.Count}");
        }
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
    /// Check collisions for all registered weapons using their own logic.
    /// This is the new pattern - weapons handle their own collision detection.
    /// </summary>
    private void CheckRegisteredWeaponCollisions()
    {
        if (registeredWeapons.Count == 0)
        {
            // Only warn once per second to avoid spam
            if (Time.frameCount % 60 == 0)
            {
                DebugLog.Warning("[CollisionManager] No weapons registered for collision detection!");
            }
            return;
        }
        
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