using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

/// <summary>
/// UI for treasure chest reveal with lottery-style animation.
/// Player clicks/presses space to trigger the reveal, then items are shown with animated reveal.
/// </summary>
public class TreasureUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject treasurePanel;
    [SerializeField] private Image chestImage;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private GameObject itemRevealPanel;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemStatsText;
    
    [Header("Animation Settings")]
    [SerializeField] private float shakeDuration = 1.5f;
    [SerializeField] private float shakeIntensity = 0.2f;
    [SerializeField] private float revealDelay = 0.5f;
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private ItemDefinition currentItem;
    private bool isAnimating = false;
    private bool hasRevealed = false;
    
    private void Awake()
    {
        // Create UI if not already set up
        if (treasurePanel == null)
        {
            CreateUI();
        }
        
        HideUI();
    }
    
    private void Update()
    {
        if (!isAnimating && !hasRevealed && treasurePanel.activeSelf)
        {
            // Wait for player input to trigger reveal
            bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
            bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            
            if (spacePressed || mouseClicked)
            {
                StartCoroutine(RevealItem());
            }
        }
        else if (hasRevealed && treasurePanel.activeSelf)
        {
            // Wait for input to close
            bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
            bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            
            if (spacePressed || mouseClicked)
            {
                CloseAndApplyItem();
            }
        }
    }
    
    /// <summary>
    /// Show the treasure UI with a specific item
    /// </summary>
    public void ShowTreasure(ItemDefinition item)
    {
        currentItem = item;
        hasRevealed = false;
        isAnimating = false;
        
        treasurePanel.SetActive(true);
        itemRevealPanel.SetActive(false);
        
        // Reset prompt position to center for initial state
        if (promptText != null)
        {
            RectTransform promptRect = promptText.GetComponent<RectTransform>();
            if (promptRect != null)
            {
                promptRect.anchorMin = new Vector2(0.5f, 0.3f);
                promptRect.anchorMax = new Vector2(0.5f, 0.3f);
                promptRect.anchoredPosition = Vector2.zero;
            }
            promptText.text = "Press SPACE or Click to Open!";
            promptText.gameObject.SetActive(true);
        }
        
        // Reset chest scale
        if (chestImage != null)
        {
            chestImage.transform.localScale = Vector3.one;
        }
        
        GameState.SetPaused(true);
        
        DebugLog.Info($"[TreasureUI] Showing treasure chest with item: {item.displayName}");
    }
    
    /// <summary>
    /// Animate the chest opening and reveal the item
    /// </summary>
    private IEnumerator RevealItem()
    {
        isAnimating = true;
        promptText.gameObject.SetActive(false);
        
        // Get animation parameters based on rarity
        float maxScale = GetMaxScaleForRarity(currentItem.rarity);
        int shakeCount = GetShakeCountForRarity(currentItem.rarity);
        Color glowColor = ItemRarityUtils.GetColor(currentItem.rarity);
        
        Vector3 originalScale = chestImage.transform.localScale;
        Vector3 originalPos = chestImage.transform.localPosition;
        
        // Shaking phase
        float shakeTimer = 0f;
        while (shakeTimer < shakeDuration)
        {
            shakeTimer += Time.unscaledDeltaTime; // Use unscaled time since game is paused
            float progress = shakeTimer / shakeDuration;
            
            // Scale up over time
            float currentScale = Mathf.Lerp(1f, maxScale, shakeCurve.Evaluate(progress));
            chestImage.transform.localScale = originalScale * currentScale;
            
            // Shake with decreasing intensity
            float shakeAmount = shakeIntensity * (1f - progress) * Mathf.Sin(progress * shakeCount * Mathf.PI * 2f);
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount),
                0
            ) * 100f; // Multiply for UI space
            
            chestImage.transform.localPosition = originalPos + shakeOffset;
            
            // Pulse color
            if (chestImage != null)
            {
                float pulse = Mathf.Sin(progress * shakeCount * Mathf.PI * 4f) * 0.5f + 0.5f;
                chestImage.color = Color.Lerp(Color.white, glowColor, pulse * progress);
            }
            
            yield return null;
        }
        
        // Pop effect
        chestImage.transform.localPosition = originalPos;
        float popTime = 0f;
        float popDuration = 0.3f;
        
        while (popTime < popDuration)
        {
            popTime += Time.unscaledDeltaTime;
            float t = popTime / popDuration;
            
            float scale = t < 0.5f 
                ? Mathf.Lerp(maxScale, maxScale * 1.3f, t * 2f)
                : Mathf.Lerp(maxScale * 1.3f, maxScale, (t - 0.5f) * 2f);
            
            chestImage.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        // Flash white
        chestImage.color = Color.white;
        
        yield return new WaitForSecondsRealtime(revealDelay);
        
        // Show item reveal
        RevealItemDetails();
        
        isAnimating = false;
        hasRevealed = true;
    }
    
    /// <summary>
    /// Display the item details after animation
    /// </summary>
    private void RevealItemDetails()
    {
        itemRevealPanel.SetActive(true);
        
        // Set item name with rarity color
        Color rarityColor = ItemRarityUtils.GetColor(currentItem.rarity);
        itemNameText.text = currentItem.displayName;
        itemNameText.color = rarityColor;
        
        // Set description
        itemDescriptionText.text = currentItem.description;
        
        // Build stats text
        itemStatsText.text = BuildStatsText(currentItem);
        
        // Load and display item icon
        if (itemIcon != null)
        {
            Sprite itemSprite = Resources.Load<Sprite>($"Sprites/{currentItem.spriteType}");
            if (itemSprite != null)
            {
                itemIcon.sprite = itemSprite;
                itemIcon.color = Color.white; // Reset color
                itemIcon.gameObject.SetActive(true);
                DebugLog.Verbose($"[TreasureUI] Loaded icon for {currentItem.displayName}");
            }
            else
            {
                // Fallback: Show colored square if sprite not found
                itemIcon.sprite = null;
                itemIcon.color = rarityColor;
                itemIcon.gameObject.SetActive(true);
                DebugLog.Warning($"[TreasureUI] Icon not found for {currentItem.spriteType}, using fallback");
            }
        }
        
        // Move prompt to bottom and make it visible
        if (promptText != null)
        {
            RectTransform promptRect = promptText.GetComponent<RectTransform>();
            if (promptRect != null)
            {
                promptRect.anchorMin = new Vector2(0.5f, 0.1f);
                promptRect.anchorMax = new Vector2(0.5f, 0.1f);
                promptRect.anchoredPosition = Vector2.zero;
            }
            promptText.text = "Press SPACE or Click to Continue";
            promptText.gameObject.SetActive(true);
        }
        
        DebugLog.Info($"[TreasureUI] Revealed: [{currentItem.rarity}] {currentItem.displayName}");
    }
    
    /// <summary>
    /// Build the stats text for the item
    /// </summary>
    private string BuildStatsText(ItemDefinition item)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        if (item.bonusMaxHealth > 0) sb.AppendLine($"+{item.bonusMaxHealth:F0} Max Health");
        if (item.bonusHealthRegen > 0) sb.AppendLine($"+{item.bonusHealthRegen:F1} HP/sec");
        if (item.bonusDamagePercent > 0) sb.AppendLine($"+{item.bonusDamagePercent * 100:F0}% Damage");
        if (item.bonusAttackSpeedPercent > 0) sb.AppendLine($"+{item.bonusAttackSpeedPercent * 100:F0}% Attack Speed");
        if (item.bonusMoveSpeedPercent > 0) sb.AppendLine($"+{item.bonusMoveSpeedPercent * 100:F0}% Move Speed");
        if (item.bonusCritChance > 0) sb.AppendLine($"+{item.bonusCritChance * 100:F0}% Crit Chance");
        if (item.bonusCritDamage > 0) sb.AppendLine($"+{item.bonusCritDamage * 100:F0}% Crit Damage");
        if (item.bonusPickupRadiusPercent > 0) sb.AppendLine($"+{item.bonusPickupRadiusPercent * 100:F0}% Pickup Radius");
        if (item.bonusPierce > 0) sb.AppendLine($"+{item.bonusPierce} Pierce");
        if (item.bonusProjectileCount > 0) sb.AppendLine($"+{item.bonusProjectileCount} Projectiles");
        if (item.bonusProjectileSizePercent > 0) sb.AppendLine($"+{item.bonusProjectileSizePercent * 100:F0}% Projectile Size");
        
        return sb.ToString().TrimEnd();
    }
    
    /// <summary>
    /// Close the UI and apply the item to the player
    /// </summary>
    private void CloseAndApplyItem()
    {
        if (currentItem != null)
        {
            // Add to player's inventory
            Player player = GameServices.Player;
            if (player != null)
            {
                PlayerInventory inventory = player.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    inventory.AddItem(currentItem);
                }
                else
                {
                    DebugLog.Warning("[TreasureUI] PlayerInventory not found, applying item directly");
                    currentItem.ApplyToPlayer(player);
                }
            }
        }
        
        HideUI();
        GameState.SetPaused(false);
    }
    
    /// <summary>
    /// Hide the treasure UI
    /// </summary>
    public void HideUI()
    {
        if (treasurePanel != null)
        {
            treasurePanel.SetActive(false);
        }
        
        currentItem = null;
        hasRevealed = false;
        isAnimating = false;
    }
    
    /// <summary>
    /// Get max scale based on rarity
    /// </summary>
    private float GetMaxScaleForRarity(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => 1.3f,
            ItemRarity.Rare => 1.8f,
            ItemRarity.Exotic => 2.3f,
            ItemRarity.Legendary => 2.8f,
            ItemRarity.Supreme => 3.5f,
            _ => 1.3f
        };
    }
    
    /// <summary>
    /// Get shake count based on rarity
    /// </summary>
    private int GetShakeCountForRarity(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => 3,
            ItemRarity.Rare => 5,
            ItemRarity.Exotic => 7,
            ItemRarity.Legendary => 10,
            ItemRarity.Supreme => 15,
            _ => 3
        };
    }
    
    /// <summary>
    /// Create the UI procedurally if not set up in editor
    /// </summary>
    private void CreateUI()
    {
        // Find or create canvas
        Canvas canvas = UIManager.GetOrCreateCanvas();
        if (canvas == null)
        {
            DebugLog.Error("[TreasureUI] Cannot create UI - no Canvas found!");
            return;
        }
        
        // Create treasure panel
        GameObject panel = new GameObject("TreasurePanel");
        panel.transform.SetParent(canvas.transform, false);
        treasurePanel = panel;
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.9f);
        
        // Create chest image
        GameObject chest = new GameObject("ChestImage");
        chest.transform.SetParent(panel.transform, false);
        
        RectTransform chestRect = chest.AddComponent<RectTransform>();
        chestRect.anchorMin = new Vector2(0.5f, 0.5f);
        chestRect.anchorMax = new Vector2(0.5f, 0.5f);
        chestRect.sizeDelta = new Vector2(300, 300);
        
        chestImage = chest.AddComponent<Image>();
        chestImage.color = new Color(0.6f, 0.4f, 0.2f); // Brown chest color
        
        // Create prompt text
        GameObject prompt = new GameObject("PromptText");
        prompt.transform.SetParent(panel.transform, false);
        
        RectTransform promptRect = prompt.AddComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0.3f);
        promptRect.anchorMax = new Vector2(0.5f, 0.3f);
        promptRect.sizeDelta = new Vector2(600, 60);
        
        promptText = prompt.AddComponent<TextMeshProUGUI>();
        promptText.text = "Press SPACE or Click to Open!";
        promptText.fontSize = 32;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = Color.white;
        
        // Create item reveal panel
        CreateItemRevealPanel(panel);
        
        DebugLog.Info("[TreasureUI] Created procedural UI");
    }
    
    /// <summary>
    /// Create the item reveal panel
    /// </summary>
    private void CreateItemRevealPanel(GameObject parent)
    {
        GameObject revealPanel = new GameObject("ItemRevealPanel");
        revealPanel.transform.SetParent(parent.transform, false);
        itemRevealPanel = revealPanel;
        
        RectTransform revealRect = revealPanel.AddComponent<RectTransform>();
        revealRect.anchorMin = new Vector2(0.5f, 0.5f);
        revealRect.anchorMax = new Vector2(0.5f, 0.5f);
        revealRect.sizeDelta = new Vector2(600, 450);
        
        Image revealBg = revealPanel.AddComponent<Image>();
        revealBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        // Item icon (centered at top)
        GameObject iconObj = new GameObject("ItemIcon");
        iconObj.transform.SetParent(revealPanel.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.85f);
        iconRect.anchorMax = new Vector2(0.5f, 0.85f);
        iconRect.sizeDelta = new Vector2(96, 96); // 3x scale for 32x32 icons
        itemIcon = iconObj.AddComponent<Image>();
        itemIcon.preserveAspect = true;
        
        // Item name
        GameObject nameObj = new GameObject("ItemName");
        nameObj.transform.SetParent(revealPanel.transform, false);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 0.70f);
        nameRect.anchorMax = new Vector2(0.5f, 0.70f);
        nameRect.sizeDelta = new Vector2(550, 50);
        itemNameText = nameObj.AddComponent<TextMeshProUGUI>();
        itemNameText.fontSize = 36;
        itemNameText.alignment = TextAlignmentOptions.Center;
        itemNameText.fontStyle = FontStyles.Bold;
        
        // Item description
        GameObject descObj = new GameObject("ItemDescription");
        descObj.transform.SetParent(revealPanel.transform, false);
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.5f, 0.52f);
        descRect.anchorMax = new Vector2(0.5f, 0.52f);
        descRect.sizeDelta = new Vector2(550, 80);
        itemDescriptionText = descObj.AddComponent<TextMeshProUGUI>();
        itemDescriptionText.fontSize = 20;
        itemDescriptionText.alignment = TextAlignmentOptions.Center;
        itemDescriptionText.color = new Color(0.8f, 0.8f, 0.8f);
        
        // Item stats
        GameObject statsObj = new GameObject("ItemStats");
        statsObj.transform.SetParent(revealPanel.transform, false);
        RectTransform statsRect = statsObj.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.5f, 0.25f);
        statsRect.anchorMax = new Vector2(0.5f, 0.25f);
        statsRect.sizeDelta = new Vector2(550, 200);
        itemStatsText = statsObj.AddComponent<TextMeshProUGUI>();
        itemStatsText.fontSize = 22;
        itemStatsText.alignment = TextAlignmentOptions.Center;
        itemStatsText.color = new Color(0.4f, 1f, 0.4f);
        
        revealPanel.SetActive(false);
    }
}

