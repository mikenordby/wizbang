using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// UI component for a single character card in the selection screen.
/// Designed to be attached to a prefab and populated at runtime.
/// </summary>
public class CharacterCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text weaponText;
    [SerializeField] private Image cardBackground;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Button selectButton;
    
    [Header("Visual Settings")]
    [SerializeField] private Color unlockedColor = new Color(0.2f, 0.3f, 0.4f, 0.95f);
    [SerializeField] private Color lockedColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    [SerializeField] private Color hoverColor = new Color(0.3f, 0.5f, 0.7f, 1f);
    
    private HeroDefinition heroDefinition;
    private bool isUnlocked;
    private Action<HeroDefinition> onHoverCallback;
    private Action<HeroDefinition> onSelectCallback;
    private Color originalColor;
    
    /// <summary>
    /// Initialize the character card with hero data and callbacks.
    /// </summary>
    public void Initialize(HeroDefinition hero, bool unlocked, Action<HeroDefinition> onHover, Action<HeroDefinition> onSelect)
    {
        heroDefinition = hero;
        isUnlocked = unlocked;
        onHoverCallback = onHover;
        onSelectCallback = onSelect;
        
        UpdateVisuals();
        SetupButton();
    }
    
    private void UpdateVisuals()
    {
        if (heroDefinition == null) return;
        
        // Set name
        if (nameText != null)
        {
            nameText.text = isUnlocked ? heroDefinition.displayName : "LOCKED";
        }
        
        // Set weapon text
        if (weaponText != null)
        {
            weaponText.text = GetWeaponDisplayName(heroDefinition.startingWeaponType);
        }
        
        // Load portrait sprite
        if (portraitImage != null)
        {
            Sprite portrait = LoadPortraitSprite(heroDefinition.spriteType);
            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.color = isUnlocked ? Color.white : new Color(0.3f, 0.3f, 0.3f);
            }
        }
        
        // Set card background color
        if (cardBackground != null)
        {
            originalColor = isUnlocked ? unlockedColor : lockedColor;
            cardBackground.color = originalColor;
        }
        
        // Show/hide locked overlay
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!isUnlocked);
        }
        
        // Enable/disable button
        if (selectButton != null)
        {
            selectButton.interactable = isUnlocked;
        }
    }
    
    private void SetupButton()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => OnCardClicked());
        }
    }
    
    private void OnCardClicked()
    {
        if (isUnlocked && heroDefinition != null)
        {
            DebugLog.Info($"[CharacterCard] Selected: {heroDefinition.displayName}");
            onSelectCallback?.Invoke(heroDefinition);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (heroDefinition != null && isUnlocked)
        {
            // Highlight card
            if (cardBackground != null)
            {
                cardBackground.color = hoverColor;
            }
            
            // Trigger hover callback for preview panel
            onHoverCallback?.Invoke(heroDefinition);
            
            DebugLog.Verbose($"[CharacterCard] Hovered: {heroDefinition.displayName}");
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Restore original color
        if (cardBackground != null)
        {
            cardBackground.color = originalColor;
        }
    }
    
    /// <summary>
    /// Load hero portrait sprite from Resources.
    /// </summary>
    private Sprite LoadPortraitSprite(string spriteType)
    {
        // Try loading from Heroes folder first
        string heroPath = $"Sprites/Heroes/{spriteType}/south";
        Sprite sprite = Resources.Load<Sprite>(heroPath);
        
        if (sprite != null)
        {
            DebugLog.Verbose($"[CharacterCard] Loaded portrait from: {heroPath}");
            return sprite;
        }
        
        // Fallback to direct sprite folder
        string fallbackPath = $"Sprites/{spriteType}/south";
        sprite = Resources.Load<Sprite>(fallbackPath);
        
        if (sprite != null)
        {
            DebugLog.Verbose($"[CharacterCard] Loaded portrait from fallback: {fallbackPath}");
            return sprite;
        }
        
        DebugLog.Warning($"[CharacterCard] Could not load portrait sprite for {spriteType}");
        return null;
    }
    
    /// <summary>
    /// Get display name for weapon type with icon.
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
}

