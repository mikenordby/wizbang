using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages orbiter projectiles that circle the player.
/// </summary>
public class OrbiterManager : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private int maxOrbiters = 8;
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
        count = Mathf.Clamp(count, 0, maxOrbiters);
        
        if (count == currentOrbiterCount) return;
        
        // Add new orbiters
        while (orbiters.Count < count)
        {
            CreateOrbiter(orbiters.Count);
        }
        
        // Activate/deactivate orbiters
        for (int i = 0; i < orbiters.Count; i++)
        {
            if (i < count)
            {
                orbiters[i].gameObject.SetActive(true);
                // Reposition to new spacing
                float angle = (Mathf.PI * 2f / count) * i;
                orbiters[i].Initialize(playerTransform, angle, orbitSpeed, orbitRadius);
            }
            else
            {
                orbiters[i].gameObject.SetActive(false);
            }
        }
        
        currentOrbiterCount = count;
        DebugLog.Info($"OrbiterManager: Set orbiter count to {count}");
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
        orbiterObj.transform.localScale = Vector3.one * 0.3f;
        
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
