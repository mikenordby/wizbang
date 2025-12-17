using UnityEngine;
using System.Collections.Generic;

public enum Direction8
{
    South,
    SouthWest,
    West,
    NorthWest,
    North,
    NorthEast,
    East,
    SouthEast
}

public class DirectionalSpriteController : MonoBehaviour
{
    [SerializeField] private string entityType; // "wizard", "goblin", etc.
    [SerializeField] private int numDirections = 8; // 4 or 8
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Direction8 startDirection = Direction8.South;
    
    private Direction8 currentDirection = Direction8.South;
    private Dictionary<Direction8, Sprite> spriteCache;
    private bool isInitialized = false;

    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        DebugLog.Info($"[DirectionalSpriteController] Awake on {gameObject.name}: SpriteRenderer {(spriteRenderer != null ? "found" : "NULL")}");
    }

    void Start()
    {
        LoadAllDirectionSprites();
    }

    public void SetEntityType(string type)
    {
        entityType = type;
        
        // Ensure we have sprite renderer
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        DebugLog.Info($"[DirectionalSpriteController] SetEntityType: {type}, numDirections: {numDirections}");
        LoadAllDirectionSprites();
    }

    private void LoadAllDirectionSprites()
    {
        if (string.IsNullOrEmpty(entityType)) return;

        spriteCache = new Dictionary<Direction8, Sprite>();

        if (numDirections == 8)
        {
            // Load all 8 directions with detailed logging
            spriteCache[Direction8.South] = SpriteLoader.LoadCharacterSprite(entityType, "south");
            spriteCache[Direction8.SouthWest] = SpriteLoader.LoadCharacterSprite(entityType, "south_west");
            spriteCache[Direction8.West] = SpriteLoader.LoadCharacterSprite(entityType, "west");
            spriteCache[Direction8.NorthWest] = SpriteLoader.LoadCharacterSprite(entityType, "north_west");
            spriteCache[Direction8.North] = SpriteLoader.LoadCharacterSprite(entityType, "north");
            spriteCache[Direction8.NorthEast] = SpriteLoader.LoadCharacterSprite(entityType, "north_east");
            spriteCache[Direction8.East] = SpriteLoader.LoadCharacterSprite(entityType, "east");
            spriteCache[Direction8.SouthEast] = SpriteLoader.LoadCharacterSprite(entityType, "south_east");
            
            // Log which sprites failed to load
            int failedCount = 0;
            foreach (var kvp in spriteCache)
            {
                if (kvp.Value == null)
                {
                    DebugLog.Error($"[DirectionalSpriteController] FAILED to load {entityType}/{kvp.Key}");
                    failedCount++;
                }
            }
            if (failedCount > 0)
            {
                DebugLog.Error($"[DirectionalSpriteController] {entityType}: {failedCount}/8 sprites FAILED to load! Check that all sprite files exist in Resources/Sprites/{entityType}/");
            }
        }
        else
        {
            // Load 4 directions, map diagonals to closest cardinal
            spriteCache[Direction8.South] = SpriteLoader.LoadCharacterSprite(entityType, "south");
            spriteCache[Direction8.West] = SpriteLoader.LoadCharacterSprite(entityType, "west");
            spriteCache[Direction8.North] = SpriteLoader.LoadCharacterSprite(entityType, "north");
            spriteCache[Direction8.East] = SpriteLoader.LoadCharacterSprite(entityType, "east");
            
            // Map diagonals to cardinals for 4-direction mode
            spriteCache[Direction8.SouthWest] = spriteCache[Direction8.West];
            spriteCache[Direction8.NorthWest] = spriteCache[Direction8.West];
            spriteCache[Direction8.SouthEast] = spriteCache[Direction8.East];
            spriteCache[Direction8.NorthEast] = spriteCache[Direction8.East];
        }

        isInitialized = true;
        
        // Set initial sprite
        currentDirection = startDirection;
        if (spriteRenderer != null && spriteCache.ContainsKey(currentDirection))
        {
            Sprite initialSprite = spriteCache[currentDirection];
            spriteRenderer.sprite = initialSprite;
            DebugLog.Info($"[DirectionalSpriteController] Initialized {entityType} with {numDirections} directions, starting {currentDirection}");
            DebugLog.Info($"[DirectionalSpriteController] Initial sprite: {(initialSprite != null ? initialSprite.name : "NULL")}, SpriteRenderer.enabled={spriteRenderer.enabled}, color={spriteRenderer.color}, sortingOrder={spriteRenderer.sortingOrder}, layer={gameObject.layer}");
        }
        else
        {
            DebugLog.Error($"[DirectionalSpriteController] Failed to set initial sprite for {entityType}: spriteRenderer={(spriteRenderer != null ? "OK" : "NULL")}, cache has {currentDirection}={spriteCache.ContainsKey(currentDirection)}");
        }
    }

    public void UpdateDirection(Vector2 movement)
    {
        if (!isInitialized)
        {
            DebugLog.Warning($"[DirectionalSpriteController] UpdateDirection called but not initialized yet");
            return;
        }
        
        if (movement == Vector2.zero)
        {
            return;
        }

        Direction8 newDir = GetDirectionFromVector(movement);
        
        DebugLog.Info($"[DirectionalSpriteController] UpdateDirection: movement={movement}, newDir={newDir}, currentDir={currentDirection}");
        
        if (newDir != currentDirection)
        {
            currentDirection = newDir;
            
            if (spriteCache.ContainsKey(newDir) && spriteRenderer != null)
            {
                spriteRenderer.sprite = spriteCache[newDir];
                DebugLog.Info($"[DirectionalSpriteController] ✓ Changed sprite to {newDir}: {spriteRenderer.sprite.name}");
            }
            else
            {
                DebugLog.Error($"[DirectionalSpriteController] ✗ No sprite for direction {newDir}, cache has key: {spriteCache.ContainsKey(newDir)}, renderer: {(spriteRenderer != null)}");
            }
        }
    }

    private Direction8 GetDirectionFromVector(Vector2 input)
    {
        if (numDirections == 8)
        {
            return GetDirection8(input);
        }
        else
        {
            return GetDirection4(input);
        }
    }

    private Direction8 GetDirection8(Vector2 input)
    {
        // Calculate angle in degrees (0° = right, 90° = up)
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        
        // Normalize to 0-360
        if (angle < 0) angle += 360f;
        
        // Map to 8 directions with 45° segments
        // East: 337.5° - 22.5° (0°)
        // SouthEast: 292.5° - 337.5° (315°)
        // South: 247.5° - 292.5° (270°)
        // SouthWest: 202.5° - 247.5° (225°)
        // West: 157.5° - 202.5° (180°)
        // NorthWest: 112.5° - 157.5° (135°)
        // North: 67.5° - 112.5° (90°)
        // NorthEast: 22.5° - 67.5° (45°)
        
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
        else // angle >= 292.5f && angle < 337.5f
            return Direction8.SouthEast;
    }

    private Direction8 GetDirection4(Vector2 input)
    {
        // Prioritize vertical movement (up/down) when nearly diagonal
        if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
        {
            return input.y > 0 ? Direction8.North : Direction8.South;
        }
        else
        {
            return input.x > 0 ? Direction8.East : Direction8.West;
        }
    }

    public Direction8 GetCurrentDirection()
    {
        return currentDirection;
    }

    public int GetNumDirections()
    {
        return numDirections;
    }
}
