using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Factory for creating UI components programmatically when prefabs are not available.
/// This serves as a fallback to ensure the game works even without prefabs assigned.
/// </summary>
public static class UIComponentFactory
{
    /// <summary>
    /// Create a CharacterCard GameObject with all required components and children.
    /// This is a fallback for when no prefab is assigned.
    /// </summary>
    public static GameObject CreateCharacterCard(Transform parent)
    {
        // Root card object
        GameObject cardObj = new GameObject("CharacterCard");
        cardObj.transform.SetParent(parent, false);
        
        // Add RectTransform
        RectTransform cardRect = cardObj.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(250f, 320f);
        
        // Add background image
        Image cardBackground = cardObj.AddComponent<Image>();
        cardBackground.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        
        // Add Button component
        Button button = cardObj.AddComponent<Button>();
        button.targetGraphic = cardBackground;
        
        // Configure button colors
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.35f, 1f);
        colors.pressedColor = new Color(0.3f, 0.3f, 0.45f, 1f);
        colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        
        // Create Portrait
        GameObject portraitObj = new GameObject("Portrait");
        portraitObj.transform.SetParent(cardObj.transform, false);
        Image portraitImage = portraitObj.AddComponent<Image>();
        portraitImage.color = new Color(0.3f, 0.3f, 0.3f);
        portraitImage.raycastTarget = false;
        
        RectTransform portraitRect = portraitObj.GetComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0.1f, 0.4f);
        portraitRect.anchorMax = new Vector2(0.9f, 0.9f);
        portraitRect.sizeDelta = Vector2.zero;
        
        // Create Name Text
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(cardObj.transform, false);
        Text nameText = nameObj.AddComponent<Text>();
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 24;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.color = Color.white;
        nameText.fontStyle = FontStyle.Bold;
        nameText.text = "Hero Name";
        nameText.raycastTarget = false;
        
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.25f);
        nameRect.anchorMax = new Vector2(1f, 0.38f);
        nameRect.sizeDelta = Vector2.zero;
        
        // Create Weapon Text
        GameObject weaponObj = new GameObject("WeaponText");
        weaponObj.transform.SetParent(cardObj.transform, false);
        Text weaponText = weaponObj.AddComponent<Text>();
        weaponText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        weaponText.fontSize = 14;
        weaponText.alignment = TextAnchor.MiddleCenter;
        weaponText.color = new Color(0.7f, 0.9f, 1f);
        weaponText.text = "Starting Weapon";
        weaponText.raycastTarget = false;
        
        RectTransform weaponRect = weaponObj.GetComponent<RectTransform>();
        weaponRect.anchorMin = new Vector2(0f, 0.12f);
        weaponRect.anchorMax = new Vector2(1f, 0.24f);
        weaponRect.sizeDelta = Vector2.zero;
        
        // Create Locked Overlay
        GameObject lockedOverlayObj = new GameObject("LockedOverlay");
        lockedOverlayObj.transform.SetParent(cardObj.transform, false);
        lockedOverlayObj.SetActive(false); // Hidden by default
        
        Image lockedOverlayImage = lockedOverlayObj.AddComponent<Image>();
        lockedOverlayImage.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform lockedRect = lockedOverlayObj.GetComponent<RectTransform>();
        lockedRect.anchorMin = Vector2.zero;
        lockedRect.anchorMax = Vector2.one;
        lockedRect.sizeDelta = Vector2.zero;
        
        // Lock Icon (child of overlay)
        GameObject lockIconObj = new GameObject("LockIcon");
        lockIconObj.transform.SetParent(lockedOverlayObj.transform, false);
        Text lockText = lockIconObj.AddComponent<Text>();
        lockText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lockText.fontSize = 60;
        lockText.alignment = TextAnchor.MiddleCenter;
        lockText.color = Color.white;
        lockText.text = "🔒";
        lockText.raycastTarget = false;
        
        RectTransform lockRect = lockIconObj.GetComponent<RectTransform>();
        lockRect.anchorMin = new Vector2(0.5f, 0.5f);
        lockRect.anchorMax = new Vector2(0.5f, 0.5f);
        lockRect.sizeDelta = new Vector2(100, 100);
        lockRect.anchoredPosition = Vector2.zero;
        
        // Add CharacterCard component and wire up references
        CharacterCard card = cardObj.AddComponent<CharacterCard>();
        // Use reflection to set private fields (since they're SerializeFields)
        var cardType = typeof(CharacterCard);
        cardType.GetField("portraitImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(card, portraitImage);
        cardType.GetField("nameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(card, nameText);
        cardType.GetField("weaponText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(card, weaponText);
        cardType.GetField("cardBackground", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(card, cardBackground);
        cardType.GetField("lockedOverlay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(card, lockedOverlayObj);
        cardType.GetField("selectButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(card, button);
        
        DebugLog.Verbose("[UIComponentFactory] Created CharacterCard programmatically (fallback mode)");
        return cardObj;
    }
    
    /// <summary>
    /// Create an UpgradeOption GameObject with all required components and children.
    /// This is a fallback for when no prefab is assigned.
    /// </summary>
    public static GameObject CreateUpgradeOption(Transform parent)
    {
        // Root option object
        GameObject optionObj = new GameObject("UpgradeOption");
        optionObj.transform.SetParent(parent, false);
        
        // Add RectTransform (larger size for better visibility)
        RectTransform optionRect = optionObj.AddComponent<RectTransform>();
        optionRect.sizeDelta = new Vector2(320f, 280f);
        
        // Add background image
        Image backgroundImage = optionObj.AddComponent<Image>();
        backgroundImage.color = new Color(0.2f, 0.6f, 0.8f); // Default blue
        
        // Add Button component
        Button button = optionObj.AddComponent<Button>();
        button.targetGraphic = backgroundImage;
        button.navigation = new Navigation { mode = Navigation.Mode.Vertical };
        
        // Create Category Text
        GameObject categoryObj = new GameObject("CategoryText");
        categoryObj.transform.SetParent(optionObj.transform, false);
        Text categoryText = categoryObj.AddComponent<Text>();
        categoryText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        categoryText.fontSize = 12;
        categoryText.alignment = TextAnchor.MiddleCenter;
        categoryText.color = Color.white;
        categoryText.fontStyle = FontStyle.Bold;
        categoryText.text = "[CATEGORY]";
        categoryText.raycastTarget = false;
        
        RectTransform categoryRect = categoryObj.GetComponent<RectTransform>();
        categoryRect.anchorMin = new Vector2(0f, 0.85f);
        categoryRect.anchorMax = new Vector2(1f, 0.95f);
        categoryRect.sizeDelta = Vector2.zero;
        
        // Create Name Text
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(optionObj.transform, false);
        Text nameText = nameObj.AddComponent<Text>();
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 20;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.color = Color.white;
        nameText.fontStyle = FontStyle.Bold;
        nameText.text = "Upgrade Name";
        nameText.raycastTarget = false;
        
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.55f);
        nameRect.anchorMax = new Vector2(1f, 0.8f);
        nameRect.sizeDelta = Vector2.zero;
        
        // Create Description Text
        GameObject descObj = new GameObject("DescriptionText");
        descObj.transform.SetParent(optionObj.transform, false);
        Text descText = descObj.AddComponent<Text>();
        descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        descText.fontSize = 14;
        descText.alignment = TextAnchor.UpperCenter;
        descText.color = new Color(0.8f, 0.8f, 0.8f);
        descText.text = "Description of the upgrade";
        descText.raycastTarget = false;
        descText.horizontalOverflow = HorizontalWrapMode.Wrap;
        descText.verticalOverflow = VerticalWrapMode.Truncate;
        
        RectTransform descRect = descObj.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.05f, 0.1f);
        descRect.anchorMax = new Vector2(0.95f, 0.5f);
        descRect.sizeDelta = Vector2.zero;
        
        // Add UpgradeOption component and wire up references
        UpgradeOption option = optionObj.AddComponent<UpgradeOption>();
        var optionType = typeof(UpgradeOption);
        optionType.GetField("backgroundImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(option, backgroundImage);
        optionType.GetField("categoryText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(option, categoryText);
        optionType.GetField("nameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(option, nameText);
        optionType.GetField("descriptionText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(option, descText);
        optionType.GetField("button", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(option, button);
        
        DebugLog.Verbose("[UIComponentFactory] Created UpgradeOption programmatically (fallback mode)");
        return optionObj;
    }
}

