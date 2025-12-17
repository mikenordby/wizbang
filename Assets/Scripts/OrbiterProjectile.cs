using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Orbiter projectile that circles the player continuously.
/// Tracks hit enemies to prevent double-hitting the same target.
/// </summary>
public class OrbiterProjectile : BaseProjectile, ICollidable
{
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float orbitSpeed = 5f; // Increased from 2f to 5f - Radians per second
    [SerializeField] private float respawnDelay = 2f;
    private float hitCooldown = 0.5f; // Dynamic: set by weapon based on fire rate (1 / fireRate)
    
    private Transform playerTransform;
    private OrbiterManager orbiterManager;
    private float respawnTimer;
    private Dictionary<int, float> hitEnemies = new Dictionary<int, float>(); // enemyInstanceID -> time of last hit
    
    // Public so other orbiters can check our angle for respawn spacing
    public float currentAngle { get; private set; }
    
    public override float CollisionRadius => 0.25f; // Knife: 64px sprite, full blade width
    
    // ICollidable implementation
    public new Vector3 Position => transform.position;
    public override CollisionLayer Layer => CollisionLayer.Orbiter;
    
    public void Initialize(Transform player, float startAngle, float speed = 2f, float radius = 2f)
    {
        playerTransform = player;
        currentAngle = startAngle;
        orbitSpeed = speed;
        orbitRadius = radius;
        respawnTimer = 0f;
        hitEnemies.Clear(); // Reset hit tracking
        
        // Get reference to OrbiterManager
        if (orbiterManager == null)
        {
            orbiterManager = GetComponentInParent<OrbiterManager>();
        }
        
        Activate();
    }
    
    public void SetDamage(float newDamage) => damage = newDamage;
    public void SetOrbitSpeed(float speed) => orbitSpeed = speed;
    public void SetOrbitRadius(float radius) => orbitRadius = radius;
    public void SetHitCooldown(float cooldown) => hitCooldown = cooldown;
    public void SetSize(float sizeMultiplier)
    {
        size = sizeMultiplier;
        transform.localScale = Vector3.one * size;
    }
    
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
        
        // Clean up expired hit cooldowns
        var expiredKeys = new List<int>();
        foreach (var kvp in hitEnemies)
        {
            if (Time.time - kvp.Value > hitCooldown)
            {
                expiredKeys.Add(kvp.Key);
            }
        }
        foreach (var key in expiredKeys)
        {
            hitEnemies.Remove(key);
        }
        
        if (!isActive)
        {
            // Count down respawn timer
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                // Find best respawn angle (opposite to active orbiters)
                currentAngle = FindBestRespawnAngle();
                Activate();
                DebugLog.Info($"[ORBITER] Respawned at angle={currentAngle:F2} rad ({currentAngle * Mathf.Rad2Deg:F1} deg), position={transform.position}");
            }
            return;
        }
        
        UpdateMovement();
    }
    
    /// <summary>
    /// Find the best angle to respawn at - opposite to existing active orbiters
    /// </summary>
    private float FindBestRespawnAngle()
    {
        if (orbiterManager == null)
        {
            // Fallback: random angle
            return Random.Range(0f, Mathf.PI * 2f);
        }
        
        var activeOrbiters = orbiterManager.GetActiveOrbiters();
        
        // If no other orbiters or only this one, use random angle
        if (activeOrbiters == null || activeOrbiters.Count <= 1)
        {
            return Random.Range(0f, Mathf.PI * 2f);
        }
        
        // Calculate average angle of all active orbiters (excluding this one)
        float sumX = 0f;
        float sumY = 0f;
        int count = 0;
        
        foreach (var orbiter in activeOrbiters)
        {
            if (orbiter == null || orbiter == this || !orbiter.IsActive) continue;
            
            sumX += Mathf.Cos(orbiter.currentAngle);
            sumY += Mathf.Sin(orbiter.currentAngle);
            count++;
        }
        
        if (count == 0)
        {
            // No other active orbiters, use random
            return Random.Range(0f, Mathf.PI * 2f);
        }
        
        // Average angle of active orbiters
        float avgAngle = Mathf.Atan2(sumY / count, sumX / count);
        
        // Spawn opposite (add PI radians = 180 degrees)
        float oppositeAngle = avgAngle + Mathf.PI;
        
        // Normalize to 0-2PI range
        while (oppositeAngle < 0f) oppositeAngle += Mathf.PI * 2f;
        while (oppositeAngle >= Mathf.PI * 2f) oppositeAngle -= Mathf.PI * 2f;
        
        return oppositeAngle;
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
    
    /// <summary>
    /// Check if this orbiter can hit the given enemy (not on cooldown)
    /// </summary>
    public bool CanHitEnemy(int enemyInstanceID)
    {
        if (!hitEnemies.ContainsKey(enemyInstanceID))
            return true;
            
        return Time.time - hitEnemies[enemyInstanceID] > hitCooldown;
    }
    
    /// <summary>
    /// Record that this orbiter hit an enemy
    /// </summary>
    public void RecordHit(int enemyInstanceID)
    {
        hitEnemies[enemyInstanceID] = Time.time;
    }
}
