using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manages the game background using tilesets or generated patterns.
/// Uses Cainos grass tileset if available, falls back to procedural background.
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
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        InitializeBackground();
    }
    
    /// <summary>
    /// Initialize background - try tilemap first, fall back to sprite-based
    /// </summary>
    private void InitializeBackground()
    {
        if (isInitialized) return;
        
        // Try to find existing tilemap
        tilemap = GetComponentInChildren<Tilemap>();
        
        if (tilemap != null && useTilemap)
        {
            DebugLog.Info("[BackgroundManager] Using existing Tilemap");
            isInitialized = true;
            return;
        }
        
        // Fall back to sprite-based background
        CreateSpriteBackground();
        isInitialized = true;
    }
    
    /// <summary>
    /// Create a simple sprite-based background with checkerboard pattern
    /// </summary>
    private void CreateSpriteBackground()
    {
        // Create background GameObject
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.parent = transform;
        bgObj.transform.position = Vector3.zero;
        
        backgroundRenderer = bgObj.AddComponent<SpriteRenderer>();
        backgroundRenderer.sortingOrder = -100; // Draw behind everything
        
        // Try to load tileset from Cainos pack first
        Sprite tilesetSprite = BackgroundLoader.LoadGrassTileset();
        
        // Create background texture (larger for better coverage)
        int textureSize = 1024;
        Texture2D bgTexture = new Texture2D(textureSize, textureSize);
        bgTexture.filterMode = FilterMode.Point;
        bgTexture.wrapMode = TextureWrapMode.Repeat;
        
        // Generate grass pattern
        Color[] pixels = new Color[textureSize * textureSize];
        int tileSize = 32; // Size of each grass tile
        int tilesPerSide = textureSize / tileSize;
        
        for (int tileY = 0; tileY < tilesPerSide; tileY++)
        {
            for (int tileX = 0; tileX < tilesPerSide; tileX++)
            {
                // Create unique grass tile for each position
                Sprite grassTile = BackgroundLoader.CreateGrassTile(tileX + tileY * tilesPerSide);
                
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
        
        bgTexture.SetPixels(pixels);
        bgTexture.Apply();
        
        // Create sprite from texture
        Sprite bgSprite = Sprite.Create(
            bgTexture,
            new Rect(0, 0, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            32 // Pixels per unit (matches game sprites)
        );
        
        backgroundRenderer.sprite = bgSprite;
        
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
        }
        
        DebugLog.Info($"[BackgroundManager] Created procedural grass background ({textureSize}x{textureSize}, {tilesPerSide}x{tilesPerSide} tiles)");
    }
    
    /// <summary>
    /// Update background to follow camera (for infinite scrolling effect if needed)
    /// </summary>
    private void LateUpdate()
    {
        if (mainCamera != null && backgroundRenderer != null)
        {
            // Keep background centered on camera with pixel-perfect positioning to prevent stuttering
            Vector3 camPos = mainCamera.transform.position;
            
            // Calculate pixels per unit (assuming 32 PPU for the grass texture)
            float pixelsPerUnit = 32f;
            
            // Round to nearest pixel to prevent sub-pixel movement stuttering
            float roundedX = Mathf.Round(camPos.x * pixelsPerUnit) / pixelsPerUnit;
            float roundedY = Mathf.Round(camPos.y * pixelsPerUnit) / pixelsPerUnit;
            
            transform.position = new Vector3(roundedX, roundedY, 0f);
        }
    }
}
