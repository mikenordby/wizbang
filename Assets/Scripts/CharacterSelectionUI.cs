using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Character selection screen UI.
/// Shows available characters and starts game with selected character.
/// </summary>
public class CharacterSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<CharacterData> availableCharacters = new List<CharacterData>();
    
    private GameObject selectionPanel;
    private GameObject characterGrid;
    private List<Button> characterButtons = new List<Button>();
    private CharacterData selectedCharacter;
    
    public CharacterData SelectedCharacter => selectedCharacter;
    public bool HasSelectedCharacter => selectedCharacter != null;
    
    private void Awake()
    {
        // Load character data assets if not assigned
        if (availableCharacters.Count == 0)
        {
            LoadCharacterAssets();
        }
        
        CreateUI();
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
            DebugLog.Warning("[CharacterSelectionUI] No characters found in Resources/Characters. You need to create CharacterData assets.");
        }
    }
    
    private void CreateUI()
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
            canvas.sortingOrder = 1000; // Always on top
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            // If canvas already exists, ensure character selection is on top
            canvas.sortingOrder = 1000;
        }
        
        // Create selection panel
        selectionPanel = new GameObject("CharacterSelectionPanel");
        selectionPanel.transform.SetParent(canvas.transform, false);
        
        Image panelBg = selectionPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f); // Dark blue-gray
        
        RectTransform panelRect = selectionPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        // Title text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(selectionPanel.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "SELECT CHARACTER";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 60;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.yellow;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.85f);
        titleRect.anchorMax = new Vector2(0.5f, 0.85f);
        titleRect.sizeDelta = new Vector2(800, 100);
        titleRect.anchoredPosition = Vector2.zero;
        
        // Character grid container
        characterGrid = new GameObject("CharacterGrid");
        characterGrid.transform.SetParent(selectionPanel.transform, false);
        
        RectTransform gridRect = characterGrid.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0.5f, 0.5f);
        gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        gridRect.sizeDelta = new Vector2(800, 400);
        gridRect.anchoredPosition = Vector2.zero;
        
        // Create character selection buttons
        CreateCharacterButtons();
    }
    
    private void CreateCharacterButtons()
    {
        if (availableCharacters.Count == 0)
        {
            DebugLog.Error("[CharacterSelectionUI] No characters available to display!");
            return;
        }
        
        float buttonWidth = 250f;
        float buttonHeight = 350f;
        float spacing = 30f;
        
        // Calculate positions for horizontal layout
        float totalWidth = availableCharacters.Count * buttonWidth + (availableCharacters.Count - 1) * spacing;
        float startX = -totalWidth / 2f + buttonWidth / 2f;
        
        for (int i = 0; i < availableCharacters.Count; i++)
        {
            CharacterData character = availableCharacters[i];
            
            // Character button container
            GameObject buttonObj = new GameObject($"CharacterButton_{i}");
            buttonObj.transform.SetParent(characterGrid.transform, false);
            
            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.3f); // Dark gray-blue
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            buttonRect.anchoredPosition = new Vector2(startX + i * (buttonWidth + spacing), 0);
            
            // Character name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(buttonObj.transform, false);
            Text nameText = nameObj.AddComponent<Text>();
            nameText.text = character.characterName;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 24;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.color = Color.white;
            nameText.fontStyle = FontStyle.Bold;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.8f);
            nameRect.anchorMax = new Vector2(1f, 0.95f);
            nameRect.sizeDelta = Vector2.zero;
            
            // Character description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(buttonObj.transform, false);
            Text descText = descObj.AddComponent<Text>();
            descText.text = character.description;
            descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descText.fontSize = 14;
            descText.alignment = TextAnchor.UpperCenter;
            descText.color = new Color(0.8f, 0.8f, 0.8f);
            
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.1f, 0.3f);
            descRect.anchorMax = new Vector2(0.9f, 0.75f);
            descRect.sizeDelta = Vector2.zero;
            
            // Stats display
            GameObject statsObj = new GameObject("Stats");
            statsObj.transform.SetParent(buttonObj.transform, false);
            Text statsText = statsObj.AddComponent<Text>();
            statsText.text = GetCharacterStatsText(character);
            statsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statsText.fontSize = 12;
            statsText.alignment = TextAnchor.UpperLeft;
            statsText.color = new Color(0.7f, 0.9f, 0.7f); // Light green
            
            RectTransform statsRect = statsObj.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.1f, 0.05f);
            statsRect.anchorMax = new Vector2(0.9f, 0.28f);
            statsRect.sizeDelta = Vector2.zero;
            
            // Hook up button click
            CharacterData selectedChar = character; // Capture for lambda
            button.onClick.AddListener(() => OnCharacterSelected(selectedChar));
            
            characterButtons.Add(button);
        }
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
        
        // Resume game
        GameState.SetPaused(false);
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
