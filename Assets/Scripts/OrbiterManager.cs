using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages orbiter projectiles that circle the player.
/// </summary>
public class OrbiterManager : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private int maxOrbiters = 2;
    
    private List<OrbiterProjectile> orbiters;
    
    private void Start()
    {
        if (playerTransform == null)
            playerTransform = GameObject.Find("Player")?.transform;
        
        orbiters = new List<OrbiterProjectile>();
        
        // Create orbiters evenly spaced
        for (int i = 0; i < maxOrbiters; i++)
        {
            CreateOrbiter(i);
        }
        
        Debug.Log($"OrbiterManager: Created {maxOrbiters} orbiters");
    }
    
    private void CreateOrbiter(int index)
    {
        if (playerTransform == null) return;
        
        // Create orbiter GameObject
        GameObject orbiterObj = new GameObject($"Orbiter_{index}");
        orbiterObj.transform.parent = transform;
        orbiterObj.transform.localScale = Vector3.one * 0.3f;
        
        // Add sprite renderer with orbiter sprite
        SpriteRenderer spriteRenderer = orbiterObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = SpriteGenerator.CreateOrbiterSprite();
        
        // Add CircleCollider2D for collision detection
        CircleCollider2D collider = orbiterObj.AddComponent<CircleCollider2D>();
        collider.radius = 0.35f;
        collider.isTrigger = true; // Use trigger for projectile-like behavior
        
        // Add orbiter component
        OrbiterProjectile orbiter = orbiterObj.AddComponent<OrbiterProjectile>();
        
        // Space orbiters evenly around circle
        float startAngle = (Mathf.PI * 2f / maxOrbiters) * index;
        orbiter.Initialize(playerTransform, startAngle);
        
        orbiters.Add(orbiter);
    }
    
    public List<OrbiterProjectile> GetActiveOrbiters()
    {
        return orbiters.FindAll(o => o.IsActive);
    }
}
