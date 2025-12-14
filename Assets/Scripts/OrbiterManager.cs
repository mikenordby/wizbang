using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages orbiter projectiles that circle the player.
/// </summary>
public class OrbiterManager : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    private int maxOrbiters = 8; // Not serialized - prevents Inspector override limiting to 2
    [SerializeField] private float orbitSpeed = 1f;
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float damage = 15f;
    
    private List<OrbiterProjectile> orbiters;
    private List<OrbiterProjectile> cachedActiveOrbiters = new List<OrbiterProjectile>();
    private int lastCacheFrame = -1;
    private int currentOrbiterCount = 0;
    
    private void Start()
    {
        if (playerTransform == null)
            playerTransform = GameObject.Find("Player")?.transform;
        
        orbiters = new List<OrbiterProjectile>();
        
        // Don't create orbiters by default - let OrbiterWeapon control this
        DebugLog.Info($"OrbiterManager: Initialized (waiting for weapon activation)");
    }
    
    /// <summary>
    /// Set number of active orbiters
    /// </summary>
    public void SetOrbiterCount(int count)
    {
        int requestedCount = count;
        count = Mathf.Clamp(count, 0, maxOrbiters);
        
        DebugLog.Info($"[OrbiterManager.SetOrbiterCount] CALLED: requested={requestedCount}, clamped={count}, current={currentOrbiterCount}, orbiters.Count={orbiters.Count}");
        
        if (count == currentOrbiterCount)
        {
            DebugLog.Info($"[OrbiterManager.SetOrbiterCount] No change needed, count={count} already matches");
            return;
        }
        
        // Add new orbiters
        while (orbiters.Count < count)
        {
            DebugLog.Info($"[OrbiterManager.SetOrbiterCount] Creating new orbiter #{orbiters.Count}");
            CreateOrbiter(orbiters.Count);
        }
        
        // Activate/deactivate orbiters
        int activeCount = 0;
        for (int i = 0; i < orbiters.Count; i++)
        {
            if (i < count)
            {
                orbiters[i].gameObject.SetActive(true);
                activeCount++;
                // Reposition to new spacing
                float angle = (Mathf.PI * 2f / count) * i;
                orbiters[i].Initialize(playerTransform, angle, orbitSpeed, orbitRadius);
                DebugLog.Info($"[OrbiterManager.SetOrbiterCount] Activated orbiter #{i} at angle {angle * Mathf.Rad2Deg}°");
            }
            else
            {
                orbiters[i].gameObject.SetActive(false);
            }
        }
        
        currentOrbiterCount = count;
        DebugLog.Info($"[OrbiterManager.SetOrbiterCount] ✓ COMPLETE: {activeCount} orbiters now active");
    }
    
    /// <summary>
    /// Set orbiter damage
    /// </summary>
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
        foreach (var orbiter in orbiters)
        {
            if (orbiter != null)
                orbiter.SetDamage(damage);
        }
    }
    
    /// <summary>
    /// Set orbit speed multiplier
    /// </summary>
    public void SetOrbitSpeed(float speed)
    {
        orbitSpeed = speed;
        foreach (var orbiter in orbiters)
        {
            if (orbiter != null)
                orbiter.SetOrbitSpeed(orbitSpeed);
        }
    }
    
    /// <summary>
    /// Set orbit radius
    /// </summary>
    public void SetOrbitRadius(float radius)
    {
        orbitRadius = radius;
        foreach (var orbiter in orbiters)
        {
            if (orbiter != null)
                orbiter.SetOrbitRadius(orbitRadius);
        }
    }
    
    private void CreateOrbiter(int index)
    {
        if (playerTransform == null) return;
        
        // Create orbiter GameObject
        GameObject orbiterObj = new GameObject($"Orbiter_{index}");
        orbiterObj.transform.parent = transform;
        orbiterObj.transform.localScale = Vector3.one * 1.5f; // Much larger knives
        
        // Load orbiter sprite
        SpriteRenderer spriteRenderer = orbiterObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = SpriteLoader.LoadOrbiterSprite();
        
        // Add CircleCollider2D
        CircleCollider2D collider = orbiterObj.AddComponent<CircleCollider2D>();
        collider.radius = 0.35f;
        collider.isTrigger = true;
        
        // Add orbiter component
        OrbiterProjectile orbiter = orbiterObj.AddComponent<OrbiterProjectile>();
        
        // Initialize (will be repositioned when SetOrbiterCount is called)
        float startAngle = 0f;
        orbiter.Initialize(playerTransform, startAngle, orbitSpeed, orbitRadius);
        orbiter.SetDamage(damage);
        
        orbiterObj.SetActive(false); // Start inactive
        orbiters.Add(orbiter);
    }
    
    /// <summary>
    /// Get active orbiters with frame-based caching (no GC per frame)
    /// </summary>
    public List<OrbiterProjectile> GetActiveOrbiters()
    {
        // Cache active list per frame (orbiters queried multiple times per frame)
        if (Time.frameCount != lastCacheFrame)
        {
            cachedActiveOrbiters.Clear();
            foreach (var orbiter in orbiters)
            {
                if (orbiter != null && orbiter.IsActive)
                    cachedActiveOrbiters.Add(orbiter);
            }
            lastCacheFrame = Time.frameCount;
        }
        return cachedActiveOrbiters;
    }
}
