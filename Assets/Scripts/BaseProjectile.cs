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
    
    public bool IsActive => isActive;
    public virtual float CollisionRadius => collisionRadius;
    
    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
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
    /// </summary>
    public virtual void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
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
