using UnityEngine;

/// <summary>
/// Loads background tiles from Cainos pack or generates procedural grass.
/// Similar to SpriteLoader but for background textures.
/// </summary>
public static class BackgroundLoader
{
    private static Sprite grassTileset = null;
    private static bool hasTriedLoading = false;
    
    /// <summary>
    /// Load grass tileset from Resources or return null
    /// </summary>
    public static Sprite LoadGrassTileset()
    {
        if (hasTriedLoading && grassTileset == null)
            return null;
        
        if (grassTileset != null)
            return grassTileset;
        
        // Try to load from Resources
        grassTileset = Resources.Load<Sprite>("Backgrounds/grass_tileset");
        
        if (grassTileset != null)
        {
            DebugLog.Info("[BackgroundLoader] Loaded grass tileset from Resources");
        }
        else
        {
            DebugLog.Info("[BackgroundLoader] Grass tileset not found in Resources, will use procedural");
        }
        
        hasTriedLoading = true;
        return grassTileset;
    }
    
    /// <summary>
    /// Create a grass tile sprite (32x32) with variation
    /// </summary>
    public static Sprite CreateGrassTile(int variation = 0)
    {
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Point;
        
        Color[] pixels = new Color[size * size];
        
        // Base grass colors
        Color grassLight = new Color(0.4f, 0.7f, 0.3f);
        Color grassDark = new Color(0.35f, 0.6f, 0.25f);
        Color grassAccent = new Color(0.45f, 0.75f, 0.35f);
        
        // Seed random for consistent variation
        Random.InitState(variation);
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Base color with some random variation
                float noise = Random.Range(-0.1f, 0.1f);
                Color baseColor = Random.value > 0.7f ? grassDark : grassLight;
                
                // Add some grass blade details
                if (Random.value > 0.95f)
                {
                    baseColor = grassAccent;
                }
                
                pixels[y * size + x] = new Color(
                    Mathf.Clamp01(baseColor.r + noise),
                    Mathf.Clamp01(baseColor.g + noise),
                    Mathf.Clamp01(baseColor.b + noise),
                    1f
                );
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            32 // Pixels per unit
        );
    }
}
