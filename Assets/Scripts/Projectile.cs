using UnityEngine;

/// <summary>
/// Straight-line projectile that moves in a single direction.
/// Highly performant - uses transform instead of physics.
/// </summary>
public class Projectile : BaseProjectile, ICollidable
{
    [SerializeField] private float baseSpeed = 8f;
    [SerializeField] private float lifetime = 10f;
    
    private Vector3 direction;
    private float speed;
    private float lifetimeRemaining;
    
    public override float CollisionRadius => 0.2f; // Knife tip: 64px sprite, blade is ~25px wide
    
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
        
        // Rotate sprite to face movement direction (so flame tail trails behind)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        Activate(); // Call base activation
        
        DebugLog.Verbose($"[Projectile] ActivateStraight: pos=({startPos.x:F2},{startPos.y:F2}) dir=({direction.x:F2},{direction.y:F2}) damage={Damage:F1} pierce={Pierce}");
    }
    
    protected override void UpdateMovement()
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
    
    // Future: Add curved, homing, and other movement patterns
    // public void ActivateHoming(Vector3 startPos, Transform target) { }
    // public void ActivateCurved(Vector3 startPos, Vector3 direction, float curveAmount) { }
}