using UnityEngine;

/// <summary>
/// Simple, performant enemy that moves toward the player.
/// Uses direct transform manipulation instead of physics for better performance with many enemies.
/// </summary>
public class Enemy : MonoBehaviour, ICollidable
{
    [SerializeField] private EnemyStats stats;
    private Health health;
    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private bool isActive;
    private CircleCollider2D enemyCollider;
    private int currentCollisionCount = 0;
    
    public bool IsActive => isActive;
    public float CollisionRadius => 0.35f;
    public float ContactDamage => stats != null ? stats.contactDamage : 10f;
    public int XPDrop => stats != null ? stats.xpDrop : 5;
    
    // ICollidable implementation
    public Vector3 Position => transform.position;
    public CollisionLayer Layer => CollisionLayer.Enemy;
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Enemy>() != null)
        {
            currentCollisionCount++;
            UpdateOutlineColor();
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Enemy>() != null)
        {
            currentCollisionCount--;
            if (currentCollisionCount < 0) currentCollisionCount = 0;
            UpdateOutlineColor();
        }
    }
    
    private void UpdateOutlineColor()
    {
        if (spriteRenderer == null) return;
        
        // Color based on collision count: white (0), yellow (1-2), orange (3-4), red (5+)
        if (currentCollisionCount == 0)
            spriteRenderer.color = Color.white;
        else if (currentCollisionCount <= 2)
            spriteRenderer.color = Color.yellow;
        else if (currentCollisionCount <= 4)
            spriteRenderer.color = new Color(1f, 0.5f, 0f); // Orange
        else
            spriteRenderer.color = Color.red;
    }
    
    public void SetPlayer(Transform player)
    {
        playerTransform = player;
    }
    
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }
    
    /// <summary>
    /// Initialize the enemy with player reference and stats
    /// </summary>
    public void Initialize(Transform player, EnemyStats enemyStats)
    {
        playerTransform = player;
        stats = enemyStats;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<Health>();
        if (health == null)
            health = gameObject.AddComponent<Health>();
        
        // Add CircleCollider2D for enemy-enemy collision
        enemyCollider = GetComponent<CircleCollider2D>();
        if (enemyCollider == null)
        {
            enemyCollider = gameObject.AddComponent<CircleCollider2D>();
            enemyCollider.radius = 0.25f; // Slightly larger than collision detection radius
            enemyCollider.isTrigger = false; // Physical collision
        }
        
        // Add Rigidbody2D for physics
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.mass = 3f; // Increased for better collision response
        }
        
        if (spriteRenderer != null && spriteRenderer.sprite == null)
        {
            DebugLog.Warning($"Enemy.Initialize: Sprite is null, attempting to load from SpriteLoader for {stats?.enemyName}");
            if (stats != null)
            {
                spriteRenderer.sprite = SpriteLoader.LoadEnemySprite(stats.enemyName, stats.color);
                if (spriteRenderer.sprite != null)
                {
                    DebugLog.Info($"Enemy.Initialize: Loaded sprite for {stats.enemyName}: {spriteRenderer.sprite.texture.width}x{spriteRenderer.sprite.texture.height}px");
                }
                else
                {
                    DebugLog.Error($"Enemy.Initialize: Failed to load sprite for {stats.enemyName}, using fallback");
                    Texture2D texture = new Texture2D(64, 64);
                    Color[] pixels = new Color[64 * 64];
                    for (int i = 0; i < pixels.Length; i++)
                        pixels[i] = Color.white;
                    texture.SetPixels(pixels);
                    texture.Apply();
                    spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
                }
            }
        }
        
        if (stats != null && spriteRenderer != null)
        {
            spriteRenderer.color = stats.color;
            transform.localScale = Vector3.one * stats.scale;
        }
        
        health.OnDeath += HandleDeath;
    }
    
    /// <summary>
    /// Activate enemy at a specific position with stats
    /// </summary>
    public void Activate(Vector3 position, EnemyStats enemyStats)
    {
        stats = enemyStats;
        transform.position = position;
        
        // Initialize if not already done (first activation)
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite == null)
            {
                // Load sprite based on enemy type (from Resources or procedural fallback)
                spriteRenderer.sprite = SpriteLoader.LoadEnemySprite(stats.enemyName, stats.color);
                if (spriteRenderer.sprite != null)
                {
                    DebugLog.Info($"Enemy.Activate: Loaded sprite for {stats.enemyName}: {spriteRenderer.sprite.texture.width}x{spriteRenderer.sprite.texture.height}px, PPU={spriteRenderer.sprite.pixelsPerUnit}");
                }
            }
        }
        else if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            DebugLog.Info($"Enemy.Activate: Using existing sprite for {stats.enemyName}: {spriteRenderer.sprite.texture.width}x{spriteRenderer.sprite.texture.height}px");
        }
        
        // Lazy initialization if not already done
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }
        
        // Load sprite if it doesn't exist
        if (spriteRenderer != null && spriteRenderer.sprite == null)
        {
            // Load sprite based on enemy type (from Resources or procedural fallback)
            if (stats != null)
                spriteRenderer.sprite = SpriteLoader.LoadEnemySprite(stats.enemyName, stats.color);
        }
        
        if (health == null)
            health = GetComponent<Health>();
        if (health == null)
            health = gameObject.AddComponent<Health>();
        
        // Subscribe to death event if not already subscribed
        health.OnDeath -= HandleDeath; // Remove first to avoid duplicates
        health.OnDeath += HandleDeath;
        
        DebugLog.Info($"Enemy.Activate: {stats?.enemyName} subscribed to OnDeath event");
        
        if (health != null && stats != null)
            health.Initialize(stats.maxHealth);
        
        // Apply visual properties from stats
        if (stats != null && spriteRenderer != null)
        {
            spriteRenderer.color = stats.color;
            transform.localScale = Vector3.one * stats.scale;
            DebugLog.Info($"Enemy activated: {stats.enemyName}, pos={position}, color={stats.color}, scale={stats.scale}, spriteColor={spriteRenderer.color}, sprite={spriteRenderer.sprite != null}");
        }
        
        isActive = true;
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Deactivate enemy and return to pool
    /// </summary>
    public void Deactivate()
    {
        DebugLog.Info($"[Enemy.Deactivate] {stats?.enemyName} being deactivated and returned to pool");
        
        isActive = false;
        currentCollisionCount = 0;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
        gameObject.SetActive(false);
        
        // Notify pool to remove from active list (failsafe)
        EnemyPool pool = GetComponentInParent<EnemyPool>();
        if (pool != null)
            pool.ReturnEnemy(this);
    }
    
    private void HandleDeath()
    {
        DebugLog.Info($"Enemy.HandleDeath: {stats?.enemyName} died at {transform.position}");
        
        // Spawn XP orb using GameServices
        XPOrbPool pool = GameServices.XPOrbPool;
        if (pool != null && stats != null)
        {
            DebugLog.Info($"Enemy.HandleDeath: Spawning {stats.xpDrop} XP at {transform.position}");
            pool.SpawnOrb(transform.position, stats.xpDrop);
        }
        else
        {
            DebugLog.Warning($"Enemy.HandleDeath: Cannot spawn XP - pool={pool != null}, stats={stats != null}");
        }
        
        Deactivate();
    }
    
    private void Update()
    {
        if (GameState.IsPaused) return;
        
        if (!isActive || playerTransform == null)
        {
            return;
        }
        
        // Move toward player (removed verbose logging)
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        float speed = stats != null ? stats.moveSpeed : 2f;
        transform.position += direction * speed * Time.deltaTime;
    }
    
    /// <summary>
    /// Check if enemy is too far from player (for cleanup)
    /// </summary>
    public bool IsTooFarFromPlayer(float maxDistance)
    {
        if (playerTransform == null) return true;
        return Vector3.Distance(transform.position, playerTransform.position) > maxDistance;
    }
}