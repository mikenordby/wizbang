using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base class for all projectile types (straight, orbiter, homing, etc.)
/// Contains common properties and lifecycle management
/// </summary>
public abstract class BaseProjectile : MonoBehaviour, ICollidable
{
    [SerializeField] protected float baseCollisionRadius = 0.15f; // Base radius before size scaling
    
    protected bool isActive;
    protected SpriteRenderer spriteRenderer;
    protected float damage;
    protected int pierce;
    protected int enemiesHit;
    protected HashSet<int> hitEnemyIDs = new HashSet<int>();
    protected DamageType damageType = DamageType.Physical;
    protected float size = 1f; // Size multiplier for visual and hitbox

    // Callback for when projectile hits an enemy (used for explosions, special effects, etc.)
    public System.Action<BaseProjectile, Vector3> OnHitCallback { get; set; }

    public bool IsActive => isActive;
    public virtual float CollisionRadius => baseCollisionRadius * size; // Scale hitbox with size
    public float Damage => damage;
    public int Pierce => pierce;
    public int EnemiesHit => enemiesHit;
    public DamageType DamageType => damageType;
    public float Size => size;
    
    // ICollidable implementation
    public Vector3 Position => transform.position;
    public abstract CollisionLayer Layer { get; }
    
    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    }
    
    /// <summary>
    /// Set projectile stats (damage, pierce, damage type, and size)
    /// Size affects both visual scale and collision hitbox
    /// </summary>
    public virtual void SetStats(float damageValue, int pierceValue, DamageType type = DamageType.Physical, float sizeMultiplier = 1f)
    {
        damage = damageValue;
        pierce = pierceValue;
        damageType = type;
        size = sizeMultiplier;
        enemiesHit = 0;
        hitEnemyIDs.Clear();
        
        // Apply visual scaling
        transform.localScale = Vector3.one * size;
        
        DebugLog.Verbose($"[BaseProjectile] SetStats: damage={damage:F1}, pierce={pierce}, type={damageType}, size={size:F2}, collisionRadius={CollisionRadius:F3}");
    }
    
    /// <summary>
    /// Increment hit counter, returns true if projectile should be deactivated
    /// Prevents same enemy from being hit multiple times on consecutive frames
    /// </summary>
    /// <param name="enemyInstanceID">GetInstanceID() of the enemy GameObject</param>
    /// <returns>True if projectile should deactivate, false if already hit this enemy or still has pierce</returns>
    public bool RegisterHit(int enemyInstanceID)
    {
        // Check if we've already hit this enemy
        if (hitEnemyIDs.Contains(enemyInstanceID))
        {
            DebugLog.Verbose($"[BaseProjectile] RegisterHit: Already hit enemy {enemyInstanceID}, skipping");
            return false; // Don't count as a hit, projectile stays active
        }
        
        // New enemy hit
        hitEnemyIDs.Add(enemyInstanceID);
        enemiesHit++;
        bool shouldDeactivate = enemiesHit > pierce; // Deactivate if hit more enemies than pierce allows
        DebugLog.Verbose($"[BaseProjectile] RegisterHit: New enemy {enemyInstanceID}, enemiesHit={enemiesHit}, pierce={pierce}, shouldDeactivate={shouldDeactivate}");
        return shouldDeactivate;
    }
    
    /// <summary>
    /// Clear the hit enemy list - used by boomerangs to damage enemies again on return trip
    /// </summary>
    public void ClearHitList()
    {
        hitEnemyIDs.Clear();
        enemiesHit = 0;
        DebugLog.Verbose($"[BaseProjectile] ClearHitList: Reset hit tracking for new trajectory phase");
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
        if (GamePhaseManager.CurrentPhase != GamePhase.Gameplay) return;
        if (GameState.IsPaused) return;
        if (!isActive) return;
        
        UpdateMovement();
    }
    
    /// <summary>
    /// Override this method to implement specific movement patterns
    /// </summary>
    protected abstract void UpdateMovement();
}
