using UnityEngine;
using System;

/// <summary>
/// Centralized event system for game-wide communication without tight coupling.
/// Subscribe to events to react to game state changes.
/// 
/// Usage:
///   GameEvents.OnEnemyKilled += MyHandler;  // Subscribe
///   GameEvents.OnEnemyKilled -= MyHandler;  // Unsubscribe
///   GameEvents.TriggerEnemyKilled(enemy);   // Fire event
/// 
/// Benefits:
/// - Decouples systems (e.g., UI doesn't need direct reference to Player)
/// - Easier testing (mock event triggers)
/// - Clearer data flow (explicit event flow vs hidden dependencies)
/// </summary>
public static class GameEvents
{
    // ===== COMBAT EVENTS =====
    
    /// <summary>
    /// Fired when any enemy dies. Passes the enemy that died.
    /// Listeners: Player (stat tracking), UI (kill counter), achievements, etc.
    /// </summary>
    public static event Action<Enemy> OnEnemyKilled;
    
    /// <summary>
    /// Fired when any damage is dealt. Passes source, target, and amount.
    /// Listeners: DamageNumbers, UI (combat log), stat tracking
    /// </summary>
    public static event Action<GameObject, GameObject, float> OnDamageDealt;
    
    /// <summary>
    /// Fired when the player takes damage. Passes damage amount and new health.
    /// Listeners: UI (health bar), screen shake, audio, etc.
    /// </summary>
    public static event Action<float, float> OnPlayerDamaged;
    
    /// <summary>
    /// Fired when player health is healed. Passes heal amount and new health.
    /// Listeners: UI (health bar), VFX, audio
    /// </summary>
    public static event Action<float, float> OnPlayerHealed;
    
    // ===== PROGRESSION EVENTS =====
    
    /// <summary>
    /// Fired when player gains XP. Passes XP amount gained and new total.
    /// Listeners: UI (XP bar), audio
    /// </summary>
    public static event Action<int, int> OnXPGained;
    
    /// <summary>
    /// Fired when player levels up. Passes new level.
    /// Listeners: LevelUpUI, audio, VFX, stat recalculation
    /// </summary>
    public static event Action<int> OnPlayerLevelUp;
    
    /// <summary>
    /// Fired when an upgrade is selected. Passes the upgrade choice.
    /// Listeners: WeaponInventory, Player stats, UI updates
    /// </summary>
    public static event Action<UpgradeChoice> OnUpgradeSelected;
    
    // ===== WEAPON EVENTS =====
    
    /// <summary>
    /// Fired when a new weapon is added to inventory. Passes weapon instance.
    /// Listeners: UI (weapon display), audio, tutorial system
    /// </summary>
    public static event Action<Weapon> OnWeaponAdded;
    
    /// <summary>
    /// Fired when a weapon is upgraded. Passes weapon and new level.
    /// Listeners: UI (weapon HUD), VFX, audio
    /// </summary>
    public static event Action<Weapon, int> OnWeaponUpgraded;
    
    /// <summary>
    /// Fired when a weapon is removed. Passes weapon name.
    /// Listeners: UI cleanup, stat recalculation
    /// </summary>
    public static event Action<string> OnWeaponRemoved;
    
    /// <summary>
    /// Fired when a weapon fires. Passes weapon name and projectile count.
    /// Listeners: Audio (weapon sound), VFX (muzzle flash), analytics
    /// </summary>
    public static event Action<string, int> OnWeaponFired;
    
    // ===== GAME STATE EVENTS =====
    
    /// <summary>
    /// Fired when game is paused/unpaused. Passes isPaused state.
    /// Listeners: All systems that need to pause/resume
    /// </summary>
    public static event Action<bool> OnGamePaused;
    
    /// <summary>
    /// Fired when player dies / game over. Passes survival time.
    /// Listeners: Game Over UI, stat saving, leaderboard, audio
    /// </summary>
    public static event Action<float> OnGameOver;
    
    /// <summary>
    /// Fired when game is restarted.
    /// Listeners: All systems that need to reset state
    /// </summary>
    public static event Action OnGameRestart;
    
    // ===== PICKUP EVENTS =====
    
    /// <summary>
    /// Fired when player picks up an item. Passes item type and amount.
    /// Listeners: UI (notification), audio, VFX
    /// </summary>
    public static event Action<string, int> OnItemPickup;
    
    /// <summary>
    /// Fired when player picks up a powerup. Passes powerup name and duration.
    /// Listeners: UI (buff display), VFX, audio
    /// </summary>
    public static event Action<string, float> OnPowerupCollected;
    
    /// <summary>
    /// Fired when player collects an item from the item system.
    /// Listeners: UI (inventory display), audio, achievements
    /// </summary>
    public static event Action<ItemDefinition> OnItemCollected;
    
    // ===== TRIGGER METHODS =====
    // These should be called by the systems that generate the events
    
    public static void TriggerEnemyKilled(Enemy enemy)
    {
        OnEnemyKilled?.Invoke(enemy);
    }
    
    public static void TriggerDamageDealt(GameObject source, GameObject target, float amount)
    {
        OnDamageDealt?.Invoke(source, target, amount);
    }
    
    public static void TriggerPlayerDamaged(float damageAmount, float newHealth)
    {
        OnPlayerDamaged?.Invoke(damageAmount, newHealth);
    }
    
    public static void TriggerPlayerHealed(float healAmount, float newHealth)
    {
        OnPlayerHealed?.Invoke(healAmount, newHealth);
    }
    
    public static void TriggerXPGained(int amount, int newTotal)
    {
        OnXPGained?.Invoke(amount, newTotal);
    }
    
    public static void TriggerPlayerLevelUp(int newLevel)
    {
        OnPlayerLevelUp?.Invoke(newLevel);
    }
    
    public static void TriggerUpgradeSelected(UpgradeChoice choice)
    {
        OnUpgradeSelected?.Invoke(choice);
    }
    
    public static void TriggerWeaponAdded(Weapon weapon)
    {
        OnWeaponAdded?.Invoke(weapon);
    }
    
    public static void TriggerWeaponUpgraded(Weapon weapon, int newLevel)
    {
        OnWeaponUpgraded?.Invoke(weapon, newLevel);
    }
    
    public static void TriggerWeaponRemoved(string weaponName)
    {
        OnWeaponRemoved?.Invoke(weaponName);
    }
    
    public static void TriggerWeaponFired(string weaponName, int projectileCount)
    {
        OnWeaponFired?.Invoke(weaponName, projectileCount);
    }
    
    public static void TriggerGamePaused(bool isPaused)
    {
        OnGamePaused?.Invoke(isPaused);
    }
    
    public static void TriggerGameOver(float survivalTime)
    {
        OnGameOver?.Invoke(survivalTime);
    }
    
    public static void TriggerGameRestart()
    {
        OnGameRestart?.Invoke();
    }
    
    public static void TriggerItemPickup(string itemType, int amount)
    {
        OnItemPickup?.Invoke(itemType, amount);
    }
    
    public static void TriggerPowerupCollected(string powerupName, float duration)
    {
        OnPowerupCollected?.Invoke(powerupName, duration);
    }
    
    public static void TriggerItemCollected(ItemDefinition item)
    {
        OnItemCollected?.Invoke(item);
    }
    
    /// <summary>
    /// Clear all event subscriptions (call on scene unload to prevent memory leaks)
    /// </summary>
    public static void ClearAllEvents()
    {
        OnEnemyKilled = null;
        OnDamageDealt = null;
        OnPlayerDamaged = null;
        OnPlayerHealed = null;
        OnXPGained = null;
        OnPlayerLevelUp = null;
        OnUpgradeSelected = null;
        OnWeaponAdded = null;
        OnWeaponUpgraded = null;
        OnWeaponRemoved = null;
        OnWeaponFired = null;
        OnGamePaused = null;
        OnGameOver = null;
        OnGameRestart = null;
        OnItemPickup = null;
        OnPowerupCollected = null;
        OnItemCollected = null;
    }
}
