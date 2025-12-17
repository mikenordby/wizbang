using UnityEngine;
using System;

/// <summary>
/// Game phase states
/// </summary>
public enum GamePhase
{
    MainMenu,           // Title screen, no game logic running
    CharacterSelection, // Character selection UI, no spawning/combat
    Gameplay            // Active game with player, enemies, weapons
}

/// <summary>
/// Manages discrete game phases to prevent initialization race conditions.
/// Singleton that controls transitions between MainMenu → CharacterSelection → Gameplay.
/// All game systems should check CurrentPhase before running their logic.
/// </summary>
public class GamePhaseManager : MonoBehaviour
{
    private static GamePhaseManager instance;
    private GamePhase currentPhase = GamePhase.MainMenu;
    
    /// <summary>
    /// Lazy singleton - creates instance if needed
    /// </summary>
    private static GamePhaseManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GamePhaseManager");
                instance = go.AddComponent<GamePhaseManager>();
                DontDestroyOnLoad(go);
                DebugLog.Info("[GamePhaseManager] Auto-created singleton instance");
            }
            return instance;
        }
    }
    
    /// <summary>
    /// Current game phase
    /// </summary>
    public static GamePhase CurrentPhase => Instance.currentPhase;
    
    /// <summary>
    /// Quick check if game is in active gameplay (not menu/selection)
    /// </summary>
    public static bool IsInGameplay => CurrentPhase == GamePhase.Gameplay;
    
    /// <summary>
    /// Event fired when game phase changes. Subscribe to react to phase transitions.
    /// </summary>
    public static event Action<GamePhase> OnPhaseChanged;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scene loads
        
        DebugLog.Info("[GamePhaseManager] Initialized");
    }
    
    /// <summary>
    /// Transition to Main Menu phase.
    /// Called on game start or restart.
    /// </summary>
    public static void TransitionToMainMenu()
    {
        Instance.currentPhase = GamePhase.MainMenu;
        Time.timeScale = 1.0f; // Keep time running for UI animations
        GameState.SetPaused(false); // Clear any pause state
        
        OnPhaseChanged?.Invoke(GamePhase.MainMenu);
        DebugLog.Info("[GamePhase] → MAIN_MENU");
    }
    
    /// <summary>
    /// Transition to Character Selection phase.
    /// Called when user clicks Play on main menu.
    /// </summary>
    public static void TransitionToCharacterSelection()
    {
        Instance.currentPhase = GamePhase.CharacterSelection;
        Time.timeScale = 1.0f; // Keep time running
        GameState.SetPaused(false); // Clear pause state
        
        OnPhaseChanged?.Invoke(GamePhase.CharacterSelection);
        DebugLog.Info("[GamePhase] → CHARACTER_SELECTION");
    }
    
    /// <summary>
    /// Transition to Gameplay phase.
    /// Called after player selects character and initialization completes.
    /// </summary>
    public static void TransitionToGameplay()
    {
        Instance.currentPhase = GamePhase.Gameplay;
        Time.timeScale = 1.0f; // Ensure time is running
        GameState.SetPaused(false); // Start unpaused
        
        OnPhaseChanged?.Invoke(GamePhase.Gameplay);
        DebugLog.Info("[GamePhase] → GAMEPLAY (game systems now active)");
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
