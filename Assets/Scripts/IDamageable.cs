using UnityEngine;

/// <summary>
/// Interface for any entity that can take damage.
/// Implement this on Player, Enemies, and future destructible objects.
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