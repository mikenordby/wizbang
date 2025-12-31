using UnityEngine;

// Centralized game state management for pause and game over
/// <summary>
/// Centralized game state manager for pause, game over, and other global states.
/// Best practice: Single source of truth for game state accessed via static property.
/// </summary>
public class GameState : MonoBehaviour
{
    private static GameState instance;
    
    private bool isPaused = false;
    private bool isGameOver = false;
    
    /// <summary>
    /// Is the game currently paused (e.g., level-up menu)?
    /// Check this in Update() to skip game logic during pause.
    /// </summary>
    public static bool IsPaused => instance != null && instance.isPaused;
    
    /// <summary>
    /// Is the game over?
    /// </summary>
    public static bool IsGameOver => instance != null && instance.isGameOver;
    
    private void Awake()
    {
        // Singleton pattern - only one GameState should exist
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scene loads
    }
    
    /// <summary>
    /// Set pause state. Call this from LevelUpUI or pause menus.
    /// Uses Time.timeScale = 0 for true game freeze (best practice).
    /// UI animations can use Time.unscaledDeltaTime to continue during pause.
    /// </summary>
    public static void SetPaused(bool paused)
    {
        if (instance != null)
        {
            instance.isPaused = paused;

            // Best practice: Use Time.timeScale for genuine game freeze
            // This stops physics, animations, and all Time.deltaTime-based updates
            // UI that needs to animate during pause should use Time.unscaledDeltaTime
            Time.timeScale = paused ? 0f : 1f;

            DebugLog.Info($"GameState: Game {(paused ? "PAUSED (timeScale=0)" : "RESUMED (timeScale=1)")}");

            // Fire event for systems that need to know about pause state changes
            GameEvents.TriggerGamePaused(paused);
        }
    }
    
    /// <summary>
    /// Set game over state. Call this when player dies.
    /// </summary>
    public static void SetGameOver(bool gameOver)
    {
        if (instance != null)
        {
            instance.isGameOver = gameOver;
            DebugLog.Info($"GameState: Game Over = {gameOver}");
        }
    }
    
    /// <summary>
    /// Reset all game state (useful for restart).
    /// </summary>
    public static void Reset()
    {
        if (instance != null)
        {
            instance.isPaused = false;
            instance.isGameOver = false;
            Time.timeScale = 1f; // Ensure game is running
            DebugLog.Info("GameState: Reset (timeScale=1)");
        }
    }
}
