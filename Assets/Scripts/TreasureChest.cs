using UnityEngine;
using System.Collections;

/// <summary>
/// Treasure chest pickup that reveals items with an animated sequence.
/// Animation intensity increases with item rarity (inspired by Megabonk).
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
            DebugLog.Error("[TreasureChest] MISSING SPRITE: Sprites/Objects/treasure_chest_closed");
        }
        return sprite;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected) return;
        
        // Check if player touched the chest
        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            isCollected = true;
            StartCoroutine(OpenChestWithAnimation(player));
        }
    }
    
    /// <summary>
    /// Treasure chest opened - show TreasureUI with item reveal.
    /// </summary>
    private IEnumerator OpenChestWithAnimation(Player player)
    {
        // Disable collider to prevent re-triggering
        if (chestCollider != null)
            chestCollider.enabled = false;
        
        // Determine rarity of the reward
        ItemDatabase itemDb = ItemDatabase.Instance;
        ItemDefinition itemDef = null;
        
        if (itemDb != null && itemDb.ItemCount > 0)
        {
            itemDef = itemDb.GetRandomItem();
        }
        else
        {
            DebugLog.Warning("[TreasureChest] ItemDatabase not available or empty!");
            Destroy(gameObject);
            yield break;
        }
        
        DebugLog.Info($"[TreasureChest] Chest opened! Rolling item: [{itemDef.rarity}] {itemDef.displayName}");
        
        // Find or create TreasureUI
        TreasureUI treasureUI = FindFirstObjectByType<TreasureUI>();
        if (treasureUI == null)
        {
            DebugLog.Info("[TreasureChest] TreasureUI not found, creating one");
            GameObject uiObj = new GameObject("TreasureUI");
            treasureUI = uiObj.AddComponent<TreasureUI>();
        }
        
        // Show treasure UI with the rolled item
        treasureUI.ShowTreasure(itemDef);
        
        // Clean up chest immediately - animation happens in UI
        Destroy(gameObject);
        
        yield break;
    }
    
}
