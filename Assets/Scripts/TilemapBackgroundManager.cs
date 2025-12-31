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
    /// Load tileset sprites from Resources and create terrain tiles
    /// </summary>
    private void LoadAndCreateTiles()
    {
        DebugLog.Info("[TilemapBackgroundManager] Loading terrain tile sprites from Resources...");

        // Load PixelLab-generated terrain tiles
        Sprite grassSprite = Resources.Load<Sprite>("Sprites/Backgrounds/terrain_grass");
        Sprite dirtSprite = Resources.Load<Sprite>("Sprites/Backgrounds/terrain_dirt");
        Sprite stoneSprite = Resources.Load<Sprite>("Sprites/Backgrounds/terrain_stone");
        Sprite desertSprite = Resources.Load<Sprite>("Sprites/Backgrounds/terrain_desert");

        if (grassSprite == null) DebugLog.Error("[TilemapBackgroundManager] MISSING SPRITE: Sprites/Backgrounds/terrain_grass");
        if (dirtSprite == null) DebugLog.Error("[TilemapBackgroundManager] MISSING SPRITE: Sprites/Backgrounds/terrain_dirt");
        if (stoneSprite == null) DebugLog.Error("[TilemapBackgroundManager] MISSING SPRITE: Sprites/Backgrounds/terrain_stone");
        if (desertSprite == null) DebugLog.Error("[TilemapBackgroundManager] MISSING SPRITE: Sprites/Backgrounds/terrain_desert");

        pureGrassTile = grassSprite != null ? CreateTileFromSprite(grassSprite, "GrassTile") : null;
        pureDirtTile = dirtSprite != null ? CreateTileFromSprite(dirtSprite, "DirtTile") : null;
        pureStoneTile = stoneSprite != null ? CreateTileFromSprite(stoneSprite, "StoneTile") : null;
        pureDesertTile = desertSprite != null ? CreateTileFromSprite(desertSprite, "DesertTile") : null;

        DebugLog.Info("[TilemapBackgroundManager] Terrain tiles loaded");
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
