using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the level-up UI overlay with a continue button
/// </summary>
public class LevelUpUI : MonoBehaviour
{
    private static LevelUpUI instance;
    
    private GameObject levelUpPanel;
    private Button continueButton;
    private Text levelText;
    
    public bool IsShowingUI => levelUpPanel != null && levelUpPanel.activeSelf;
    
    private void Awake()
    {
        instance = this;
        CreateUI();
        HideUI();
    }
    
    private void CreateUI()
    {
        // Create EventSystem if it doesn't exist (required for UI input)
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Debug.Log("LevelUpUI: Created EventSystem with InputSystemUIInputModule for UI input");
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
        textRect.anchorMin = new Vector2(0.5f, 0.6f);
        textRect.anchorMax = new Vector2(0.5f, 0.6f);
        textRect.sizeDelta = new Vector2(400, 100);
        textRect.anchoredPosition = Vector2.zero;
        
        // Create continue button
        GameObject buttonObj = new GameObject("ContinueButton");
        buttonObj.transform.SetParent(levelUpPanel.transform, false);
        continueButton = buttonObj.AddComponent<Button>();
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.7f, 0.2f); // Green
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.4f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
        buttonRect.sizeDelta = new Vector2(200, 60);
        buttonRect.anchoredPosition = Vector2.zero;
        
        // Button text
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        Text buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.text = "Continue";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 24;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        
        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        
        // Hook up button click
        continueButton.onClick.AddListener(OnContinueClicked);
        
        Debug.Log("LevelUpUI: UI created");
    }
    
    public void ShowUI(int newLevel)
    {
        levelUpPanel.SetActive(true);
        levelText.text = $"LEVEL {newLevel}!";
        GameState.SetPaused(true);
        Debug.Log($"LevelUpUI: Showing UI for level {newLevel}");
    }
    
    public void HideUI()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
            GameState.SetPaused(false);
        }
    }
    
    private void OnContinueClicked()
    {
        Debug.Log("LevelUpUI: Continue button clicked, resuming game");
        HideUI();
    }
}
