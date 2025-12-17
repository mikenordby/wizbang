using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages the level-up UI with upgrade choices (weapons, upgrades, stats)
/// </summary>
public class LevelUpUI : MonoBehaviour
{
    private static LevelUpUI instance;
    
    private GameObject levelUpPanel;
    private Text levelText;
    private Button rerollButton;
    private Text rerollText;
    private List<Button> upgradeButtons = new List<Button>();
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
        // Create EventSystem if it doesn't exist (required for UI input)
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            DebugLog.Info("LevelUpUI: Created EventSystem with InputSystemUIInputModule for UI input");
        }
        
        // Create canvas if it doesn't exist
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
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
        
        // Create 3 upgrade choice buttons
        float[] buttonXPositions = { -280f, 0f, 280f };
        for (int i = 0; i < 3; i++)
        {
            GameObject buttonObj = new GameObject($"UpgradeButton{i}");
            buttonObj.transform.SetParent(levelUpPanel.transform, false);
            Button button = buttonObj.AddComponent<Button>();
            upgradeButtons.Add(button);
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.8f); // Blue
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(240, 220);
            buttonRect.anchoredPosition = new Vector2(buttonXPositions[i], 0);
            
            // Button text (category + name + description)
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            Text buttonText = buttonTextObj.AddComponent<Text>();
            buttonText.text = "Upgrade Option";
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 16;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = new Vector2(-20, -20);
            
            // Hook up button click
            int buttonIndex = i;
            button.onClick.AddListener(() => OnUpgradeChosen(buttonIndex));
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
        
        // Update buttons with choices
        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            if (i < currentChoices.Count)
            {
                UpgradeChoice choice = currentChoices[i];
                
                // Color code by category
                Color categoryColor = choice.Type switch
                {
                    UpgradeChoice.ChoiceType.NewWeapon => new Color(1f, 0.3f, 0.3f), // Red
                    UpgradeChoice.ChoiceType.WeaponUpgrade => new Color(0.3f, 0.8f, 1f), // Cyan
                    UpgradeChoice.ChoiceType.PlayerStat => new Color(0.4f, 1f, 0.4f), // Green
                    _ => Color.white
                };
                
                upgradeButtons[i].GetComponent<Image>().color = categoryColor;
                
                // Format text: Category badge + name + description
                string categoryLabel = choice.Type switch
                {
                    UpgradeChoice.ChoiceType.NewWeapon => "[NEW WEAPON]",
                    UpgradeChoice.ChoiceType.WeaponUpgrade => "[UPGRADE]",
                    UpgradeChoice.ChoiceType.PlayerStat => "[STAT]",
                    _ => ""
                };
                
                upgradeButtons[i].GetComponentInChildren<Text>().text = 
                    $"{categoryLabel}\n\n{choice.DisplayName}\n\n{choice.Description}";
                
                upgradeButtons[i].gameObject.SetActive(true);
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false);
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
        DebugLog.Info("[LevelUpUI] Rerolling upgrade choices...");
        
        // Regenerate choices
        ShowUI(player.CurrentLevel);
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
