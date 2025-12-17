using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility for loading and caching sprites from Resources folder.
/// Implements Issue #3 sprite caching - eliminates 12.8MB/sec garbage from procedural generation.
/// </summary>
public static class SpriteLoader
{
    private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>(32);
    
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
            if (cached != null) return cached;
            // Cached null means we already tried and failed - don't retry
            return null;
        }
        
        // Try to load from Resources/Sprites/
        string fullPath = $"Sprites/{path}";
        Sprite sprite = Resources.Load<Sprite>(fullPath);
        
        if (sprite != null)
        {
            spriteCache[path] = sprite;
            DebugLog.Verbose($"[SpriteLoader] Loaded sprite: {fullPath}");
            return sprite;
        }
        
        // Don't cache null - let caller handle fallback (Verbose since procedural fallback is expected)
        DebugLog.Verbose($"[SpriteLoader] Sprite not found in Resources, using procedural fallback: {fullPath}");
        
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
