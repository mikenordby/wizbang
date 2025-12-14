using UnityEngine;

/// <summary>
/// Base class for all projectile types (straight, orbiter, homing, etc.)
/// Contains common properties and lifecycle management
/// </summary>
public abstract class BaseProjectile : MonoBehaviour
{
    [SerializeField] protected float collisionRadius = 0.15f;
    
    protected bool isActive;
    protected SpriteRenderer spriteRenderer;
    protected float damage;
    protected int pierce;
    protected int enemiesHit;
    
    public bool IsActive => isActive;
    public virtual float CollisionRadius => collisionRadius;
    public float Damage => damage;
    public int Pierce => pierce;
    public int EnemiesHit => enemiesHit;
    
    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    }
    
    /// <summary>
    /// Set projectile stats (damage and pierce)
    /// </summary>
    public virtual void SetStats(float damageValue, int pierceValue)
    {
        damage = damageValue;
        pierce = pierceValue;
        enemiesHit = 0;
        DebugLog.Verbose($"[BaseProjectile] SetStats: damage={damage:F1}, pierce={pierce}, enemiesHit reset to 0");
    }
    
    /// <summary>
    /// Increment hit counter, returns true if projectile should be deactivated
    /// </summary>
    public bool RegisterHit()
    {
        enemiesHit++;
        bool shouldDeactivate = enemiesHit > pierce; // Deactivate if hit more enemies than pierce allows
        DebugLog.Verbose($"[BaseProjectile] RegisterHit: enemiesHit={enemiesHit}, pierce={pierce}, shouldDeactivate={shouldDeactivate}");
        return shouldDeactivate;
    }
    
    /// <summary>
    /// Activate the projectile. Override in derived classes for specific behavior.
    /// </summary>
    public virtual void Activate()
    {
        isActive = true;
        gameObject.SetActive(true);
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }
    
    /// <summary>
    /// Deactivate the projectile and return to pool or respawn state
    /// NOTE: Does NOT reset damage/pierce - those are set when projectile is reused
    /// </summary>
    public virtual void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        
        DebugLog.Verbose($"[BaseProjectile] Deactivated - damage={damage:F1} pierce={pierce} enemiesHit={enemiesHit} (stats preserved for pool reuse)");
        
        // Notify pool to remove from active list (only for Projectile, not OrbiterProjectile)
        if (this is Projectile)
        {
            ProjectilePool pool = GetComponentInParent<ProjectilePool>();
            if (pool != null)
                pool.ReturnProjectile(this as Projectile);
        }
    }
    
    /// <summary>
    /// Update projectile movement. Override in derived classes.
    /// </summary>
    protected virtual void Update()
    {
        if (GameState.IsPaused) return;
        if (!isActive) return;
        
        UpdateMovement();
    }
    
    /// <summary>
    /// Override this method to implement specific movement patterns
    /// </summary>
    protected abstract void UpdateMovement();
}
