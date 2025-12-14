using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages the level-up UI with weapon upgrade choices
/// </summary>
public class LevelUpUI : MonoBehaviour
{
    private static LevelUpUI instance;
    
    private GameObject levelUpPanel;
    private Text levelText;
    private List<Button> upgradeButtons = new List<Button>();
    private List<(Weapon weapon, WeaponUpgrade.UpgradeType upgradeType)> currentChoices = new List<(Weapon, WeaponUpgrade.UpgradeType)>();
    
    private WeaponInventory weaponInventory;
    
    public bool IsShowingUI => levelUpPanel != null && levelUpPanel.activeSelf;
    
    private void Awake()
    {
        instance = this;
        weaponInventory = FindAnyObjectByType<WeaponInventory>();
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
            buttonRect.sizeDelta = new Vector2(240, 180);
            buttonRect.anchoredPosition = new Vector2(buttonXPositions[i], 0);
            
            // Button text (weapon name + upgrade description)
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            Text buttonText = buttonTextObj.AddComponent<Text>();
            buttonText.text = "Upgrade Option";
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 18;
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
        
        DebugLog.Info("LevelUpUI: UI created with 3 upgrade choices");
    }
    
    public void ShowUI(int newLevel)
    {
        if (weaponInventory == null)
        {
            weaponInventory = FindAnyObjectByType<WeaponInventory>();
            if (weaponInventory == null)
            {
                DebugLog.Warning("[LevelUpUI] WeaponInventory not found in scene, creating new one");
                GameObject inventoryObj = new GameObject("WeaponInventory");
                weaponInventory = inventoryObj.AddComponent<WeaponInventory>();
            }
        }
        
        levelUpPanel.SetActive(true);
        levelText.text = $"LEVEL {newLevel}!";
        
        // Generate choices with special logic for level 2
        GenerateUpgradeChoices(newLevel);
        
        GameState.SetPaused(true);
        DebugLog.Info($"LevelUpUI: Showing UI for level {newLevel}");
    }
    
    private void GenerateUpgradeChoices(int currentLevel)
    {
        currentChoices.Clear();
        List<Weapon> weapons = weaponInventory.GetActiveWeapons();
        
        // Level 2: Guarantee Orbiter Weapon as one of the choices
        if (currentLevel == 2)
        {
            bool hasOrbiter = weapons.Exists(w => w is OrbiterWeapon);
            if (!hasOrbiter)
            {
                // First choice: Add Orbiter Weapon
                currentChoices.Add((null, WeaponUpgrade.UpgradeType.Damage)); // Special marker for "new weapon"
                upgradeButtons[0].GetComponentInChildren<Text>().text = "Unlock\n\nOrbiting Blades\nSpinning knives that protect you";
                
                // Remaining 2 choices: Upgrade existing weapons
                for (int i = 1; i < 3; i++)
                {
                    AddRandomUpgradeChoice(weapons, i);
                }
                
                DebugLog.Info("[LevelUpUI] Level 2: Guaranteed Orbiter Weapon unlock");
                return;
            }
        }
        
        // Normal upgrade choices
        if (weapons.Count == 0)
        {
            DebugLog.Warning("[LevelUpUI] No weapons available for upgrades! Closing UI.");
            HideUI();
            return;
        }
        
        for (int i = 0; i < 3; i++)
        {
            AddRandomUpgradeChoice(weapons, i);
        }
    }
    
    private void AddRandomUpgradeChoice(List<Weapon> weapons, int buttonIndex)
    {
        if (weapons.Count == 0) return;
        
        Weapon randomWeapon = weapons[Random.Range(0, weapons.Count)];
        List<WeaponUpgrade> availableUpgrades = randomWeapon.GetAvailableUpgrades();
        
        if (availableUpgrades.Count == 0)
        {
            DebugLog.Warning($"[LevelUpUI] {randomWeapon.WeaponName} has no available upgrades!");
            return;
        }
        
        WeaponUpgrade randomUpgrade = availableUpgrades[Random.Range(0, availableUpgrades.Count)];
        currentChoices.Add((randomWeapon, randomUpgrade.type));
        
        string weaponName = randomWeapon.WeaponName;
        string upgradeName = randomUpgrade.type.ToString();
        string upgradePreview = randomUpgrade.GetNextLevelPreview();
        upgradeButtons[buttonIndex].GetComponentInChildren<Text>().text = $"{weaponName}\n\n{upgradeName}\n{upgradePreview}";
    }
    
    private void OnUpgradeChosen(int index)
    {
        if (index >= currentChoices.Count)
        {
            DebugLog.Error($"[LevelUpUI] Invalid choice index: {index}");
            return;
        }
        
        var (weapon, upgradeType) = currentChoices[index];
        
        // Special case: New weapon unlock (weapon is null)
        if (weapon == null && index == 0) // Level 2 orbiter unlock
        {
            bool success = weaponInventory.AddWeapon("OrbiterWeapon");
            if (success)
            {
                DebugLog.Info("[LevelUpUI] Unlocked Orbiting Blades weapon!");
            }
        }
        else if (weapon != null)
        {
            weapon.ApplyUpgrade(upgradeType);
            DebugLog.Info($"[LevelUpUI] Chose upgrade: {weapon.WeaponName} - {upgradeType}");
        }
        
        HideUI();
    }
    
    public void HideUI()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
            GameState.SetPaused(false);
        }
    }
}
