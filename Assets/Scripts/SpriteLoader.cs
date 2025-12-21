using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility for loading and caching sprites from Resources folder.
/// Implements Issue #3 sprite caching - eliminates 12.8MB/sec garbage from procedural generation.
/// </summary>
public static class SpriteLoader
{
    private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>(32);

    // Track which entities we've already logged errors for (prevent spam)
    private static HashSet<string> loggedMissingEntities = new HashSet<string>();
    
    /// <summary>
    /// Load a sprite from Resources folder with caching
    /// </summary>
    /// <param name="path">Path relative to Resources/Sprites/ (e.g., "Player/wizard")</param>
    /// <returns>Cached sprite or null if not found</returns>
    public static Sprite LoadSprite(string path)
    {
        // Check cache first (O(1) lookup)
        if (spriteCache.TryGetValue(path, out Sprite cached))
        {
            if (cached != null)
            {
                DebugLog.Verbose($"[SpriteLoader] CACHE HIT: {path}");
                return cached;
            }
            // Cached null means we already tried and failed - don't retry
            DebugLog.Verbose($"[SpriteLoader] CACHE HIT (null): {path} - previous load failed");
            return null;
        }

        // Try to load from Resources/Sprites/
        string fullPath = $"Sprites/{path}";
        Sprite sprite = Resources.Load<Sprite>(fullPath);

        if (sprite != null)
        {
            spriteCache[path] = sprite;
            // Silent success - no logging needed for successful loads (reduces log spam)
            DebugLog.Verbose($"[SpriteLoader] Loaded sprite: {fullPath}");
            return sprite;
        }

        // Load failed - try to diagnose why
        // Only capture screenshot for frame_000 failures (critical), not for "no more frames" scenarios
        bool isFirstFrame = path.EndsWith("/frame_000") || path.EndsWith("/frame_000_0");
        bool captureScreenshot = isFirstFrame;

        if (isFirstFrame)
        {
            // Extract entity name from path for deduplication
            // Path format: "{category}/{entity}/animations/{anim}/{direction}/frame_000"
            string[] pathParts = path.Split('/');
            string entityKey = pathParts.Length >= 2 ? $"{pathParts[0]}/{pathParts[1]}" : path;

            // Only log detailed error ONCE per entity
            if (!loggedMissingEntities.Contains(entityKey))
            {
                PersistentLogger.Error($"Sprites missing for {pathParts[1]}: frame_000 not found in Assets/Resources/Sprites/{pathParts[0]}/{pathParts[1]}/animations/", "SpriteLoader", captureScreenshot: true);
                loggedMissingEntities.Add(entityKey);
            }
            // Subsequent failures for same entity are silent (prevents 24 errors per spawn)
        }
        else
        {
            // Silent - frame not found usually means end of animation, no logging needed
            DebugLog.Verbose($"[SpriteLoader] Frame not found: {fullPath} (expected - end of sequence)");
        }

        // Don't cache null - let caller handle fallback (Verbose since procedural fallback is expected)
        DebugLog.Verbose($"[SpriteLoader] Sprite not found in Resources: {fullPath}");

        return null;
    }
    
    /// <summary>
    /// Load character sprite by direction (for 4-directional movement)
    /// </summary>
    public static Sprite LoadCharacterSprite(string characterName, Direction direction)
    {
        string directionName = direction.ToString().ToLower(); // "north", "south", "east", "west"
        string path = $"{characterName}/{directionName}";
        
        Sprite sprite = LoadSprite(path);
        
        if (sprite == null)
        {
            DebugLog.Verbose($"[SpriteLoader] Loaded PixelLab {characterName} sprite: {directionName}");
        }
        
        return sprite;
    }
    
    /// <summary>
    /// Load character sprite by 8-directional name (for 8-directional movement)
    /// </summary>
    public static Sprite LoadCharacterSprite(string characterName, string directionName)
    {
        string path = $"{characterName}/{directionName}";
        
        Sprite sprite = LoadSprite(path);
        
        if (sprite == null)
        {
            DebugLog.Error($"[SpriteLoader] FAILED to load sprite: Resources/Sprites/{path}");
        }
        else
        {
            DebugLog.Info($"[SpriteLoader] ✓ Loaded: Resources/Sprites/{path} ({sprite.rect.width}x{sprite.rect.height}px, PPU={sprite.pixelsPerUnit})");
        }
        
        return sprite;
    }
    

    
    /// <summary>
    /// Load enemy sprite - requires PixelLab sprites
    /// </summary>
    public static Sprite LoadEnemySprite(string enemyName, Color color)
    {
        // All enemies now require PixelLab sprites - no procedural fallback
        string path = $"Enemies/{enemyName.ToLower()}";
        Sprite sprite = LoadSprite(path);
        
        if (sprite == null)
        {
            DebugLog.Error($"[SpriteLoader] CRITICAL: Failed to load enemy sprite for {enemyName} from Resources/Sprites/{path}!");
            DebugLog.Error($"[SpriteLoader] All enemies require PixelLab sprites. Generate sprites using PixelLab and place in Resources/Sprites/Enemies/{enemyName}/");
        }
        else
        {
            DebugLog.Info($"[SpriteLoader] Loaded {enemyName} sprite: {sprite.texture.width}x{sprite.texture.height}px, PPU={sprite.pixelsPerUnit}");
        }
        
        return sprite;
    }
    
    /// <summary>
    /// Load projectile sprite with procedural fallback
    /// </summary>
    public static Sprite LoadProjectileSprite(string projectileName)
    {
        string path = $"Projectiles/{projectileName.ToLower()}";
        Sprite sprite = LoadSprite(path);
        
        if (sprite == null)
        {
            DebugLog.Verbose($"[SpriteLoader] Using procedural {projectileName} sprite as fallback");
            sprite = SpriteGenerator.CreateFireballSprite();
        }
        
        return sprite;
    }
    
    /// <summary>
    /// Load fireball projectile sprite with procedural fallback
    /// </summary>
    public static Sprite LoadFireballSprite()
    {
        return LoadProjectileSprite("fireball");
    }
    
    /// <summary>
    /// Load orbiter projectile sprite with procedural fallback
    /// </summary>
    public static Sprite LoadOrbiterSprite()
    {
        Sprite sprite = LoadSprite("Projectiles/orbiter");
        
        if (sprite == null)
        {
            DebugLog.Verbose("[SpriteLoader] Using procedural orbiter sprite as fallback");
            sprite = SpriteGenerator.CreateOrbiterSprite();
        }
        
        return sprite;
    }
    
    /// <summary>
    /// Load boomerang projectile sprite with procedural fallback
    /// </summary>
    public static Sprite LoadBoomerangSprite()
    {
        Sprite sprite = LoadSprite("Projectiles/boomerang");
        
        if (sprite == null)
        {
            DebugLog.Verbose("[SpriteLoader] Using procedural boomerang sprite as fallback");
            sprite = SpriteGenerator.CreateBoomerangSprite();
        }
        
        return sprite;
    }
    
    /// <summary>
    /// Load XP gem sprite with procedural fallback
    /// </summary>
    public static Sprite LoadXPGemSprite()
    {
        Sprite sprite = LoadSprite("Effects/xp_gem");
        
        if (sprite == null)
        {
            DebugLog.Verbose("[SpriteLoader] Using procedural XP gem sprite as fallback");
            sprite = SpriteGenerator.CreateXPGemSprite();
        }
        
        return sprite;
    }
    
    /// <summary>
    /// Clear the sprite cache (call on scene unload if needed)
    /// </summary>
    public static void ClearCache()
    {
        spriteCache.Clear();
        DebugLog.Info("[SpriteLoader] Sprite cache cleared");
    }
    
    /// <summary>
    /// Get cache statistics
    /// </summary>
    public static void LogCacheStats()
    {
        int loadedCount = 0;
        long memoryBytes = 0;
        
        foreach (var kvp in spriteCache)
        {
            if (kvp.Value != null)
            {
                loadedCount++;
                if (kvp.Value.texture != null)
                {
                    memoryBytes += kvp.Value.texture.width * kvp.Value.texture.height * 4; // RGBA
                }
            }
        }
        
        DebugLog.Info($"[SpriteLoader] Cache: {loadedCount} sprites loaded, {memoryBytes / 1024}KB memory");
    }
}
