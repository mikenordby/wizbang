using UnityEngine;

/// <summary>
/// Game HUD displaying player stats, timer, and kill counter.
/// Uses OnGUI for immediate implementation (can upgrade to Canvas later).
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private EnemyPool enemyPool;
    
    [Header("Display Settings")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private int fontSize = 20;
    [SerializeField] private int largeFontSize = 32;
    
    private GUIStyle healthStyle;
    private GUIStyle xpStyle;
    private GUIStyle levelStyle;
    private GUIStyle timerStyle;
    private GUIStyle debugStyle;
    
    private float gameTime;
    private int totalKills;
    
    private void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();
        
        if (enemyPool == null)
            enemyPool = FindFirstObjectByType<EnemyPool>();
        
        InitializeStyles();
        
        // Subscribe to enemy death events
        if (enemyPool != null)
        {
            // We'll need to add a kill counter system
            // For now, calculate from pool stats
        }
    }
    
    private void Update()
    {
        if (!GameState.IsPaused)
            gameTime += Time.deltaTime;
    }
    
    private void InitializeStyles()
    {
        // Health bar style (red)
        healthStyle = new GUIStyle();
        healthStyle.normal.textColor = new Color(1f, 0.2f, 0.2f);
        healthStyle.fontSize = fontSize;
        healthStyle.fontStyle = FontStyle.Bold;
        healthStyle.alignment = TextAnchor.MiddleLeft;
        
        // XP bar style (cyan)
        xpStyle = new GUIStyle();
        xpStyle.normal.textColor = new Color(0.2f, 0.8f, 1f);
        xpStyle.fontSize = fontSize;
        xpStyle.fontStyle = FontStyle.Bold;
        xpStyle.alignment = TextAnchor.MiddleLeft;
        
        // Level style (gold)
        levelStyle = new GUIStyle();
        levelStyle.normal.textColor = new Color(1f, 0.84f, 0f);
        levelStyle.fontSize = largeFontSize;
        levelStyle.fontStyle = FontStyle.Bold;
        levelStyle.alignment = TextAnchor.MiddleRight;
        
        // Timer style (white)
        timerStyle = new GUIStyle();
        timerStyle.normal.textColor = Color.white;
        timerStyle.fontSize = largeFontSize;
        timerStyle.fontStyle = FontStyle.Bold;
        timerStyle.alignment = TextAnchor.MiddleCenter;
        
        // Debug style (green)
        debugStyle = new GUIStyle();
        debugStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
        debugStyle.fontSize = 16;
        debugStyle.alignment = TextAnchor.UpperRight;
    }
    
    private void OnGUI()
    {
        if (player == null) return;
        
        // Re-initialize styles if needed (lost on hot reload)
        if (healthStyle == null)
            InitializeStyles();
        
        DrawHealthBar();
        DrawXPBar();
        DrawLevel();
        DrawTimer();
        
        if (showDebugInfo)
            DrawDebugInfo();
    }
    
    private void DrawHealthBar()
    {
        Health health = player.GetComponent<Health>();
        if (health == null) return;
        
        float currentHealth = health.CurrentHealth;
        float maxHealth = health.MaxHealth;
        float healthPercent = currentHealth / maxHealth;
        
        // Health bar background (top left)
        Rect bgRect = new Rect(20, 20, 300, 30);
        DrawBar(bgRect, new Color(0.2f, 0.1f, 0.1f));
        
        // Health bar fill (red to orange gradient based on health)
        Rect fillRect = new Rect(20, 20, 300 * healthPercent, 30);
        Color healthColor = Color.Lerp(new Color(1f, 0.2f, 0.2f), new Color(1f, 0.6f, 0.2f), healthPercent);
        DrawBar(fillRect, healthColor);
        
        // Health text
        Rect textRect = new Rect(25, 20, 290, 30);
        GUI.Label(textRect, $"HP: {currentHealth:F0} / {maxHealth:F0}", healthStyle);
    }
    
    private void DrawXPBar()
    {
        int currentXP = player.CurrentXP;
        int xpToNextLevel = player.XPToNextLevel;
        float xpPercent = (float)currentXP / xpToNextLevel;
        
        // XP bar background (below health bar)
        Rect bgRect = new Rect(20, 60, 300, 25);
        DrawBar(bgRect, new Color(0.1f, 0.15f, 0.2f));
        
        // XP bar fill (cyan)
        Rect fillRect = new Rect(20, 60, 300 * xpPercent, 25);
        DrawBar(fillRect, new Color(0.2f, 0.8f, 1f));
        
        // XP text
        Rect textRect = new Rect(25, 60, 290, 25);
        GUI.Label(textRect, $"XP: {currentXP} / {xpToNextLevel}", xpStyle);
    }
    
    private void DrawLevel()
    {
        // Level display (top right)
        int level = player.CurrentLevel;
        Rect levelRect = new Rect(Screen.width - 220, 20, 200, 40);
        GUI.Label(levelRect, $"Level {level}", levelStyle);
    }
    
    private void DrawTimer()
    {
        // Timer display (top center)
        int minutes = (int)(gameTime / 60f);
        int seconds = (int)(gameTime % 60f);
        string timeText = $"{minutes:00}:{seconds:00}";
        
        Rect timerRect = new Rect(Screen.width / 2 - 100, 20, 200, 40);
        GUI.Label(timerRect, timeText, timerStyle);
    }
    
    private void DrawDebugInfo()
    {
        // Debug info (top right, below level)
        int activeEnemies = enemyPool != null ? enemyPool.GetActiveCount() : 0;
        float fps = 1f / Time.deltaTime;
        
        string debugText = $"FPS: {fps:F0}\n";
        debugText += $"Enemies: {activeEnemies}\n";
        debugText += $"Damage: {player.DamageMultiplier:F2}x\n";
        debugText += $"Crit: {player.CritChance * 100:F0}%\n";
        debugText += $"Speed: {player.MoveSpeedMultiplier:F2}x";
        
        Rect debugRect = new Rect(Screen.width - 220, 70, 200, 150);
        GUI.Label(debugRect, debugText, debugStyle);
    }
    
    /// <summary>
    /// Helper to draw filled rectangles (progress bars)
    /// </summary>
    private void DrawBar(Rect rect, Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        GUI.DrawTexture(rect, texture);
        Object.Destroy(texture); // Clean up temporary texture
    }
    
    /// <summary>
    /// Get total game time elapsed
    /// </summary>
    public float GetGameTime() => gameTime;
    
    /// <summary>
    /// Increment kill counter
    /// </summary>
    public void RegisterKill()
    {
        totalKills++;
    }
}
