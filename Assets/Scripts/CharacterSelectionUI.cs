using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Enhanced character selection screen with portraits, hover effects, and character progression.
/// Features:
/// - Character portraits with dynamic loading
/// - Hover preview system
/// - Locked/unlocked character states
/// - Detailed stat displays
/// - Smooth UI animations
/// </summary>
public class CharacterSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<CharacterData> availableCharacters = new List<CharacterData>();
    
    [Header("Character Unlocking")]
    [SerializeField] private List<string> unlockedCharacterNames = new List<string> { "Wizard", "Knight" }; // Default unlocked
    
    private GameObject selectionPanel;
    private GameObject characterGrid;
    private GameObject previewPanel;
    private List<Button> characterButtons = new List<Button>();
    private CharacterData selectedCharacter;
    private CharacterData hoveredCharacter;
    
    // UI Elements
    private Text previewName;
    private Text previewDescription;
    private Text previewStats;
    private Image previewPortrait;
    private GameObject lockedOverlay;
    
    public CharacterData SelectedCharacter => selectedCharacter;
    public bool HasSelectedCharacter => selectedCharacter != null;
    
    private void Awake()
    {
        // Phase fallback for direct scene testing
        if (GamePhaseManager.CurrentPhase == GamePhase.MainMenu)
        {
            DebugLog.Info("[CharacterSelectionUI] Direct scene load detected, transitioning to CharacterSelection phase");
            GamePhaseManager.TransitionToCharacterSelection();
        }
        
        // Load character data assets if not assigned
        if (availableCharacters.Count == 0)
        {
            LoadCharacterAssets();
        }
        
        CreateEnhancedUI();
        ShowSelectionScreen();
    }
    
    private void LoadCharacterAssets()
    {
        // Try to load character data from Resources
        CharacterData[] characters = Resources.LoadAll<CharacterData>("Characters");
        if (characters.Length > 0)
        {
            availableCharacters.AddRange(characters);
            DebugLog.Info($"[CharacterSelectionUI] Loaded {characters.Length} characters from Resources/Characters");
        }
        else
        {
            DebugLog.Warning("[CharacterSelectionUI] No characters found in Resources/Characters.");
        }
    }
    
    private bool IsCharacterUnlocked(CharacterData character)
    {
        return unlockedCharacterNames.Contains(character.characterName);
    }
    
    private void CreateEnhancedUI()
    {
        // Create EventSystem if needed
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
        
        // Create canvas
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("CharacterSelectionCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvas.sortingOrder = 1000;
        }
        
        // Main selection panel
        selectionPanel = new GameObject("CharacterSelectionPanel");
        selectionPanel.transform.SetParent(canvas.transform, false);
        
        Image panelBg = selectionPanel.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.08f, 1.0f); // Very dark blue-black, fully opaque
        
        RectTransform panelRect = selectionPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        // Title with glow effect
        CreateTitle();
        
        // Character selection grid
        CreateCharacterGrid();
        
        // Character preview panel (right side)
        CreatePreviewPanel();
        
        // Create character buttons with portraits
        CreateCharacterButtonsWithPortraits();
    }
    
    private void CreateTitle()
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(selectionPanel.transform, false);
        
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "CHOOSE YOUR HERO";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 72;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(1f, 0.9f, 0.3f); // Golden yellow
        titleText.fontStyle = FontStyle.Bold;
        
        // Add outline for glow effect
        Outline outline = titleObj.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.5f, 0f, 0.8f); // Orange glow
        outline.effectDistance = new Vector2(3, -3);
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.sizeDelta = new Vector2(1000, 100);
        titleRect.anchoredPosition = Vector2.zero;
    }
    
    private void CreateCharacterGrid()
    {
        characterGrid = new GameObject("CharacterGrid");
        characterGrid.transform.SetParent(selectionPanel.transform, false);
        
        RectTransform gridRect = characterGrid.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0.05f, 0.15f);
        gridRect.anchorMax = new Vector2(0.65f, 0.80f);
        gridRect.sizeDelta = Vector2.zero;
    }
    
    private void CreatePreviewPanel()
    {
        // Preview panel container
        GameObject previewContainer = new GameObject("PreviewPanel");
        previewContainer.transform.SetParent(selectionPanel.transform, false);
        
        Image previewBg = previewContainer.AddComponent<Image>();
        previewBg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f); // Slightly lighter background
        
        RectTransform previewRect = previewContainer.GetComponent<RectTransform>();
        previewRect.anchorMin = new Vector2(0.68f, 0.15f);
        previewRect.anchorMax = new Vector2(0.95f, 0.80f);
        previewRect.sizeDelta = Vector2.zero;
        
        previewPanel = previewContainer;
        
        // Preview title
        GameObject previewTitleObj = new GameObject("PreviewTitle");
        previewTitleObj.transform.SetParent(previewPanel.transform, false);
        Text previewTitleText = previewTitleObj.AddComponent<Text>();
        previewTitleText.text = "HERO PREVIEW";
        previewTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        previewTitleText.fontSize = 28;
        previewTitleText.alignment = TextAnchor.MiddleCenter;
        previewTitleText.color = new Color(0.8f, 0.8f, 1f);
        previewTitleText.fontStyle = FontStyle.Bold;
        
        RectTransform previewTitleRect = previewTitleObj.GetComponent<RectTransform>();
        previewTitleRect.anchorMin = new Vector2(0f, 0.9f);
        previewTitleRect.anchorMax = new Vector2(1f, 0.98f);
        previewTitleRect.sizeDelta = Vector2.zero;
        
        // Character portrait area
        GameObject portraitObj = new GameObject("Portrait");
        portraitObj.transform.SetParent(previewPanel.transform, false);
        previewPortrait = portraitObj.AddComponent<Image>();
        previewPortrait.color = new Color(0.3f, 0.3f, 0.3f); // Placeholder gray
        
        RectTransform portraitRect = portraitObj.GetComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0.15f, 0.55f);
        portraitRect.anchorMax = new Vector2(0.85f, 0.88f);
        portraitRect.sizeDelta = Vector2.zero;
        
        // Character name in preview
        GameObject nameObj = new GameObject("CharacterName");
        nameObj.transform.SetParent(previewPanel.transform, false);
        previewName = nameObj.AddComponent<Text>();
        previewName.text = "Select a Hero";
        previewName.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        previewName.fontSize = 32;
        previewName.alignment = TextAnchor.MiddleCenter;
        previewName.color = Color.white;
        previewName.fontStyle = FontStyle.Bold;
        
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.48f);
        nameRect.anchorMax = new Vector2(1f, 0.54f);
        nameRect.sizeDelta = Vector2.zero;
        
        // Description
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(previewPanel.transform, false);
        previewDescription = descObj.AddComponent<Text>();
        previewDescription.text = "Hover over a hero to see details";
        previewDescription.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        previewDescription.fontSize = 16;
        previewDescription.alignment = TextAnchor.UpperCenter;
        previewDescription.color = new Color(0.8f, 0.8f, 0.8f);
        
        RectTransform descRect = descObj.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.05f, 0.32f);
        descRect.anchorMax = new Vector2(0.95f, 0.46f);
        descRect.sizeDelta = Vector2.zero;
        
        // Stats display
        GameObject statsObj = new GameObject("Stats");
        statsObj.transform.SetParent(previewPanel.transform, false);
        previewStats = statsObj.AddComponent<Text>();
        previewStats.text = "";
        previewStats.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        previewStats.fontSize = 18;
        previewStats.alignment = TextAnchor.UpperLeft;
        previewStats.color = new Color(0.6f, 1f, 0.6f); // Bright green
        previewStats.fontStyle = FontStyle.Bold;
        
        RectTransform statsRect = statsObj.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.1f, 0.05f);
        statsRect.anchorMax = new Vector2(0.9f, 0.30f);
        statsRect.sizeDelta = Vector2.zero;
        
        // Locked overlay (hidden by default)
        GameObject lockedObj = new GameObject("LockedOverlay");
        lockedObj.transform.SetParent(previewPanel.transform, false);
        Image lockedBg = lockedObj.AddComponent<Image>();
        lockedBg.color = new Color(0f, 0f, 0f, 0.7f);
        
        RectTransform lockedRect = lockedObj.GetComponent<RectTransform>();
        lockedRect.anchorMin = Vector2.zero;
        lockedRect.anchorMax = Vector2.one;
        lockedRect.sizeDelta = Vector2.zero;
        
        GameObject lockedTextObj = new GameObject("LockedText");
        lockedTextObj.transform.SetParent(lockedObj.transform, false);
        Text lockedText = lockedTextObj.AddComponent<Text>();
        lockedText.text = "🔒 LOCKED\n\nComplete challenges\nto unlock this hero!";
        lockedText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lockedText.fontSize = 24;
        lockedText.alignment = TextAnchor.MiddleCenter;
        lockedText.color = new Color(1f, 0.3f, 0.3f);
        lockedText.fontStyle = FontStyle.Bold;
        
        RectTransform lockedTextRect = lockedTextObj.GetComponent<RectTransform>();
        lockedTextRect.anchorMin = Vector2.zero;
        lockedTextRect.anchorMax = Vector2.one;
        lockedTextRect.sizeDelta = Vector2.zero;
        
        lockedOverlay = lockedObj;
        lockedOverlay.SetActive(false);
    }
    
    private void CreateCharacterButtonsWithPortraits()
    {
        if (availableCharacters == null || availableCharacters.Count == 0)
        {
            Debug.LogWarning("No character data loaded for selection screen!");
            return;
        }
        
        int characterCount = availableCharacters.Count;
        int columns = Mathf.CeilToInt(Mathf.Sqrt(characterCount));
        int rows = Mathf.CeilToInt((float)characterCount / columns);
        
        float cardWidth = 250f;
        float cardHeight = 320f;
        float spacing = 30f;
        
        for (int i = 0; i < characterCount; i++)
        {
            CharacterData character = availableCharacters[i];
            bool isUnlocked = IsCharacterUnlocked(character);
            
            int col = i % columns;
            int row = i / columns;
            
            // Character card container
            GameObject cardObj = new GameObject($"Card_{character.characterName}");
            cardObj.transform.SetParent(characterGrid.transform, false);
            
            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.color = isUnlocked ? new Color(0.15f, 0.15f, 0.2f, 0.9f) : new Color(0.1f, 0.1f, 0.1f, 0.7f);
            
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            cardRect.anchorMin = Vector2.zero;
            cardRect.anchorMax = Vector2.zero;
            cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);
            
            float xPos = col * (cardWidth + spacing) + cardWidth / 2;
            float yPos = (rows - 1 - row) * (cardHeight + spacing) + cardHeight / 2;
            cardRect.anchoredPosition = new Vector2(xPos, yPos);
            
            // Portrait image (top section)
            GameObject portraitObj = new GameObject("Portrait");
            portraitObj.transform.SetParent(cardObj.transform, false);
            Image portraitImg = portraitObj.AddComponent<Image>();
            portraitImg.color = new Color(0.3f, 0.3f, 0.3f); // Placeholder gray
            
            RectTransform portraitRect = portraitObj.GetComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.1f, 0.4f);
            portraitRect.anchorMax = new Vector2(0.9f, 0.9f);
            portraitRect.sizeDelta = Vector2.zero;
            
            // Try to load character sprite as portrait
            LoadPortraitForCharacter(character, portraitImg);
            
            // Character name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(cardObj.transform, false);
            Text nameText = nameObj.AddComponent<Text>();
            nameText.text = character.characterName;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 24;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            nameText.fontStyle = FontStyle.Bold;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.25f);
            nameRect.anchorMax = new Vector2(1f, 0.38f);
            nameRect.sizeDelta = Vector2.zero;
            
            // Starting weapon indicator
            GameObject weaponObj = new GameObject("Weapon");
            weaponObj.transform.SetParent(cardObj.transform, false);
            Text weaponText = weaponObj.AddComponent<Text>();
            weaponText.text = GetWeaponDisplayName(character.startingWeaponType);
            weaponText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            weaponText.fontSize = 14;
            weaponText.alignment = TextAnchor.MiddleCenter;
            weaponText.color = new Color(0.7f, 0.9f, 1f);
            
            RectTransform weaponRect = weaponObj.GetComponent<RectTransform>();
            weaponRect.anchorMin = new Vector2(0f, 0.12f);
            weaponRect.anchorMax = new Vector2(1f, 0.24f);
            weaponRect.sizeDelta = Vector2.zero;
            
            // Button component for selection
            Button cardButton = cardObj.AddComponent<Button>();
            cardButton.interactable = isUnlocked;
            cardButton.targetGraphic = cardBg;
            
            ColorBlock colors = cardButton.colors;
            colors.normalColor = isUnlocked ? new Color(0.15f, 0.15f, 0.2f, 0.9f) : new Color(0.1f, 0.1f, 0.1f, 0.7f);
            colors.highlightedColor = isUnlocked ? new Color(0.25f, 0.25f, 0.35f, 1f) : colors.normalColor;
            colors.pressedColor = isUnlocked ? new Color(0.3f, 0.3f, 0.45f, 1f) : colors.normalColor;
            colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            cardButton.colors = colors;
            
            CharacterData capturedCharacter = character;
            cardButton.onClick.AddListener(() => SelectCharacter(capturedCharacter));
            
            // Hover events using EventTrigger
            EventTrigger trigger = cardObj.AddComponent<EventTrigger>();
            
            EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryEnter.callback.AddListener((data) => { OnCharacterHover(capturedCharacter); });
            trigger.triggers.Add(entryEnter);
            
            EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryExit.callback.AddListener((data) => { OnCharacterHoverExit(); });
            trigger.triggers.Add(entryExit);
            
            // Locked overlay on card
            if (!isUnlocked)
            {
                GameObject lockIconObj = new GameObject("LockIcon");
                lockIconObj.transform.SetParent(cardObj.transform, false);
                
                Image lockIcon = lockIconObj.AddComponent<Image>();
                lockIcon.color = new Color(1f, 0f, 0f, 0.6f);
                
                RectTransform lockRect = lockIconObj.GetComponent<RectTransform>();
                lockRect.anchorMin = new Vector2(0.4f, 0.6f);
                lockRect.anchorMax = new Vector2(0.6f, 0.75f);
                lockRect.sizeDelta = Vector2.zero;
                
                GameObject lockTextObj = new GameObject("LockText");
                lockTextObj.transform.SetParent(lockIconObj.transform, false);
                Text lockText = lockTextObj.AddComponent<Text>();
                lockText.text = "🔒";
                lockText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                lockText.fontSize = 48;
                lockText.alignment = TextAnchor.MiddleCenter;
                lockText.color = Color.white;
                
                RectTransform lockTextRect = lockTextObj.GetComponent<RectTransform>();
                lockTextRect.anchorMin = Vector2.zero;
                lockTextRect.anchorMax = Vector2.one;
                lockTextRect.sizeDelta = Vector2.zero;
            }
        }
    }
    
    private void LoadPortraitForCharacter(CharacterData character, Image portraitImage)
    {
        // Try to load character sprite as portrait
        Sprite characterSprite = SpriteLoader.LoadCharacterSprite(character.spriteType, "south");
        if (characterSprite != null)
        {
            portraitImage.sprite = characterSprite;
            portraitImage.color = Color.white;
        }
        else
        {
            Debug.LogWarning($"Could not load portrait sprite for {character.characterName}");
        }
    }
    
    private string GetWeaponDisplayName(string weaponType)
    {
        switch (weaponType)
        {
            case "ProjectileWeapon": return "⚡ Magic Missile";
            case "RapidFireWeapon": return "🔥 Rapid Fire";
            case "BoomerangWeapon": return "🪃 Boomerang";
            case "OrbiterWeapon": return "⭕ Orbiting Blades";
            case "FireRingWeapon": return "🔥 Circle of Fire";
            default: return weaponType;
        }
    }
    
    private void OnCharacterHover(CharacterData character)
    {
        hoveredCharacter = character;
        UpdatePreviewPanel(character);
    }
    
    private void OnCharacterHoverExit()
    {
        hoveredCharacter = null;
        ClearPreviewPanel();
    }
    
    private void UpdatePreviewPanel(CharacterData character)
    {
        if (previewPanel == null) return;
        
        bool isUnlocked = IsCharacterUnlocked(character);
        
        previewName.text = character.characterName;
        previewDescription.text = character.description;
        
        // Load portrait in preview panel
        Sprite characterSprite = SpriteLoader.LoadCharacterSprite(character.spriteType, "south");
        if (characterSprite != null)
        {
            previewPortrait.sprite = characterSprite;
            previewPortrait.color = Color.white;
        }
        
        // Display stats
        string statsText = $"<b>STATS</b>\n\n";
        statsText += $"⚡ Speed: {character.moveSpeedModifier:F2}x\n";
        statsText += $"❤️ Health: {character.baseMaxHealth:F0}\n";
        statsText += $"⚔️ Damage: {character.damageModifier:F2}x\n";
        statsText += $"\n<b>Starting Weapon:</b>\n";
        statsText += GetWeaponDisplayName(character.startingWeaponType);
        
        previewStats.text = statsText;
        
        // Show/hide locked overlay
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!isUnlocked);
        }
    }
    
    private void ClearPreviewPanel()
    {
        if (previewPanel == null) return;
        
        previewName.text = "Select a Hero";
        previewDescription.text = "Hover over a hero to see details";
        previewStats.text = "";
        previewPortrait.sprite = null;
        previewPortrait.color = new Color(0.3f, 0.3f, 0.3f);
        
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(false);
        }
    }

    
    private void SelectCharacter(CharacterData character)
    {
        OnCharacterSelected(character);
    }
    
    private string GetCharacterStatsText(CharacterData character)
    {
        return $"Health: {character.baseMaxHealth:F0}\n" +
               $"Move Speed: {character.moveSpeedModifier:F1}x\n" +
               $"Damage: {character.damageModifier:F1}x\n" +
               $"Starting Weapon:\n{character.startingWeaponType}";
    }
    
    private void OnCharacterSelected(CharacterData character)
    {
        selectedCharacter = character;
        DebugLog.Info($"[CharacterSelectionUI] Selected: {character.characterName}");
        
        // Hide selection screen
        HideSelectionScreen();
        
        // Initialize player with selected character
        InitializePlayerCharacter();
        
        // CRITICAL: Transition to gameplay AFTER player initialization
        GamePhaseManager.TransitionToGameplay();
    }
    
    private void InitializePlayerCharacter()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            DebugLog.Error("[CharacterSelectionUI] Player object not found!");
            return;
        }
        
        Player player = playerObj.GetComponent<Player>();
        if (player == null)
        {
            DebugLog.Error("[CharacterSelectionUI] Player component not found!");
            return;
        }
        
        // Apply character data to player
        player.InitializeWithCharacter(selectedCharacter);
        
        DebugLog.Info($"[CharacterSelectionUI] Initialized player as {selectedCharacter.characterName}");
    }
    
    public void ShowSelectionScreen()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
            GameState.SetPaused(true);
        }
    }
    
    public void HideSelectionScreen()
    {
        if (selectionPanel != null)
        {
            // Reset canvas sorting order if it was created by this system
            Canvas canvas = selectionPanel.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.gameObject.name == "CharacterSelectionCanvas")
            {
                canvas.sortingOrder = 0; // Reset to default
            }
            
            Destroy(selectionPanel); // Destroy instead of hide to prevent graphical issues
            selectionPanel = null;
        }
    }
}
