using UnityEngine;

/// <summary>
/// XP orb that moves toward player and grants XP on collection
/// </summary>
public class XPOrb : MonoBehaviour
{
    private Transform playerTransform;
    private Player player;
    private SpriteRenderer spriteRenderer;
    private int xpValue;
    private bool isActive;
    private float baseMoveSpeed = 6f; // Base XP orb speed (slightly faster than default player speed)
    private float collectionRadius = 0.5f;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        if (GamePhaseManager.CurrentPhase != GamePhase.Gameplay) return;
        if (GameState.IsPaused) return;
        
        if (!isActive || playerTransform == null)
            return;
        
        // Get player stats if not cached
        if (player == null)
            player = playerTransform.GetComponent<Player>();
        
        Vector3 toPlayer = playerTransform.position - transform.position;
        float distance = toPlayer.magnitude;
        
        float pickupRange = player != null ? player.PickupRadius : collectionRadius;
        float magnetRange = player != null ? player.XPMagnetRange : 3f;
        
        // Check collection
        if (distance <= pickupRange)
        {
            Collect();
            return;
        }
        
        // Move toward player if within magnet radius
        if (distance <= magnetRange)
        {
            // Move slightly faster than player's current speed to ensure absorption
            float playerSpeed = player != null ? player.MoveSpeedMultiplier * 5f : 5f; // 5f is base player speed
            float effectiveSpeed = Mathf.Max(baseMoveSpeed, playerSpeed * 1.15f); // 15% faster than player
            transform.position += toPlayer.normalized * effectiveSpeed * Time.deltaTime;
        }
    }
    
    /// <summary>
    /// Activate orb at position with XP value
    /// </summary>
    public void Activate(Vector3 position, int xpAmount, Transform player)
    {
        transform.position = position;
        xpValue = xpAmount;
        playerTransform = player;
        isActive = true;
        gameObject.SetActive(true);

        // Load XP gem sprite if missing (from Resources or procedural fallback)
        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = SpriteLoader.LoadXPGemSprite();
        }

        UpdateVisuals();

        DebugLog.Verbose($"[XPOrb] Activated with {xpValue} XP, color={spriteRenderer.color}");
    }

    /// <summary>
    /// Merge additional XP into this orb (used when pool is full)
    /// </summary>
    public void MergeXP(int additionalXP)
    {
        xpValue += additionalXP;
        UpdateVisuals();
        DebugLog.Info($"[XPOrb] Merged +{additionalXP} XP, now worth {xpValue} XP (yellow orb)");
    }

    /// <summary>
    /// Update orb visuals based on XP value
    /// </summary>
    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            // Only change color/size for merged orbs (2+ XP)
            if (xpValue > 1)
            {
                spriteRenderer.color = Color.yellow; // Highlight merged orbs
                transform.localScale = Vector3.one * 0.5f; // Slightly larger for merged orbs
            }
            else
            {
                // Normal orbs: keep original appearance
                spriteRenderer.color = Color.white; // Use sprite's own colors
                transform.localScale = Vector3.one * 0.4f; // Normal size
            }
        }
    }
    
    /// <summary>
    /// Deactivate and return to pool
    /// </summary>
    public void Deactivate()
    {
        isActive = false;
        xpValue = 1; // Reset to default 1 XP
        gameObject.SetActive(false);
    }
    
    private void Collect()
    {
        if (playerTransform != null)
        {
            // Find player component to grant XP
            Player player = playerTransform.GetComponent<Player>();
            if (player != null)
            {
                player.AddXP(xpValue);
                DebugLog.Info($"XPOrb.Collect: Player collected {xpValue} XP at {transform.position}");
            }
        }
        
        Deactivate();
    }
    
    public bool IsActive()
    {
        return isActive;
    }
}
