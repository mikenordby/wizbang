using UnityEngine;

/// <summary>
/// Simple, performant enemy that moves toward the player.
/// Uses direct transform manipulation instead of physics for better performance with many enemies.
/// </summary>
public class Enemy : MonoBehaviour, ICollidable
{
    /// <summary>
    /// Global speed multiplier applied to all enemies (0.4 = 60% slower for slower-paced gameplay)
    /// </summary>
    public static float GlobalSpeedMultiplier = 0.4f;

    [SerializeField] private EnemyStats stats;
    private Health health;
    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    // OLD: DirectionalSpriteController removed - only using AnimatedSpriteController now
    private AnimatedSpriteController animController;
    private bool isActive;
    private CircleCollider2D enemyCollider;
    private Rigidbody2D rb;
    private int currentCollisionCount = 0;
    private bool isTouchingObstacle = false;
    private Vector2 obstacleAvoidanceDirection = Vector2.zero;
    
    public bool IsActive => isActive;
    // Collision radius scales with enemy visual size for accurate hitbox
    // Base 0.3f * localScale gives reasonable hitbox relative to visual
    public float CollisionRadius => 0.3f * transform.localScale.x;
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

        // Track obstacle collisions (rocks) and calculate avoidance direction
        if (collision.gameObject.GetComponent<RockObstacle>() != null)
        {
            isTouchingObstacle = true;

            // Get collision normal and calculate perpendicular slide direction
            if (collision.contactCount > 0)
            {
                Vector2 normal = collision.GetContact(0).normal;
                // Calculate which perpendicular direction leads toward player
                Vector2 toPlayer = (playerTransform != null)
                    ? (Vector2)(playerTransform.position - transform.position).normalized
                    : Vector2.zero;

                // Two perpendicular options: rotate normal 90° left or right
                Vector2 perpLeft = new Vector2(-normal.y, normal.x);
                Vector2 perpRight = new Vector2(normal.y, -normal.x);

                // Choose the one that points more toward the player
                obstacleAvoidanceDirection = Vector2.Dot(perpLeft, toPlayer) > Vector2.Dot(perpRight, toPlayer)
                    ? perpLeft : perpRight;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Continuously update avoidance direction while touching obstacle
        if (collision.gameObject.GetComponent<RockObstacle>() != null && collision.contactCount > 0)
        {
            Vector2 normal = collision.GetContact(0).normal;
            Vector2 toPlayer = (playerTransform != null)
                ? (Vector2)(playerTransform.position - transform.position).normalized
                : Vector2.zero;

            Vector2 perpLeft = new Vector2(-normal.y, normal.x);
            Vector2 perpRight = new Vector2(normal.y, -normal.x);

            obstacleAvoidanceDirection = Vector2.Dot(perpLeft, toPlayer) > Vector2.Dot(perpRight, toPlayer)
                ? perpLeft : perpRight;
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

        // Track obstacle collisions (rocks)
        if (collision.gameObject.GetComponent<RockObstacle>() != null)
        {
            isTouchingObstacle = false;
            obstacleAvoidanceDirection = Vector2.zero;
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
    /// Activate enemy at a specific position with stats
    /// </summary>
    /// <param name="position">Spawn position</param>
    /// <param name="enemyStats">Base enemy stats</param>
    /// <param name="healthMultiplier">Health scaling multiplier (for difficulty progression)</param>
    public void Activate(Vector3 position, EnemyStats enemyStats, float healthMultiplier = 1f)
    {
        stats = enemyStats;
        transform.position = position;
        isTouchingObstacle = false;
        obstacleAvoidanceDirection = Vector2.zero;

        // Lazy initialization if not already done
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        if (health == null)
            health = GetComponent<Health>();
        if (health == null)
            health = gameObject.AddComponent<Health>();

        // CRITICAL: Ensure physics components exist for enemy-enemy collision
        // This was previously in Initialize() which was never called
        if (enemyCollider == null)
        {
            enemyCollider = GetComponent<CircleCollider2D>();
            if (enemyCollider == null)
            {
                enemyCollider = gameObject.AddComponent<CircleCollider2D>();
                enemyCollider.radius = 0.15f; // Very tight hitbox - 50% smaller diameter for dense enemy packs
                enemyCollider.isTrigger = false; // Physical collision enabled - enemies push each other
            }
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.mass = 0.5f; // Low mass so enemies don't stack heavily
                rb.linearDamping = 0.5f; // Lower damping allows natural sliding around obstacles
            }
        }

        // Subscribe to death event if not already subscribed
        health.OnDeath -= HandleDeath; // Remove first to avoid duplicates
        health.OnDeath += HandleDeath;
        
        DebugLog.Info($"Enemy.Activate: {stats?.enemyName} subscribed to OnDeath event");
        
        // Initialize health with scaled max health (avoids creating ScriptableObject copies)
        if (health != null && stats != null)
        {
            float scaledMaxHealth = stats.maxHealth * healthMultiplier;
            health.Initialize(scaledMaxHealth);
        }
        
        // CRITICAL: Always reload sprites for this enemy type (for pooled reuse)
        // NOTE: Only use AnimatedSpriteController (DirectionalSpriteController conflicts with animation)
        
        // Set up AnimatedSpriteController for walking animation
        animController = GetComponent<AnimatedSpriteController>();
        if (animController == null)
            animController = gameObject.AddComponent<AnimatedSpriteController>();

        // CRITICAL: Explicitly set the SpriteRenderer reference (fixes null reference when added dynamically)
        animController.SetSpriteRenderer(spriteRenderer);

        // CRITICAL: Stop first to reset animation state (fixes pooled enemies getting stuck on single frame)
        animController.Stop();

        animController.SetEntityType(stats.enemyName); // Use just enemy name, sprites at Resources/Sprites/{enemyName}/
        animController.SetAnimation("walking-8-frames");
        animController.LoadAllAnimations();
        animController.UpdateSprite(); // Ensure spriteRenderer.sprite is set to first frame
        animController.Play();
        DebugLog.Info($"[Enemy.Activate] AnimatedSpriteController configured for {stats.enemyName}/walking-8-frames");
        
        // Verify sprites loaded
        if (spriteRenderer != null && spriteRenderer.sprite == null)
        {
            DebugLog.Error($"[Enemy] *** CRITICAL: AnimatedSpriteController FAILED to load sprites for {stats.enemyName} ***");
            DebugLog.Error($"[Enemy] All enemies require PixelLab sprites in Resources/Sprites/{stats.enemyName}/walking-8-frames/");
            DebugLog.Error($"[Enemy] Generate using: mcp_pixellab_create_character(description='{stats.enemyName}', n_directions=8)");
            DebugLog.Error($"[Enemy] No sprites available for {stats.enemyName} - enemy will be invisible!");
        }
        else if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            DebugLog.Verbose($"[Enemy] Successfully loaded sprites for {stats.enemyName}: sprite={spriteRenderer.sprite.name}, PPU={spriteRenderer.sprite.pixelsPerUnit}");
        }
        
        // Apply visual properties from stats
        if (stats != null && spriteRenderer != null)
        {
            spriteRenderer.color = stats.color;
            
            // Scale calculation: PixelLab sprites are 48×48@PPU16 = 3 units
            // We want stats.scale=1.0 to create a 2-unit enemy (matches old behavior)
            // So we need: scale = stats.scale * (2/3) = stats.scale * 0.67
            float spritePPU = spriteRenderer.sprite != null ? spriteRenderer.sprite.pixelsPerUnit : 16f;
            float spriteWorldSize = spriteRenderer.sprite != null ? (spriteRenderer.sprite.texture.width / spritePPU) : 3f;
            float targetWorldSize = 2f; // Standard enemy size
            float baseScale = stats.scale * (targetWorldSize / spriteWorldSize);
            
            transform.localScale = Vector3.one * baseScale;
            
            DebugLog.Info($"Enemy activated: {stats.enemyName}, pos={position}, scale={baseScale} (stats.scale={stats.scale}, spritePPU={spritePPU}, spriteWorldSize={spriteWorldSize:F2}), localScale={transform.localScale}, sprite={spriteRenderer.sprite?.name}");
        }
        
        isActive = true;
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Deactivate enemy and return to pool
    /// </summary>
    public void Deactivate()
    {
        DebugLog.Verbose($"[Enemy.Deactivate] {stats?.enemyName} being deactivated and returned to pool");

        isActive = false;
        currentCollisionCount = 0;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        // CRITICAL: Reset velocity to prevent residual movement when pooled/reactivated
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // CRITICAL: Stop animation to prevent stuck frames when pooled enemy is reactivated
        if (animController != null)
        {
            animController.Stop();
        }

        gameObject.SetActive(false);
        
        // Notify pool to remove from active list (failsafe)
        EnemyPool pool = GetComponentInParent<EnemyPool>();
        if (pool != null)
            pool.ReturnEnemy(this);
    }
    
    private void HandleDeath()
    {
        DebugLog.Info($"Enemy.HandleDeath: {stats?.enemyName} died at {transform.position}");
        
        // Fire event for other systems to react
        GameEvents.TriggerEnemyKilled(this);
        
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
    
    /// <summary>
    /// Maintain correct scale after AnimatedSpriteController changes sprites in Update()
    /// </summary>
    private void LateUpdate()
    {
        // CRITICAL: AnimatedSpriteController.UpdateSprite() may cause scale drift
        // Reapply correct scale based on current sprite's actual dimensions
        if (isActive && stats != null && spriteRenderer != null && spriteRenderer.sprite != null)
        {
            float spritePPU = spriteRenderer.sprite.pixelsPerUnit;
            float spriteWorldSize = spriteRenderer.sprite.texture.width / spritePPU;
            float targetWorldSize = 2f; // Standard enemy size
            float desiredScale = stats.scale * (targetWorldSize / spriteWorldSize);
            
            // Only update if scale has drifted (avoid constant vector allocations)
            if (Mathf.Abs(transform.localScale.x - desiredScale) > 0.01f)
            {
                transform.localScale = Vector3.one * desiredScale;
            }
        }
    }
    
    private void Update()
    {
        if (GamePhaseManager.CurrentPhase != GamePhase.Gameplay) return;
        if (GameState.IsPaused) return;

        if (!isActive || playerTransform == null)
        {
            return;
        }

        // Move toward player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        float speed = (stats != null ? stats.moveSpeed : 2f) * GlobalSpeedMultiplier;

        // Update animation direction using AnimatedSpriteController
        if (animController != null && animController.IsPlaying)
        {
            animController.UpdateDirection(direction);
        }

        if (rb != null)
        {
            // When touching an obstacle, slide along it using the calculated avoidance direction
            if (isTouchingObstacle && obstacleAvoidanceDirection != Vector2.zero)
            {
                // Blend between sliding along obstacle and moving toward player
                Vector2 slideDirection = (obstacleAvoidanceDirection + (Vector2)direction * 0.3f).normalized;
                rb.linearVelocity = slideDirection * speed;
            }
            else
            {
                // Not blocked - move directly toward player
                rb.linearVelocity = (Vector2)direction * speed;
            }
        }
        else
        {
            transform.position += direction * speed * Time.deltaTime;
        }
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