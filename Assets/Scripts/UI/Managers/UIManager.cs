using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Centralized UI manager for canvas and event system management.
/// Ensures only one Canvas and EventSystem exist in the scene.
/// Prevents multiple UI systems from creating duplicates.
/// </summary>
public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;
    
    [Header("UI System References")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    
    public Canvas MainCanvas => mainCanvas;
    public EventSystem EventSystem => eventSystem;
    
    private void Awake()
    {
        // Enforce singleton
        if (instance != null && instance != this)
        {
            DebugLog.Warning("[UIManager] Duplicate instance detected, destroying");
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        EnsureCanvas();
        EnsureEventSystem();
        
        DebugLog.Info("[UIManager] Initialized successfully");
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    
    /// <summary>
    /// Ensure a Canvas exists for UI rendering.
    /// Creates one if missing, or uses existing if found.
    /// </summary>
    private void EnsureCanvas()
    {
        // Check if already assigned
        if (mainCanvas != null)
        {
            DebugLog.Verbose("[UIManager] Canvas already assigned");
            return;
        }
        
        // Try to find existing canvas
        mainCanvas = GetComponentInChildren<Canvas>();
        
        if (mainCanvas == null)
        {
            // Create new canvas
            GameObject canvasObj = new GameObject("MainCanvas");
            canvasObj.transform.SetParent(transform);
            
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 0;
            
            canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.matchWidthOrHeight = 0.5f;
            
            graphicRaycaster = canvasObj.AddComponent<GraphicRaycaster>();
            
            DebugLog.Info("[UIManager] Created new Canvas");
        }
        else
        {
            canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
            graphicRaycaster = mainCanvas.GetComponent<GraphicRaycaster>();
            DebugLog.Info("[UIManager] Found existing Canvas");
        }
    }
    
    /// <summary>
    /// Ensure an EventSystem exists for UI input.
    /// Creates one if missing, or uses existing if found.
    /// </summary>
    private void EnsureEventSystem()
    {
        // Check if already assigned
        if (eventSystem != null)
        {
            DebugLog.Verbose("[UIManager] EventSystem already assigned");
            return;
        }
        
        // Try to find existing event system (anywhere in scene)
        eventSystem = FindAnyObjectByType<EventSystem>();
        
        if (eventSystem == null)
        {
            // Create new event system
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.transform.SetParent(transform);
            
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            
            DebugLog.Info("[UIManager] Created new EventSystem");
        }
        else
        {
            DebugLog.Info("[UIManager] Found existing EventSystem");
        }
    }
    
    /// <summary>
    /// Get or create the main canvas for UI rendering.
    /// Call this instead of creating canvases in individual UI scripts.
    /// </summary>
    public static Canvas GetOrCreateCanvas()
    {
        if (Instance == null)
        {
            // Create UIManager if it doesn't exist
            GameObject uiManagerObj = new GameObject("UIManager");
            instance = uiManagerObj.AddComponent<UIManager>();
            DebugLog.Warning("[UIManager] Auto-created instance (should exist in scene)");
        }
        
        return Instance.MainCanvas;
    }
    
    /// <summary>
    /// Get or create the event system for UI input.
    /// Call this instead of creating event systems in individual UI scripts.
    /// </summary>
    public static EventSystem GetOrCreateEventSystem()
    {
        if (Instance == null)
        {
            GameObject uiManagerObj = new GameObject("UIManager");
            instance = uiManagerObj.AddComponent<UIManager>();
            DebugLog.Warning("[UIManager] Auto-created instance (should exist in scene)");
        }
        
        return Instance.EventSystem;
    }
}

