using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages the level-up UI with upgrade choices (weapons, upgrades, stats)
/// </summary>
public class LevelUpUI : MonoBehaviour
{
    private static LevelUpUI instance;
    
    [Header("Prefab References")]
    [SerializeField] private GameObject upgradeOptionPrefab;
    
    private GameObject levelUpPanel;
    private Text levelText;
    private Button rerollButton;
    private Text rerollText;
    private List<GameObject> upgradeOptionObjects = new List<GameObject>();
    private List<UpgradeChoice> currentChoices = new List<UpgradeChoice>();
    
    private UpgradeChoiceGenerator choiceGenerator;
    private Player player;
    
    public bool IsShowingUI => levelUpPanel != null && levelUpPanel.activeSelf;
    
    private void Awake()
    {
        instance = this;
        player = FindFirstObjectByType<Player>();
        
        if (player != null)
        {
            choiceGenerator = player.GetComponent<UpgradeChoiceGenerator>();
            if (choiceGenerator == null)
            {
                choiceGenerator = player.gameObject.AddComponent<UpgradeChoiceGenerator>();
                DebugLog.Info("[LevelUpUI] Created UpgradeChoiceGenerator on Player");
            }
        }
        
        CreateUI();
        // Don't call HideUI() in Awake - it unpauses the game and interferes with character selection
        // Panel is already inactive by default when created
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
    }
    
    private void CreateUI()
    {
        // Use UIManager to get/create Canvas and EventSystem (prevents duplicates)
        Canvas canvas = UIManager.GetOrCreateCanvas();
        UIManager.GetOrCreateEventSystem();
        
        // Create level-up panel
        levelUpPanel = new GameObject("LevelUpPanel");
        levelUpPanel.transform.SetParent(canvas.transform, false);
        
        Image panelImage = levelUpPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f); // Semi-transparent black
        
        RectTransform panelRect = levelUpPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        // Create level-up text
        GameObject textObj = new GameObject("LevelText");
        textObj.transform.SetParent(levelUpPanel.transform, false);
        levelText = textObj.AddComponent<Text>();
        levelText.text = "LEVEL UP!";
        levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        levelText.fontSize = 48;
        levelText.alignment = TextAnchor.MiddleCenter;
        levelText.color = Color.yellow;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.8f);
        textRect.anchorMax = new Vector2(0.5f, 0.8f);
        textRect.sizeDelta = new Vector2(600, 100);
        textRect.anchoredPosition = Vector2.zero;
        
        // Create 3 upgrade choice options (from prefab or procedurally)
        upgradeOptionObjects.Clear();
        
        bool usingPrefab = (upgradeOptionPrefab != null);
        
        if (!usingPrefab)
        {
            DebugLog.Warning("[LevelUpUI] UpgradeOption prefab not assigned. Using procedural fallback.");
            DebugLog.Warning("[LevelUpUI] For better visuals, create a prefab and assign it in the Inspector.");
        }
        
        float[] buttonXPositions = { -450f, 0f, 450f };  // Wide spacing for large cards
        for (int i = 0; i < 3; i++)
        {
            GameObject optionGO;
            if (usingPrefab)
            {
                optionGO = Instantiate(upgradeOptionPrefab, levelUpPanel.transform);
            }
            else
            {
                optionGO = UIComponentFactory.CreateUpgradeOption(levelUpPanel.transform);
            }
            
            optionGO.name = $"UpgradeOption{i}";
            
            // Position the option
            RectTransform optionRect = optionGO.GetComponent<RectTransform>();
            if (optionRect != null)
            {
                optionRect.anchorMin = new Vector2(0.5f, 0.5f);
                optionRect.anchorMax = new Vector2(0.5f, 0.5f);
                optionRect.anchoredPosition = new Vector2(buttonXPositions[i], 0);
            }
            
            upgradeOptionObjects.Add(optionGO);
            DebugLog.Verbose($"[LevelUpUI] Created upgrade option {i}");
        }
        
        // Create reroll button
        GameObject rerollBtnObj = new GameObject("RerollButton");
        rerollBtnObj.transform.SetParent(levelUpPanel.transform, false);
        rerollButton = rerollBtnObj.AddComponent<Button>();
        
        Image rerollImage = rerollBtnObj.AddComponent<Image>();
        rerollImage.color = new Color(0.8f, 0.4f, 0.2f); // Orange
        
        RectTransform rerollRect = rerollBtnObj.GetComponent<RectTransform>();
        rerollRect.anchorMin = new Vector2(0.5f, 0.25f);
        rerollRect.anchorMax = new Vector2(0.5f, 0.25f);
        rerollRect.sizeDelta = new Vector2(200, 50);
        rerollRect.anchoredPosition = Vector2.zero;
        
        // Reroll button text
        GameObject rerollTextObj = new GameObject("Text");
        rerollTextObj.transform.SetParent(rerollBtnObj.transform, false);
        rerollText = rerollTextObj.AddComponent<Text>();
        rerollText.text = "Reroll (1 left)";
        rerollText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        rerollText.fontSize = 20;
        rerollText.alignment = TextAnchor.MiddleCenter;
        rerollText.color = Color.white;
        
        RectTransform rerollTextRect = rerollTextObj.GetComponent<RectTransform>();
        rerollTextRect.anchorMin = Vector2.zero;
        rerollTextRect.anchorMax = Vector2.one;
        rerollTextRect.sizeDelta = Vector2.zero;
        
        rerollButton.onClick.AddListener(OnRerollClicked);
        
        DebugLog.Info("LevelUpUI: UI created with 3 upgrade choices and reroll button");
    }
    
    public void ShowUI(int newLevel)
    {
        if (choiceGenerator == null)
        {
            DebugLog.Error("[LevelUpUI] UpgradeChoiceGenerator not found!");
            return;
        }

        // Reset rerolls for new level-up
        choiceGenerator.ResetRerolls();

        levelUpPanel.SetActive(true);
        levelText.text = $"LEVEL {newLevel}!";

        // Generate choices using new system
        currentChoices = choiceGenerator.GenerateChoices();

        DisplayChoices();
    }
    
    /// <summary>
    /// Show UI for treasure chest (displays "TREASURE!" instead of level)
    /// </summary>
    public void ShowChestReward(int currentLevel)
    {
        if (choiceGenerator == null)
        {
            DebugLog.Error("[LevelUpUI] UpgradeChoiceGenerator not found!");
            return;
        }

        // Reset rerolls for new chest reward
        choiceGenerator.ResetRerolls();

        levelUpPanel.SetActive(true);
        levelText.text = "TREASURE!";
        levelText.color = new Color(1f, 0.84f, 0f); // Gold color

        // Generate choices using new system
        currentChoices = choiceGenerator.GenerateChoices();

        DisplayChoices();
    }
    
    private void DisplayChoices()
    {
        if (currentChoices.Count == 0)
        {
            DebugLog.Warning("[LevelUpUI] No upgrade choices available!");
            HideUI();
            return;
        }
        
        // Update upgrade options with choices
        for (int i = 0; i < upgradeOptionObjects.Count; i++)
        {
            if (i < currentChoices.Count)
            {
                UpgradeChoice choice = currentChoices[i];
                GameObject optionGO = upgradeOptionObjects[i];
                
                // Get UpgradeOption component and initialize it
                UpgradeOption option = optionGO.GetComponent<UpgradeOption>();
                if (option != null)
                {
                    int choiceIndex = i;
                    option.Initialize(choice, (selectedChoice) => OnUpgradeChosen(choiceIndex));
                    optionGO.SetActive(true);
                }
                else
                {
                    DebugLog.Error($"[LevelUpUI] UpgradeOption component not found on upgrade option {i}!");
                    optionGO.SetActive(false);
                }
            }
            else
            {
                upgradeOptionObjects[i].SetActive(false);
            }
        }
        
        // Update reroll button
        UpdateRerollButton();
        
        // Only pause if we're in gameplay phase
        if (GamePhaseManager.CurrentPhase == GamePhase.Gameplay)
        {
            GameState.SetPaused(true);
        }
        
        DebugLog.Info($"[LevelUpUI] Showing {currentChoices.Count} upgrade choices");
    }
    
    private void OnUpgradeChosen(int index)
    {
        if (index >= currentChoices.Count)
        {
            DebugLog.Error($"[LevelUpUI] Invalid choice index: {index}");
            return;
        }
        
        UpgradeChoice choice = currentChoices[index];
        
        // Apply the upgrade via ChoiceGenerator
        choiceGenerator.ApplyChoice(choice);
        
        DebugLog.Info($"[LevelUpUI] Player selected: {choice.DisplayName} ({choice.Type})");
        
        HideUI();
    }
    
    private void OnRerollClicked()
    {
        if (!choiceGenerator.CanReroll())
        {
            DebugLog.Warning("[LevelUpUI] No rerolls remaining!");
            return;
        }

        choiceGenerator.UseReroll();
        DebugLog.Info($"[LevelUpUI] Rerolling upgrade choices... ({choiceGenerator.GetRemainingRerolls()} left)");

        // Regenerate choices WITHOUT resetting rerolls (don't call ShowUI)
        currentChoices = choiceGenerator.GenerateChoices();
        DisplayChoices();
    }
    
    private void UpdateRerollButton()
    {
        if (rerollButton == null) return;
        
        bool canReroll = choiceGenerator.CanReroll();
        rerollButton.interactable = canReroll;
        
        if (rerollText != null)
        {
            int remaining = choiceGenerator.GetRemainingRerolls();
            rerollText.text = canReroll ? $"Reroll ({remaining} left)" : "No Rerolls";
            rerollText.color = canReroll ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        }
    }
    
    public void HideUI()
    {
        if (levelUpPanel != null)
        {
            // Only unpause if panel was actually visible (not initial hide on startup)
            bool wasVisible = levelUpPanel.activeSelf;
            levelUpPanel.SetActive(false);
            
            // Only unpause if we were visible AND in gameplay phase
            if (wasVisible && GamePhaseManager.CurrentPhase == GamePhase.Gameplay)
            {
                GameState.SetPaused(false);
            }
        }
    }
}
