using UnityEngine;

/// <summary>
/// Orbiter projectile that circles the player and despawns on enemy contact.
/// Respawns after a delay.
/// </summary>
public class OrbiterProjectile : BaseProjectile, ICollidable
{
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float orbitSpeed = 2f; // Radians per second
    [SerializeField] private float respawnDelay = 2f;
    
    private Transform playerTransform;
    private float currentAngle;
    private float respawnTimer;
    
    public override float CollisionRadius => 0.35f;
    
    // ICollidable implementation
    public Vector3 Position => transform.position;
    public CollisionLayer Layer => CollisionLayer.Orbiter;
    
    public void Initialize(Transform player, float startAngle, float speed = 2f, float radius = 2f)
    {
        playerTransform = player;
        currentAngle = startAngle;
        orbitSpeed = speed;
        orbitRadius = radius;
        respawnTimer = 0f;
        Activate();
    }
    
    public void SetDamage(float newDamage) => damage = newDamage;
    public void SetOrbitSpeed(float speed) => orbitSpeed = speed;
    public void SetOrbitRadius(float radius) => orbitRadius = radius;
    
    public override void Deactivate()
    {
        isActive = false;
        respawnTimer = respawnDelay;
        DebugLog.Info($"[ORBITER] Deactivated, will respawn in {respawnDelay}s at position {transform.position}");
        // Keep GameObject active but hide sprite during respawn
        if (spriteRenderer != null) 
            spriteRenderer.enabled = false;
    }
    
    protected override void Update()
    {
        if (GameState.IsPaused) return;
        
        if (playerTransform == null) return;
        
        if (!isActive)
        {
            // Count down respawn timer
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                Activate();
                DebugLog.Info($"[ORBITER] Respawned at angle={currentAngle:F2}, position={transform.position}");
            }
            return;
        }
        
        UpdateMovement();
    }
    
    protected override void UpdateMovement()
    {
        // Orbit around player
        currentAngle += orbitSpeed * Time.deltaTime;
        if (currentAngle > Mathf.PI * 2f)
            currentAngle -= Mathf.PI * 2f;
        
        float x = playerTransform.position.x + Mathf.Cos(currentAngle) * orbitRadius;
        float y = playerTransform.position.y + Mathf.Sin(currentAngle) * orbitRadius;
        transform.position = new Vector3(x, y, 0f);
    }
}
