using UnityEngine;

/// <summary>
/// Interface for any entity that can take damage.
/// Implemented by Health component.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Apply damage to this entity
    /// </summary>
    /// <param name="damage">Amount of damage to deal</param>
    /// <returns>True if entity died from this damage</returns>
    bool TakeDamage(float damage);
    
    /// <summary>
    /// Check if entity is currently alive
    /// </summary>
    bool IsAlive { get; }
    
    /// <summary>
    /// Transform for position/collision calculations
    /// </summary>
    Transform Transform { get; }
}

