using UnityEngine;

/// <summary>
/// Boomerang projectile that flies in an arc and returns to the player.
/// Can hit enemies on both outward and return journeys.
/// </summary>
public class BoomerangProjectile : BaseProjectile, ICollidable
{
    [SerializeField] private float outSpeed = 10f;
    [SerializeField] private float returnSpeed = 12f;
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private float arcHeight = 2f;
    
    private Transform playerTransform;
    private Vector3 throwDirection;
    private Vector3 startPosition;
    private float travelDistance;
    private bool isReturning;
    private float lifetime;
    private float maxLifetime = 5f;
    
    public override float CollisionRadius => 0.3f; // Boomerang arc blade
    
    // ICollidable implementation
    public Vector3 Position => transform.position;
    public CollisionLayer Layer => CollisionLayer.Projectile;
    
    /// <summary>
    /// Activate boomerang with throw direction
    /// </summary>
    public void ActivateArc(Vector3 startPos, Vector3 direction, Transform player)
    {
        transform.position = startPos;
        startPosition = startPos;
        throwDirection = direction.normalized;
        playerTransform = player;
        travelDistance = 0f;
        isReturning = false;
        lifetime = 0f;
        
        // Rotate sprite to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        Activate();
        
        DebugLog.Verbose($"[BoomerangProjectile] Activated at {startPos}, direction={direction}, damage={Damage:F1} pierce={Pierce}");
    }
    
    protected override void UpdateMovement()
    {
        lifetime += Time.deltaTime;
        
        // Deactivate after max lifetime (safety)
        if (lifetime > maxLifetime)
        {
            Deactivate();
            return;
        }
        
        if (!isReturning)
        {
            // Outward journey: arc away from player
            float speed = outSpeed * Time.deltaTime;
            travelDistance += speed;
            
            // Calculate arc position
            float progress = travelDistance / maxDistance;
            float arcOffset = Mathf.Sin(progress * Mathf.PI) * arcHeight;
            
            // Perpendicular direction for arc
            Vector3 perpendicular = new Vector3(-throwDirection.y, throwDirection.x, 0);
            Vector3 targetPos = startPosition + throwDirection * travelDistance + perpendicular * arcOffset;
            
            transform.position = targetPos;
            
            // Rotate based on movement
            Vector3 moveDir = (targetPos - transform.position).normalized;
            if (moveDir != Vector3.zero)
            {
                float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            
            // Start returning when max distance reached
            if (travelDistance >= maxDistance)
            {
                isReturning = true;
                DebugLog.Verbose("[BoomerangProjectile] Starting return journey");
            }
        }
        else
        {
            // Return journey: fly back to player
            if (playerTransform == null)
            {
                Deactivate();
                return;
            }
            
            Vector3 toPlayer = (playerTransform.position - transform.position);
            float distanceToPlayer = toPlayer.magnitude;
            
            // Return complete if close to player
            if (distanceToPlayer < 0.5f)
            {
                Deactivate();
                DebugLog.Verbose("[BoomerangProjectile] Returned to player");
                return;
            }
            
            // Move toward player
            Vector3 direction = toPlayer.normalized;
            transform.position += direction * returnSpeed * Time.deltaTime;
            
            // Rotate to face player
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // Add spin for effect
            transform.Rotate(0, 0, 360f * Time.deltaTime * 5f);
        }
    }
}
