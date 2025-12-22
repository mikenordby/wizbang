using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Displays collected items in the HUD as small icons.
/// Shows item icons along the bottom or side of the screen.
/// </summary>
public class ItemInventoryHUD : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private Vector2 startPosition = new Vector2(10, 10);
    [SerializeField] private float iconSize = 32f;
    [SerializeField] private float iconSpacing = 36f;
    [SerializeField] private int maxIconsPerRow = 10;
    
    private PlayerInventory inventory;
    private List<Texture2D> cachedTextures = new List<Texture2D>();
    private GUIStyle countStyle;
    
    private void Start()
    {
        // Find player inventory
        Player player = GameServices.Player;
        if (player != null)
        {
            inventory = player.GetComponent<PlayerInventory>();
        }
    }
    
    private void OnGUI()
    {
        // Only show during gameplay
        if (GamePhaseManager.CurrentPhase != GamePhase.Gameplay) return;
        if (inventory == null) return;
        
        // Initialize style
        if (countStyle == null)
        {
            countStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerRight
            };
            countStyle.normal.textColor = Color.white;
        }
        
        List<InventorySlot> items = inventory.GetAllItems();
        if (items.Count == 0) return;
        
        // Position from bottom-left of screen
        float baseX = startPosition.x;
        float baseY = Screen.height - startPosition.y - iconSize;
        
        for (int i = 0; i < items.Count; i++)
        {
            InventorySlot slot = items[i];
            if (slot.definition == null) continue;
            
            // Calculate position
            int row = i / maxIconsPerRow;
            int col = i % maxIconsPerRow;
            float x = baseX + col * iconSpacing;
            float y = baseY - row * iconSpacing;
            
            Rect iconRect = new Rect(x, y, iconSize, iconSize);
            
            // Draw background based on rarity
            Color rarityColor = ItemRarityUtils.GetColor(slot.definition.rarity);
            Color bgColor = new Color(rarityColor.r * 0.3f, rarityColor.g * 0.3f, rarityColor.b * 0.3f, 0.8f);
            
            // Draw background
            Texture2D bgTex = GetColorTexture(bgColor);
            GUI.DrawTexture(iconRect, bgTex);
            
            // Draw border
            Color borderColor = rarityColor;
            DrawBorder(iconRect, borderColor, 2);
            
            // Draw item icon (placeholder - use rarity color)
            Rect innerRect = new Rect(x + 4, y + 4, iconSize - 8, iconSize - 8);
            Texture2D iconTex = GetColorTexture(rarityColor);
            GUI.DrawTexture(innerRect, iconTex);
            
            // Draw stack count
            if (slot.count > 1)
            {
                Rect countRect = new Rect(x, y, iconSize - 2, iconSize - 2);
                GUI.Label(countRect, slot.count.ToString(), countStyle);
            }
        }
        
        // Draw total item count label
        string countLabel = $"Items: {inventory.TotalItemCount}";
        GUI.Label(new Rect(baseX, baseY - 20, 100, 20), countLabel);
    }
    
    private void DrawBorder(Rect rect, Color color, int thickness)
    {
        Texture2D tex = GetColorTexture(color);
        
        // Top
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), tex);
        // Bottom
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), tex);
        // Left
        GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), tex);
        // Right
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), tex);
    }
    
    // Cache for color textures
    private Dictionary<Color32, Texture2D> colorTextureCache = new Dictionary<Color32, Texture2D>();
    
    private Texture2D GetColorTexture(Color color)
    {
        Color32 key = color;
        if (!colorTextureCache.TryGetValue(key, out Texture2D tex) || tex == null)
        {
            tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            colorTextureCache[key] = tex;
        }
        return tex;
    }
}

