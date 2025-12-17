using UnityEngine;

/// <summary>
/// Debug overlay showing current game phase and phase transition history.
/// Useful for debugging phase-related issues.
/// Toggle with F3 key.
/// </summary>
public class PhaseDebugOverlay : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F3;
    
    private bool isVisible = true;
    private GUIStyle labelStyle;
    private GUIStyle headerStyle;
    private System.Collections.Generic.List<string> phaseHistory = new System.Collections.Generic.List<string>();
    private const int MAX_HISTORY = 10;
    
    private void Awake()
    {
        isVisible = showOnStart;
        GamePhaseManager.OnPhaseChanged += OnPhaseChanged;
        
        // Log initial phase
        phaseHistory.Add($"[{System.DateTime.Now:HH:mm:ss}] Initial: {GamePhaseManager.CurrentPhase}");
    }
    
    private void OnDestroy()
    {
        GamePhaseManager.OnPhaseChanged -= OnPhaseChanged;
    }
    
    private void OnPhaseChanged(GamePhase newPhase)
    {
        string entry = $"[{System.DateTime.Now:HH:mm:ss}] → {newPhase}";
        phaseHistory.Add(entry);
        
        // Keep only recent history
        if (phaseHistory.Count > MAX_HISTORY)
        {
            phaseHistory.RemoveAt(0);
        }
    }
    
    private void Update()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.f3Key.wasPressedThisFrame)
        {
            isVisible = !isVisible;
        }
    }
    
    private void OnGUI()
    {
        if (!isVisible) return;
        
        // Initialize styles if needed
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 14;
            labelStyle.fontStyle = FontStyle.Normal;
            
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.normal.textColor = Color.yellow;
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
        }
        
        // Background panel
        Rect panelRect = new Rect(10, 10, 350, 200);
        GUI.Box(panelRect, "");
        
        // Draw overlay content
        GUILayout.BeginArea(new Rect(20, 20, 330, 180));
        
        // Header
        GUILayout.Label("GAME PHASE DEBUG", headerStyle);
        GUILayout.Space(5);
        
        // Current phase with color coding
        GUIStyle currentPhaseStyle = new GUIStyle(labelStyle);
        currentPhaseStyle.fontSize = 18;
        currentPhaseStyle.fontStyle = FontStyle.Bold;
        currentPhaseStyle.normal.textColor = GetPhaseColor(GamePhaseManager.CurrentPhase);
        
        GUILayout.Label($"Current: {GamePhaseManager.CurrentPhase}", currentPhaseStyle);
        GUILayout.Space(5);
        
        // Pause state
        string pauseState = GameState.IsPaused ? "PAUSED" : "RUNNING";
        Color pauseColor = GameState.IsPaused ? Color.red : Color.green;
        GUIStyle pauseStyle = new GUIStyle(labelStyle);
        pauseStyle.normal.textColor = pauseColor;
        GUILayout.Label($"Game State: {pauseState}", pauseStyle);
        
        GUILayout.Space(10);
        
        // Phase history
        GUILayout.Label("Phase History:", labelStyle);
        foreach (string entry in phaseHistory)
        {
            GUILayout.Label($"  {entry}", labelStyle);
        }
        
        GUILayout.Space(10);
        GUILayout.Label($"Press {toggleKey} to toggle", labelStyle);
        
        GUILayout.EndArea();
    }
    
    private Color GetPhaseColor(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.MainMenu:
                return new Color(0.5f, 0.5f, 1f); // Light blue
            case GamePhase.CharacterSelection:
                return new Color(1f, 0.84f, 0f); // Gold
            case GamePhase.Gameplay:
                return new Color(0f, 1f, 0f); // Green
            default:
                return Color.white;
        }
    }
}
