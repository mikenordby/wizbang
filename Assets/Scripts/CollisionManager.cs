using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Efficient collision detection manager using distance checks.
/// Handles projectile-enemy and player-enemy collisions.
/// </summary>
public class CollisionManager : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private OrbiterManager orbiterManager;
    [SerializeField] private float playerCollisionRadius = 0.35f;
    [SerializeField] private float projectileDamage = 100f; // 1-hit kill for all enemies
    
    private float lastPlayerDamageTime = -999f;
    private float playerDamageCooldown = 0.5f;
    
    private bool gameOver = false;
    private Health playerHealth;

    
    public bool IsGameOver => gameOver;
    
    private void Start()
    {
        // Auto-find OrbiterManager if not assigned
        if (orbiterManager == null)
        {
            orbiterManager = GetComponent<OrbiterManager>();
            if (orbiterManager != null)
            {
                Debug.Log("[CollisionManager] Auto-found OrbiterManager on same GameObject");
            }
        }
        
        Debug.Log($"[CollisionManager] Starting - orbiterManager={orbiterManager != null}, enemyPool={enemyPool != null}, projectilePool={projectilePool != null}");
        
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
        
        CheckProjectileEnemyCollisions();
        CheckOrbiterEnemyCollisions();
        CheckPlayerEnemyCollisions();
    }
    
    /// <summary>
    /// Check all active projectiles against all active enemies
    /// </summary>
private void CheckProjectileEnemyCollisions()
    {
        if (projectilePool == null || enemyPool == null) return;
        
        List<Projectile> activeProjectiles = projectilePool.GetActiveProjectiles();
        Enemy[] allEnemies = enemyPool.GetComponentsInChildren<Enemy>();
        
        foreach (var projectile in activeProjectiles)
        {
            if (!projectile.IsActive) continue;
            
            foreach (var enemy in allEnemies)
            {
                if (!enemy.gameObject.activeInHierarchy) continue;
                
                float distance = Vector3.Distance(projectile.transform.position, enemy.transform.position);
                float combinedRadius = projectile.CollisionRadius + enemy.CollisionRadius;
                
                if (distance < combinedRadius)
                {
                    Health enemyHealth = enemy.GetComponent<Health>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(projectileDamage);
                        Debug.Log($"Collision: Projectile hit enemy at {enemy.transform.position}, distance={distance:F2}");
                    }
                    
                    projectile.Deactivate();
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Check all active orbiters against all active enemies
    /// </summary>
    private void CheckOrbiterEnemyCollisions()
    {
        if (orbiterManager == null)
        {
            return;
        }
        if (enemyPool == null)
        {
            return;
        }
        
        List<OrbiterProjectile> activeOrbiters = orbiterManager.GetActiveOrbiters();
        if (activeOrbiters == null)
        {
            return;
        }
        if (activeOrbiters.Count == 0)
        {
            return; // No active orbiters, this is normal
        }
        
        Enemy[] allEnemies = enemyPool.GetComponentsInChildren<Enemy>();
        
        int checkCount = 0;
        foreach (var orbiter in activeOrbiters)
        {
            if (orbiter == null || !orbiter.IsActive) continue;
            
            foreach (var enemy in allEnemies)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy || !enemy.IsActive) continue;
                
                checkCount++;
                float distance = Vector3.Distance(orbiter.transform.position, enemy.transform.position);
                float combinedRadius = orbiter.CollisionRadius + enemy.CollisionRadius;
                
                if (distance < combinedRadius)
                {
                    Health enemyHealth = enemy.GetComponent<Health>();
                    if (enemyHealth != null && enemyHealth.IsAlive)
                    {
                        Debug.Log($"[ORBITER HIT] Orbiter collided with enemy! Distance={distance:F3}, CombinedRadius={combinedRadius:F3}, OrbiterRadius={orbiter.CollisionRadius:F3}, EnemyRadius={enemy.CollisionRadius:F3}");
                        Debug.Log($"[ORBITER HIT] Positions: Orbiter={orbiter.transform.position}, Enemy={enemy.transform.position}");
                        Debug.Log($"[ORBITER HIT] Enemy health before: {enemyHealth.CurrentHealth}, damage: {projectileDamage}");
                        
                        bool died = enemyHealth.TakeDamage(projectileDamage);
                        
                        Debug.Log($"[ORBITER HIT] Enemy health after: {enemyHealth.CurrentHealth}, died: {died}");
                    }
                    
                    Debug.Log($"[ORBITER HIT] Deactivating orbiter for 2 seconds");
                    orbiter.Deactivate();
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Check if player touched any enemy with cooldown-based damage and knockback
    /// </summary>
    private void CheckPlayerEnemyCollisions()
    {
        if (playerTransform == null || enemyPool == null || playerHealth == null) return;
        
        // Skip if level-up UI is showing
        LevelUpUI levelUpUI = FindAnyObjectByType<LevelUpUI>();
        if (levelUpUI != null && levelUpUI.IsShowingUI) return;
        
        Enemy[] allEnemies = enemyPool.GetComponentsInChildren<Enemy>();
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        
        foreach (var enemy in allEnemies)
        {
            if (!enemy.gameObject.activeInHierarchy || !enemy.IsActive) continue;
            
            float distance = Vector3.Distance(playerTransform.position, enemy.transform.position);
            float combinedRadius = playerCollisionRadius + enemy.CollisionRadius;
            
            if (distance < combinedRadius)
            {
                // Only apply damage if cooldown elapsed
                if (Time.time - lastPlayerDamageTime >= playerDamageCooldown)
                {
                    float damage = enemy.ContactDamage;
                    bool died = playerHealth.TakeDamage(damage);
                    lastPlayerDamageTime = Time.time;
                    
                    Debug.Log($"Player hit by {enemy.name}! Took {damage} damage, HP: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
                    
                    if (died)
                    {
                        Debug.Log("Player DIED!");
                        gameOver = true;
                        Time.timeScale = 0f;
                        return;
                    }
                    
                    // Apply knockback to player
                    if (playerRb != null)
                    {
                        Vector2 knockbackDir = (playerTransform.position - enemy.transform.position).normalized;
                        playerRb.AddForce(knockbackDir * 250f, ForceMode2D.Impulse);
                        Debug.Log($"Player knocked back from enemy at {enemy.transform.position}");
                    }
                }
                
                break; // Only process one enemy collision per frame
            }
        }
    }
    
    /// <summary>
    /// Display game over UI
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
    }
}