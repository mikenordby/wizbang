using UnityEngine;

/// <summary>
/// Quick setup script to initialize game background.
/// Add this to any GameObject in your scene and it will create the background automatically.
/// </summary>
public class BackgroundSetup : MonoBehaviour
{
    [Header("Auto-Setup on Start")]
    [SerializeField] private bool autoSetupOnStart = true;
    
    [Header("Background Colors")]
    [SerializeField] private Color grassColor1 = new Color(0.4f, 0.7f, 0.3f);
    [SerializeField] private Color grassColor2 = new Color(0.35f, 0.6f, 0.25f);
    
    private void Start()
    {
        PersistentLogger.Separator("BACKGROUND SETUP START");
        PersistentLogger.Info($"Start() called on {gameObject.name}, autoSetupOnStart={autoSetupOnStart}", "BackgroundSetup");
        Debug.Log($"[BackgroundSetup] Start() called on {gameObject.name}, autoSetupOnStart={autoSetupOnStart}");
        DebugLog.Info($"[BackgroundSetup] Start() called on {gameObject.name}, autoSetupOnStart={autoSetupOnStart}");
        
        if (autoSetupOnStart)
        {
            SetupBackground();
        }
        else
        {
            PersistentLogger.Warning("autoSetupOnStart is FALSE, background will not be created", "BackgroundSetup");
            Debug.LogWarning("[BackgroundSetup] autoSetupOnStart is FALSE, background will not be created");
            DebugLog.Warning("[BackgroundSetup] autoSetupOnStart is FALSE, background will not be created");
        }
    }
    
    [ContextMenu("Setup Background Now")]
    public void SetupBackground()
    {
        PersistentLogger.Info("SetupBackground() called", "BackgroundSetup");
        Debug.Log("[BackgroundSetup] SetupBackground() called");
        DebugLog.Info("[BackgroundSetup] SetupBackground() called");
        
        // Check if BackgroundManager already exists
        BackgroundManager existing = FindAnyObjectByType<BackgroundManager>();
        if (existing != null)
        {
            DebugLog.Warning($"[BackgroundSetup] BackgroundManager already exists on {existing.gameObject.name}, skipping setup");
            return;
        }
        
        DebugLog.Info("[BackgroundSetup] Creating new BackgroundManager...");
        
        // Create BackgroundManager GameObject
        GameObject bgManagerObj = new GameObject("BackgroundManager");
        BackgroundManager bgManager = bgManagerObj.AddComponent<BackgroundManager>();
        
        DebugLog.Info($"[BackgroundSetup] Created BackgroundManager on GameObject: {bgManagerObj.name}");
        
        // Set camera reference
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            DebugLog.Info($"[BackgroundSetup] Found Camera.main: {mainCam.name}");
            
            // Use reflection to set the private mainCamera field
            var field = typeof(BackgroundManager).GetField("mainCamera", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(bgManager, mainCam);
                DebugLog.Info("[BackgroundSetup] Successfully set mainCamera field via reflection");
            }
            else
            {
                DebugLog.Error("[BackgroundSetup] Failed to find mainCamera field via reflection!");
            }
        }
        else
        {
            DebugLog.Error("[BackgroundSetup] Camera.main is NULL!");
        }
        
        DebugLog.Info("[BackgroundSetup] Background setup complete! BackgroundManager will initialize on its Start()");
    }
}
