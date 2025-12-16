using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-space health bar that follows the player character
/// Shows a small red fill bar below the player
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0, -0.8f, 0);
    [SerializeField] private Vector2 barSize = new Vector2(1f, 0.12f);
    [SerializeField] private Color fillColor = new Color(0.8f, 0.1f, 0.1f); // Dark red
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f); // Dark gray
    [SerializeField] private Color outlineColor = Color.black;
    
    private GameObject canvasObject;
    private Canvas canvas;
    private Image backgroundImage;
    private Image fillImage;
    private Outline fillOutline;
    
    private Health playerHealth;
    private Transform playerTransform;
    
    private void Start()
    {
        // Find player
        Player player = FindFirstObjectByType<Player>();
        if (player == null)
        {
            DebugLog.Warning("[PlayerHealthBar] Player not found! Will retry in Update");
            return;
        }
        
        playerTransform = player.transform;
        playerHealth = player.GetComponent<Health>();
        
        if (playerHealth == null)
        {
            DebugLog.Error("[PlayerHealthBar] Player has no Health component!");
            return;
        }
        
        CreateHealthBar();
        
        // Subscribe to health changes
        playerHealth.OnHealthChanged += OnHealthChanged;
        
        // Initial update
        UpdateHealthBar();
        
        DebugLog.Info("[PlayerHealthBar] Created health bar below player");
    }
    
    private void CreateHealthBar()
    {
        // Create world-space canvas for health bar
        canvasObject = new GameObject("PlayerHealthBarCanvas");
        canvasObject.transform.SetParent(transform, false);
        
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Position canvas below player
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(barSize.x, barSize.y);
        canvasRect.localScale = Vector3.one * 0.01f; // Scale down for world space
        
        // Add CanvasScaler for proper sizing
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;
        
        // Create background bar
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(canvasObject.transform, false);
        
        backgroundImage = backgroundObj.AddComponent<Image>();
        backgroundImage.color = backgroundColor;
        
        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // Create fill bar
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(canvasObject.transform, false);
        
        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fillColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        
        // Add outline to fill bar
        fillOutline = fillObj.AddComponent<Outline>();
        fillOutline.effectColor = outlineColor;
        fillOutline.effectDistance = new Vector2(1, -1);
        
        // Set sorting order to render above background
        canvas.sortingOrder = 20; // Above player (sortingOrder 10)
    }
    
    private void OnHealthChanged(float newHealth)
    {
        UpdateHealthBar();
    }
    
    private void UpdateHealthBar()
    {
        if (playerHealth == null || fillImage == null) return;
        
        float fillAmount = playerHealth.CurrentHealth / playerHealth.MaxHealth;
        fillImage.fillAmount = Mathf.Clamp01(fillAmount);
        
        // Color interpolation from red -> yellow -> (none, stays red)
        if (fillAmount < 0.3f)
        {
            // Low health: darker red
            fillImage.color = new Color(0.6f, 0.1f, 0.1f);
        }
        else if (fillAmount < 0.5f)
        {
            // Medium-low health: red-orange
            fillImage.color = new Color(0.8f, 0.3f, 0.1f);
        }
        else
        {
            // Normal health: standard red
            fillImage.color = fillColor;
        }
    }
    
    private void LateUpdate()
    {
        if (playerTransform == null)
        {
            // Try to find player if not yet found
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                playerTransform = player.transform;
                playerHealth = player.GetComponent<Health>();
                
                if (playerHealth != null && canvasObject == null)
                {
                    CreateHealthBar();
                    playerHealth.OnHealthChanged += OnHealthChanged;
                    UpdateHealthBar();
                }
            }
            return;
        }
        
        if (canvasObject == null) return;
        
        // Follow player with offset
        canvasObject.transform.position = playerTransform.position + offset;
        
        // Always face camera (billboard effect)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            canvasObject.transform.rotation = mainCamera.transform.rotation;
        }
    }
    
    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
        }
    }
}
