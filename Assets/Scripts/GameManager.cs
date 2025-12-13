using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles game restart functionality
/// </summary>
public class GameManager : MonoBehaviour
{
    private CollisionManager collisionManager;
    private CollisionDebugVisualizer debugVisualizer;
    
    private void Start()
    {
        collisionManager = GetComponent<CollisionManager>();
        
        // Add debug visualizer
        debugVisualizer = gameObject.AddComponent<CollisionDebugVisualizer>();
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