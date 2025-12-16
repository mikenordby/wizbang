using UnityEngine;

/// <summary>
/// Rock obstacle that blocks player and enemy movement but not projectiles.
/// Procedurally generated sprite with grey rock texture.
/// </summary>
public class RockObstacle : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D obstacleCollider;
    
    private void Awake()
    {
        // Create sprite renderer
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateRockSprite();
        spriteRenderer.sortingOrder = 10; // Above projectiles (5) and grass (-100)
        
        // Create collision (physical, not trigger)
        // Rock sprite: 32px diameter at 16 PPU = 2 world units diameter = 1.0 radius
        // Visual circle has radius 0.45 of sprite (14.4px) = 0.9 world units
        obstacleCollider = gameObject.AddComponent<CircleCollider2D>();
        obstacleCollider.radius = 0.9f; // Match visual rock radius exactly
        obstacleCollider.isTrigger = false; // Physical collision blocks movement
        
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
    /// Create a procedural rock sprite with grey texture and noise
    /// </summary>
    private Sprite CreateRockSprite()
    {
        int resolution = 32;
        Texture2D texture = new Texture2D(resolution, resolution);
        texture.filterMode = FilterMode.Point;
        
        // Rock colors (grey scale)
        Color rockBase = new Color(0.5f, 0.5f, 0.5f);
        Color rockDark = new Color(0.3f, 0.3f, 0.3f);
        Color rockLight = new Color(0.65f, 0.65f, 0.65f);
        
        // Draw circular rock with noise texture
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float radius = resolution * 0.45f;
        
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                
                if (dist < radius)
                {
                    // Add Perlin noise for rock texture
                    float noise = Mathf.PerlinNoise(x * 0.15f, y * 0.15f);
                    Color rockColor = Color.Lerp(rockDark, rockBase, noise);
                    
                    // Add some lighter spots for highlights
                    float highlightNoise = Mathf.PerlinNoise(x * 0.3f + 10f, y * 0.3f + 10f);
                    if (highlightNoise > 0.7f)
                    {
                        rockColor = Color.Lerp(rockColor, rockLight, (highlightNoise - 0.7f) * 3f);
                    }
                    
                    // Darken edges for 3D effect
                    float edgeFade = 1f - (dist / radius);
                    edgeFade = Mathf.Pow(edgeFade, 0.7f); // Softer falloff
                    rockColor = Color.Lerp(rockDark, rockColor, edgeFade);
                    
                    texture.SetPixel(x, y, rockColor);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 16);
    }
    
    /// <summary>
    /// Initialize rock (size is fixed, no scaling)
    /// </summary>
    public void Initialize(float rockSize)
    {
        // Rock size is fixed at creation, no need to scale
    }
}
