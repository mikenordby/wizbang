using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Displays active synergies in the top-right corner of the screen.
/// Shows tag bonuses when multiple weapons share tags.
/// </summary>
public class SynergyUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private int fontSize = 18;
    [SerializeField] private Color synergyTextColor = Color.cyan;

    private GameObject synergyPanel;
    private Text synergyTitleText;
    private GameObject synergyListContainer;
    private List<Text> synergyTextElements = new List<Text>();

    private Text weaponsTitleText;
    private GameObject weaponsListContainer;
    private List<Text> weaponTextElements = new List<Text>();

    private SynergyManager synergyManager;
    private WeaponInventory weaponInventory;

    void Awake()
    {
        CreateUI();
        DebugLog.Info("[SynergyUI] Awake - UI creation started");
    }

    void Start()
    {
        synergyManager = GameServices.SynergyManager;
        if (synergyManager == null)
        {
            DebugLog.Warning("[SynergyUI] SynergyManager not found - UI will not update");
            DebugLog.Warning("[SynergyUI] Make sure you added SynergyManager component to the Player object!");
        }
        else
        {
            DebugLog.Info("[SynergyUI] SynergyManager found, UI is active");
        }

        weaponInventory = GameServices.Player?.GetComponent<WeaponInventory>();
        if (weaponInventory == null)
        {
            DebugLog.Warning("[SynergyUI] WeaponInventory not found - weapon list will not update");
        }
    }

    void Update()
    {
        // Update every frame to show current synergies and weapons
        if (GamePhaseManager.CurrentPhase == GamePhase.Gameplay)
        {
            if (synergyManager != null)
                UpdateSynergyDisplay();

            if (weaponInventory != null)
                UpdateWeaponDisplay();
        }
    }

    void CreateUI()
    {
        // Get or create canvas
        Canvas canvas = UIManager.GetOrCreateCanvas();
        DebugLog.Info($"[SynergyUI] Canvas found: {canvas != null}, name: {canvas?.name}");

        // Create synergy panel (top-right corner)
        synergyPanel = new GameObject("SynergyPanel");
        synergyPanel.transform.SetParent(canvas.transform, false);
        DebugLog.Info($"[SynergyUI] Created SynergyPanel, parent: {synergyPanel.transform.parent?.name}");

        RectTransform panelRect = synergyPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f); // Top-right
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-220, -10); // 220px from right edge (avoid level counter)
        panelRect.sizeDelta = new Vector2(350, 300); // Larger to fit weapon list

        // Semi-transparent background
        Image panelBg = synergyPanel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.5f);

        // Add vertical layout group for auto-layout
        VerticalLayoutGroup layoutGroup = synergyPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.spacing = 5;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;

        // Add content size fitter to auto-resize
        ContentSizeFitter fitter = synergyPanel.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create title text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(synergyPanel.transform, false);

        synergyTitleText = titleObj.AddComponent<Text>();
        synergyTitleText.text = "SYNERGIES";
        synergyTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        synergyTitleText.fontSize = fontSize + 4;
        synergyTitleText.alignment = TextAnchor.MiddleLeft;
        synergyTitleText.color = Color.yellow;

        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = fontSize + 8;

        // Create container for synergy list (populated dynamically)
        synergyListContainer = new GameObject("SynergyList");
        synergyListContainer.transform.SetParent(synergyPanel.transform, false);

        VerticalLayoutGroup listLayout = synergyListContainer.AddComponent<VerticalLayoutGroup>();
        listLayout.childAlignment = TextAnchor.UpperLeft;
        listLayout.spacing = 2;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = false;
        listLayout.childControlHeight = true;
        listLayout.childControlWidth = true;

        LayoutElement containerLayout = synergyListContainer.AddComponent<LayoutElement>();
        containerLayout.flexibleHeight = 1;

        // === WEAPONS SECTION ===

        // Create weapons title text
        GameObject weaponsTitleObj = new GameObject("WeaponsTitle");
        weaponsTitleObj.transform.SetParent(synergyPanel.transform, false);

        weaponsTitleText = weaponsTitleObj.AddComponent<Text>();
        weaponsTitleText.text = "WEAPONS";
        weaponsTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        weaponsTitleText.fontSize = fontSize + 4;
        weaponsTitleText.alignment = TextAnchor.MiddleLeft;
        weaponsTitleText.color = Color.yellow;

        LayoutElement weaponsTitleLayout = weaponsTitleObj.AddComponent<LayoutElement>();
        weaponsTitleLayout.preferredHeight = fontSize + 8;

        // Create container for weapons list (populated dynamically)
        weaponsListContainer = new GameObject("WeaponsList");
        weaponsListContainer.transform.SetParent(synergyPanel.transform, false);

        VerticalLayoutGroup weaponsLayout = weaponsListContainer.AddComponent<VerticalLayoutGroup>();
        weaponsLayout.childAlignment = TextAnchor.UpperLeft;
        weaponsLayout.spacing = 2;
        weaponsLayout.childForceExpandWidth = true;
        weaponsLayout.childForceExpandHeight = false;
        weaponsLayout.childControlHeight = true;
        weaponsLayout.childControlWidth = true;

        LayoutElement weaponsContainerLayout = weaponsListContainer.AddComponent<LayoutElement>();
        weaponsContainerLayout.flexibleHeight = 1;

        // Make sure panel starts visible
        synergyPanel.SetActive(true);

        // Show initial "None" text so player can see the UI exists
        CreateSynergyText("None");

        DebugLog.Info("[SynergyUI] UI created in top-right corner");
        DebugLog.Info($"[SynergyUI] Panel active: {synergyPanel.activeSelf}, position: {panelRect.anchoredPosition}");
    }

    void UpdateSynergyDisplay()
    {
        if (synergyManager == null) return;

        Dictionary<WeaponTag, float> activeSynergies = synergyManager.GetActiveSynergies();

        // Clear old text elements
        foreach (Text textElement in synergyTextElements)
        {
            if (textElement != null)
                Destroy(textElement.gameObject);
        }
        synergyTextElements.Clear();

        // If no synergies, show "None"
        if (activeSynergies.Count == 0)
        {
            CreateSynergyText("None");
            return;
        }

        // Create text for each synergy
        foreach (var kvp in activeSynergies)
        {
            WeaponTag tag = kvp.Key;
            float bonus = kvp.Value;

            int percentage = Mathf.RoundToInt(bonus * 100);
            string synergyText = $"[{tag}] +{percentage}%";

            CreateSynergyText(synergyText);
            DebugLog.Info($"[SynergyUI] Displaying synergy: {synergyText}");
        }
    }

    void CreateSynergyText(string text)
    {
        GameObject textObj = new GameObject("SynergyText");
        textObj.transform.SetParent(synergyListContainer.transform, false);

        Text synergyText = textObj.AddComponent<Text>();
        synergyText.text = text;
        synergyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        synergyText.fontSize = fontSize;
        synergyText.alignment = TextAnchor.MiddleLeft;
        synergyText.color = synergyTextColor;

        LayoutElement layout = textObj.AddComponent<LayoutElement>();
        layout.preferredHeight = fontSize + 4;

        synergyTextElements.Add(synergyText);
    }

    void UpdateWeaponDisplay()
    {
        if (weaponInventory == null) return;

        // Clear old text elements
        foreach (Text textElement in weaponTextElements)
        {
            if (textElement != null)
                Destroy(textElement.gameObject);
        }
        weaponTextElements.Clear();

        // Get weapons from inventory
        List<Weapon> weapons = weaponInventory.GetWeapons();

        if (weapons.Count == 0)
        {
            CreateWeaponText("None");
            return;
        }

        // Create text for each weapon showing name and tags
        foreach (Weapon weapon in weapons)
        {
            if (weapon == null) continue;

            string weaponName = weapon.WeaponName;
            List<WeaponTag> tags = weapon.GetTags();

            // Format: "Magic Missile [Projectile][Arcane]"
            string tagText = "";
            foreach (WeaponTag tag in tags)
            {
                tagText += $"[{tag}]";
            }

            string fullText = $"{weaponName} {tagText}";
            CreateWeaponText(fullText);
        }
    }

    void CreateWeaponText(string text)
    {
        GameObject textObj = new GameObject("WeaponText");
        textObj.transform.SetParent(weaponsListContainer.transform, false);

        Text weaponText = textObj.AddComponent<Text>();
        weaponText.text = text;
        weaponText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        weaponText.fontSize = fontSize - 2; // Slightly smaller than synergy text
        weaponText.alignment = TextAnchor.MiddleLeft;
        weaponText.color = Color.white;

        LayoutElement layout = textObj.AddComponent<LayoutElement>();
        layout.preferredHeight = fontSize + 2;

        weaponTextElements.Add(weaponText);
    }

    void OnDestroy()
    {
        if (synergyPanel != null)
            Destroy(synergyPanel);
    }
}
