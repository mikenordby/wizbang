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
    [Header("Prefab References")]
    [SerializeField] private GameObject characterCardPrefab;
    
    [Header("Hero Data")]
    [SerializeField] private List<HeroDefinition> availableHeroes = new List<HeroDefinition>();
    
    [Header("Hero Unlocking")]
    [SerializeField] private List<string> unlockedHeroIds = new List<string> { "wizard", "knight" }; // Default unlocked
    
    private GameObject selectionPanel;
    private GameObject characterGrid;
    private GameObject previewPanel;
    private List<Button> characterButtons = new List<Button>();
    private HeroDefinition selectedHero;
    private HeroDefinition hoveredHero;
    
    // UI Elements
    private Text previewName;
    private Text previewDescription;
    private Text previewStats;
    private Image previewPortrait;
    private GameObject lockedOverlay;
    
    public HeroDefinition SelectedHero => selectedHero;
    public bool HasSelectedHero => selectedHero != null;
    
    private void Awake()
    {
        // Phase fallback for direct scene testing
        if (GamePhaseManager.CurrentPhase == GamePhase.MainMenu)
        {
            DebugLog.Info("[CharacterSelectionUI] Direct scene load detected, transitioning to CharacterSelection phase");
            GamePhaseManager.TransitionToCharacterSelection();
        }
        
        // Load hero definition assets if not assigned
        if (availableHeroes.Count == 0)
        {
            LoadHeroAssets();
        }
        
        CreateEnhancedUI();
        ShowSelectionScreen();
    }
    
    private void LoadHeroAssets()
    {
        // Try to load HeroDefinition assets from Resources/Characters
        HeroDefinition[] heroes = Resources.LoadAll<HeroDefinition>("Characters");
        if (heroes.Length > 0)
        {
            availableHeroes.AddRange(heroes);
            DebugLog.Info($"[CharacterSelectionUI] Loaded {heroes.Length} hero(es) from Resources/Characters");
            
            // Log what was loaded
            foreach (var hero in heroes)
            {
                DebugLog.Info($"[CharacterSelectionUI]   - {hero.displayName} (id: {hero.heroId})");
            }
        }
        else
        {
            DebugLog.Warning("[CharacterSelectionUI] No heroes found in Resources/Characters");
            DebugLog.Warning("[CharacterSelectionUI] Make sure HeroDefinition assets exist in Assets/Resources/Characters/");
        }
    }
    
    private bool IsHeroUnlocked(HeroDefinition hero)
    {
        return unlockedHeroIds.Contains(hero.heroId);
    }
    
    private void CreateEnhancedUI()
    {
        DebugLog.Verbose("[CharacterSelectionUI] CreateEnhancedUI started");
        
        // Use UIManager to get/create Canvas and EventSystem (prevents duplicates)
        Canvas canvas = UIManager.GetOrCreateCanvas();
        UIManager.GetOrCreateEventSystem();
        
        // Set canvas to high sorting order for character selection screen
        canvas.sortingOrder = 1000;
        
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
        if (availableHeroes == null || availableHeroes.Count == 0)
        {
            DebugLog.Warning("[CharacterSelectionUI] No hero definitions loaded for selection screen!");
            return;
        }
        
        bool usingPrefab = (characterCardPrefab != null);
        
        if (!usingPrefab)
        {
            DebugLog.Warning("[CharacterSelectionUI] CharacterCard prefab not assigned. Using procedural fallback.");
            DebugLog.Warning("[CharacterSelectionUI] For better visuals, create a prefab and assign it in the Inspector.");
        }
        
        int heroCount = availableHeroes.Count;
        // Use 3 columns for better scalability (supports up to 6+ heroes nicely)
        int columns = 3;
        int rows = Mathf.CeilToInt((float)heroCount / columns);
        
        float cardWidth = 250f;
        float cardHeight = 320f;
        float spacing = 30f;
        
        DebugLog.Info($"[CharacterSelectionUI] Creating {heroCount} character cards from prefabs (grid: {columns}x{rows})");
        
        for (int i = 0; i < heroCount; i++)
        {
            HeroDefinition hero = availableHeroes[i];
            bool isUnlocked = IsHeroUnlocked(hero);
            
            // Calculate grid position
            int col = i % columns;
            int row = i / columns;
            float xPos = col * (cardWidth + spacing) + cardWidth / 2;
            float yPos = (rows - 1 - row) * (cardHeight + spacing) + cardHeight / 2;
            
            // Create character card (from prefab or procedurally)
            GameObject cardGO;
            if (usingPrefab)
            {
                cardGO = Instantiate(characterCardPrefab, characterGrid.transform);
            }
            else
            {
                cardGO = UIComponentFactory.CreateCharacterCard(characterGrid.transform);
            }
            
            cardGO.name = $"Card_{hero.displayName}";
            
            // Position the card in the grid
            RectTransform cardRect = cardGO.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.anchorMin = Vector2.zero;
                cardRect.anchorMax = Vector2.zero;
                cardRect.anchoredPosition = new Vector2(xPos, yPos);
            }
            
            // Initialize the CharacterCard component with hero data
            CharacterCard card = cardGO.GetComponent<CharacterCard>();
            if (card != null)
            {
                card.Initialize(hero, isUnlocked, OnHeroHover, SelectHero);
                DebugLog.Verbose($"[CharacterSelectionUI] Initialized card for {hero.displayName} (unlocked={isUnlocked})");
            }
            else
            {
                DebugLog.Error($"[CharacterSelectionUI] CharacterCard component not found! Card for {hero.displayName} will not function.");
            }
        }
        
        DebugLog.Info($"[CharacterSelectionUI] Successfully created all character cards");
    }
    
    private void OnHeroHover(HeroDefinition hero)
    {
        hoveredHero = hero;
        UpdatePreviewPanel(hero);
    }
    
    /// <summary>
    /// Helper method to get display name for weapon types with icons.
    /// </summary>
    private string GetWeaponDisplayName(string weaponType)
    {
        switch (weaponType)
        {
            case "ProjectileWeapon": return "⚡ Magic Missile";
            case "RapidFireWeapon": return "🔥 Rapid Fire";
            case "BoomerangWeapon": return "🪃 Boomerang";
            case "OrbiterWeapon": return "⭕ Orbiting Blades";
            case "FireRingWeapon": return "🔥 Circle of Fire";
            case "LaserWeapon": return "⚡ Piercing Laser";
            case "LightningWeapon": return "⚡ Chain Lightning";
            case "PoisonWeapon": return "☠️ Poison Cloud";
            default: return weaponType;
        }
    }
    
    private void OnHeroHoverExit()
    {
        hoveredHero = null;
        ClearPreviewPanel();
    }
    
    private void UpdatePreviewPanel(HeroDefinition hero)
    {
        if (previewPanel == null) return;
        
        bool isUnlocked = IsHeroUnlocked(hero);
        
        previewName.text = hero.displayName;
        previewDescription.text = hero.description;
        
        // Load portrait in preview panel
        Sprite heroSprite = SpriteLoader.LoadCharacterSprite(hero.spriteType, "south");
        if (heroSprite != null)
        {
            previewPortrait.sprite = heroSprite;
            previewPortrait.color = Color.white;
        }
        
        // Display stats
        string statsText = $"<b>STATS</b>\n\n";
        statsText += $"⚡ Speed: {hero.baseMoveSpeed:F2}x\n";
        statsText += $"❤️ Health: {hero.baseMaxHealth:F0}\n";
        statsText += $"⚔️ Damage: {hero.baseDamage:F2}x\n";
        statsText += $"\n<b>Starting Weapon:</b>\n";
        statsText += GetWeaponDisplayName(hero.startingWeaponType);
        
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

    
    private void SelectHero(HeroDefinition hero)
    {
        DebugLog.Info($"[CharacterSelectionUI] SelectHero called for: {hero?.displayName ?? "NULL"}");
        OnHeroSelected(hero);
    }
    
    private string GetHeroStatsText(HeroDefinition hero)
    {
        return $"Health: {hero.baseMaxHealth:F0}\n" +
               $"Move Speed: {hero.baseMoveSpeed:F1}x\n" +
               $"Damage: {hero.baseDamage:F1}x\n" +
               $"Starting Weapon:\n{hero.startingWeaponType}";
    }
    
    private void OnHeroSelected(HeroDefinition hero)
    {
        selectedHero = hero;
        DebugLog.Info($"[CharacterSelectionUI] Selected: {hero.displayName}");
        
        // Hide selection screen
        HideSelectionScreen();
        
        // Initialize player with selected hero
        InitializePlayerHero();

        // CRITICAL: Transition to gameplay AFTER player initialization
        DebugLog.Verbose($"[CharacterSelectionUI] About to call TransitionToGameplay, current phase: {GamePhaseManager.CurrentPhase}");
        GamePhaseManager.TransitionToGameplay();
        DebugLog.Info($"[CharacterSelectionUI] After TransitionToGameplay, current phase: {GamePhaseManager.CurrentPhase}");
    }
    
    private void InitializePlayerHero()
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
        
        // Apply hero definition to player
        player.InitializeWithHero(selectedHero);
        
        DebugLog.Info($"[CharacterSelectionUI] Initialized player as {selectedHero.displayName}");
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
