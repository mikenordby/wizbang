using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// UI component for a single upgrade option button in the level-up screen.
/// Designed to be attached to a prefab and populated at runtime with upgrade data.
/// </summary>
public class UpgradeOption : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Text categoryText;
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button button;
    
    [Header("Visual Settings")]
    [SerializeField] private Color weaponColor = new Color(1f, 0.3f, 0.3f);      // Red
    [SerializeField] private Color upgradeColor = new Color(0.3f, 0.8f, 1f);     // Cyan
    [SerializeField] private Color statColor = new Color(0.4f, 1f, 0.4f);        // Green
    [SerializeField] private Color defaultColor = new Color(0.5f, 0.5f, 0.5f);   // Gray
    
    private UpgradeChoice upgradeChoice;
    private Action<UpgradeChoice> onSelectCallback;
    
    /// <summary>
    /// Initialize the upgrade option with choice data and selection callback.
    /// </summary>
    public void Initialize(UpgradeChoice choice, Action<UpgradeChoice> onSelect)
    {
        upgradeChoice = choice;
        onSelectCallback = onSelect;
        
        UpdateDisplay();
        SetupButton();
    }
    
    private void UpdateDisplay()
    {
        if (upgradeChoice == null)
        {
            DebugLog.Warning("[UpgradeOption] Attempted to display null upgrade choice");
            return;
        }
        
        // Set category text and background color based on upgrade type
        if (categoryText != null && backgroundImage != null)
        {
            switch (upgradeChoice.Type)
            {
                case UpgradeChoice.ChoiceType.NewWeapon:
                    categoryText.text = "[NEW WEAPON]";
                    backgroundImage.color = weaponColor;
                    break;
                    
                case UpgradeChoice.ChoiceType.WeaponUpgrade:
                    categoryText.text = "[UPGRADE]";
                    backgroundImage.color = upgradeColor;
                    break;
                    
                case UpgradeChoice.ChoiceType.PlayerStat:
                    categoryText.text = "[STAT]";
                    backgroundImage.color = statColor;
                    break;
                    
                default:
                    categoryText.text = "[UPGRADE]";
                    backgroundImage.color = defaultColor;
                    break;
            }
        }
        
        // Set name and description
        if (nameText != null)
        {
            nameText.text = upgradeChoice.DisplayName;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = upgradeChoice.Description;
        }
        
        DebugLog.Verbose($"[UpgradeOption] Displayed: {upgradeChoice.DisplayName}");
    }
    
    private void SetupButton()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnButtonClicked());
        }
    }
    
    private void OnButtonClicked()
    {
        if (upgradeChoice != null)
        {
            DebugLog.Info($"[UpgradeOption] Selected: {upgradeChoice.DisplayName}");
            onSelectCallback?.Invoke(upgradeChoice);
        }
    }
    
    /// <summary>
    /// Clear the upgrade option (reset to default state).
    /// </summary>
    public void Clear()
    {
        upgradeChoice = null;
        
        if (categoryText != null)
            categoryText.text = "";
        
        if (nameText != null)
            nameText.text = "Upgrade Option";
        
        if (descriptionText != null)
            descriptionText.text = "";
        
        if (backgroundImage != null)
            backgroundImage.color = defaultColor;
        
        if (button != null)
            button.onClick.RemoveAllListeners();
    }
}

