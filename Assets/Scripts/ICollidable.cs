using UnityEngine;

/// <summary>
/// Interface for all entities that participate in collision detection.
/// Enables polymorphic collision handling through spatial hash grid.
/// </summary>
public interface ICollidable
{
    /// <summary>
    /// Current world position of the entity
    /// </summary>
    Vector3 Position { get; }
    
    /// <summary>
    /// Collision radius for circular collision detection
    /// </summary>
    float CollisionRadius { get; }
    
    /// <summary>
    /// Whether this entity should be checked for collisions
    /// </summary>
    bool IsActive { get; }
    
    /// <summary>
    /// Layer mask for collision filtering
    /// </summary>
    CollisionLayer Layer { get; }
}

/// <summary>
/// Collision layer flags for filtering what collides with what.
/// Uses bit flags for efficient masking.
/// </summary>
[System.Flags]
public enum CollisionLayer
{
    None = 0,
    Player = 1 << 0,        // 1
    Enemy = 1 << 1,         // 2
    Projectile = 1 << 2,    // 4
    Orbiter = 1 << 3,       // 8
    XPOrb = 1 << 4,         // 16
    Boomerang = 1 << 5      // 32
}
