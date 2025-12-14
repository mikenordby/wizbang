using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility for loading and caching sprites from Resources folder.
/// Implements Issue #3 sprite caching - eliminates 12.8MB/sec garbage from procedural generation.
/// </summary>
public static class SpriteLoader
{
    private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>(32);
    private static bool useProceduralFallback = true;
    
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
            if (!useProceduralFallback)
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
    /// Load wizard sprite with procedural fallback
    /// </summary>
    public static Sprite LoadWizardSprite()
    {
        Sprite sprite = LoadSprite("Player/wizard");
        
        // Fallback to procedural generation if asset not found
        if (sprite == null && useProceduralFallback)
        {
            DebugLog.Info("[SpriteLoader] Using procedural wizard sprite (512px) as fallback");
            sprite = SpriteGenerator.CreateWizardSprite();
            if (sprite != null)
            {
                DebugLog.Info($"[SpriteLoader] Generated wizard sprite: {sprite.texture.width}x{sprite.texture.height}px, PPU={sprite.pixelsPerUnit}");
            }
        }
        else if (sprite != null)
        {
            DebugLog.Info($"[SpriteLoader] Loaded wizard sprite from Resources: {sprite.texture.width}x{sprite.texture.height}px, PPU={sprite.pixelsPerUnit}");
        }
        
        return sprite;
    }
    
    /// <summary>
    /// Load enemy sprite with procedural fallback
    /// </summary>
    public static Sprite LoadEnemySprite(string enemyName, Color color)
    {
        // Try to load asset first
        string path = $"Enemies/{enemyName.ToLower()}";
        Sprite sprite = LoadSprite(path);
        
        // Fallback to procedural generation
        if (sprite == null && useProceduralFallback)
        {
            DebugLog.Info($"[SpriteLoader] Using procedural {enemyName} sprite as fallback");
            
            if (enemyName.ToLower().Contains("blob") || enemyName.ToLower().Contains("slime"))
                sprite = SpriteGenerator.CreateBlobSprite(color);
            else if (enemyName.ToLower().Contains("skeleton"))
                sprite = SpriteGenerator.CreateSkeletonSprite(color);
            else
                sprite = SpriteGenerator.CreateBlobSprite(color); // Default fallback
            
            if (sprite != null)
            {
                DebugLog.Info($"[SpriteLoader] Generated {enemyName} sprite: {sprite.texture.width}x{sprite.texture.height}px, PPU={sprite.pixelsPerUnit}");
            }
        }
        else if (sprite != null)
        {
            DebugLog.Info($"[SpriteLoader] Loaded {enemyName} sprite from Resources: {sprite.texture.width}x{sprite.texture.height}px, PPU={sprite.pixelsPerUnit}");
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
        
        if (sprite == null && useProceduralFallback)
        {
            DebugLog.Verbose($"[SpriteLoader] Using procedural {projectileName} sprite as fallback");
            sprite = SpriteGenerator.CreateFireballSprite();
        }
        
        return sprite;
    }
    
    /// <summary>
    /// Load orbiter projectile sprite with procedural fallback
    /// </summary>
    public static Sprite LoadOrbiterSprite()
    {
        Sprite sprite = LoadSprite("Projectiles/orbiter");
        
        if (sprite == null && useProceduralFallback)
        {
            DebugLog.Verbose("[SpriteLoader] Using procedural orbiter sprite as fallback");
            sprite = SpriteGenerator.CreateOrbiterSprite();
        }
        
        return sprite;
    }
    
    /// <summary>
    /// Load XP gem sprite with procedural fallback
    /// </summary>
    public static Sprite LoadXPGemSprite()
    {
        Sprite sprite = LoadSprite("Effects/xp_gem");
        
        if (sprite == null && useProceduralFallback)
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
    
    /// <summary>
    /// Enable or disable procedural generation fallback
    /// </summary>
    public static void SetProceduralFallback(bool enabled)
    {
        useProceduralFallback = enabled;
        DebugLog.Info($"[SpriteLoader] Procedural fallback: {(enabled ? "ENABLED" : "DISABLED")}");
    }
}
