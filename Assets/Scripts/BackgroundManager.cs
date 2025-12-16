using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manages the game background using procedurally generated grass with dirt paths.
/// Creates infinite scrolling grass background that follows camera.
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    [Header("Background Settings")]
    [SerializeField] private bool useTilemap = true;
    [SerializeField] private Color grassColor1 = new Color(0.4f, 0.7f, 0.3f); // Light green
    [SerializeField] private Color grassColor2 = new Color(0.35f, 0.6f, 0.25f); // Dark green
    
    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera;
    
    private SpriteRenderer backgroundRenderer;
    private Tilemap tilemap;
    private bool isInitialized = false;
    
    private void Start()
    {
        PersistentLogger.Separator("BACKGROUND MANAGER START");
        PersistentLogger.Info("Start() called", "BackgroundManager");
        Debug.Log("[BackgroundManager] Start() called");
        DebugLog.Info("[BackgroundManager] Start() called");
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                PersistentLogger.Info($"Found Camera.main: {mainCamera.name}", "BackgroundManager");
                Debug.Log($"[BackgroundManager] Found Camera.main: {mainCamera.name}");
                DebugLog.Info($"[BackgroundManager] Found Camera.main: {mainCamera.name}");
            }
            else
            {
                PersistentLogger.Error("Camera.main is NULL!", "BackgroundManager");
                Debug.LogError("[BackgroundManager] Camera.main is NULL!");
                DebugLog.Error("[BackgroundManager] Camera.main is NULL!");
            }
        }
        else
        {
            PersistentLogger.Info($"Using assigned camera: {mainCamera.name}", "BackgroundManager");
            Debug.Log($"[BackgroundManager] Using assigned camera: {mainCamera.name}");
            DebugLog.Info($"[BackgroundManager] Using assigned camera: {mainCamera.name}");
        }
        
        InitializeBackground();
    }
    
    /// <summary>
    /// Initialize background - try tilemap first, fall back to sprite-based
    /// </summary>
    private void InitializeBackground()
    {
        PersistentLogger.Info("InitializeBackground() called", "BackgroundManager");
        Debug.Log("[BackgroundManager] InitializeBackground() called");
        DebugLog.Info("[BackgroundManager] InitializeBackground() called");
        
        if (isInitialized)
        {
            DebugLog.Warning("[BackgroundManager] Already initialized, skipping");
            return;
        }
        
        // Try to find existing tilemap
        tilemap = GetComponentInChildren<Tilemap>();
        
        if (tilemap != null && useTilemap)
        {
            DebugLog.Info($"[BackgroundManager] Using existing Tilemap: {tilemap.name}");
            isInitialized = true;
            return;
        }
        
        PersistentLogger.Info("No tilemap found, creating sprite-based background", "BackgroundManager");
        Debug.Log("[BackgroundManager] No tilemap found, creating sprite-based background");
        DebugLog.Info("[BackgroundManager] No tilemap found, creating sprite-based background");
        
        // Fall back to sprite-based background
        CreateSpriteBackground();
        isInitialized = true;
        
        Debug.Log("[BackgroundManager] Background initialization complete");
        DebugLog.Info("[BackgroundManager] Background initialization complete");
    }
    
    /// <summary>
    /// Create a simple sprite-based background with checkerboard pattern
    /// </summary>
    private void CreateSpriteBackground()
    {
        PersistentLogger.Info("CreateSpriteBackground() starting...", "BackgroundManager");
        Debug.Log("[BackgroundManager] CreateSpriteBackground() starting...");
        DebugLog.Info("[BackgroundManager] CreateSpriteBackground() starting...");
        
        // Create background GameObject
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.parent = transform;
        bgObj.transform.position = Vector3.zero;
        DebugLog.Info($"[BackgroundManager] Created Background GameObject at position {bgObj.transform.position}");
        
        backgroundRenderer = bgObj.AddComponent<SpriteRenderer>();
        backgroundRenderer.sortingOrder = -100; // Draw behind everything
        DebugLog.Info($"[BackgroundManager] Added SpriteRenderer with sortingOrder: {backgroundRenderer.sortingOrder}");
        
        // Create background texture (larger for better coverage)
        int textureSize = 1024;
        Texture2D bgTexture = new Texture2D(textureSize, textureSize);
        bgTexture.filterMode = FilterMode.Point;
        bgTexture.wrapMode = TextureWrapMode.Repeat;
        DebugLog.Info($"[BackgroundManager] Created texture: {textureSize}x{textureSize}, FilterMode={bgTexture.filterMode}");
        
        // Generate grass pattern
        Color[] pixels = new Color[textureSize * textureSize];
        int tileSize = 32; // Size of each grass tile
        int tilesPerSide = textureSize / tileSize;
        
        DebugLog.Info($"[BackgroundManager] Generating {tilesPerSide}x{tilesPerSide} grass tiles...");
        
        for (int tileY = 0; tileY < tilesPerSide; tileY++)
        {
            for (int tileX = 0; tileX < tilesPerSide; tileX++)
            {
                // Create unique grass tile for each position
                Sprite grassTile = BackgroundLoader.CreateGrassTile(tileX + tileY * tilesPerSide);
                
                if (grassTile == null)
                {
                    PersistentLogger.Error($"BackgroundLoader.CreateGrassTile returned NULL at tile ({tileX}, {tileY})!", "BackgroundManager");
                    Debug.LogError($"[BackgroundManager] BackgroundLoader.CreateGrassTile returned NULL at tile ({tileX}, {tileY})!");
                    DebugLog.Error($"[BackgroundManager] BackgroundLoader.CreateGrassTile returned NULL at tile ({tileX}, {tileY})!");
                    continue;
                }
                
                // Copy tile pixels to main texture
                Color[] tilePixels = grassTile.texture.GetPixels();
                for (int py = 0; py < tileSize; py++)
                {
                    for (int px = 0; px < tileSize; px++)
                    {
                        int texX = tileX * tileSize + px;
                        int texY = tileY * tileSize + py;
                        pixels[texY * textureSize + texX] = tilePixels[py * tileSize + px];
                    }
                }
            }
        }
        
        DebugLog.Info("[BackgroundManager] Grass tiles generated, applying to texture...");
        bgTexture.SetPixels(pixels);
        
        // Add dirt paths through the grass
        DebugLog.Info("[BackgroundManager] Adding dirt paths...");
        AddDirtPaths(bgTexture, tileSize);
        
        bgTexture.Apply();
        DebugLog.Info("[BackgroundManager] Texture finalized");
        
        // Create sprite from texture
        Sprite bgSprite = Sprite.Create(
            bgTexture,
            new Rect(0, 0, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            32 // Pixels per unit (matches game sprites)
        );
        
        if (bgSprite == null)
        {
            PersistentLogger.Error("Sprite.Create returned NULL!", "BackgroundManager");
            Debug.LogError("[BackgroundManager] Sprite.Create returned NULL!");
            DebugLog.Error("[BackgroundManager] Sprite.Create returned NULL!");
            return;
        }
        
        PersistentLogger.Info($"Created sprite: {bgSprite.texture.width}x{bgSprite.texture.height}, PPU={bgSprite.pixelsPerUnit}", "BackgroundManager");
        Debug.Log($"[BackgroundManager] Created sprite: {bgSprite.texture.width}x{bgSprite.texture.height}, PPU={bgSprite.pixelsPerUnit}");
        DebugLog.Info($"[BackgroundManager] Created sprite: {bgSprite.texture.width}x{bgSprite.texture.height}, PPU={bgSprite.pixelsPerUnit}");
        
        backgroundRenderer.sprite = bgSprite;
        DebugLog.Info("[BackgroundManager] Assigned sprite to SpriteRenderer");
        
        // Scale to cover camera view with extra margin
        if (mainCamera != null)
        {
            float camHeight = mainCamera.orthographicSize * 2f;
            float camWidth = camHeight * mainCamera.aspect;
            
            // Scale sprite to cover camera view plus movement area
            float scaleX = (camWidth * 3f) / (textureSize / 32f);
            float scaleY = (camHeight * 3f) / (textureSize / 32f);
            float scale = Mathf.Max(scaleX, scaleY);
            
            bgObj.transform.localScale = Vector3.one * scale;
            
            DebugLog.Info($"[BackgroundManager] Camera size: {camWidth}x{camHeight}, Background scale: {scale}");
        }
        else
        {
            DebugLog.Warning("[BackgroundManager] mainCamera is NULL, skipping scale calculation");
        }
        
        // Verify renderer is enabled and visible
        Debug.Log($"[BackgroundManager] SpriteRenderer enabled: {backgroundRenderer.enabled}, sprite: {backgroundRenderer.sprite != null}");
        Debug.Log($"[BackgroundManager] GameObject active: {bgObj.activeSelf}, layer: {bgObj.layer}");
        DebugLog.Info($"[BackgroundManager] SpriteRenderer enabled: {backgroundRenderer.enabled}, sprite: {backgroundRenderer.sprite != null}");
        DebugLog.Info($"[BackgroundManager] GameObject active: {bgObj.activeSelf}, layer: {bgObj.layer}");
        
        PersistentLogger.Info($"Created procedural grass background ({textureSize}x{textureSize}, {tilesPerSide}x{tilesPerSide} tiles)", "BackgroundManager");
        PersistentLogger.Separator();
        Debug.Log($"[BackgroundManager] Created procedural grass background ({textureSize}x{textureSize}, {tilesPerSide}x{tilesPerSide} tiles)");
        DebugLog.Info($"[BackgroundManager] Created procedural grass background ({textureSize}x{textureSize}, {tilesPerSide}x{tilesPerSide} tiles)");
    }
    
    /// <summary>
    /// Add winding dirt paths through the grass using Perlin noise
    /// </summary>
    private void AddDirtPaths(Texture2D texture, int tileSize)
    {
        int width = texture.width;
        int height = texture.height;
        
        Color dirtColor = new Color(0.45f, 0.35f, 0.25f); // Brown dirt
        Color dirtDark = new Color(0.35f, 0.25f, 0.18f); // Darker dirt for edges
        int pathWidth = 48; // pixels (about 1.5 tiles)
        
        // Create horizontal winding path
        for (int x = 0; x < width; x++)
        {
            // Use Perlin noise for organic path variation
            float noiseValue = Mathf.PerlinNoise(x * 0.02f, 0.5f);
            int centerY = Mathf.RoundToInt(noiseValue * height * 0.6f + height * 0.2f); // Path through middle-ish area
            
            for (int y = centerY - pathWidth/2; y < centerY + pathWidth/2; y++)
            {
                if (y >= 0 && y < height)
                {
                    // Distance from path center for blending
                    float distFromCenter = Mathf.Abs(y - centerY) / (pathWidth * 0.5f);
                    
                    // Add small pebbles/texture
                    float pebbleNoise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    Color dirtVariation = Color.Lerp(dirtDark, dirtColor, pebbleNoise);
                    
                    // Random small stones
                    if (Random.value < 0.02f) // 2% chance per pixel
                    {
                        dirtVariation = new Color(0.6f, 0.55f, 0.5f); // Light grey pebble
                    }
                    
                    // Blend with grass at edges for smooth transition
                    Color grassPixel = texture.GetPixel(x, y);
                    Color finalColor = Color.Lerp(dirtVariation, grassPixel, distFromCenter * distFromCenter);
                    texture.SetPixel(x, y, finalColor);
                }
            }
        }
        
        // Create vertical winding path
        for (int y = 0; y < height; y++)
        {
            // Different Perlin noise seed for different path
            float noiseValue = Mathf.PerlinNoise(0.3f, y * 0.02f);
            int centerX = Mathf.RoundToInt(noiseValue * width * 0.6f + width * 0.2f);
            
            for (int x = centerX - pathWidth/2; x < centerX + pathWidth/2; x++)
            {
                if (x >= 0 && x < width)
                {
                    float distFromCenter = Mathf.Abs(x - centerX) / (pathWidth * 0.5f);
                    
                    float pebbleNoise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    Color dirtVariation = Color.Lerp(dirtDark, dirtColor, pebbleNoise);
                    
                    if (Random.value < 0.02f)
                    {
                        dirtVariation = new Color(0.6f, 0.55f, 0.5f);
                    }
                    
                    Color grassPixel = texture.GetPixel(x, y);
                    Color finalColor = Color.Lerp(dirtVariation, grassPixel, distFromCenter * distFromCenter);
                    texture.SetPixel(x, y, finalColor);
                }
            }
        }
    }
}
