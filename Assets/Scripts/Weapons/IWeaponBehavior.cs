using UnityEngine;

/// <summary>
/// Context object passed to weapon behaviors.
/// Contains all information needed to execute weapon attacks.
/// </summary>
public class WeaponContext
{
    /// <summary>
    /// The weapon definition (stats, settings, etc.)
    /// </summary>
    public WeaponDefinition Definition { get; set; }

    /// <summary>
    /// Player transform for positioning/targeting
    /// </summary>
    public Transform PlayerTransform { get; set; }

    /// <summary>
    /// Player component for stat multipliers
    /// </summary>
    public Player Player { get; set; }

    /// <summary>
    /// Current calculated damage (after multipliers)
    /// </summary>
    public float CurrentDamage { get; set; }

    /// <summary>
    /// Current calculated fire rate (after multipliers)
    /// </summary>
    public float CurrentFireRate { get; set; }

    /// <summary>
    /// Current calculated projectile count (after bonuses)
    /// </summary>
    public int CurrentProjectileCount { get; set; }

    /// <summary>
    /// Current calculated range (after multipliers)
    /// </summary>
    public float CurrentRange { get; set; }

    /// <summary>
    /// Current calculated projectile size (after multipliers)
    /// </summary>
    public float CurrentProjectileSize { get; set; }

    /// <summary>
    /// Current calculated pierce count
    /// </summary>
    public int CurrentPierce { get; set; }

    /// <summary>
    /// Current weapon level (1-5)
    /// </summary>
    public int WeaponLevel { get; set; } = 1;

    /// <summary>
    /// Whether weapon is at max level (level 5)
    /// </summary>
    public bool IsMaxLevel => WeaponLevel >= Weapon.MaxLevel;

    /// <summary>
    /// Reference to the GenericWeapon MonoBehaviour.
    /// Used for accessing Unity-specific functionality.
    /// </summary>
    public MonoBehaviour WeaponMonoBehaviour { get; set; }
}

/// <summary>
/// Interface for weapon behavior strategies.
/// Each behavior type implements this to define how the weapon attacks.
/// </summary>
public interface IWeaponBehavior
{
    /// <summary>
    /// Execute the weapon's primary attack.
    /// Called by GenericWeapon when fire timer triggers.
    /// </summary>
    void Execute(WeaponContext context);

    /// <summary>
    /// Update called every frame.
    /// Used for continuous behaviors (beams, orbiters, etc.)
    /// </summary>
    void OnUpdate(WeaponContext context);

    /// <summary>
    /// Called when weapon stats change (player leveled up, item equipped, etc.)
    /// </summary>
    void OnStatsChanged(WeaponContext context);

    /// <summary>
    /// Initialize the behavior with context.
    /// Called when weapon is first created.
    /// </summary>
    void Initialize(WeaponContext context);

    /// <summary>
    /// Cleanup when weapon is destroyed or disabled.
    /// </summary>
    void Cleanup();
}

/// <summary>
/// Base implementation of IWeaponBehavior with default empty methods.
/// Behaviors can override only what they need.
/// </summary>
public abstract class WeaponBehaviorBase : IWeaponBehavior
{
    protected WeaponContext cachedContext;

    public virtual void Initialize(WeaponContext context)
    {
        cachedContext = context;
    }

    public abstract void Execute(WeaponContext context);

    public virtual void OnUpdate(WeaponContext context)
    {
        // Default: no per-frame behavior
    }

    public virtual void OnStatsChanged(WeaponContext context)
    {
        cachedContext = context;
    }

    public virtual void Cleanup()
    {
        cachedContext = null;
    }

    /// <summary>
    /// Utility: Find nearest enemy using spatial grid query.
    /// </summary>
    protected Enemy FindNearestEnemy(Vector3 position, float maxRange, Enemy exclude = null)
    {
        CollisionManager collisionMgr = GameServices.CollisionManager;
        if (collisionMgr == null)
        {
            DebugLog.Error("[WeaponBehavior] CollisionManager is NULL! Cannot find enemies.");
            return null;
        }

        var nearbyEntities = collisionMgr.QueryNearbyEnemies(position, maxRange);

        Enemy nearest = null;
        float nearestDistance = maxRange;

        foreach (var entity in nearbyEntities)
        {
            if (!(entity is Enemy enemy) || !enemy.IsActive) continue;
            if (exclude != null && enemy == exclude) continue;

            float distance = Vector3.Distance(position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }
}
