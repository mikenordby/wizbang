using UnityEngine;

/// <summary>
/// Movement type for projectiles.
/// </summary>
public enum ProjectileMovementType
{
    Straight,  // Default - moves in fixed direction
    Homing,    // Tracks a target
    Orbital,   // Orbits around a point (future)
    Boomerang  // Returns to origin (future)
}

/// <summary>
/// Versatile projectile that supports multiple movement types.
/// Highly performant - uses transform instead of physics.
/// </summary>
public class Projectile : BaseProjectile, ICollidable
{
    [SerializeField] private float baseSpeed = 8f;
    [SerializeField] private float lifetime = 10f;

    private Vector3 direction;
    private float speed;
    private float lifetimeRemaining;
    private ProjectileMovementType movementType = ProjectileMovementType.Straight;

    // Homing-specific
    private Transform homingTarget;
    private float homingStrength;

    // Orbital-specific (for future use)
    private Transform orbitCenter;
    private float orbitRadius;
    private float orbitAngle;
    private float orbitSpeed;

    // ICollidable implementation
    public new Vector3 Position => transform.position;
    public override CollisionLayer Layer => CollisionLayer.Projectile;

    /// <summary>
    /// Activate projectile with straight movement
    /// </summary>
    public void ActivateStraight(Vector3 startPos, Vector3 targetDirection)
    {
        transform.position = startPos;
        direction = targetDirection.normalized;
        speed = baseSpeed;
        lifetimeRemaining = lifetime;
        movementType = ProjectileMovementType.Straight;
        homingTarget = null;

        // Rotate sprite to face movement direction (so flame tail trails behind)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Activate(); // Call base activation

        DebugLog.Verbose($"[Projectile] ActivateStraight: pos=({startPos.x:F2},{startPos.y:F2}) dir=({direction.x:F2},{direction.y:F2}) damage={Damage:F1} pierce={Pierce}");
    }

    /// <summary>
    /// Activate projectile with homing movement that tracks a target.
    /// </summary>
    public void ActivateHoming(Vector3 startPos, Vector3 initialDirection, Transform target, float strength = 5f)
    {
        transform.position = startPos;
        direction = initialDirection.normalized;
        speed = baseSpeed;
        lifetimeRemaining = lifetime;
        movementType = ProjectileMovementType.Homing;
        homingTarget = target;
        homingStrength = strength;

        // Initial rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Activate();

        DebugLog.Verbose($"[Projectile] ActivateHoming: pos=({startPos.x:F2},{startPos.y:F2}) target={target?.name ?? "null"} strength={strength:F1}");
    }

    /// <summary>
    /// Activate projectile with orbital movement around a center point.
    /// </summary>
    public void ActivateOrbital(Transform center, float radius, float speed, float startAngle = 0f)
    {
        orbitCenter = center;
        orbitRadius = radius;
        orbitSpeed = speed;
        orbitAngle = startAngle;
        movementType = ProjectileMovementType.Orbital;
        lifetimeRemaining = float.MaxValue; // Orbitals don't expire by time

        // Set initial position
        if (center != null)
        {
            float x = center.position.x + Mathf.Cos(orbitAngle) * orbitRadius;
            float y = center.position.y + Mathf.Sin(orbitAngle) * orbitRadius;
            transform.position = new Vector3(x, y, 0f);
        }

        Activate();

        DebugLog.Verbose($"[Projectile] ActivateOrbital: center={center?.name ?? "null"} radius={radius:F1} speed={speed:F1}");
    }

    protected override void UpdateMovement()
    {
        switch (movementType)
        {
            case ProjectileMovementType.Straight:
                UpdateStraightMovement();
                break;
            case ProjectileMovementType.Homing:
                UpdateHomingMovement();
                break;
            case ProjectileMovementType.Orbital:
                UpdateOrbitalMovement();
                break;
        }
    }

    private void UpdateStraightMovement()
    {
        // Move in direction
        transform.position += direction * speed * Time.deltaTime;

        // Decrease lifetime
        lifetimeRemaining -= Time.deltaTime;
        if (lifetimeRemaining <= 0f)
        {
            Deactivate();
        }
    }

    private void UpdateHomingMovement()
    {
        // If target exists and is active, adjust direction toward it
        if (homingTarget != null && homingTarget.gameObject.activeInHierarchy)
        {
            Vector3 toTarget = (homingTarget.position - transform.position).normalized;
            direction = Vector3.Lerp(direction, toTarget, homingStrength * Time.deltaTime).normalized;
        }

        // Move in current direction
        transform.position += direction * speed * Time.deltaTime;

        // Update rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Decrease lifetime
        lifetimeRemaining -= Time.deltaTime;
        if (lifetimeRemaining <= 0f)
        {
            Deactivate();
        }
    }

    private void UpdateOrbitalMovement()
    {
        if (orbitCenter == null)
        {
            Deactivate();
            return;
        }

        orbitAngle += orbitSpeed * Time.deltaTime;
        if (orbitAngle >= Mathf.PI * 2f)
            orbitAngle -= Mathf.PI * 2f;

        float x = orbitCenter.position.x + Mathf.Cos(orbitAngle) * orbitRadius;
        float y = orbitCenter.position.y + Mathf.Sin(orbitAngle) * orbitRadius;
        transform.position = new Vector3(x, y, transform.position.z);

        // Rotate to face movement direction (tangent to orbit)
        float tangentAngle = orbitAngle + Mathf.PI / 2f;
        transform.rotation = Quaternion.Euler(0, 0, tangentAngle * Mathf.Rad2Deg);
    }
}