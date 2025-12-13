using UnityEngine;

/// <summary>
/// Orbiter projectile that circles the player and despawns on enemy contact.
/// Respawns after a delay.
/// </summary>
public class OrbiterProjectile : BaseProjectile
{
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float orbitSpeed = 2f; // Radians per second
    [SerializeField] private float respawnDelay = 2f;
    
    private Transform playerTransform;
    private float currentAngle;
    private float respawnTimer;
    
    public override float CollisionRadius => 0.35f; // Larger collision for orbiters
    
    public void Initialize(Transform player, float startAngle)
    {
        playerTransform = player;
        currentAngle = startAngle;
        respawnTimer = 0f;
        Activate();
    }
    
    public override void Deactivate()
    {
        isActive = false;
        respawnTimer = respawnDelay;
        Debug.Log($"[ORBITER] Deactivated, will respawn in {respawnDelay}s at position {transform.position}");
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
                Debug.Log($"[ORBITER] Respawned at angle={currentAngle:F2}, position={transform.position}");
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
