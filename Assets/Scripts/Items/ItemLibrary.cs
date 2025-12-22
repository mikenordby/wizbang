using UnityEngine;

/// <summary>
/// Static library of item definitions for runtime creation.
/// Creates default items if no ScriptableObject assets exist.
/// </summary>
public static class ItemLibrary
{
    /// <summary>
    /// Create all default items programmatically
    /// </summary>
    public static ItemDefinition[] CreateDefaultItems()
    {
        return new ItemDefinition[]
        {
            // ===== COMMON ITEMS (50% drop rate) =====
            CreateItem("leather_boots", "Leather Boots", ItemRarity.Common,
                "Worn but reliable footwear.",
                bonusMoveSpeedPercent: 0.05f), // +5% move speed
            
            CreateItem("iron_ring", "Iron Ring", ItemRarity.Common,
                "A simple iron band.",
                bonusMaxHealth: 10f), // +10 HP
            
            CreateItem("lucky_coin", "Lucky Coin", ItemRarity.Common,
                "Increases your fortune slightly.",
                bonusPickupRadiusPercent: 0.15f), // +15% pickup radius
            
            CreateItem("shabby_gloves", "Shabby Gloves", ItemRarity.Common,
                "Fingerless gloves that help you attack faster.",
                bonusAttackSpeedPercent: 0.05f), // +5% attack speed
            
            CreateItem("minor_focus", "Minor Focus Charm", ItemRarity.Common,
                "A small charm that steadies your aim.",
                bonusDamagePercent: 0.05f), // +5% damage
            
            // ===== RARE ITEMS (25% drop rate) =====
            CreateItem("swift_boots", "Swift Boots", ItemRarity.Rare,
                "Enchanted boots that make you faster.",
                bonusMoveSpeedPercent: 0.15f), // +15% move speed
            
            CreateItem("vampiric_fang", "Vampiric Fang", ItemRarity.Rare,
                "Drain life from your enemies.",
                bonusHealthRegen: 1.5f), // +1.5 HP/sec
            
            CreateItem("sturdy_armor", "Sturdy Armor", ItemRarity.Rare,
                "Increases your maximum health.",
                bonusMaxHealth: 30f), // +30 HP
            
            CreateItem("power_gloves", "Power Gloves", ItemRarity.Rare,
                "Strike with greater force.",
                bonusDamagePercent: 0.15f), // +15% damage
            
            CreateItem("precision_scope", "Precision Scope", ItemRarity.Rare,
                "Improves your critical hit chance.",
                bonusCritChance: 0.08f), // +8% crit chance
            
            CreateItem("rapid_quiver", "Rapid Quiver", ItemRarity.Rare,
                "Attack much faster.",
                bonusAttackSpeedPercent: 0.15f), // +15% attack speed
            
            CreateItem("magnet_amulet", "Magnet Amulet", ItemRarity.Rare,
                "Attracts XP from much farther away.",
                bonusPickupRadiusPercent: 0.35f), // +35% pickup radius
            
            // ===== EXOTIC ITEMS (15% drop rate) =====
            CreateItem("winged_sandals", "Winged Sandals", ItemRarity.Exotic,
                "Legendary footwear blessed by the gods.",
                bonusMoveSpeedPercent: 0.30f), // +30% move speed
            
            CreateItem("berserker_belt", "Berserker's Belt", ItemRarity.Exotic,
                "Massive damage boost for the fearless.",
                bonusDamagePercent: 0.30f), // +30% damage
            
            CreateItem("phoenix_heart", "Phoenix Heart", ItemRarity.Exotic,
                "Regenerate health at an incredible rate.",
                bonusHealthRegen: 3f, // +3 HP/sec
                bonusMaxHealth: 50f), // +50 HP
            
            CreateItem("eagle_eye", "Eagle Eye Lens", ItemRarity.Exotic,
                "See weaknesses others cannot.",
                bonusCritChance: 0.15f, // +15% crit chance
                bonusCritDamage: 0.50f), // +50% crit damage
            
            CreateItem("piercing_needle", "Piercing Needle", ItemRarity.Exotic,
                "Your projectiles pierce through enemies.",
                bonusPierce: 1), // +1 pierce
            
            CreateItem("multi_quiver", "Splitting Quiver", ItemRarity.Exotic,
                "Fire an additional projectile.",
                bonusProjectileCount: 1), // +1 projectile
            
            // ===== LEGENDARY ITEMS (8% drop rate) =====
            CreateItem("titans_heart", "Titan's Heart", ItemRarity.Legendary,
                "The essence of an ancient giant.",
                bonusMaxHealth: 100f, // +100 HP
                bonusDamagePercent: 0.20f, // +20% damage
                bonusHealthRegen: 2f), // +2 HP/sec
            
            CreateItem("time_warp_gloves", "Time Warp Gloves", ItemRarity.Legendary,
                "Bend time to attack with blinding speed.",
                bonusAttackSpeedPercent: 0.35f, // +35% attack speed
                bonusMoveSpeedPercent: 0.20f), // +20% move speed
            
            CreateItem("god_slayer_ring", "God Slayer Ring", ItemRarity.Legendary,
                "A ring forged to slay the divine.",
                bonusDamagePercent: 0.40f, // +40% damage
                bonusCritChance: 0.15f), // +15% crit chance
            
            CreateItem("twin_souls", "Twin Souls Amulet", ItemRarity.Legendary,
                "Your projectiles split into two.",
                bonusProjectileCount: 2, // +2 projectiles
                bonusProjectileSizePercent: 0.20f), // +20% size
            
            CreateItem("piercing_infinity", "Infinity Pierce", ItemRarity.Legendary,
                "Projectiles never stop.",
                bonusPierce: 3, // +3 pierce
                bonusDamagePercent: 0.15f), // +15% damage
            
            // ===== SUPREME ITEMS (2% drop rate) =====
            CreateItem("crown_of_eternity", "Crown of Eternity", ItemRarity.Supreme,
                "The ultimate power. All stats massively increased.",
                bonusMaxHealth: 150f, // +150 HP
                bonusDamagePercent: 0.50f, // +50% damage
                bonusAttackSpeedPercent: 0.30f, // +30% attack speed
                bonusMoveSpeedPercent: 0.30f, // +30% move speed
                bonusCritChance: 0.20f, // +20% crit chance
                bonusCritDamage: 1.0f, // +100% crit damage
                bonusHealthRegen: 5f), // +5 HP/sec
            
            CreateItem("infinity_gauntlet", "Infinity Gauntlet", ItemRarity.Supreme,
                "Control over space and projectiles.",
                bonusProjectileCount: 3, // +3 projectiles
                bonusPierce: 5, // +5 pierce
                bonusProjectileSizePercent: 0.50f, // +50% size
                bonusDamagePercent: 0.40f), // +40% damage
            
            CreateItem("soul_harvester", "Soul Harvester", ItemRarity.Supreme,
                "Drain the essence of all you slay.",
                bonusHealthRegen: 10f, // +10 HP/sec
                bonusPickupRadiusPercent: 1.0f, // +100% pickup radius
                bonusDamagePercent: 0.30f, // +30% damage
                bonusCritChance: 0.25f), // +25% crit chance
        };
    }
    
    /// <summary>
    /// Helper method to create item definitions
    /// </summary>
    private static ItemDefinition CreateItem(
        string itemId, 
        string displayName, 
        ItemRarity rarity, 
        string description,
        float bonusMaxHealth = 0f,
        float bonusHealthRegen = 0f,
        float bonusDamagePercent = 0f,
        float bonusAttackSpeedPercent = 0f,
        float bonusMoveSpeedPercent = 0f,
        float bonusCritChance = 0f,
        float bonusCritDamage = 0f,
        float bonusPickupRadiusPercent = 0f,
        float bonusProjectileSizePercent = 0f,
        int bonusPierce = 0,
        int bonusProjectileCount = 0,
        string specialEffectId = "",
        float specialEffectValue = 0f)
    {
        ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.itemId = itemId;
        item.displayName = displayName;
        item.description = description;
        item.rarity = rarity;
        item.spriteType = $"Items/{itemId}"; // Set sprite path for loading
        item.bonusMaxHealth = bonusMaxHealth;
        item.bonusHealthRegen = bonusHealthRegen;
        item.bonusDamagePercent = bonusDamagePercent;
        item.bonusAttackSpeedPercent = bonusAttackSpeedPercent;
        item.bonusMoveSpeedPercent = bonusMoveSpeedPercent;
        item.bonusCritChance = bonusCritChance;
        item.bonusCritDamage = bonusCritDamage;
        item.bonusPickupRadiusPercent = bonusPickupRadiusPercent;
        item.bonusProjectileSizePercent = bonusProjectileSizePercent;
        item.bonusPierce = bonusPierce;
        item.bonusProjectileCount = bonusProjectileCount;
        item.specialEffectId = specialEffectId;
        item.specialEffectValue = specialEffectValue;
        item.isStackable = true;
        item.maxStack = 99;
        
        return item;
    }
}

