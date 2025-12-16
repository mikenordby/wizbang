using UnityEngine;

/// <summary>
/// Bootstrap script that initializes core systems before anything else runs.
/// Attach this to a GameObject in your first scene with early execution order.
/// Set Script Execution Order to -1000 or earlier in Project Settings.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enablePersistentLogging = true;
    
    private void Awake()
    {
        // Initialize persistent logger first
        if (enablePersistentLogging)
        {
            PersistentLogger.Initialize();
            PersistentLogger.Separator("GAME BOOTSTRAP");
            PersistentLogger.Info("GameBootstrap.Awake() called", "Bootstrap");
            PersistentLogger.Info($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}", "Bootstrap");
        }
        
        // Initialize other core systems here
        InitializeGameEvents();
        
        PersistentLogger.Info("Bootstrap complete", "Bootstrap");
        PersistentLogger.Separator();
    }
    
    private void InitializeGameEvents()
    {
        // Test if GameEvents system is working
        bool eventsWorking = true;
        
        try
        {
            // Subscribe to a test event (use OnGamePaused which exists)
            System.Action<bool> testHandler = (paused) => { };
            GameEvents.OnGamePaused += testHandler;
            GameEvents.OnGamePaused -= testHandler;
            
            PersistentLogger.Info("GameEvents system verified - working correctly", "Bootstrap");
        }
        catch (System.Exception ex)
        {
            eventsWorking = false;
            PersistentLogger.Error($"GameEvents system FAILED: {ex.Message}", "Bootstrap");
        }
        
        if (!eventsWorking)
        {
            Debug.LogError("[Bootstrap] GameEvents system is not working properly!");
        }
    }
    
    private void OnApplicationQuit()
    {
        if (enablePersistentLogging)
        {
            PersistentLogger.Separator("APPLICATION QUIT");
            PersistentLogger.Info("Application shutting down", "Bootstrap");
            PersistentLogger.Close();
        }
    }
}
