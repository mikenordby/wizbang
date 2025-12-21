using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Manages tilemap-based background using PixelLab generated tilesets.
/// Creates procedural terrain patterns with dirt, grass, stone, and desert tiles.
/// </summary>
public class TilemapBackgroundManager : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [SerializeField] private Tilemap tilemap;
    
    // Pure terrain tiles extracted from Wang tilesets
    private TileBase pureDirtTile;
    private TileBase pureGrassTile;
    private TileBase pureStoneTile;
    private TileBase pureDesertTile;
    
    [Header("Generation Settings")]
    [SerializeField] private int worldWidth = 300;
    [SerializeField] private int worldHeight = 300;
    
    private Camera mainCamera;
    private Dictionary<Vector3Int, TileBase> generatedTiles = new Dictionary<Vector3Int, TileBase>();
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        if (tilemap == null)
        {
            tilemap = GetComponentInChildren<Tilemap>();
        }
        
        if (tilemap == null)
        {
            DebugLog.Error("[TilemapBackgroundManager] No Tilemap found! Please add a Grid with Tilemap as child.");
            return;
        }
        
        // Set tilemap renderer to render behind all other sprites
        TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = -100;
            DebugLog.Info("[TilemapBackgroundManager] Set TilemapRenderer.sortingOrder = -100");
        }
        
        // Load tileset sprites and create tiles
        LoadAndCreateTiles();
        
        GenerateBackgroundTilemap();
        
        DebugLog.Info($"[TilemapBackgroundManager] Generated {generatedTiles.Count} tiles for background");
    }
    
    /// <summary>
    /// Load tileset sprites from Resources and extract pure terrain tiles
    /// </summary>
    private void LoadAndCreateTiles()
    {
        DebugLog.Info("[TilemapBackgroundManager] Loading terrain tile sprites from Resources...");
        
        // Try to load PixelLab-generated terrain tiles
        Sprite grassSprite = Resources.Load<Sprite>("Sprites/Backgrounds/terrain_grass");
        Sprite dirtSprite = Resources.Load<Sprite>("Sprites/Backgrounds/terrain_dirt");
        Sprite stoneSprite = Resources.Load<Sprite>("Sprites/Backgrounds/terrain_stone");
        Sprite desertSprite = Resources.Load<Sprite>("Sprites/Backgrounds/terrain_desert");
        
        // Use PixelLab sprites if available, otherwise use solid colors
        if (grassSprite != null)
        {
            pureGrassTile = CreateTileFromSprite(grassSprite, "GrassTile");
            DebugLog.Info("[TilemapBackgroundManager] Using PixelLab grass tile");
        }
        else
        {
            pureGrassTile = CreateColoredTile(new Color(0.35f, 0.65f, 0.25f), "PureGrass");
        }
        
        if (dirtSprite != null)
        {
            pureDirtTile = CreateTileFromSprite(dirtSprite, "DirtTile");
            DebugLog.Info("[TilemapBackgroundManager] Using PixelLab dirt tile");
        }
        else
        {
            pureDirtTile = CreateColoredTile(new Color(0.4f, 0.25f, 0.15f), "PureDirt");
        }
        
        if (stoneSprite != null)
        {
            pureStoneTile = CreateTileFromSprite(stoneSprite, "StoneTile");
            DebugLog.Info("[TilemapBackgroundManager] Using PixelLab stone tile");
        }
        else
        {
            pureStoneTile = CreateColoredTile(new Color(0.55f, 0.5f, 0.45f), "PureStone");
        }
        
        if (desertSprite != null)
        {
            pureDesertTile = CreateTileFromSprite(desertSprite, "DesertTile");
            DebugLog.Info("[TilemapBackgroundManager] Using PixelLab desert tile");
        }
        else
        {
            pureDesertTile = CreateColoredTile(new Color(0.75f, 0.65f, 0.4f), "PureDesert");
        }
        
        DebugLog.Info("[TilemapBackgroundManager] Terrain tiles loaded successfully");
    }
    
    /// <summary>
    /// Create a Tile from a sprite
    /// </summary>
    private TileBase CreateTileFromSprite(Sprite sprite, string tileName)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.name = tileName;
        return tile;
    }
    
    /// <summary>
    /// Extract a single 32x32 tile from a 128x128 Wang tileset texture
    /// </summary>
    private TileBase ExtractWangTile(Texture2D sourceTexture, int tileIndex, string tileName)
    {
        // Wang tileset is 4x4 grid of 32x32 tiles
        int tilesPerRow = 4;
        int tileSize = 32;
        
        // Calculate tile position in grid (0 = top-left, 15 = bottom-right)
        int tileX = tileIndex % tilesPerRow;
        int tileY = (tilesPerRow - 1) - (tileIndex / tilesPerRow); // Flip Y for Unity coords
        
        // Extract the 32x32 pixel region
        int pixelX = tileX * tileSize;
        int pixelY = tileY * tileSize;
        
        Color[] pixels = sourceTexture.GetPixels(pixelX, pixelY, tileSize, tileSize);
        
        // Create new texture for this tile
        Texture2D tileTexture = new Texture2D(tileSize, tileSize);
        tileTexture.filterMode = FilterMode.Point;
        tileTexture.SetPixels(pixels);
        tileTexture.Apply();
        
        // Create sprite from texture
        Sprite tileSprite = Sprite.Create(
            tileTexture,
            new Rect(0, 0, tileSize, tileSize),
            new Vector2(0.5f, 0.5f),
            32  // PPU = 32 to match your game standard
        );
        
        // Create Unity Tile
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = tileSprite;
        tile.name = tileName;
        return tile;
    }
    
    /// <summary>
    /// Create fallback colored tiles matching Wang tileset colors
    /// </summary>
    private void CreateFallbackTiles()
    {
        DebugLog.Info("[TilemapBackgroundManager] Creating solid color terrain tiles");
        
        // Colors sampled from the actual Wang tilesets
        pureDirtTile = CreateColoredTile(new Color(0.4f, 0.25f, 0.15f), "PureDirt");       // Rich brown dirt
        pureGrassTile = CreateColoredTile(new Color(0.35f, 0.65f, 0.25f), "PureGrass");   // Vibrant grass green
        pureStoneTile = CreateColoredTile(new Color(0.55f, 0.5f, 0.45f), "PureStone");    // Gray stone
        pureDesertTile = CreateColoredTile(new Color(0.75f, 0.65f, 0.4f), "PureDesert");   // Sandy tan
    }
    
    /// <summary>
    /// Create a simple colored tile
    /// </summary>
    private TileBase CreateColoredTile(Color color, string tileName)
    {
        // Create a simple 32x32 colored texture
        Texture2D texture = new Texture2D(32, 32);
        texture.filterMode = FilterMode.Point;
        
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.name = tileName;
        return tile;
    }
    
    /// <summary>
    /// Generate procedural tilemap background with large cohesive biomes
    /// </summary>
    private void GenerateBackgroundTilemap()
    {
        int centerX = worldWidth / 2;
        int centerY = worldHeight / 2;
        
        // Use much larger scale for biome placement (creates big regions)
        float biomeScale = 0.02f; // Was 0.1f - now creates ~50x50 tile regions
        
        for (int x = -centerX; x < centerX; x++)
        {
            for (int y = -centerY; y < centerY; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                
                // Use Perlin noise with large scale to determine biome regions
                float noiseValue = Mathf.PerlinNoise(x * biomeScale + 1000, y * biomeScale + 1000);
                
                TileBase tileToPlace = null;
                
                // Create distinct biome regions with wider thresholds
                // This creates large cohesive areas of each terrain type
                if (noiseValue < 0.2f)
                {
                    tileToPlace = pureStoneTile; // Stone biome (20%)
                }
                else if (noiseValue < 0.35f)
                {
                    tileToPlace = pureDirtTile; // Dirt biome (15%)
                }
                else if (noiseValue < 0.65f)
                {
                    tileToPlace = pureGrassTile; // Grass biome (30% - most common)
                }
                else
                {
                    tileToPlace = pureDesertTile; // Desert biome (35%)
                }
                
                if (tileToPlace != null)
                {
                    tilemap.SetTile(tilePos, tileToPlace);
                    generatedTiles[tilePos] = tileToPlace;
                }
            }
        }
        
        tilemap.RefreshAllTiles();
    }
    
    /// <summary>
    /// Get tile at world position
    /// </summary>
    public TileBase GetTileAtPosition(Vector3 worldPosition)
    {
        Vector3Int cellPos = tilemap.WorldToCell(worldPosition);
        return tilemap.GetTile(cellPos);
    }
}
