using UnityEngine;

/// <summary>
/// A dropped item that can be collected by the player.
/// Spawned from treasure chests or enemy drops.
/// </summary>
public class ItemPickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer glowRenderer;
    
    [Header("Animation")]
    [SerializeField] private float bobAmplitude = 0.1f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float rotateSpeed = 0f; // Set > 0 to spin
    [SerializeField] private float pulseSpeed = 1.5f;
    
    [Header("Collection")]
    [SerializeField] private float pickupRadius = 0.8f;
    [SerializeField] private float magnetRadius = 2f;
    [SerializeField] private float magnetSpeed = 8f;
    
    private ItemDefinition item;
    private Transform playerTransform;
    private Vector3 startPosition;
    private float bobOffset;
    private bool isBeingCollected = false;
    
    public ItemDefinition Item => item;
    
    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Create glow effect if not assigned
        if (glowRenderer == null)
        {
            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(transform, false);
            glowObj.transform.localPosition = Vector3.zero;
            glowObj.transform.localScale = Vector3.one * 1.5f;
            glowRenderer = glowObj.AddComponent<SpriteRenderer>();
            glowRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        }
    }
    
    /// <summary>
    /// Initialize the pickup with an item definition.
    /// </summary>
    public void Initialize(ItemDefinition itemDef, Vector3 position)
    {
        item = itemDef;
        transform.position = position;
        startPosition = position;
        bobOffset = Random.Range(0f, Mathf.PI * 2f); // Random phase offset
        isBeingCollected = false;
        
        // Find player
        playerTransform = GameServices.Player?.transform;
        
        // Set up visuals
        SetupVisuals();
        
        gameObject.SetActive(true);
        DebugLog.Verbose($"[ItemPickup] Spawned: {item.displayName} at {position}");
    }
    
    private void SetupVisuals()
    {
        if (item == null) return;
        
        // Load item sprite
        Sprite itemSprite = SpriteLoader.LoadSprite(item.spriteType);
        if (itemSprite != null)
        {
            spriteRenderer.sprite = itemSprite;
            spriteRenderer.color = item.iconTint;
        }
        else
        {
            // Fallback: use a colored square
            spriteRenderer.sprite = SpriteGenerator.CreateItemSprite(item.rarity);
            spriteRenderer.color = Color.white;
        }
        
        // Set up glow based on rarity
        Color glowColor = ItemRarityUtils.GetGlowColor(item.rarity);
        glowRenderer.sprite = spriteRenderer.sprite;
        glowRenderer.color = glowColor;
        
        // Higher rarity = bigger glow
        float glowScale = item.rarity switch
        {
            ItemRarity.Common => 1.3f,
            ItemRarity.Rare => 1.5f,
            ItemRarity.Exotic => 1.7f,
            ItemRarity.Legendary => 2.0f,
            ItemRarity.Supreme => 2.5f,
            _ => 1.3f
        };
        glowRenderer.transform.localScale = Vector3.one * glowScale;
        
        // Set sorting order
        spriteRenderer.sortingOrder = 5;
        glowRenderer.sortingOrder = 4;
    }
    
    private void Update()
    {
        if (GamePhaseManager.CurrentPhase != GamePhase.Gameplay) return;
        if (GameState.IsPaused) return;
        
        if (item == null) return;
        
        // Bob animation
        float bob = Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobAmplitude;
        
        // Pulse glow
        if (glowRenderer != null)
        {
            float pulse = Mathf.Lerp(0.3f, 0.6f, (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
            Color glowColor = ItemRarityUtils.GetGlowColor(item.rarity);
            glowColor.a = pulse;
            glowRenderer.color = glowColor;
        }
        
        // Rotate if enabled
        if (rotateSpeed > 0)
        {
            transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
        }
        
        // Check for player collection
        if (playerTransform != null)
        {
            Vector3 toPlayer = playerTransform.position - transform.position;
            float distance = toPlayer.magnitude;
            
            // Direct pickup
            if (distance <= pickupRadius)
            {
                CollectItem();
                return;
            }
            
            // Magnet effect
            if (distance <= magnetRadius || isBeingCollected)
            {
                isBeingCollected = true;
                Vector3 moveDir = toPlayer.normalized;
                transform.position += moveDir * magnetSpeed * Time.deltaTime;
                startPosition = transform.position; // Update bob center
            }
            else
            {
                // Apply bob only when not being collected
                transform.position = new Vector3(startPosition.x, startPosition.y + bob, startPosition.z);
            }
        }
    }
    
    private void CollectItem()
    {
        if (item == null) return;
        
        // Find player inventory
        PlayerInventory inventory = playerTransform?.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            // Add inventory component if missing
            inventory = playerTransform?.gameObject.AddComponent<PlayerInventory>();
        }
        
        if (inventory != null)
        {
            bool added = inventory.AddItem(item);
            if (added)
            {
                // Play collection effect (future: particle, sound)
                DebugLog.Info($"[ItemPickup] Player collected: {item.displayName}");
                
                // Show floating text
                DamageNumberPool damagePool = GameServices.DamageNumberPool;
                if (damagePool != null)
                {
                    // Use custom color for item pickups
                    Color rarityColor = ItemRarityUtils.GetColor(item.rarity);
                    // Note: DamageNumberPool may need extension for custom colored text
                }
            }
        }
        
        // Destroy pickup
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Set the player transform for pickup detection.
    /// </summary>
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }
}

