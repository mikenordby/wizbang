using System.Collections.Generic;

/// <summary>
/// Interface for projectile pooling systems.
/// Standardizes pool management across different weapon types.
/// </summary>
/// <typeparam name="T">Type of projectile being pooled</typeparam>
public interface IProjectilePool<T> where T : BaseProjectile
{
    /// <summary>
    /// Get an available projectile from the pool.
    /// </summary>
    /// <returns>Available projectile, or null if pool exhausted</returns>
    T GetProjectile();
    
    /// <summary>
    /// Return a projectile to the pool for reuse.
    /// </summary>
    /// <param name="projectile">Projectile to return</param>
    void ReturnProjectile(T projectile);
    
    /// <summary>
    /// Get all currently active projectiles.
    /// </summary>
    /// <returns>List of active projectiles (should be cached to avoid GC)</returns>
    List<T> GetActiveProjectiles();
}
