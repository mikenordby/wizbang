using UnityEngine;

/// <summary>
/// Utility for generating simple sprites at runtime
/// </summary>
public static class SpriteGenerator
{
    /// <summary>
    /// Create a wizard sprite (pointed hat with robes)
    /// </summary>
    public static Sprite CreateWizardSprite()
    {
        int size = 256;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        // Clear to transparent
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        Color robeColor = new Color(0.2f, 0.3f, 0.8f); // Blue robe
        Color hatColor = new Color(0.3f, 0.1f, 0.5f); // Purple hat
        Color skinColor = new Color(1f, 0.8f, 0.6f); // Skin tone
        
        // Draw wizard (centered, facing down)
        int cx = size / 2;
        int cy = size / 2;
        
        // Body/robe (triangle/trapezoid)
        for (int y = cy - 10; y < cy + 15; y++)
        {
            int width = 8 + (y - (cy - 10)) / 2;
            for (int x = cx - width; x <= cx + width; x++)
            {
                SetPixel(pixels, size, x, y, robeColor);
            }
        }
        
        // Head (circle)
        DrawCircle(pixels, size, cx, cy - 12, 6, skinColor);
        
        // Pointed hat
        for (int y = cy - 18; y < cy - 5; y++)
        {
            int width = (cy - 5 - y) / 2;
            for (int x = cx - width; x <= cx + width; x++)
            {
                SetPixel(pixels, size, x, y, hatColor);
            }
        }
        
        // Hat brim
        for (int x = cx - 8; x <= cx + 8; x++)
            SetPixel(pixels, size, x, cy - 5, hatColor);
        
        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
    }
    
    /// <summary>
    /// Create a blob enemy sprite (round, blobby)
    /// </summary>
    public static Sprite CreateBlobSprite(Color color)
    {
        int size = 256;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        int cx = size / 2;
        int cy = size / 2;
        
        // Main blob body (squashed circle)
        DrawEllipse(pixels, size, cx, cy + 2, 14, 10, color);
        
        // Eyes (two dark dots)
        DrawCircle(pixels, size, cx - 5, cy - 2, 2, Color.black);
        DrawCircle(pixels, size, cx + 5, cy - 2, 2, Color.black);
        
        // Mouth (simple line)
        for (int x = cx - 6; x <= cx + 6; x++)
            SetPixel(pixels, size, x, cy + 4, Color.black);
        
        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
    }
    
    /// <summary>
    /// Create a skeleton enemy sprite (skull with bones)
    /// </summary>
    public static Sprite CreateSkeletonSprite(Color tint)
    {
        int size = 256;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        Color boneColor = new Color(tint.r * 0.9f, tint.g * 0.9f, tint.b * 0.9f);
        
        int cx = size / 2;
        int cy = size / 2;
        
        // Skull (roundish)
        DrawCircle(pixels, size, cx, cy - 5, 10, boneColor);
        
        // Eye sockets (dark)
        DrawCircle(pixels, size, cx - 4, cy - 7, 3, Color.black);
        DrawCircle(pixels, size, cx + 4, cy - 7, 3, Color.black);
        
        // Nose hole (triangle)
        SetPixel(pixels, size, cx, cy - 2, Color.black);
        SetPixel(pixels, size, cx - 1, cy - 1, Color.black);
        SetPixel(pixels, size, cx + 1, cy - 1, Color.black);
        
        // Jaw
        for (int x = cx - 6; x <= cx + 6; x++)
            SetPixel(pixels, size, x, cy + 2, boneColor);
        
        // Ribcage (simplified)
        for (int y = cy + 4; y < cy + 16; y += 3)
        {
            for (int x = cx - 8; x <= cx + 8; x++)
                SetPixel(pixels, size, x, y, boneColor);
        }
        
        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
    }
    
    /// <summary>
    /// Create a fireball projectile sprite
    /// </summary>
    public static Sprite CreateFireballSprite()
    {
        int size = 192;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        int cx = size / 2;
        int cy = size / 2;
        
        // Outer red glow
        DrawCircle(pixels, size, cx, cy, 10, new Color(1f, 0.3f, 0f, 0.8f));
        // Inner orange
        DrawCircle(pixels, size, cx, cy, 7, new Color(1f, 0.6f, 0f));
        // Core yellow
        DrawCircle(pixels, size, cx, cy, 4, Color.yellow);
        
        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
    }
    
    /// <summary>
    /// Create an orbiter projectile sprite (energy orb)
    /// </summary>
    public static Sprite CreateOrbiterSprite()
    {
        int size = 192;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        int cx = size / 2;
        int cy = size / 2;
        
        // Outer purple glow
        DrawCircle(pixels, size, cx, cy, 10, new Color(0.8f, 0.3f, 1f, 0.6f));
        // Inner bright purple
        DrawCircle(pixels, size, cx, cy, 7, new Color(0.9f, 0.5f, 1f));
        // Core white
        DrawCircle(pixels, size, cx, cy, 3, Color.white);
        
        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
    }
    
    /// <summary>
    /// Create an XP gem sprite (cyan glowing gem)
    /// </summary>
    public static Sprite CreateXPGemSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        // Initialize transparent
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        int cx = size / 2;
        int cy = size / 2;
        
        // Outer glow (cyan)
        DrawCircle(pixels, size, cx, cy, 18, new Color(0f, 1f, 1f, 0.3f));
        
        // Middle layer (bright cyan)
        DrawCircle(pixels, size, cx, cy, 14, new Color(0f, 0.8f, 1f, 0.8f));
        
        // Inner gem (bright white-cyan)
        DrawCircle(pixels, size, cx, cy, 10, new Color(0.7f, 1f, 1f, 1f));
        
        // Core highlight (white)
        DrawCircle(pixels, size, cx, cy, 5, Color.white);
        
        // Add sparkle in top-right
        SetPixel(pixels, size, cx + 6, cy - 6, Color.white);
        SetPixel(pixels, size, cx + 7, cy - 6, Color.white);
        SetPixel(pixels, size, cx + 6, cy - 7, Color.white);
        
        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
    }
    
    // Helper methods
    private static void SetPixel(Color[] pixels, int size, int x, int y, Color color)
    {
        // Flip Y coordinate because Unity textures are bottom-up
        y = size - 1 - y;
        if (x >= 0 && x < size && y >= 0 && y < size)
            pixels[y * size + x] = color;
    }
    
    private static void DrawCircle(Color[] pixels, int size, int cx, int cy, int radius, Color color)
    {
        for (int y = cy - radius; y <= cy + radius; y++)
        {
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                int dx = x - cx;
                int dy = y - cy;
                if (dx * dx + dy * dy <= radius * radius)
                    SetPixel(pixels, size, x, y, color);
            }
        }
    }
    
    private static void DrawEllipse(Color[] pixels, int size, int cx, int cy, int radiusX, int radiusY, Color color)
    {
        for (int y = cy - radiusY; y <= cy + radiusY; y++)
        {
            for (int x = cx - radiusX; x <= cx + radiusX; x++)
            {
                float dx = (float)(x - cx) / radiusX;
                float dy = (float)(y - cy) / radiusY;
                if (dx * dx + dy * dy <= 1f)
                    SetPixel(pixels, size, x, y, color);
            }
        }
    }
}
