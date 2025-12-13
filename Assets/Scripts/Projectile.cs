using UnityEngine;

/// <summary>
/// Straight-line projectile that moves in a single direction.
/// Highly performant - uses transform instead of physics.
/// </summary>
public class Projectile : BaseProjectile
{
    [SerializeField] private float baseSpeed = 8f;
    [SerializeField] private float lifetime = 10f;
    
    private Vector3 direction;
    private float speed;
    private float lifetimeRemaining;
    
    public override float CollisionRadius => 0.15f; // Smaller collision for straight projectiles
    
    /// <summary>
    /// Activate projectile with straight movement
    /// </summary>
    public void ActivateStraight(Vector3 startPos, Vector3 targetDirection)
    {
        transform.position = startPos;
        direction = targetDirection.normalized;
        speed = baseSpeed;
        lifetimeRemaining = lifetime;
        
        Activate(); // Call base activation
        
        Debug.Log($"Projectile.Activate: pos={startPos}, dir={direction}, speed={speed}");
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