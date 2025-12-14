using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main menu shown at game start. Pauses game until Play button clicked.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button playButton;
    
    private bool isShowingMenu = true;
    
    private void Awake()
    {
        // Create UI if not already in scene
        if (menuPanel == null)
        {
            CreateMainMenuUI();
        }
        
        // Pause game on start
        Time.timeScale = 0f;
        isShowingMenu = true;
        
        DebugLog.Info("[MainMenu] Game paused, waiting for Play button");
    }
    
    private void CreateMainMenuUI()
    {
        // Create Canvas if needed
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("MainMenuCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Ensure EventSystem exists
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // Create semi-transparent background panel
        GameObject panelObj = new GameObject("MainMenuPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        menuPanel = panelObj;
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.8f); // Dark semi-transparent
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        // Create title text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "WIZBANG";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 80;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = new Color(0.9f, 0.8f, 0.2f); // Gold
        titleText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.65f);
        titleRect.anchorMax = new Vector2(0.5f, 0.65f);
        titleRect.sizeDelta = new Vector2(600, 100);
        titleRect.anchoredPosition = Vector2.zero;
        
        // Create subtitle text
        GameObject subtitleObj = new GameObject("SubtitleText");
        subtitleObj.transform.SetParent(panelObj.transform, false);
        
        Text subtitleText = subtitleObj.AddComponent<Text>();
        subtitleText.text = "Survive the endless horde!";
        subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subtitleText.fontSize = 24;
        subtitleText.color = Color.white;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.5f, 0.55f);
        subtitleRect.anchorMax = new Vector2(0.5f, 0.55f);
        subtitleRect.sizeDelta = new Vector2(500, 40);
        subtitleRect.anchoredPosition = Vector2.zero;
        
        // Create Play button
        GameObject buttonObj = new GameObject("PlayButton");
        buttonObj.transform.SetParent(panelObj.transform, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f); // Green
        
        playButton = buttonObj.AddComponent<Button>();
        playButton.targetGraphic = buttonImage;
        playButton.onClick.AddListener(OnPlayButtonClicked);
        
        // Button color transitions
        ColorBlock colors = playButton.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.2f);
        colors.highlightedColor = new Color(0.3f, 0.8f, 0.3f);
        colors.pressedColor = new Color(0.1f, 0.4f, 0.1f);
        playButton.colors = colors;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.4f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
        buttonRect.sizeDelta = new Vector2(300, 80);
        buttonRect.anchoredPosition = Vector2.zero;
        
        // Button text
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        
        Text buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.text = "PLAY";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 48;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        
        DebugLog.Info("[MainMenu] Created main menu UI");
    }
    
    private void OnPlayButtonClicked()
    {
        DebugLog.Info("[MainMenu] Play button clicked, starting game!");
        
        // Hide menu
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
        
        // Unpause game
        Time.timeScale = 1f;
        isShowingMenu = false;
        
        // Destroy this menu (one-time use)
        Destroy(gameObject, 0.1f);
    }
    
    public bool IsShowingMenu => isShowingMenu;
}
