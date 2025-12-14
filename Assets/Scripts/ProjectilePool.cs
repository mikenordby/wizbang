using UnityEngine;

/// <summary>
/// High-performance object pool for projectiles
/// </summary>
public class ProjectilePool : ObjectPool<Projectile>
{
    [SerializeField] private GameObject projectilePrefab;
    
    /// <summary>
    /// Get an available projectile from the pool
    /// </summary>
    public Projectile GetProjectile()
    {
        return GetItem();
    }
    
    /// <summary>
    /// Get all currently active projectiles (cached list, no GC)
    /// </summary>
    public System.Collections.Generic.List<Projectile> GetActiveProjectiles()
    {
        return activeItems;
    }
    
    /// <summary>
    /// Remove projectile from active list (called by Projectile.Deactivate)
    /// </summary>
    public void ReturnProjectile(Projectile projectile)
    {
        ReturnItem(projectile);
    }
    
    protected override Projectile CreateNewItem()
    {
        GameObject go = Instantiate(projectilePrefab, transform);
        Projectile projectile = go.GetComponent<Projectile>();
        
        // Load fireball sprite (from Resources or procedural fallback)
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteLoader.LoadProjectileSprite("fireball");
        go.transform.localScale = Vector3.one * 1.5f; // Larger projectiles
        
        projectile.Deactivate();
        pool.Add(projectile);
        
        // Add to active list if item is being created during GetItem()
        if (!activeItems.Contains(projectile))
            activeItems.Add(projectile);
        
        return projectile;
    }
    
    protected override bool IsActive(Projectile item)
    {
        return item.IsActive;
    }
}