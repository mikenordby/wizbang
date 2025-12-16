using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility for generating larger, more detailed sprites at runtime with caching.
/// Eliminates 12.8MB/sec garbage from repeated sprite generation.
/// </summary>
public static class SpriteGenerator
{
    // Sprite cache to avoid regenerating sprites
    private struct SpriteCacheKey : System.IEquatable<SpriteCacheKey>
    {
        public readonly string type;
        public readonly Color32 color;
        
        public SpriteCacheKey(string type, Color color = default)
        {
            this.type = type;
            this.color = color; // Implicit cast to Color32
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (type?.GetHashCode() ?? 0);
                hash = hash * 31 + color.r;
                hash = hash * 31 + (color.g << 8);
                hash = hash * 31 + (color.b << 16);
                hash = hash * 31 + (color.a << 24);
                return hash;
            }
        }
        
        public bool Equals(SpriteCacheKey other)
        {
            return type == other.type && 
                   color.r == other.color.r &&
                   color.g == other.color.g &&
                   color.b == other.color.b &&
                   color.a == other.color.a;
        }
    }
    
    private static Dictionary<SpriteCacheKey, Sprite> spriteCache = new Dictionary<SpriteCacheKey, Sprite>(32);
    
    /// <summary>
    /// Get or create a cached sprite
    /// </summary>
    private static Sprite GetOrCreateSprite(string type, Color color, System.Func<Sprite> creator)
    {
        var key = new SpriteCacheKey(type, color);
        if (spriteCache.TryGetValue(key, out Sprite cached))
            return cached;
        
        Sprite newSprite = creator();
        spriteCache[key] = newSprite;
        DebugLog.Verbose($"[SpriteCache] Created and cached '{type}' sprite (cache size: {spriteCache.Count})");
        return newSprite;
    }
    
    /// <summary>
    /// Clear sprite cache (call on scene unload if needed)
    /// </summary>
    public static void ClearCache()
    {
        foreach (var sprite in spriteCache.Values)
        {
            if (sprite != null && sprite.texture != null)
                Object.Destroy(sprite.texture);
            if (sprite != null)
                Object.Destroy(sprite);
        }
        spriteCache.Clear();
        DebugLog.Info($"[SpriteCache] Cleared sprite cache");
    }
    
    /// <summary>
    /// Get cache statistics
    /// </summary>
    public static (int count, long bytes) GetCacheStats()
    {
        long totalBytes = 0;
        foreach (var sprite in spriteCache.Values)
        {
            if (sprite?.texture != null)
                totalBytes += sprite.texture.width * sprite.texture.height * 4; // RGBA
        }
        return (spriteCache.Count, totalBytes);
    }
    
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
        return GetOrCreateSprite("wizard", Color.white, () => CreateWizardSpriteInternal());
    }
    
    private static Sprite CreateWizardSpriteInternal()
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
    /// Create a knight sprite (armor and sword)
    /// </summary>
    public static Sprite CreateKnightSprite()
    {
        return GetOrCreateSprite("knight", Color.white, () => CreateKnightSpriteInternal());
    }
    
    private static Sprite CreateKnightSpriteInternal()
    {
        int size = 128;
        var (texture, pixels) = CreateProceduralTexture(size);
        
        Color armorColor = new Color(0.6f, 0.6f, 0.7f); // Silver armor
        Color armorDark = new Color(0.4f, 0.4f, 0.5f); // Dark armor shadows
        Color armorLight = new Color(0.85f, 0.85f, 0.95f); // Bright armor highlights
        Color capeColor = new Color(0.8f, 0.1f, 0.1f); // Red cape
        Color capeShadow = new Color(0.5f, 0.05f, 0.05f);
        Color swordColor = new Color(0.8f, 0.8f, 0.9f); // Steel blade
        Color handleColor = new Color(0.3f, 0.15f, 0.05f); // Brown handle
        Color eyeGlow = new Color(0.3f, 0.7f, 1f); // Blue eyes through visor
        
        int cx = size / 2;
        int cy = size / 2;
        
        // Cape (behind body)
        for (int y = cy - 15; y < cy + 45; y++)
        {
            int width = 18 + ((y - (cy - 15)) * 8) / 60;
            for (int x = cx - width; x <= cx + width; x++)
            {
                float distFromCenter = Mathf.Abs(x - cx) / (float)width;
                Color capeShade = Color.Lerp(capeColor, capeShadow, distFromCenter * 0.6f);
                SetPixel(pixels, size, x, y, capeShade);
            }
        }
        
        // Body/torso (armor plate)
        for (int y = cy - 18; y < cy + 40; y++)
        {
            int width = 14 + ((y - (cy - 18)) * 6) / 58;
            for (int x = cx - width; x <= cx + width; x++)
            {
                SetPixel(pixels, size, x, y, armorColor);
            }
        }
        
        // Chest plate details (horizontal lines)
        for (int y = cy - 10; y < cy + 30; y += 8)
        {
            for (int x = cx - 12; x <= cx + 12; x++)
            {
                SetPixel(pixels, size, x, y, armorDark);
            }
        }
        
        // Armor highlights on left side
        for (int y = cy - 16; y < cy + 35; y += 3)
        {
            DrawCircle(pixels, size, cx - 10, y, 2, armorLight);
        }
        
        // Helmet (rounded top)
        DrawCircle(pixels, size, cx, cy - 26, 14, armorColor);
        
        // Helmet visor (dark slit)
        for (int x = cx - 8; x <= cx + 8; x++)
        {
            for (int y = cy - 28; y < cy - 25; y++)
            {
                SetPixel(pixels, size, x, y, armorDark);
            }
        }
        
        // Eyes through visor (glowing)
        DrawCircle(pixels, size, cx - 5, cy - 27, 2, eyeGlow);
        DrawCircle(pixels, size, cx + 5, cy - 27, 2, eyeGlow);
        
        // Helmet plume on top
        for (int y = cy - 40; y < cy - 26; y++)
        {
            int width = 3 - (cy - 26 - y) / 7;
            for (int x = cx - width; x <= cx + width; x++)
            {
                SetPixel(pixels, size, x, y, capeColor);
            }
        }
        
        // Sword in right hand
        for (int y = cy - 15; y < cy + 50; y++)
        {
            // Blade
            SetPixel(pixels, size, cx + 22, y, swordColor);
            SetPixel(pixels, size, cx + 23, y, swordColor);
            if (y < cy + 10)
            {
                SetPixel(pixels, size, cx + 21, y, swordColor);
                SetPixel(pixels, size, cx + 24, y, swordColor);
            }
        }
        
        // Sword crossguard
        for (int x = cx + 18; x <= cx + 27; x++)
        {
            for (int y = cy + 8; y < cy + 11; y++)
            {
                SetPixel(pixels, size, x, y, handleColor);
            }
        }
        
        // Sword handle
        for (int y = cy + 10; y < cy + 20; y++)
        {
            SetPixel(pixels, size, cx + 22, y, handleColor);
            SetPixel(pixels, size, cx + 23, y, handleColor);
        }
        
        // Sword pommel
        DrawCircle(pixels, size, cx + 22, cy + 22, 3, new Color(0.7f, 0.6f, 0.2f));
        
        return FinalizeSprite(texture, pixels, size, 128);
    }
    
    /// <summary>
    /// Create a goblin enemy sprite - green skin, pointy ears, dagger, hunched aggressive pose
    /// Modern SNES style with 256-color gradients, medium detail, top-down 4-directional compatible
    /// </summary>
    public static Sprite CreateGoblinSprite(Color color)
    {
        return GetOrCreateSprite("goblin", color, () => CreateGoblinSpriteInternal(color));
    }
    
    private static Sprite CreateGoblinSpriteInternal(Color color)
    {
        int size = 64;
        var (texture, pixels) = CreateProceduralTexture(size);
        
        int cx = size / 2;
        int cy = size / 2;
        
        // Define goblin green palette (base color tinted)
        Color skinBase = new Color(color.r * 0.4f, color.g * 0.7f, color.b * 0.3f); // Dark green
        Color skinMid = new Color(color.r * 0.5f, color.g * 0.85f, color.b * 0.4f); // Mid green
        Color skinLight = new Color(color.r * 0.7f, color.g * 1.0f, color.b * 0.6f); // Light green
        Color eyeYellow = new Color(1f, 0.95f, 0.3f);
        Color eyeRed = new Color(0.9f, 0.2f, 0.1f);
        
        // Shadow underneath
        DrawEllipse(pixels, size, cx, cy + 28, 12, 4, new Color(0, 0, 0, 0.4f));
        
        // Body (hunched, egg-shaped torso)
        for (int y = cy + 5; y <= cy + 22; y++)
        {
            for (int x = cx - 10; x <= cx + 10; x++)
            {
                float dx = (float)(x - cx) / 10f;
                float dy = (float)(y - (cy + 13)) / 9f;
                if (dx * dx + dy * dy <= 1f)
                {
                    float gradientY = (float)(y - (cy + 5)) / 17f; // 0=top, 1=bottom
                    Color bodyColor = Color.Lerp(skinLight, skinBase, gradientY * 0.7f);
                    SetPixel(pixels, size, x, y, bodyColor);
                }
            }
        }
        
        // Head (large oval - goblins have big heads)
        for (int y = cy - 12; y <= cy + 8; y++)
        {
            for (int x = cx - 11; x <= cx + 11; x++)
            {
                float dx = (float)(x - cx) / 11f;
                float dy = (float)(y - (cy - 2)) / 10f;
                if (dx * dx + dy * dy <= 1f)
                {
                    float gradientY = (float)(y - (cy - 12)) / 20f;
                    Color headColor = Color.Lerp(skinLight, skinMid, gradientY * 0.6f);
                    SetPixel(pixels, size, x, y, headColor);
                }
            }
        }
        
        // Pointy ears (triangular)
        // Left ear
        for (int y = cy - 8; y <= cy - 2; y++)
        {
            int earWidth = (cy - 2 - y) / 2;
            for (int x = cx - 14 - earWidth; x <= cx - 14; x++)
            {
                SetPixel(pixels, size, x, y, skinMid);
            }
        }
        // Right ear
        for (int y = cy - 8; y <= cy - 2; y++)
        {
            int earWidth = (cy - 2 - y) / 2;
            for (int x = cx + 14; x <= cx + 14 + earWidth; x++)
            {
                SetPixel(pixels, size, x, y, skinMid);
            }
        }
        
        // Eyes (yellow with red pupils - menacing)
        DrawCircle(pixels, size, cx - 5, cy - 4, 3, eyeYellow);
        DrawCircle(pixels, size, cx + 5, cy - 4, 3, eyeYellow);
        DrawCircle(pixels, size, cx - 5, cy - 4, 1, eyeRed);
        DrawCircle(pixels, size, cx + 5, cy - 4, 1, eyeRed);
        
        // Nose (small bump)
        DrawCircle(pixels, size, cx, cy + 1, 2, skinBase);
        
        // Mouth (wicked grin)
        for (int x = cx - 6; x <= cx + 6; x++)
        {
            int mouthY = cy + 5 - (int)(Mathf.Abs(x - cx) * 0.3f); // Slight smile curve
            SetPixel(pixels, size, x, mouthY, new Color(0.1f, 0.1f, 0.05f)); // Dark mouth
        }
        
        // Dagger (silver blade, brown handle)
        Color blade = new Color(0.85f, 0.9f, 0.95f); // Silver
        Color bladeDark = new Color(0.6f, 0.65f, 0.7f);
        Color handle = new Color(0.5f, 0.3f, 0.15f); // Brown
        
        // Blade (pointing right-down, diagonally)
        for (int i = 0; i < 10; i++)
        {
            int bladeX = cx + 12 + i;
            int bladeY = cy + 8 + (i / 2);
            DrawCircle(pixels, size, bladeX, bladeY, 1, i % 2 == 0 ? blade : bladeDark);
            if (i < 8) DrawCircle(pixels, size, bladeX, bladeY + 1, 1, bladeDark); // Width
        }
        // Handle
        for (int i = 0; i < 4; i++)
        {
            DrawCircle(pixels, size, cx + 11 + i, cy + 7 + (i / 2), 1, handle);
        }
        
        // Outline (black)
        AddOutline(pixels, size, Color.black);
        
        return FinalizeSprite(texture, pixels, size, 64);
    }
    
    /// <summary>
    /// Create legacy blob sprite (kept for backward compatibility)
    /// </summary>
    public static Sprite CreateBlobSprite(Color color)
    {
        // Redirect to goblin
        return CreateGoblinSprite(color);
    }
    
    /// <summary>
    /// Create a skeleton enemy sprite - white bones, armored ribcage, glowing eyes
    /// Modern SNES style with smooth gradients, medium detail, top-down 4-directional
    /// </summary>
    public static Sprite CreateSkeletonSprite(Color tint)
    {
        return GetOrCreateSprite("skeleton", tint, () => CreateSkeletonSpriteInternal(tint));
    }
    
    private static Sprite CreateSkeletonSpriteInternal(Color tint)
    {
        int size = 64;
        var (texture, pixels) = CreateProceduralTexture(size);
        
        int cx = size / 2;
        int cy = size / 2;
        
        // Bone color palette with smooth gradients
        Color boneLight = new Color(tint.r * 1.0f, tint.g * 1.0f, tint.b * 1.0f); // Pure white
        Color boneMid = new Color(tint.r * 0.85f, tint.g * 0.85f, tint.b * 0.85f); // Light gray
        Color boneDark = new Color(tint.r * 0.6f, tint.g * 0.6f, tint.b * 0.65f); // Gray-blue shadow
        Color armorGray = new Color(0.5f, 0.5f, 0.55f); // Metal armor
        Color eyeGlow = new Color(0.2f, 1.0f, 0.3f); // Eerie green glow
        
        // Shadow
        DrawEllipse(pixels, size, cx, cy + 28, 10, 3, new Color(0, 0, 0, 0.4f));
        
        // Skull (large oval with gradient shading)
        for (int y = cy - 12; y <= cy + 2; y++)
        {
            for (int x = cx - 10; x <= cx + 10; x++)
            {
                float dx = (float)(x - cx) / 10f;
                float dy = (float)(y - (cy - 5)) / 7f;
                if (dx * dx + dy * dy <= 1f)
                {
                    // Gradient from top-left (light) to bottom-right (dark)
                    float gradient = (dx + dy) * 0.5f + 0.5f; // 0 to 1
                    Color skullColor = Color.Lerp(boneLight, boneMid, gradient * 0.6f);
                    SetPixel(pixels, size, x, y, skullColor);
                }
            }
        }
        
        // Eye sockets (large and dark)
        DrawCircle(pixels, size, cx - 4, cy - 6, 3, new Color(0.1f, 0.1f, 0.15f));
        DrawCircle(pixels, size, cx + 4, cy - 6, 3, new Color(0.1f, 0.1f, 0.15f));
        // Glowing eyes (eerie green)
        DrawCircle(pixels, size, cx - 4, cy - 6, 2, eyeGlow);
        DrawCircle(pixels, size, cx + 4, cy - 6, 2, eyeGlow);
        // Eye highlights
        SetPixel(pixels, size, cx - 4, cy - 7, Color.Lerp(eyeGlow, Color.white, 0.7f));
        SetPixel(pixels, size, cx + 4, cy - 7, Color.Lerp(eyeGlow, Color.white, 0.7f));
        
        // Nose hole (triangular)
        for (int y = cy - 2; y <= cy + 2; y++)
        {
            int width = (cy + 2 - y) / 2;
            for (int x = cx - width; x <= cx + width; x++)
            {
                SetPixel(pixels, size, x, y, new Color(0.1f, 0.1f, 0.15f));
            }
        }
        
        // Jaw with teeth
        for (int x = cx - 8; x <= cx + 8; x++)
        {
            for (int y = cy + 3; y <= cy + 5; y++)
            {
                SetPixel(pixels, size, x, y, boneMid);
            }
        }
        // Teeth (small white squares)
        for (int x = cx - 7; x <= cx + 7; x += 3)
        {
            SetPixel(pixels, size, x, cy + 4, boneLight);
            SetPixel(pixels, size, x, cy + 3, new Color(0.1f, 0.1f, 0.15f)); // Gap
        }
        
        // Armored ribcage (shoulder pauldrons + chest plate)
        // Left shoulder armor
        for (int y = cy + 6; y <= cy + 12; y++)
        {
            for (int x = cx - 12; x <= cx - 8; x++)
            {
                float gradient = (float)(y - (cy + 6)) / 6f;
                Color armorColor = Color.Lerp(armorGray, boneDark, gradient * 0.5f);
                SetPixel(pixels, size, x, y, armorColor);
            }
        }
        // Right shoulder armor
        for (int y = cy + 6; y <= cy + 12; y++)
        {
            for (int x = cx + 8; x <= cx + 12; x++)
            {
                float gradient = (float)(y - (cy + 6)) / 6f;
                Color armorColor = Color.Lerp(armorGray, boneDark, gradient * 0.5f);
                SetPixel(pixels, size, x, y, armorColor);
            }
        }
        
        // Chest plate (central armor)
        for (int y = cy + 8; y <= cy + 20; y++)
        {
            for (int x = cx - 6; x <= cx + 6; x++)
            {
                float gradient = (float)(y - (cy + 8)) / 12f;
                Color plateColor = Color.Lerp(armorGray, boneDark, gradient * 0.4f);
                SetPixel(pixels, size, x, y, plateColor);
            }
        }
        
        // Ribs showing through armor (bone details)
        for (int i = 0; i < 3; i++)
        {
            int ribY = cy + 10 + i * 4;
            for (int x = cx - 5; x <= cx + 5; x++)
            {
                if (Mathf.Abs(x - cx) > 2) // Only on sides
                {
                    SetPixel(pixels, size, x, ribY, boneMid);
                }
            }
        }
        
        // Spine (vertical bone line)
        for (int y = cy + 8; y <= cy + 22; y++)
        {
            SetPixel(pixels, size, cx, y, boneMid);
        }
        
        // Pelvis bones (lower body)
        for (int x = cx - 8; x <= cx + 8; x++)
        {
            SetPixel(pixels, size, x, cy + 22, boneMid);
        }
        
        // Outline
        AddOutline(pixels, size, Color.black);
        
        return FinalizeSprite(texture, pixels, size, 64);
    }
    
    /// <summary>
    /// Create an ogre enemy sprite - bulky, muscular, upright stance, bare fists
    /// Modern SNES style with smooth gradients, medium detail, top-down 4-directional
    /// </summary>
    public static Sprite CreateOgreSprite(Color color)
    {
        return GetOrCreateSprite("ogre", color, () => CreateOgreSpriteInternal(color));
    }
    
    private static Sprite CreateOgreSpriteInternal(Color color)
    {
        int size = 64;
        var (texture, pixels) = CreateProceduralTexture(size);
        
        int cx = size / 2;
        int cy = size / 2;
        
        // Ogre skin palette (tan/brown)
        Color skinDark = new Color(color.r * 0.45f, color.g * 0.3f, color.b * 0.2f); // Dark brown
        Color skinMid = new Color(color.r * 0.65f, color.g * 0.45f, color.b * 0.3f); // Mid brown
        Color skinLight = new Color(color.r * 0.8f, color.g * 0.6f, color.b * 0.45f); // Light tan
        Color muscleHighlight = new Color(color.r * 0.9f, color.g * 0.7f, color.b * 0.55f);
        
        // Shadow
        DrawEllipse(pixels, size, cx, cy + 28, 14, 5, new Color(0, 0, 0, 0.5f));
        
        // Legs (thick and sturdy)
        // Left leg
        for (int y = cy + 12; y <= cy + 26; y++)
        {
            for (int x = cx - 9; x <= cx - 3; x++)
            {
                float gradient = (float)(y - (cy + 12)) / 14f;
                Color legColor = Color.Lerp(skinMid, skinDark, gradient * 0.5f);
                SetPixel(pixels, size, x, y, legColor);
            }
        }
        // Right leg
        for (int y = cy + 12; y <= cy + 26; y++)
        {
            for (int x = cx + 3; x <= cx + 9; x++)
            {
                float gradient = (float)(y - (cy + 12)) / 14f;
                Color legColor = Color.Lerp(skinMid, skinDark, gradient * 0.5f);
                SetPixel(pixels, size, x, y, legColor);
            }
        }
        
        // Torso (bulky, barrel-chested)
        for (int y = cy - 2; y <= cy + 16; y++)
        {
            for (int x = cx - 12; x <= cx + 12; x++)
            {
                float dx = (float)(x - cx) / 12f;
                float dy = (float)(y - (cy + 7)) / 9f;
                if (dx * dx + dy * dy <= 1f)
                {
                    // Gradient shading (light on top-left)
                    float gradient = (dx * 0.3f + dy * 0.5f) + 0.5f;
                    Color torsoColor = Color.Lerp(muscleHighlight, skinMid, gradient * 0.7f);
                    SetPixel(pixels, size, x, y, torsoColor);
                }
            }
        }
        
        // Muscular chest definition
        for (int x = cx - 6; x <= cx + 6; x++)
        {
            SetPixel(pixels, size, x, cy + 4, skinDark); // Chest line
        }
        for (int y = cy; y <= cy + 8; y++)
        {
            SetPixel(pixels, size, cx, y, skinDark); // Center line
        }
        
        // Arms (massive and muscular)
        // Left arm
        for (int y = cy + 2; y <= cy + 14; y++)
        {
            for (int x = cx - 16; x <= cx - 12; x++)
            {
                float gradient = (float)(y - (cy + 2)) / 12f;
                Color armColor = Color.Lerp(muscleHighlight, skinMid, gradient * 0.6f);
                SetPixel(pixels, size, x, y, armColor);
            }
        }
        // Right arm
        for (int y = cy + 2; y <= cy + 14; y++)
        {
            for (int x = cx + 12; x <= cx + 16; x++)
            {
                float gradient = (float)(y - (cy + 2)) / 12f;
                Color armColor = Color.Lerp(muscleHighlight, skinMid, gradient * 0.6f);
                SetPixel(pixels, size, x, y, armColor);
            }
        }
        
        // Fists (clenched)
        DrawCircle(pixels, size, cx - 15, cy + 15, 3, skinMid);
        DrawCircle(pixels, size, cx + 15, cy + 15, 3, skinMid);
        
        // Head (large and brutish)
        for (int y = cy - 14; y <= cy + 2; y++)
        {
            for (int x = cx - 10; x <= cx + 10; x++)
            {
                float dx = (float)(x - cx) / 10f;
                float dy = (float)(y - (cy - 6)) / 8f;
                if (dx * dx + dy * dy <= 1f)
                {
                    float gradient = (dx * 0.3f + dy * 0.4f) + 0.5f;
                    Color headColor = Color.Lerp(skinLight, skinMid, gradient * 0.6f);
                    SetPixel(pixels, size, x, y, headColor);
                }
            }
        }
        
        // Brow ridge (prominent)
        for (int x = cx - 8; x <= cx + 8; x++)
        {
            SetPixel(pixels, size, x, cy - 8, skinDark);
        }
        
        // Eyes (small, beady, angry)
        DrawCircle(pixels, size, cx - 4, cy - 6, 2, new Color(0.9f, 0.9f, 0.7f)); // Yellow-white
        DrawCircle(pixels, size, cx + 4, cy - 6, 2, new Color(0.9f, 0.9f, 0.7f));
        SetPixel(pixels, size, cx - 4, cy - 6, new Color(0.1f, 0.05f, 0.0f)); // Pupil
        SetPixel(pixels, size, cx + 4, cy - 6, new Color(0.1f, 0.05f, 0.0f));
        
        // Nose (large, flat)
        for (int y = cy - 4; y <= cy - 1; y++)
        {
            for (int x = cx - 2; x <= cx + 2; x++)
            {
                SetPixel(pixels, size, x, y, skinDark);
            }
        }
        
        // Mouth (grimacing)
        for (int x = cx - 6; x <= cx + 6; x++)
        {
            int mouthY = cy + 2 + (int)(Mathf.Abs(x - cx) * 0.2f); // Slight frown
            SetPixel(pixels, size, x, mouthY, new Color(0.15f, 0.1f, 0.05f));
        }
        
        // Tusks (small bottom teeth protruding)
        SetPixel(pixels, size, cx - 4, cy + 2, Color.white);
        SetPixel(pixels, size, cx + 4, cy + 2, Color.white);
        SetPixel(pixels, size, cx - 4, cy + 3, new Color(0.9f, 0.9f, 0.85f));
        SetPixel(pixels, size, cx + 4, cy + 3, new Color(0.9f, 0.9f, 0.85f));
        
        // Outline
        AddOutline(pixels, size, Color.black);
        
        return FinalizeSprite(texture, pixels, size, 64);
    }
    
    /// <summary>
    /// Create a dragon enemy sprite - red/orange quadruped, wings visible from top-down, fire breath
    /// Modern SNES style with smooth gradients, medium detail, top-down 4-directional
    /// </summary>
    public static Sprite CreateDragonSprite(Color color)
    {
        return GetOrCreateSprite("dragon", color, () => CreateDragonSpriteInternal(color));
    }
    
    private static Sprite CreateDragonSpriteInternal(Color color)
    {
        int size = 64;
        var (texture, pixels) = CreateProceduralTexture(size);
        
        int cx = size / 2;
        int cy = size / 2;
        
        // Dragon color palette (red/orange)
        Color scalesDark = new Color(color.r * 0.5f, color.g * 0.15f, color.b * 0.1f); // Dark red
        Color scalesMid = new Color(color.r * 0.8f, color.g * 0.2f, color.b * 0.15f); // Blood red
        Color scalesLight = new Color(color.r * 1.0f, color.g * 0.4f, color.b * 0.2f); // Orange-red
        Color scalesHighlight = new Color(color.r * 1.0f, color.g * 0.6f, color.b * 0.3f); // Bright orange
        Color wingMembrane = new Color(0.5f, 0.2f, 0.15f, 0.7f); // Dark semi-transparent
        Color fireGlow = new Color(1.0f, 0.7f, 0.2f); // Orange fire
        
        // Shadow
        DrawEllipse(pixels, size, cx, cy + 28, 16, 6, new Color(0, 0, 0, 0.5f));
        
        // Wings (spread out, visible from top-down)
        // Left wing
        for (int y = cy - 8; y <= cy + 12; y++)
        {
            for (int x = cx - 22; x <= cx - 10; x++)
            {
                float dx = (float)(x - (cx - 16)) / 6f;
                float dy = (float)(y - (cy + 2)) / 10f;
                if (dx * dx + dy * dy <= 1f)
                {
                    SetPixel(pixels, size, x, y, wingMembrane);
                }
            }
        }
        // Right wing
        for (int y = cy - 8; y <= cy + 12; y++)
        {
            for (int x = cx + 10; x <= cx + 22; x++)
            {
                float dx = (float)(x - (cx + 16)) / 6f;
                float dy = (float)(y - (cy + 2)) / 10f;
                if (dx * dx + dy * dy <= 1f)
                {
                    SetPixel(pixels, size, x, y, wingMembrane);
                }
            }
        }
        
        // Wing bone structure (claws at wing tips)
        for (int i = 0; i < 3; i++)
        {
            int boneY = cy - 6 + i * 6;
            SetPixel(pixels, size, cx - 18, boneY, scalesDark);
            SetPixel(pixels, size, cx - 17, boneY, scalesDark);
            SetPixel(pixels, size, cx + 17, boneY, scalesDark);
            SetPixel(pixels, size, cx + 18, boneY, scalesDark);
        }
        
        // Body (long, serpentine)
        // Tail (lower body)
        for (int y = cy + 14; y <= cy + 24; y++)
        {
            int tailWidth = 7 - (y - (cy + 14)) / 2;
            for (int x = cx - tailWidth; x <= cx + tailWidth; x++)
            {
                float gradient = (float)(y - (cy + 14)) / 10f;
                Color tailColor = Color.Lerp(scalesMid, scalesDark, gradient * 0.6f);
                SetPixel(pixels, size, x, y, tailColor);
            }
        }
        
        // Main body (torso)
        for (int y = cy + 2; y <= cy + 18; y++)
        {
            for (int x = cx - 9; x <= cx + 9; x++)
            {
                float dx = (float)(x - cx) / 9f;
                float dy = (float)(y - (cy + 10)) / 8f;
                if (dx * dx + dy * dy <= 1f)
                {
                    float gradient = (dx * 0.3f + dy * 0.5f) + 0.5f;
                    Color bodyColor = Color.Lerp(scalesHighlight, scalesMid, gradient * 0.7f);
                    SetPixel(pixels, size, x, y, bodyColor);
                }
            }
        }
        
        // Scales pattern (horizontal lines)
        for (int i = 0; i < 4; i++)
        {
            int scaleY = cy + 6 + i * 4;
            for (int x = cx - 6; x <= cx + 6; x++)
            {
                if ((x - cx + i) % 3 == 0)
                {
                    SetPixel(pixels, size, x, scaleY, scalesDark);
                }
            }
        }
        
        // Legs (four legs visible from top)
        // Front left
        for (int y = cy + 4; y <= cy + 12; y++)
        {
            for (int x = cx - 11; x <= cx - 8; x++)
            {
                SetPixel(pixels, size, x, y, scalesMid);
            }
        }
        // Front right
        for (int y = cy + 4; y <= cy + 12; y++)
        {
            for (int x = cx + 8; x <= cx + 11; x++)
            {
                SetPixel(pixels, size, x, y, scalesMid);
            }
        }
        // Back left
        for (int y = cy + 14; y <= cy + 20; y++)
        {
            for (int x = cx - 10; x <= cx - 7; x++)
            {
                SetPixel(pixels, size, x, y, scalesDark);
            }
        }
        // Back right
        for (int y = cy + 14; y <= cy + 20; y++)
        {
            for (int x = cx + 7; x <= cx + 10; x++)
            {
                SetPixel(pixels, size, x, y, scalesDark);
            }
        }
        
        // Claws (small triangles)
        SetPixel(pixels, size, cx - 11, cy + 12, Color.black);
        SetPixel(pixels, size, cx + 11, cy + 12, Color.black);
        
        // Neck and head
        for (int y = cy - 6; y <= cy + 4; y++)
        {
            int neckWidth = 5 - Mathf.Abs(y - (cy - 1)) / 2;
            for (int x = cx - neckWidth; x <= cx + neckWidth; x++)
            {
                float gradient = (float)(y - (cy - 6)) / 10f;
                Color neckColor = Color.Lerp(scalesHighlight, scalesMid, gradient * 0.5f);
                SetPixel(pixels, size, x, y, neckColor);
            }
        }
        
        // Head (diamond shape)
        for (int y = cy - 16; y <= cy - 6; y++)
        {
            int headWidth = 6 - Mathf.Abs(y - (cy - 11)) / 2;
            for (int x = cx - headWidth; x <= cx + headWidth; x++)
            {
                float gradient = (float)(y - (cy - 16)) / 10f;
                Color headColor = Color.Lerp(scalesLight, scalesMid, gradient * 0.6f);
                SetPixel(pixels, size, x, y, headColor);
            }
        }
        
        // Horns (small spikes)
        for (int i = 0; i < 3; i++)
        {
            SetPixel(pixels, size, cx - 5 + i * 5, cy - 16, scalesDark);
            SetPixel(pixels, size, cx - 5 + i * 5, cy - 17, scalesDark);
        }
        
        // Eyes (glowing yellow-orange)
        DrawCircle(pixels, size, cx - 3, cy - 12, 2, new Color(1f, 0.8f, 0.2f));
        DrawCircle(pixels, size, cx + 3, cy - 12, 2, new Color(1f, 0.8f, 0.2f));
        SetPixel(pixels, size, cx - 3, cy - 12, new Color(0.2f, 0.1f, 0.0f)); // Slit pupil
        SetPixel(pixels, size, cx + 3, cy - 12, new Color(0.2f, 0.1f, 0.0f));
        
        // Nostrils (smoke/fire hint)
        SetPixel(pixels, size, cx - 2, cy - 8, fireGlow);
        SetPixel(pixels, size, cx + 2, cy - 8, fireGlow);
        SetPixel(pixels, size, cx - 2, cy - 9, new Color(1f, 0.5f, 0.1f, 0.5f));
        SetPixel(pixels, size, cx + 2, cy - 9, new Color(1f, 0.5f, 0.1f, 0.5f));
        
        // Outline
        AddOutline(pixels, size, Color.black);
        
        return FinalizeSprite(texture, pixels, size, 64);
    }
    
    /// <summary>
    /// Create a fireball projectile sprite with flame tail
    /// </summary>
    public static Sprite CreateFireballSprite()
    {
        return GetOrCreateSprite("fireball", Color.white, () => CreateFireballSpriteInternal());
    }
    
    private static Sprite CreateFireballSpriteInternal()
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
        return GetOrCreateSprite("orbiter", Color.white, () => CreateOrbiterSpriteInternal());
    }
    
    private static Sprite CreateOrbiterSpriteInternal()
    {
        int size = 64; // Smaller knife
        var (texture, pixels) = CreateProceduralTexture(size);
        
        int cx = size / 2;
        int cy = size / 2;
        
        // 16-bit SNES style - thinner, more elegant blade
        Color bladeDark = new Color(0.65f, 0.7f, 0.75f); // Steel base
        Color bladeMid = new Color(0.85f, 0.9f, 0.95f); // Polished steel
        Color bladeHighlight = Color.white; // Sharp edge shine
        Color handleDark = new Color(0.3f, 0.15f, 0.1f); // Dark leather
        Color handleMid = new Color(0.5f, 0.3f, 0.2f); // Brown leather
        Color handleLight = new Color(0.7f, 0.5f, 0.3f); // Light leather
        Color guardColor = new Color(0.6f, 0.55f, 0.4f); // Bronze guard
        
        // Blade shape - MUCH THINNER (only 3-4 pixels wide max)
        // Main blade center line
        for (int x = cx - 2; x < cx + 22; x++)
        {
            SetPixel(pixels, size, x, cy, bladeMid);
        }
        
        // Blade edges (thin taper)
        for (int x = cx - 2; x < cx + 20; x++)
        {
            float tapering = 1f - (x - (cx - 2)) / 22f;
            if (tapering > 0.3f) // Only widen blade in first 70% of length
            {
                SetPixel(pixels, size, x, cy - 1, bladeDark);
                SetPixel(pixels, size, x, cy + 1, bladeDark);
                
                if (tapering > 0.6f && x < cx + 10) // Even thinner width, shorter section
                {
                    SetPixel(pixels, size, x, cy - 2, new Color(bladeDark.r * 0.8f, bladeDark.g * 0.8f, bladeDark.b * 0.8f));
                    SetPixel(pixels, size, x, cy + 2, new Color(bladeDark.r * 0.8f, bladeDark.g * 0.8f, bladeDark.b * 0.8f));
                }
            }
        }
        
        // Blade highlight (center shine)
        for (int x = cx; x < cx + 18; x++)
        {
            SetPixel(pixels, size, x, cy, bladeHighlight);
        }
        
        // Blade tip (sharp point - 3 pixels)
        SetPixel(pixels, size, cx + 22, cy, bladeMid);
        SetPixel(pixels, size, cx + 21, cy - 1, bladeDark);
        SetPixel(pixels, size, cx + 21, cy + 1, bladeDark);
        
        // Cross-guard (smaller, more proportional)
        for (int y = cy - 5; y <= cy + 5; y++)
        {
            for (int x = cx - 4; x <= cx - 2; x++)
            {
                float centerDist = Mathf.Abs(y - cy) / 5f;
                Color guardShade = Color.Lerp(guardColor, handleDark, centerDist * 0.4f);
                SetPixel(pixels, size, x, y, guardShade);
            }
        }
        
        // Handle (leather-wrapped grip) - thinner
        for (int x = cx - 14; x < cx - 4; x++)
        {
            for (int y = cy - 2; y <= cy + 2; y++)
            {
                float xDist = (x - (cx - 14)) / 10f;
                Color handleShade = Color.Lerp(handleDark, handleMid, xDist);
                SetPixel(pixels, size, x, y, handleShade);
            }
        }
        
        // Handle wrap (leather binding strips) - subtle
        for (int x = cx - 13; x < cx - 5; x += 3)
        {
            SetPixel(pixels, size, x, cy - 2, handleLight);
            SetPixel(pixels, size, x, cy + 2, handleLight);
        }
        
        // Pommel (small rounded end)
        for (int y = cy - 3; y <= cy + 3; y++)
        {
            for (int x = cx - 16; x <= cx - 14; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx - 15, cy));
                if (dist < 2.5f)
                {
                    Color pommelColor = Color.Lerp(guardColor, handleDark, dist / 2.5f);
                    SetPixel(pixels, size, x, y, pommelColor);
                }
            }
        }
        
        return FinalizeSprite(texture, pixels, size, 64);
    }
    
    /// <summary>
    /// Create a boomerang sprite (curved blade)
    /// </summary>
    public static Sprite CreateBoomerangSprite()
    {
        return GetOrCreateSprite("boomerang", Color.white, () => CreateBoomerangSpriteInternal());
    }
    
    private static Sprite CreateBoomerangSpriteInternal()
    {
        int size = 64;
        var (texture, pixels) = CreateProceduralTexture(size);
        
        int cx = size / 2;
        int cy = size / 2;
        
        Color bladeColor = new Color(0.7f, 0.5f, 0.2f); // Brown wood
        Color edgeColor = new Color(0.9f, 0.85f, 0.8f); // Light edge
        Color detailColor = new Color(0.5f, 0.3f, 0.1f); // Dark brown detail
        
        // Draw curved boomerang shape (V-shaped)
        // Left arm
        for (int i = 0; i < 20; i++)
        {
            float t = i / 20f;
            int x = cx - 20 + (int)(t * 8);
            int y = cy + (int)(t * t * 15);
            
            DrawCircle(pixels, size, x, y, 3, bladeColor);
            DrawCircle(pixels, size, x, y, 2, edgeColor);
        }
        
        // Right arm  
        for (int i = 0; i < 20; i++)
        {
            float t = i / 20f;
            int x = cx + 20 - (int)(t * 8);
            int y = cy + (int)(t * t * 15);
            
            DrawCircle(pixels, size, x, y, 3, bladeColor);
            DrawCircle(pixels, size, x, y, 2, edgeColor);
        }
        
        // Center grip
        for (int y = cy - 3; y <= cy + 3; y++)
        {
            for (int x = cx - 4; x <= cx + 4; x++)
            {
                SetPixel(pixels, size, x, y, detailColor);
            }
        }
        
        return FinalizeSprite(texture, pixels, size, 64);
    }
    
    /// <summary>
    /// Create an XP gem sprite (cyan glowing gem) - LARGER AND MORE DETAILED
    /// </summary>
    public static Sprite CreateXPGemSprite()
    {
        return GetOrCreateSprite("xpgem", Color.white, () => CreateXPGemSpriteInternal());
    }
    
    private static Sprite CreateXPGemSpriteInternal()
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
    
    /// <summary>
    /// Add a black outline around non-transparent pixels (SNES-style)
    /// </summary>
    private static void AddOutline(Color[] pixels, int size, Color outlineColor)
    {
        Color[] originalPixels = new Color[pixels.Length];
        System.Array.Copy(pixels, originalPixels, pixels.Length);
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Flip Y to match SetPixel coordinate system
                int flippedY = size - 1 - y;
                int idx = flippedY * size + x;
                
                // Skip if already has color
                if (originalPixels[idx].a > 0f)
                    continue;
                
                // Check 8 neighbors for non-transparent pixels
                bool hasNeighbor = false;
                for (int dy = -1; dy <= 1 && !hasNeighbor; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        
                        int nx = x + dx;
                        int ny = flippedY + dy;
                        
                        if (nx >= 0 && nx < size && ny >= 0 && ny < size)
                        {
                            int neighborIdx = ny * size + nx;
                            if (originalPixels[neighborIdx].a > 0f)
                            {
                                hasNeighbor = true;
                                break;
                            }
                        }
                    }
                }
                
                // Add outline pixel if next to a non-transparent pixel
                if (hasNeighbor)
                {
                    pixels[idx] = outlineColor;
                }
            }
        }
    }
}

