using UnityEngine;

/// <summary>
/// Treasure chest pickup that grants a level-up perk selection without leveling up.
/// </summary>
public class TreasureChest : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D chestCollider;
    private bool isCollected = false;
    
    private void Awake()
    {
        // Create sprite renderer
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = LoadChestSprite();
        spriteRenderer.sortingOrder = 5; // Same as projectiles (above ground)
        
        // Create trigger collider for pickup
        // Chest sprite is ~64px at 64 PPU = 1.0 world unit
        // Use smaller radius for tighter hitbox that matches chest body
        chestCollider = gameObject.AddComponent<CircleCollider2D>();
        chestCollider.radius = 0.45f; // Tighter fit to chest body (was 0.6f)
        chestCollider.isTrigger = true;
    }
    
    /// <summary>
    /// Load treasure chest sprite from Resources (PixelLab generated)
    /// </summary>
    private Sprite LoadChestSprite()
    {
        Sprite sprite = Resources.Load<Sprite>("Sprites/Objects/treasure_chest_closed");
        if (sprite == null)
        {
            DebugLog.Error("[TreasureChest] Failed to load treasure_chest_closed sprite from Resources");
            return CreateFallbackChestSprite();
        }
        return sprite;
    }
    
    /// <summary>
    /// Create a fallback procedural treasure chest sprite if loading fails
    /// </summary>
    private Sprite CreateFallbackChestSprite()
    {
        int resolution = 64;
        Texture2D texture = new Texture2D(resolution, resolution);
        texture.filterMode = FilterMode.Point;
        
        // Chest colors
        Color woodBase = new Color(0.4f, 0.25f, 0.1f); // Dark brown
        Color woodLight = new Color(0.55f, 0.35f, 0.15f); // Light brown
        Color gold = new Color(1f, 0.84f, 0f); // Gold trim
        Color goldDark = new Color(0.7f, 0.55f, 0f); // Dark gold
        Color lockColor = new Color(0.3f, 0.3f, 0.3f); // Dark grey lock
        
        // Clear background
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                texture.SetPixel(x, y, Color.clear);
            }
        }
        
        int cx = resolution / 2;
        int cy = resolution / 2;
        
        // Draw chest body (rectangular)
        for (int y = cy - 20; y < cy + 15; y++)
        {
            for (int x = cx - 22; x <= cx + 22; x++)
            {
                // Wood planks with vertical grain
                float grain = Mathf.PerlinNoise(x * 0.5f, y * 0.1f);
                Color woodColor = Color.Lerp(woodBase, woodLight, grain);
                texture.SetPixel(x, y, woodColor);
            }
        }
        
        // Draw chest lid (curved top)
        for (int y = cy + 10; y < cy + 28; y++)
        {
            int width = 22 - ((y - (cy + 10)) * 22 / 18);
            for (int x = cx - width; x <= cx + width; x++)
            {
                float grain = Mathf.PerlinNoise(x * 0.5f, y * 0.1f);
                Color woodColor = Color.Lerp(woodBase, woodLight, grain);
                texture.SetPixel(x, y, woodColor);
            }
        }
        
        // Gold horizontal bands (3 bands)
        int[] bandY = { cy - 15, cy, cy + 10 };
        foreach (int y in bandY)
        {
            for (int x = cx - 22; x <= cx + 22; x++)
            {
                for (int dy = 0; dy < 3; dy++)
                {
                    Color bandColor = (dy == 1) ? gold : goldDark;
                    texture.SetPixel(x, y + dy, bandColor);
                }
            }
        }
        
        // Lock (center front)
        for (int y = cy - 5; y < cy + 5; y++)
        {
            for (int x = cx - 4; x <= cx + 4; x++)
            {
                texture.SetPixel(x, y, lockColor);
            }
        }
        
        // Keyhole (black dot)
        for (int y = cy - 2; y < cy + 2; y++)
        {
            for (int x = cx - 2; x <= cx + 2; x++)
            {
                texture.SetPixel(x, y, Color.black);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 64);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected) return;
        
        // Check if player touched the chest
        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            isCollected = true;
            
            // Show perk selection UI (not actual level-up)
            LevelUpUI levelUpUI = GameServices.LevelUpUI;
            if (levelUpUI != null)
            {
                DebugLog.Info($"[TreasureChest] Player collected chest! Showing perk selection");
                levelUpUI.ShowChestReward(player.CurrentLevel); // Special method for treasure chest
            }
            
            // Destroy chest
            Destroy(gameObject);
        }
    }
}
