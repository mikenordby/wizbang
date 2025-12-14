using UnityEngine;

/// <summary>
/// Utility for generating larger, more detailed sprites at runtime
/// </summary>
public static class SpriteGenerator
{
    /// <summary>
    /// Create a base texture with transparent background
    /// </summary>
    private static (Texture2D texture, Color[] pixels) CreateProceduralTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        // Initialize to transparent
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        
        return (texture, pixels);
    }
    
    /// <summary>
    /// Finalize texture and create sprite
    /// </summary>
    private static Sprite FinalizeSprite(Texture2D texture, Color[] pixels, int size, float pixelsPerUnit = 128)
    {
        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point; // Pixel-perfect rendering
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }
    
    /// <summary>
    /// Create a wizard sprite (pointed hat with robes) - Properly scaled for 128px
    /// </summary>
    public static Sprite CreateWizardSprite()
    {
        int size = 128; // Reduced for smaller on-screen size
        var (texture, pixels) = CreateProceduralTexture(size);
        
        Color robeColor = new Color(0.15f, 0.25f, 0.75f); // Deep blue robe
        Color robeShadow = new Color(0.1f, 0.15f, 0.5f); // Darker blue for shadows
        Color robeHighlight = new Color(0.3f, 0.45f, 0.95f); // Lighter blue for highlights
        Color hatColor = new Color(0.25f, 0.1f, 0.5f); // Purple hat
        Color hatTrim = new Color(0.9f, 0.8f, 0.2f); // Gold trim
        Color skinColor = new Color(1f, 0.85f, 0.7f); // Skin tone
        Color beardColor = new Color(0.9f, 0.9f, 0.95f); // White beard
        Color eyeColor = new Color(0.3f, 0.6f, 1f); // Glowing blue eyes
        
        int cx = size / 2;
        int cy = size / 2;
        
        // All coordinates scaled by 0.5 (128/256) from original
        
        // Draw wizard body/robe (flowing triangle shape)
        for (int y = cy - 20; y < cy + 40; y++)
        {
            int width = 16 + ((y - (cy - 20)) * 12) / 60;
            for (int x = cx - width; x <= cx + width; x++)
            {
                float distFromCenter = Mathf.Abs(x - cx) / (float)width;
                Color robeShade = Color.Lerp(robeColor, robeShadow, distFromCenter * 0.5f);
                SetPixel(pixels, size, x, y, robeShade);
            }
        }
        
        // Robe highlight on left side
        for (int y = cy - 15; y < cy + 35; y++)
        {
            int highlightX = cx - 12 - (y - (cy - 15)) / 4;
            DrawCircle(pixels, size, highlightX, y, 2, robeHighlight);
        }
        
        // Collar
        for (int x = cx - 10; x <= cx + 10; x++)
        {
            for (int y = cy - 21; y < cy - 19; y++)
            {
                SetPixel(pixels, size, x, y, robeShadow);
            }
        }
        
        // Head (scaled circle)
        DrawCircle(pixels, size, cx, cy - 28, 12, skinColor);
        
        // Face details
        // Eyes (glowing)
        DrawCircle(pixels, size, cx - 5, cy - 29, 2, eyeColor);
        DrawCircle(pixels, size, cx + 5, cy - 29, 2, eyeColor);
        DrawCircle(pixels, size, cx - 5, cy - 29, 1, Color.white);
        DrawCircle(pixels, size, cx + 5, cy - 29, 1, Color.white);
        
        // Beard (scaled)
        for (int y = cy - 25; y < cy - 15; y++)
        {
            int width = 4 + (cy - 15 - y) / 2;
            for (int x = cx - width; x <= cx + width; x++)
            {
                SetPixel(pixels, size, x, y, beardColor);
            }
        }
        
        // Pointed wizard hat (scaled to fit in texture)
        for (int y = cy - 58; y < cy - 24; y++)
        {
            int width = (cy - 24 - y) / 3;
            for (int x = cx - width; x <= cx + width; x++)
            {
                SetPixel(pixels, size, x, y, hatColor);
            }
        }
        
        // Hat brim (scaled)
        for (int x = cx - 16; x <= cx + 16; x++)
        {
            for (int y = cy - 26; y < cy - 24; y++)
            {
                SetPixel(pixels, size, x, y, hatColor);
            }
        }
        
        // Gold trim on hat brim
        for (int x = cx - 16; x <= cx + 16; x++)
        {
            SetPixel(pixels, size, x, cy - 24, hatTrim);
        }
        
        // Star on hat
        DrawCircle(pixels, size, cx, cy - 43, 4, hatTrim);
        
        // Staff in hand (right side, scaled)
        for (int y = cy - 10; y < cy + 45; y++)
        {
            SetPixel(pixels, size, cx + 20, y, new Color(0.4f, 0.25f, 0.1f));
            SetPixel(pixels, size, cx + 21, y, new Color(0.4f, 0.25f, 0.1f));
        }
        // Crystal on staff top
        DrawCircle(pixels, size, cx + 20, cy - 12, 3, new Color(0.6f, 0.3f, 1f, 0.9f));
        DrawCircle(pixels, size, cx + 20, cy - 12, 2, Color.white);
        
        return FinalizeSprite(texture, pixels, size, 128);
    }
    
    /// <summary>
    /// Create a blob enemy sprite (round, blobby) - LARGER AND MORE DETAILED
    /// </summary>
    public static Sprite CreateBlobSprite(Color color)
    {
        int size = 96; // Smaller for smaller on-screen size
        var (texture, pixels) = CreateProceduralTexture(size);
        
        int cx = size / 2;
        int cy = size / 2;
        
        // Shadow underneath
        DrawEllipse(pixels, size, cx, cy + 50, 40, 15, new Color(0, 0, 0, 0.3f));
        
        // Main blob body (large squashed ellipse with gradient)
        for (int y = cy - 30; y <= cy + 30; y++)
        {
            for (int x = cx - 50; x <= cx + 50; x++)
            {
                float dx = (float)(x - cx) / 50f;
                float dy = (float)(y - cy) / 30f;
                if (dx * dx + dy * dy <= 1f)
                {
                    // Add highlight/shadow gradient
                    float lightness = 1f - (dy * 0.3f); // Top lighter, bottom darker
                    Color blobColor = new Color(color.r * lightness, color.g * lightness, color.b * lightness);
                    SetPixel(pixels, size, x, y, blobColor);
                }
            }
        }
        
        // Highlight blob on top-left
        DrawEllipse(pixels, size, cx - 15, cy - 10, 15, 10, Color.Lerp(color, Color.white, 0.5f));
        
        // Eyes (large expressive dark dots)
        DrawCircle(pixels, size, cx - 18, cy - 8, 8, Color.black);
        DrawCircle(pixels, size, cx + 18, cy - 8, 8, Color.black);
        // Eye highlights
        DrawCircle(pixels, size, cx - 15, cy - 11, 3, Color.white);
        DrawCircle(pixels, size, cx + 21, cy - 11, 3, Color.white);
        
        // Mouth (curved line)
        for (int x = cx - 20; x <= cx + 20; x++)
        {
            int mouthY = cy + 12 + (int)(5 * Mathf.Sin((x - cx + 20) * Mathf.PI / 40f));
            DrawCircle(pixels, size, x, mouthY, 2, Color.black);
        }
        
        return FinalizeSprite(texture, pixels, size, 64);
    }
    
    /// <summary>
    /// Create a skeleton enemy sprite (skull with bones) - LARGER AND MORE DETAILED
    /// </summary>
    public static Sprite CreateSkeletonSprite(Color tint)
    {
        int size = 112; // Smaller for smaller on-screen size
        var (texture, pixels) = CreateProceduralTexture(size);
        
        Color boneColor = new Color(tint.r * 0.9f, tint.g * 0.9f, tint.b * 0.9f);
        Color boneShade = new Color(tint.r * 0.7f, tint.g * 0.7f, tint.b * 0.7f);
        
        int cx = size / 2;
        int cy = size / 2;
        
        // Skull (large oval)
        DrawCircle(pixels, size, cx, cy - 20, 35, boneColor);
        
        // Skull shading (left side darker)
        for (int y = cy - 50; y < cy + 10; y++)
        {
            for (int x = cx - 35; x < cx - 15; x++)
            {
                int dx = x - cx;
                int dy = y - (cy - 20);
                if (dx * dx + dy * dy <= 35 * 35)
                {
                    SetPixel(pixels, size, x, y, boneShade);
                }
            }
        }
        
        // Eye sockets (large and dark)
        DrawCircle(pixels, size, cx - 15, cy - 28, 10, Color.black);
        DrawCircle(pixels, size, cx + 15, cy - 28, 10, Color.black);
        // Glowing red eyes inside sockets
        DrawCircle(pixels, size, cx - 15, cy - 28, 5, new Color(1f, 0.2f, 0f));
        DrawCircle(pixels, size, cx + 15, cy - 28, 5, new Color(1f, 0.2f, 0f));
        
        // Nose hole (triangular)
        for (int y = cy - 12; y < cy - 2; y++)
        {
            int width = (cy - 2 - y) / 3;
            for (int x = cx - width; x <= cx + width; x++)
            {
                SetPixel(pixels, size, x, y, Color.black);
            }
        }
        
        // Jaw/teeth
        for (int x = cx - 25; x <= cx + 25; x++)
        {
            for (int y = cy + 5; y < cy + 12; y++)
            {
                SetPixel(pixels, size, x, y, boneColor);
            }
        }
        // Teeth
        for (int x = cx - 20; x <= cx + 20; x += 8)
        {
            for (int y = cy + 7; y < cy + 12; y++)
            {
                SetPixel(pixels, size, x, y, Color.black);
            }
        }
        
        // Ribcage (detailed)
        for (int i = 0; i < 5; i++)
        {
            int ribY = cy + 20 + i * 12;
            // Horizontal rib bones
            for (int x = cx - 30; x <= cx + 30; x++)
            {
                DrawCircle(pixels, size, x, ribY, 3, boneColor);
            }
            // Vertical spine
            for (int y = ribY - 5; y < ribY + 5; y++)
            {
                SetPixel(pixels, size, cx, y, boneColor);
            }
        }
        
        return FinalizeSprite(texture, pixels, size, 64);
    }
    
    /// <summary>
    /// Create a fireball projectile sprite with flame tail
    /// </summary>
    public static Sprite CreateFireballSprite()
    {
        int size = 64; // Smaller projectile
        var (texture, pixels) = CreateProceduralTexture(size);
        
        int cx = size / 2;
        int cy = size / 2;
        
        // Flame tail (streaking behind) - multiple elongated wisps
        for (int i = 0; i < 5; i++)
        {
            int tailX = cx - 30 - i * 6;
            int tailY = cy + (int)((i - 2) * 3f);
            int tailRadius = 8 - i;
            float alpha = 0.6f - i * 0.1f;
            DrawCircle(pixels, size, tailX, tailY, tailRadius, new Color(1f, 0.3f + i * 0.1f, 0f, alpha));
        }
        
        // Tail wisps (trailing particles)
        DrawCircle(pixels, size, cx - 25, cy - 8, 4, new Color(1f, 0.4f, 0f, 0.5f));
        DrawCircle(pixels, size, cx - 32, cy + 6, 3, new Color(1f, 0.5f, 0f, 0.4f));
        DrawCircle(pixels, size, cx - 20, cy + 10, 3, new Color(1f, 0.6f, 0f, 0.5f));
        
        // Main fireball body - outer red glow
        DrawCircle(pixels, size, cx, cy, 18, new Color(1f, 0.2f, 0f, 0.4f));
        DrawCircle(pixels, size, cx, cy, 14, new Color(1f, 0.3f, 0f, 0.7f));
        
        // Middle orange layer
        DrawCircle(pixels, size, cx, cy, 10, new Color(1f, 0.5f, 0f));
        
        // Inner bright layer
        DrawCircle(pixels, size, cx, cy, 7, new Color(1f, 0.8f, 0f));
        
        // Core (white-yellow)
        DrawCircle(pixels, size, cx, cy, 5, Color.yellow);
        DrawCircle(pixels, size, cx, cy, 3, Color.white);
        
        // Leading edge flame licks (pointing forward)
        DrawCircle(pixels, size, cx + 12, cy - 3, 3, new Color(1f, 0.6f, 0f, 0.6f));
        DrawCircle(pixels, size, cx + 10, cy + 5, 2, new Color(1f, 0.7f, 0f, 0.5f));
        
        return FinalizeSprite(texture, pixels, size, 64);
    }
    
    /// <summary>
    /// Create an orbiter projectile sprite (spinning knife)
    /// </summary>
    public static Sprite CreateOrbiterSprite()
    {
        int size = 64; // Smaller knife
        var (texture, pixels) = CreateProceduralTexture(size);
        
        int cx = size / 2;
        int cy = size / 2;
        
        Color bladeColor = new Color(0.8f, 0.85f, 0.9f); // Silver blade
        Color bladeEdge = Color.white; // Sharp edge
        Color handleColor = new Color(0.4f, 0.25f, 0.15f); // Brown handle
        Color handleWrap = new Color(0.6f, 0.5f, 0.3f); // Leather wrap
        
        // Blade (elongated triangle pointing right)
        for (int y = cy - 15; y <= cy + 15; y++)
        {
            int bladeLength = (int)(25 * (1f - Mathf.Abs(y - cy) / 18f));
            for (int x = cx - 6; x < cx - 6 + bladeLength; x++)
            {
                if (x >= 0 && x < size)
                {
                    SetPixel(pixels, size, x, y, bladeColor);
                }
            }
        }
        
        // Blade edge (bright line)
        for (int x = cx - 6; x < cx + 19; x++)
        {
            SetPixel(pixels, size, x, cy - 1, bladeEdge);
            SetPixel(pixels, size, x, cy, bladeEdge);
        }
        
        // Blade tip (sharp point)
        for (int i = 0; i < 4; i++)
        {
            SetPixel(pixels, size, cx + 19 - i, cy - i, bladeColor);
            SetPixel(pixels, size, cx + 19 - i, cy + i, bladeColor);
        }
        
        // Handle (to the left of blade)
        for (int x = cx - 16; x < cx - 6; x++)
        {
            for (int y = cy - 5; y <= cy + 5; y++)
            {
                SetPixel(pixels, size, x, y, handleColor);
            }
        }
        
        // Handle wrap (horizontal lines)
        for (int x = cx - 15; x < cx - 7; x += 3)
        {
            for (int y = cy - 4; y <= cy + 4; y++)
            {
                SetPixel(pixels, size, x, y, handleWrap);
            }
        }
        
        // Pommel (end of handle)
        DrawCircle(pixels, size, cx - 16, cy, 4, new Color(0.7f, 0.6f, 0.3f));
        
        return FinalizeSprite(texture, pixels, size, 64);
    }
    
    /// <summary>
    /// Create an XP gem sprite (cyan glowing gem) - LARGER AND MORE DETAILED
    /// </summary>
    public static Sprite CreateXPGemSprite()
    {
        int size = 48; // Small XP gem
        var (texture, pixels) = CreateProceduralTexture(size);
        
        int cx = size / 2;
        int cy = size / 2;
        
        // Outer glow (cyan) - multiple layers
        DrawCircle(pixels, size, cx, cy, 60, new Color(0f, 0.8f, 1f, 0.2f));
        DrawCircle(pixels, size, cx, cy, 50, new Color(0f, 0.9f, 1f, 0.3f));
        
        // Middle layer (bright cyan)
        DrawCircle(pixels, size, cx, cy, 40, new Color(0f, 1f, 1f, 0.7f));
        
        // Inner gem (diamond-ish shape)
        // Draw octagon for gem facets
        for (int y = cy - 30; y <= cy + 30; y++)
        {
            for (int x = cx - 30; x <= cx + 30; x++)
            {
                int dx = Mathf.Abs(x - cx);
                int dy = Mathf.Abs(y - cy);
                if (dx + dy <= 35) // Diamond shape
                {
                    float bright = 1f - ((dx + dy) / 70f);
                    Color gemColor = new Color(0.4f * bright, 1f * bright, 1f * bright);
                    SetPixel(pixels, size, x, y, gemColor);
                }
            }
        }
        
        // Core highlight (bright white)
        DrawCircle(pixels, size, cx - 5, cy - 5, 12, Color.white);
        
        // Sparkle effects
        DrawCircle(pixels, size, cx + 18, cy - 18, 4, Color.white);
        DrawCircle(pixels, size, cx - 20, cy + 15, 3, Color.white);
        SetPixel(pixels, size, cx + 25, cy, Color.white);
        SetPixel(pixels, size, cx, cy + 28, Color.white);
        
        return FinalizeSprite(texture, pixels, size, 64);
    }
    
    // Helper methods
    private static void SetPixel(Color[] pixels, int size, int x, int y, Color color)
    {
        // Flip Y coordinate because Unity textures are bottom-up
        y = size - 1 - y;
        if (x >= 0 && x < size && y >= 0 && y < size)
        {
            // Alpha blend with existing pixel
            int idx = y * size + x;
            if (color.a >= 1f || pixels[idx].a == 0f)
            {
                pixels[idx] = color;
            }
            else
            {
                // Simple alpha blending
                float alpha = color.a;
                pixels[idx] = new Color(
                    pixels[idx].r * (1 - alpha) + color.r * alpha,
                    pixels[idx].g * (1 - alpha) + color.g * alpha,
                    pixels[idx].b * (1 - alpha) + color.b * alpha,
                    Mathf.Max(pixels[idx].a, alpha)
                );
            }
        }
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
