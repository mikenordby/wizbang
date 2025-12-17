using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Canvas-based HUD displaying player stats, health, XP, and timer
/// </summary>
public class CanvasHUD : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Player player;
    [SerializeField] private Health playerHealth;
    
    [Header("Health Bar")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Header("XP Bar")]
    [SerializeField] private Image xpFillImage;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI levelText;
    
    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    
    [Header("Stats Display")]
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private bool showStats = true;
    
    [Header("Colors")]
    [SerializeField] private Color healthHighColor = new Color(0.2f, 1f, 0.2f);
    [SerializeField] private Color healthMidColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color healthLowColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color xpColor = new Color(0.2f, 0.8f, 1f);
    
    private float gameTime;
    private EnemyPool enemyPool;
    
    private void Start()
    {
        // Auto-find references if not set
        if (player == null)
            player = FindFirstObjectByType<Player>();
        
        if (playerHealth == null && player != null)
            playerHealth = player.GetComponent<Health>();
        
        enemyPool = FindFirstObjectByType<EnemyPool>();
        
        // Subscribe to health events
        if (playerHealth != null)
            playerHealth.OnHealthChanged += UpdateHealthBar;
    }
    
    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthBar;
    }
    
    private void Update()
    {
        if (GamePhaseManager.CurrentPhase != GamePhase.Gameplay) return;
        if (player == null || GameState.IsPaused) return;
        
        gameTime += Time.deltaTime;
        
        UpdateHealthBar(playerHealth?.CurrentHealth ?? 0);
        UpdateXPBar();
        UpdateTimer();
        
        if (showStats)
            UpdateStats();
    }
    
    /// <summary>
    /// Update health bar fill and text
    /// </summary>
    private void UpdateHealthBar(float currentHealth)
    {
        if (playerHealth == null) return;
        
        float healthPercent = currentHealth / playerHealth.MaxHealth;
        
        // Update fill
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = healthPercent;
            
            // Color gradient based on health
            if (healthPercent > 0.5f)
                healthFillImage.color = Color.Lerp(healthMidColor, healthHighColor, (healthPercent - 0.5f) * 2f);
            else
                healthFillImage.color = Color.Lerp(healthLowColor, healthMidColor, healthPercent * 2f);
        }
        
        // Update text
        if (healthText != null)
            healthText.text = $"{currentHealth:F0} / {playerHealth.MaxHealth:F0}";
    }
    
    /// <summary>
    /// Update XP bar fill and text
    /// </summary>
    private void UpdateXPBar()
    {
        if (player == null) return;
        
        float xpPercent = player.XPProgress;
        
        // Update fill
        if (xpFillImage != null)
        {
            xpFillImage.fillAmount = xpPercent;
            xpFillImage.color = xpColor;
        }
        
        // Update text
        if (xpText != null)
            xpText.text = $"{player.CurrentXP} / {player.XPToNextLevel}";
        
        if (levelText != null)
            levelText.text = $"LVL {player.CurrentLevel}";
    }
    
    /// <summary>
    /// Update game timer
    /// </summary>
    private void UpdateTimer()
    {
        if (timerText == null) return;
        
        int minutes = (int)(gameTime / 60f);
        int seconds = (int)(gameTime % 60f);
        timerText.text = $"{minutes:D2}:{seconds:D2}";
    }
    
    /// <summary>
    /// Update debug stats display
    /// </summary>
    private void UpdateStats()
    {
        if (statsText == null || player == null) return;
        
        int enemyCount = enemyPool?.GetActiveCount() ?? 0;
        float fps = 1f / Time.deltaTime;
        
        string stats = $"FPS: {fps:F0}\n";
        stats += $"Enemies: {enemyCount}\n";
        stats += $"Damage: {player.DamageMultiplier:F2}x\n";
        stats += $"Speed: {player.AttackSpeedMultiplier:F2}x\n";
        stats += $"Crit: {player.CritChance * 100:F0}%";
        
        statsText.text = stats;
    }
    
    /// <summary>
    /// Toggle stats display
    /// </summary>
    public void ToggleStats()
    {
        showStats = !showStats;
        if (statsText != null)
            statsText.gameObject.SetActive(showStats);
    }
    
    /// <summary>
    /// Get elapsed game time
    /// </summary>
    public float GetGameTime() => gameTime;
}
