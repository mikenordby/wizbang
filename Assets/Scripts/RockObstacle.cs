using UnityEngine;

/// <summary>
/// Rock obstacle that blocks player and enemy movement but not projectiles.
/// Procedurally generated sprite with grey rock texture.
/// </summary>
public class RockObstacle : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D obstacleCollider;
    private static Sprite rockSprite; // Cached sprite shared by all rocks
    
    private void Awake()
    {
        // Create sprite renderer
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = LoadRockSprite();
        spriteRenderer.sortingOrder = 10; // Above projectiles (5) and grass (-100)
        
        // Create collision (physical, not trigger)
        // Rock sprite: 32px diameter at 16 PPU = 2 world units diameter = 1.0 radius
        // Visual rock is roughly 24-28px of usable space = 1.5-1.75 world units
        // Reduce radius to match visible rock shape more closely
        obstacleCollider = gameObject.AddComponent<CircleCollider2D>();
        obstacleCollider.radius = 0.6f; // Tight fit to visible rock core (20% smaller)
        obstacleCollider.isTrigger = false; // Physical collision blocks movement

        // Add static Rigidbody2D for proper collision detection
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static; // Immovable object

        // Set layer to Obstacles (will create if needed)
        int obstacleLayer = LayerMask.NameToLayer("Obstacles");
        if (obstacleLayer == -1)
        {
            DebugLog.Warning("[RockObstacle] 'Obstacles' layer not found - create it in Project Settings > Tags and Layers");
            obstacleLayer = 0; // Default layer as fallback
        }
        gameObject.layer = obstacleLayer;
    }
    
    /// <summary>
    /// Load rock sprite from Resources (PixelLab generated)
    /// </summary>
    private Sprite LoadRockSprite()
    {
        // Load cached sprite if available
        if (rockSprite != null)
        {
            return rockSprite;
        }

        // Load from Resources
        rockSprite = Resources.Load<Sprite>("Sprites/Objects/rock_medium_mossy");
        if (rockSprite == null)
        {
            DebugLog.Error("[RockObstacle] MISSING SPRITE: Sprites/Objects/rock_medium_mossy");
        }

        return rockSprite;
    }
    
    /// <summary>
    /// Initialize rock (size is fixed, no scaling)
    /// </summary>
    public void Initialize(float rockSize)
    {
        // Rock size is fixed at creation, no need to scale
    }
}
