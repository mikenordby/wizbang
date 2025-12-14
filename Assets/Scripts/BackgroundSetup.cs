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
        if (autoSetupOnStart)
        {
            SetupBackground();
        }
    }
    
    [ContextMenu("Setup Background Now")]
    public void SetupBackground()
    {
        // Check if BackgroundManager already exists
        BackgroundManager existing = FindAnyObjectByType<BackgroundManager>();
        if (existing != null)
        {
            Debug.Log("[BackgroundSetup] BackgroundManager already exists, skipping setup");
            return;
        }
        
        // Create BackgroundManager GameObject
        GameObject bgManagerObj = new GameObject("BackgroundManager");
        BackgroundManager bgManager = bgManagerObj.AddComponent<BackgroundManager>();
        
        // Set camera reference
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Use reflection to set the private mainCamera field
            var field = typeof(BackgroundManager).GetField("mainCamera", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(bgManager, mainCam);
        }
        
        Debug.Log("[BackgroundSetup] Background created successfully! Background will generate on scene start.");
        Debug.Log("[BackgroundSetup] To use Cainos grass tileset, copy TX Tileset Grass.png to Assets/Resources/Backgrounds/grass_tileset.png");
    }
}
