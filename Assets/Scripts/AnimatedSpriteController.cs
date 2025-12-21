using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles frame-based sprite animations for 8-directional characters
/// Loads animation frames from Resources/Sprites/{entityType}/walking/
/// </summary>
public class AnimatedSpriteController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private string entityType; // "PlayerWizard", "Enemies/Goblin", etc.
    [SerializeField] private string animationName = "walking"; // "walking", "idle", etc.
    [SerializeField] private float framesPerSecond = 10f;
    [SerializeField] private bool playOnStart = false;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    // Animation state
    private bool isPlaying = false;
    private Direction8 currentDirection = Direction8.South;
    private int currentFrame = 0;
    private float frameTimer = 0f;
    
    // Cached animation data (per-instance)
    private Dictionary<Direction8, Sprite[]> animationCache = new Dictionary<Direction8, Sprite[]>();
    private bool isInitialized = false;
    
    // Static cache shared across all instances to avoid reloading same animations
    // Key: "{category}/{entityType}/{animationName}" (e.g., "Heroes/wizard/walking-8-frames")
    private static Dictionary<string, Dictionary<Direction8, Sprite[]>> globalAnimationCache 
        = new Dictionary<string, Dictionary<Direction8, Sprite[]>>();
    
    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Log if spriteRenderer is still null after Awake (diagnostic)
        if (spriteRenderer == null)
        {
            DebugLog.Warning($"AnimatedSpriteController.Awake() on {gameObject.name}: SpriteRenderer is NULL after GetComponent()", "AnimSprite");
        }
    }

    /// <summary>
    /// Explicitly set the SpriteRenderer (useful when component is added dynamically)
    /// </summary>
    public void SetSpriteRenderer(SpriteRenderer renderer)
    {
        spriteRenderer = renderer;
    }
    
    void Start()
    {
        if (playOnStart)
        {
            LoadAllAnimations();
            Play();
        }
    }
    
    void Update()
    {
        if (!isPlaying || !isInitialized)
            return;
        
        // Update frame timer
        frameTimer += Time.deltaTime;
        float frameDuration = 1f / framesPerSecond;
        
        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            AdvanceFrame();
        }
    }
    
    public void SetEntityType(string type)
    {
        entityType = type;
        isInitialized = false;
    }
    
    public void SetAnimation(string animation)
    {
        animationName = animation;
        isInitialized = false;
    }
    
    /// <summary>
    /// Load all animation frames for all 8 directions
    /// </summary>
    public void LoadAllAnimations()
    {
        if (string.IsNullOrEmpty(entityType))
        {
            DebugLog.Warning($"[AnimatedSpriteController] EntityType not set, cannot load animations");
            return;
        }        
        // Check global cache first to avoid reloading
        string category = entityType.StartsWith("Player", System.StringComparison.OrdinalIgnoreCase) ? "Heroes" : "Enemies";
        string entityTypeLower = entityType.Replace("Player", "").ToLower();
        string cacheKey = $"{category}/{entityTypeLower}/{animationName}";
        
        if (globalAnimationCache.TryGetValue(cacheKey, out var cachedAnimations))
        {
            // Reuse cached animations (huge performance boost for spawning many enemies)
            animationCache = cachedAnimations;
            isInitialized = true;
            currentFrame = 0;
            UpdateSprite();
            DebugLog.Verbose($"[AnimatedSpriteController] Loaded {cachedAnimations.Count} cached directions for {cacheKey}");
            // Silent cache usage - only log if sprite assignment fails
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                DebugLog.Warning($"CACHED animations loaded but sprite NOT assigned for {cacheKey}", "AnimSprite");
            }
            return;
        }
        // CRITICAL FIX: Create NEW dictionary instance instead of clearing
        // Clearing would affect other enemies sharing the same dictionary reference from global cache!
        // This fixes the bug where recycled enemies (skeleton->dragon) would corrupt other enemies' sprites
        animationCache = new Dictionary<Direction8, Sprite[]>();
        
        Direction8[] directions = new Direction8[]
        {
            Direction8.South,
            Direction8.SouthWest,
            Direction8.West,
            Direction8.NorthWest,
            Direction8.North,
            Direction8.NorthEast,
            Direction8.East,
            Direction8.SouthEast
        };
        
        int loadedDirections = 0;
        foreach (Direction8 dir in directions)
        {
            Sprite[] frames = LoadAnimationFrames(dir);
            if (frames != null && frames.Length > 0)
            {
                animationCache[dir] = frames;
                loadedDirections++;
                DebugLog.Verbose($"[AnimatedSpriteController] Loaded {frames.Length} frames for {entityType}/{animationName}/{GetDirectionName(dir)}", "AnimSprite");
            }
        }
        
        if (loadedDirections > 0)
        {
            isInitialized = true;
            currentFrame = 0;
            UpdateSprite();

            // Cache loaded animations globally for reuse (reuse variables from above)
            globalAnimationCache[cacheKey] = animationCache;

            DebugLog.Info($"[AnimatedSpriteController] Successfully loaded {loadedDirections}/8 directions for {entityType}/{animationName}", "AnimSprite");
            // Only log if something went wrong
            if (loadedDirections < 8)
            {
                DebugLog.Warning($"Loaded ONLY {loadedDirections}/8 directions for {entityType}/{animationName}", "AnimSprite");
            }
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                PersistentLogger.Error($"Animations loaded but sprite NOT assigned for {entityType}/{animationName}", "AnimSprite", captureScreenshot: true);
            }
        }
        else
        {
            DebugLog.Warning($"[AnimatedSpriteController] No animation frames found for {entityType}/{animationName}");
            PersistentLogger.Error($"NO FRAMES LOADED for {entityType}/{animationName} - all 8 directions returned 0 frames!", "AnimSprite", captureScreenshot: true);
        }
    }
    
    /// <summary>
    /// Load all frames for a specific direction
    /// Unified path structure: Heroes/{hero}/animations/{animation}/{direction}/frame_XXX.png
    ///                         Enemies/{enemy}/animations/{animation}/{direction}/frame_XXX.png
    /// </summary>
    private Sprite[] LoadAnimationFrames(Direction8 direction)
    {
        string dirName = GetDirectionName(direction);
        
        // Determine category (Heroes or Enemies) based on entityType
        string category = entityType.StartsWith("Player", System.StringComparison.OrdinalIgnoreCase) ? "Heroes" : "Enemies";
        string entityTypeLower = entityType.Replace("Player", "").ToLower();
        
        // Unified path: {category}/{entityType}/animations/{animationName}/{direction}/frame_XXX
        string basePath = $"{category}/{entityTypeLower}/animations/{animationName}/{dirName}";
        
        DebugLog.Verbose($"[AnimatedSpriteController.LoadAnimationFrames] entityType='{entityType}', category='{category}', entityTypeLower='{entityTypeLower}', animationName='{animationName}', dirName='{dirName}'");
        DebugLog.Verbose($"[AnimatedSpriteController.LoadAnimationFrames] Attempting to load from: '{basePath}/frame_XXX'");
        
        // Try to load frames - keep loading until we hit a missing frame
        List<Sprite> frames = new List<Sprite>();
        int frameIndex = 0;
        const int maxFrames = 20; // Safety limit
        
        while (frameIndex < maxFrames)
        {
            // Unity imports PNGs as sprite atlases - try loading without the _0 suffix first
            string framePath = $"{basePath}/frame_{frameIndex:D3}";
            Sprite frame = SpriteLoader.LoadSprite(framePath);
            
            if (frame == null)
            {
                // No more frames for this direction (expected when reaching end of sequence)
                break;
            }
            
            frames.Add(frame);
            frameIndex++;
        }
        
        if (frames.Count > 0)
        {
            DebugLog.Verbose($"[AnimatedSpriteController] Loaded {frames.Count} frames from {basePath}");
            // Silent success - parent method will log summary
            return frames.ToArray();
        }

        // Silent - SpriteLoader already logged error for missing entity
        DebugLog.Verbose($"[AnimatedSpriteController] No frames loaded from {basePath}");
        return null;
    }
    
    /// <summary>
    /// Start playing the animation
    /// </summary>
    public void Play()
    {
        if (!isInitialized)
        {
            LoadAllAnimations();
        }
        
        if (isInitialized)
        {
            isPlaying = true;
            currentFrame = 0;
            frameTimer = 0f;
            UpdateSprite();
        }
    }
    
    /// <summary>
    /// Stop playing the animation
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        currentFrame = 0;
        frameTimer = 0f;
    }
    
    /// <summary>
    /// Pause the animation at current frame
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
    }
    
    /// <summary>
    /// Resume playing from current frame
    /// </summary>
    public void Resume()
    {
        if (isInitialized)
        {
            isPlaying = true;
        }
    }
    
    /// <summary>
    /// Update the current direction and display appropriate sprite
    /// </summary>
    public void UpdateDirection(Vector2 moveDirection)
    {
        if (moveDirection == Vector2.zero)
            return;
        
        Direction8 newDirection = GetDirection8(moveDirection);
        
        if (newDirection != currentDirection)
        {
            currentDirection = newDirection;
            currentFrame = 0; // Reset to first frame when changing direction
            frameTimer = 0f;
            UpdateSprite();
        }
    }
    
    /// <summary>
    /// Advance to next frame in the animation
    /// </summary>
    private void AdvanceFrame()
    {
        if (!animationCache.ContainsKey(currentDirection))
            return;
        
        Sprite[] frames = animationCache[currentDirection];
        if (frames.Length == 0)
            return;
        
        currentFrame = (currentFrame + 1) % frames.Length;
        UpdateSprite();
    }
    
    /// <summary>
    /// Update the sprite renderer with current frame
    /// </summary>
    public void UpdateSprite()
    {
        // CRITICAL: Check for null spriteRenderer and log error if missing
        if (spriteRenderer == null)
        {
            PersistentLogger.Error($"UpdateSprite() called but spriteRenderer is NULL on {gameObject.name} (entity={entityType})", "AnimSprite", captureScreenshot: false);
            // Try to get it again as a fallback
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                PersistentLogger.Error($"GetComponent<SpriteRenderer>() FAILED - no SpriteRenderer on {gameObject.name}", "AnimSprite", captureScreenshot: true);
                return;
            }
        }

        if (!isInitialized)
            return;

        if (!animationCache.ContainsKey(currentDirection))
        {
            DebugLog.Warning($"[AnimatedSpriteController] No animation cached for direction {currentDirection}");
            return;
        }

        Sprite[] frames = animationCache[currentDirection];
        if (frames.Length == 0)
            return;

        // Clamp frame index
        currentFrame = Mathf.Clamp(currentFrame, 0, frames.Length - 1);

        spriteRenderer.sprite = frames[currentFrame];
    }
    
    /// <summary>
    /// Convert Direction8 enum to folder name (uses hyphens)
    /// </summary>
    private string GetDirectionName(Direction8 direction)
    {
        switch (direction)
        {
            case Direction8.South: return "south";
            case Direction8.SouthWest: return "south-west";
            case Direction8.West: return "west";
            case Direction8.NorthWest: return "north-west";
            case Direction8.North: return "north";
            case Direction8.NorthEast: return "north-east";
            case Direction8.East: return "east";
            case Direction8.SouthEast: return "south-east";
            default: return "south";
        }
    }
    
    /// <summary>
    /// Convert a 2D movement vector into an 8-directional heading
    /// </summary>
    private Direction8 GetDirection8(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Normalize to 0-360
        if (angle < 0) angle += 360;
        
        // Map angles to 8 directions
        // East = 0°, North = 90°, West = 180°, South = 270°
        if (angle >= 337.5f || angle < 22.5f)
            return Direction8.East;
        else if (angle >= 22.5f && angle < 67.5f)
            return Direction8.NorthEast;
        else if (angle >= 67.5f && angle < 112.5f)
            return Direction8.North;
        else if (angle >= 112.5f && angle < 157.5f)
            return Direction8.NorthWest;
        else if (angle >= 157.5f && angle < 202.5f)
            return Direction8.West;
        else if (angle >= 202.5f && angle < 247.5f)
            return Direction8.SouthWest;
        else if (angle >= 247.5f && angle < 292.5f)
            return Direction8.South;
        else // 292.5f to 337.5f
            return Direction8.SouthEast;
    }
    
    // Public getters
    public bool IsPlaying => isPlaying;
    public bool IsInitialized => isInitialized;
    public Direction8 CurrentDirection => currentDirection;
    public int CurrentFrame => currentFrame;
    public int GetFrameCount(Direction8 direction)
    {
        if (animationCache.ContainsKey(direction))
            return animationCache[direction].Length;
        return 0;
    }
}
