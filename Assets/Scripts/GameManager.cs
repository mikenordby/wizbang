using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles game restart functionality and main menu
/// </summary>
public class GameManager : MonoBehaviour
{
    private CollisionManager collisionManager;
    private MainMenuUI mainMenu;
    
    private void Start()
    {
        collisionManager = GetComponent<CollisionManager>();
        
        // Subscribe to phase changes for cleanup/initialization
        GamePhaseManager.OnPhaseChanged += HandlePhaseChanged;
        
        // Create main menu on game start
        GameObject menuObj = new GameObject("MainMenu");
        mainMenu = menuObj.AddComponent<MainMenuUI>();
        
        // Debug visualizer removed during cleanup (can be re-added if needed for debugging)
    }
    
    private void OnDestroy()
    {
        GamePhaseManager.OnPhaseChanged -= HandlePhaseChanged;
    }
    
    private void HandlePhaseChanged(GamePhase newPhase)
    {
        DebugLog.Info($"[GameManager] Phase changed to {newPhase}");
        
        switch (newPhase)
        {
            case GamePhase.MainMenu:
                // Clean up gameplay state if restarting
                break;
                
            case GamePhase.CharacterSelection:
                // Prepare for character selection
                break;
                
            case GamePhase.Gameplay:
                // Game is starting - all systems should be ready
                DebugLog.Info("[GameManager] Gameplay phase started - all systems active");
                break;
        }
    }
    
    private void Update()
    {
        // Restart on R key
        if (collisionManager != null && collisionManager.IsGameOver)
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                RestartGame();
            }
        }
    }
    
    private void RestartGame()
    {
        Time.timeScale = 1f; // Unpause
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}