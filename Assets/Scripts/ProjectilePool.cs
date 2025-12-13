using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// High-performance object pool for projectiles
/// </summary>
public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int initialPoolSize = 50;
    [SerializeField] private int maxPoolSize = 200;
    
    private List<Projectile> pool = new List<Projectile>();
    
    private void Awake()
    {
        // Pre-allocate projectiles
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewProjectile();
        }
    }
    
    /// <summary>
    /// Get an available projectile from the pool
    /// </summary>
    public Projectile GetProjectile()
    {
        // Find inactive projectile
        foreach (var projectile in pool)
        {
            if (!projectile.IsActive)
            {
                return projectile;
            }
        }
        
        // Create new if under max
        if (pool.Count < maxPoolSize)
        {
            return CreateNewProjectile();
        }
        
        // All in use, reuse oldest
        return pool[0];
    }
    
    /// <summary>
    /// Get all currently active projectiles
    /// </summary>
    public List<Projectile> GetActiveProjectiles()
    {
        List<Projectile> active = new List<Projectile>();
        foreach (var projectile in pool)
        {
            if (projectile.IsActive)
            {
                active.Add(projectile);
            }
        }
        return active;
    }
    
    private Projectile CreateNewProjectile()
    {
        GameObject go = Instantiate(projectilePrefab, transform);
        Projectile projectile = go.GetComponent<Projectile>();
        
        // Create fireball sprite
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateFireballSprite();
        
        projectile.Deactivate();
        pool.Add(projectile);
        return projectile;
    }
}