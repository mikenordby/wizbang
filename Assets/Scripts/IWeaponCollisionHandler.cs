using UnityEngine;

/// <summary>
/// Interface for weapons that need to handle their own collision detection.
/// Allows weapons to register with CollisionManager and handle collisions independently.
/// </summary>
public interface IWeaponCollisionHandler
{
    /// <summary>
    /// Called each frame by CollisionManager to check collisions for this weapon.
    /// Weapon is responsible for querying spatial grid and applying damage.
    /// </summary>
    /// <param name="grid">Spatial hash grid containing all collidable entities</param>
    /// <param name="enemyPool">Pool of active enemies</param>
    void CheckCollisions(SpatialHashGrid grid, EnemyPool enemyPool);
    
    /// <summary>
    /// Whether this weapon is currently active and should check collisions.
    /// </summary>
    bool IsActive { get; }
}
