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
        
        // Create main menu on game start
        GameObject menuObj = new GameObject("MainMenu");
        mainMenu = menuObj.AddComponent<MainMenuUI>();
        
        // Add debug visualizer to Main Camera for OnRenderObject to work
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.GetComponent<CollisionDebugVisualizer>() == null)
        {
            mainCam.gameObject.AddComponent<CollisionDebugVisualizer>();
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