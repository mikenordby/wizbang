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
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            DebugLog.Info($"Game started: {sceneName} (Unity {Application.unityVersion}, {Application.platform})", "Bootstrap");
        }

        // Initialize other core systems here
        InitializeGameEvents();
    }
    
    private void InitializeGameEvents()
    {
        // Test if GameEvents system is working (silent unless error)
        try
        {
            // Subscribe to a test event (use OnGamePaused which exists)
            System.Action<bool> testHandler = (paused) => { };
            GameEvents.OnGamePaused += testHandler;
            GameEvents.OnGamePaused -= testHandler;
            // Success - no logging needed
        }
        catch (System.Exception ex)
        {
            DebugLog.Error($"GameEvents system initialization FAILED: {ex.Message}", "Bootstrap");
        }
    }
    
    private void OnApplicationQuit()
    {
        if (enablePersistentLogging)
        {
            DebugLog.Info("Game stopped", "Bootstrap");
            PersistentLogger.Close();
        }
    }
}
