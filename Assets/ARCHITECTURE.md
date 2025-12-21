# Wizbang - Architecture & Code Quality Guide

> **Purpose**: Comprehensive technical reference for AI agents and developers working on this project.  
> **Last Updated**: December 21, 2025  
> **Unity Version**: 6.0.0 LTS

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Project Overview](#project-overview)
3. [Core Architecture](#core-architecture)
4. [System Deep Dives](#system-deep-dives)
5. [Architectural Issues & Technical Debt](#architectural-issues--technical-debt)
6. [Recommended Improvements](#recommended-improvements)
7. [Performance Considerations](#performance-considerations)
8. [Code Patterns & Conventions](#code-patterns--conventions)
9. [Quick Reference for AI Agents](#quick-reference-for-ai-agents)

---

## Executive Summary

**Wizbang** is a top-down bullet heaven survival game (Vampire Survivors-like) built in Unity 6 LTS. The codebase demonstrates several **good architectural decisions** (object pooling, spatial hashing, service locator, data-driven design) but also has **notable technical debt** that should be addressed before major feature expansion.

### Current Strengths ✓
- Custom spatial hash grid collision detection (O(n) instead of O(n²))
- Object pooling for enemies, projectiles, XP orbs, damage numbers
- Service Locator pattern for centralized system access
- Data-driven enemy/character system via ScriptableObjects
- Event-driven communication via `GameEvents`
- Game phase state machine preventing race conditions

### Key Areas for Improvement ✗
- **Player/Hero abstraction** - `Player.cs` conflates runtime state with character identity
- **Collision handling duplication** - weapons duplicate collision code instead of using strategy pattern
- **Missing Entity Component System (ECS) mindset** - components are tightly coupled
- **UI creation is fully procedural** - no prefabs, hard to maintain
- **Limited use of interfaces** - concrete dependencies throughout
- **Spawner logic embedded in pools** - violates single responsibility

---

## Project Overview

### Technology Stack
| Component | Technology |
|-----------|-----------|
| Engine | Unity 6.0.0 LTS |
| Runtime | .NET Framework 4.x |
| Input | Unity Input System |
| Rendering | URP (Universal Render Pipeline) |
| UI | Unity UGUI (Legacy UI) |
| External Packages | None (pure Unity) |

### Game Loop Summary
```
1. CharacterSelection → Player picks hero
2. Player.InitializeWithCharacter() → Apply stats, sprite, starting weapon
3. GamePhaseManager.TransitionToGameplay() → Enable all systems
4. EnemySpawner spawns enemies → Enemies home toward player
5. Weapons auto-fire → Projectiles collide via SpatialHashGrid
6. Enemies die → Drop XP → Player levels up → Choose upgrades
7. Player dies → Game Over
```

### Key Entry Points
| File | Purpose |
|------|---------|
| `GamePhaseManager.cs` | State machine controlling game flow |
| `GameServices.cs` | Service locator for system access |
| `GameBootstrap.cs` | Scene initialization |
| `CharacterSelectionUI.cs` | Starting point for gameplay |
| `CollisionManager.cs` | Main game loop collision detection |

---

## Core Architecture

### 1. Service Locator Pattern (`GameServices.cs`)

Provides centralized, null-safe access to all major systems.

```csharp
// Usage anywhere in codebase
Projectile p = GameServices.ProjectilePool.GetProjectile();
Enemy e = GameServices.EnemyPool.GetEnemy(stats);
GameServices.DamageNumberPool.ShowDamage(50, position);
```

**Registered Services:**
- `XPOrbPool`, `EnemyPool`, `ProjectilePool`, `DamageNumberPool`
- `CollisionManager`, `OrbiterManager`, `LevelUpUI`
- `Player`, `DamageCalculator`

**Auto-Discovery**: Services are found via `FindFirstObjectByType<T>()` in `Awake()`.

**⚠️ Issue**: This creates hidden dependencies. Consider registering services explicitly or using dependency injection for testability.

---

### 2. Game Phase State Machine (`GamePhaseManager.cs`)

Controls discrete game states to prevent initialization race conditions.

```
                ┌─────────────────┐
                │    MainMenu     │
                └────────┬────────┘
                         │ TransitionToCharacterSelection()
                         ▼
              ┌──────────────────────┐
              │  CharacterSelection  │
              └──────────┬───────────┘
                         │ TransitionToGameplay()
                         ▼
              ┌──────────────────────┐
              │      Gameplay        │◄────────┐
              └──────────┬───────────┘         │
                         │                     │
                    ┌────┴────┐           LevelUpUI
                    ▼         ▼           dismisses
              ┌─────────┐ ┌─────────┐          │
              │GameOver │ │ LevelUp │──────────┘
              └─────────┘ └─────────┘
```

**Critical Rule**: All gameplay systems check `GamePhaseManager.CurrentPhase == GamePhase.Gameplay` before running logic. This prevents weapons firing during menus.

---

### 3. Object Pooling System (`ObjectPool<T>`)

Generic base class eliminating duplicate pooling logic. All pools use "Active List Caching" to avoid `GetComponentsInChildren()` GC allocations.

```csharp
// Pool hierarchy
ObjectPool<T> (abstract base)
    ├── EnemyPool (100 initial, 500 max)
    ├── ProjectilePool (50 initial, 200 max)
    ├── XPOrbPool (50 initial, 200 max)
    └── DamageNumberPool (20 fixed, circular buffer)
```

**Get/Return Pattern:**
```csharp
// Get from pool (adds to activeItems list)
Enemy e = enemyPool.GetEnemy(stats);
e.Activate(position, stats, healthMultiplier);

// Return to pool (removes from activeItems list)
e.Deactivate(); // Calls pool.ReturnEnemy(this) internally
```

**⚠️ Issue**: `Deactivate()` calling `ReturnEnemy()` can cause infinite recursion if not careful. The pattern should be more explicit.

---

### 4. Spatial Hash Grid Collision (`SpatialHashGrid.cs`)

Custom collision detection achieving O(n) instead of O(n²).

**How it works:**
1. World divided into 2.0-unit cells
2. Each frame: Clear grid → Insert all enemies into cells → Weapons query nearby cells
3. Only entities in neighboring cells (3×3 = 9 cells) are checked

**Performance Impact:**
- Before: 100,000 collision checks/frame (200 projectiles × 500 enemies)
- After: ~9,000 collision checks/frame (90% reduction)

```csharp
// Cell coordinate calculation
cellX = Floor((worldX - originX) / cellSize)
cellY = Floor((worldY - originY) / cellSize)

// Query returns entities in nearby cells
List<ICollidable> nearby = grid.Query(position, radius, CollisionLayer.Enemy);
```

**⚠️ Issue**: Query allocates a new `List<ICollidable>` every call. Should use pooled list or callback pattern to eliminate GC.

---

### 5. Weapon System (`Weapon.cs` + Subclasses)

Component-based hierarchy where weapons manage their own firing logic.

```
Weapon (abstract base)
    ├── ProjectileWeapon (auto-aim magic missiles)
    ├── OrbiterWeapon (spinning blades around player)
    ├── BoomerangWeapon (arc trajectory, returns)
    ├── RapidFireWeapon (high fire rate, no auto-aim)
    ├── FireRingWeapon (circular fire burst)
    ├── PoisonWeapon (AoE cloud)
    ├── LaserWeapon (continuous beam)
    └── LightningWeapon (chain lightning)
```

**Upgrade System:**
Each weapon tracks 6 upgrade types independently:
- `Damage` - +25% per level (max 10)
- `FireRate` - +20% per level (max 8)
- `ProjectileCount` - +1 per level (max 5)
- `Pierce` - +1 per level (max 5)
- `Range` - +30% per level (max 5)
- `ProjectileSize` - +25% per level (max 5)

**Collision Registration:**
Weapons implementing `IWeaponCollisionHandler` auto-register with `CollisionManager` and handle their own collision detection.

```csharp
public interface IWeaponCollisionHandler
{
    void CheckCollisions(SpatialHashGrid grid, EnemyPool enemyPool);
    bool IsActive { get; }
}
```

---

### 6. Event System (`GameEvents.cs`)

Static event bus for decoupled communication between systems.

```csharp
// Subscribe
GameEvents.OnEnemyKilled += HandleEnemyKilled;
GameEvents.OnPlayerLevelUp += ShowLevelUpUI;

// Trigger
GameEvents.TriggerEnemyKilled(enemy);
GameEvents.TriggerPlayerLevelUp(newLevel);

// Cleanup (on scene unload)
GameEvents.ClearAllEvents();
```

**Available Events:**
- Combat: `OnEnemyKilled`, `OnDamageDealt`, `OnPlayerDamaged`, `OnPlayerHealed`
- Progression: `OnXPGained`, `OnPlayerLevelUp`, `OnUpgradeSelected`
- Weapons: `OnWeaponAdded`, `OnWeaponUpgraded`, `OnWeaponFired`
- Game State: `OnGamePaused`, `OnGameOver`, `OnGameRestart`
- Pickups: `OnItemPickup`, `OnPowerupCollected`

---

## System Deep Dives

### Character System

**Current Implementation:**

```
CharacterData (ScriptableObject) ─── defines identity, stats, sprite, starting weapon
        │
        ▼
CharacterSelectionUI ─── UI for picking character
        │
        │ InitializeWithCharacter(data)
        ▼
Player (MonoBehaviour) ─── runtime state, implements ICollidable
        │
        ├── Health (component)
        ├── PlayerMovement (component)
        ├── WeaponInventory (component)
        ├── DirectionalSpriteController (component)
        └── AnimatedSpriteController (component)
```

**`CharacterData.cs` (ScriptableObject):**
```csharp
// Identity
string characterName = "Wizard";
string description = "Balanced character...";
string spriteType = "wizard";  // Used to load sprites

// Stats
float baseMaxHealth = 100f;
float moveSpeedModifier = 1f;
float damageModifier = 1f;
float attackSpeedModifier = 1f;
float startingCritChance = 0f;

// Starting loadout
string startingWeaponType = "ProjectileWeapon";
```

**`Player.cs` Responsibilities:**
1. Store runtime combat stats (damage multiplier, crit chance, etc.)
2. Manage XP and leveling
3. Track runtime statistics (enemies killed, damage dealt)
4. Implement `ICollidable` for collision detection
5. Apply `CharacterData` on initialization

---

### Enemy System

**Data-Driven Architecture:**

```
EnemyStats (ScriptableObject) ─── GoblinStats.asset, SkeletonStats.asset, etc.
        │
        ▼
EnemyPool ─── manages pooled Enemy instances
        │
        │ GetEnemy(stats)
        ▼
Enemy (MonoBehaviour) ─── runtime behavior, implements ICollidable
        │
        ├── Health (component)
        ├── AnimatedSpriteController (component)
        ├── Rigidbody2D (for physics)
        └── CircleCollider2D (for enemy-enemy collision)
```

**`EnemyStats.cs` Fields:**
```csharp
string enemyName = "Goblin";
float maxHealth = 20f;
float contactDamage = 10f;
float moveSpeed = 2f;
int xpDrop = 5;
Color color = Color.green;
float scale = 1f;
```

**Enemy Progression (time-gated unlocks):**
- 0-30s: Goblins only
- 30-60s: Goblins + Skeletons
- 60s+: Goblins + Skeletons + Dragons
- Ogre: Disabled (no sprites)

---

### Damage Calculation Pipeline

```
Projectile.Damage (base value)
        │
        ▼
DamageCalculator.CalculateDamage(context)
        │
        ├── Apply Player.DamageMultiplier
        ├── Roll for critical hit (Player.CritChance)
        │   └── If crit: multiply by Player.CritDamage
        ├── [Future: Elemental bonuses]
        └── [Future: Status effect modifiers]
        │
        ▼
Health.TakeDamage(finalDamage)
        │
        ├── Check i-frames
        ├── Subtract from currentHealth
        ├── Trigger OnHealthChanged
        ├── Check death → OnDeath event
        └── Show damage number
```

**Damage Types (future expansion ready):**
```csharp
enum DamageType { Physical, Fire, Poison, Ice, Lightning }
```

---

## Architectural Issues & Technical Debt

### 🔴 Critical Issue 1: Player/Hero Abstraction Mismatch

**Problem:**
The `Player` class conflates two distinct concerns:
1. **Character Identity** (which hero is selected, base stats, sprite type)
2. **Runtime State** (current health, XP, active weapons, position)

**Current Design:**
```csharp
// Player.cs contains BOTH identity AND runtime state
public class Player : MonoBehaviour, ICollidable
{
    // Identity (from CharacterData)
    private CharacterData characterData;  // ← Stored but barely used after init
    
    // Runtime state
    private float damageMultiplier = 1f;  // ← Duplicated from CharacterData
    private int currentXP = 0;
    private int currentLevel = 1;
    // ... 30+ fields mixing concerns
}
```

**Industry Standard Pattern:**

```csharp
// IDENTITY - What the hero IS (ScriptableObject, immutable during play)
[CreateAssetMenu]
public class HeroDefinition : ScriptableObject
{
    public string heroId;
    public string displayName;
    public Sprite portrait;
    public float baseHealth;
    public float baseMoveSpeed;
    public string startingWeaponId;
    public HeroAbility passiveAbility;
}

// RUNTIME STATE - What the hero has/does NOW (MonoBehaviour, mutable)
public class HeroController : MonoBehaviour
{
    [SerializeField] private HeroDefinition definition;  // Reference, not copied
    
    // Runtime modifiers (can change during play)
    private float healthModifier = 1f;
    private float damageModifier = 1f;
    
    // Computed properties
    public float MaxHealth => definition.baseHealth * healthModifier;
    public float MoveSpeed => definition.baseMoveSpeed * moveSpeedModifier;
}

// STAT CONTAINER - Holds all runtime stat bonuses
public class HeroStats
{
    private Dictionary<StatType, float> flatBonuses;
    private Dictionary<StatType, float> percentBonuses;
    
    public float GetFinalValue(StatType stat, float baseValue)
    {
        float flat = flatBonuses.GetValueOrDefault(stat, 0f);
        float percent = percentBonuses.GetValueOrDefault(stat, 0f);
        return (baseValue + flat) * (1f + percent);
    }
}
```

**Benefits of Separation:**
1. Easy to add new heroes without touching runtime code
2. Stats system becomes composable (items, buffs, debuffs all modify same stats)
3. Hero progression persists across sessions (unlock system)
4. Easier to balance (tweak ScriptableObject, no code changes)

---

### 🔴 Critical Issue 2: Weapon Collision Code Duplication

**Problem:**
Every weapon implementing `IWeaponCollisionHandler` duplicates the same collision detection logic:

```csharp
// ProjectileWeapon.cs
public void CheckCollisions(SpatialHashGrid grid, EnemyPool enemyPool)
{
    foreach (var projectile in activeProjectiles)
    {
        var nearby = grid.Query(projectile.Position, projectile.CollisionRadius, CollisionLayer.Enemy);
        foreach (var entity in nearby)
        {
            if (entity is Enemy enemy)
            {
                float distance = Vector3.Distance(projectile.Position, enemy.Position);
                if (distance < combinedRadius)
                {
                    // Apply damage, register hit, show damage number...
                    // THIS ENTIRE BLOCK IS COPY-PASTED IN EVERY WEAPON
                }
            }
        }
    }
}
```

**Industry Standard Pattern:**
Use **Strategy Pattern** or **Collision Resolution System**:

```csharp
// Single collision processor
public class ProjectileCollisionProcessor
{
    public void ProcessCollisions(List<BaseProjectile> projectiles, SpatialHashGrid grid)
    {
        foreach (var projectile in projectiles)
        {
            ProcessSingleProjectile(projectile, grid);
        }
    }
    
    private void ProcessSingleProjectile(BaseProjectile projectile, SpatialHashGrid grid)
    {
        var nearby = grid.Query(projectile.Position, projectile.CollisionRadius, CollisionLayer.Enemy);
        // Unified collision handling...
    }
}

// Weapons just register projectiles, don't handle collisions
public class ProjectileWeapon : Weapon
{
    protected override void Fire()
    {
        var projectile = projectilePool.GetProjectile();
        projectile.SetStats(currentDamage, currentPierce, damageType, currentProjectileSize);
        projectile.Activate(playerPos, direction);
        // Collision handling is automatic via pool registration
    }
}
```

---

### 🟡 Issue 3: UI Built Entirely in Code

**Problem:**
`CharacterSelectionUI.cs`, `LevelUpUI.cs`, and other UI classes create all UI elements procedurally with `new GameObject()` and `AddComponent<>()`. This is:
- Hard to maintain and modify
- No visual editor preview
- Difficult for non-programmers to tweak
- Prone to layout bugs

**Example from `CharacterSelectionUI.cs`:**
```csharp
private void CreateTitle()
{
    GameObject titleObj = new GameObject("Title");
    titleObj.transform.SetParent(selectionPanel.transform, false);
    Text titleText = titleObj.AddComponent<Text>();
    titleText.text = "CHOOSE YOUR HERO";
    titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    titleText.fontSize = 72;
    // 20+ more lines of manual layout...
}
```

**Industry Standard:**
Use **UI Prefabs** with **Canvas components** designed in Unity Editor:
```csharp
[SerializeField] private GameObject characterCardPrefab;
[SerializeField] private Transform characterGrid;

private void PopulateCharacterGrid()
{
    foreach (var character in availableCharacters)
    {
        var card = Instantiate(characterCardPrefab, characterGrid);
        card.GetComponent<CharacterCard>().Setup(character);
    }
}
```

---

### 🟡 Issue 4: Spawner Logic in Pool Classes

**Problem:**
`EnemyPool.GetRandomEnemyType(float gameTime)` contains game design logic (enemy unlock progression) that belongs in `EnemySpawner`:

```csharp
// EnemyPool.cs - This shouldn't know about game time or progression
public EnemyStats GetRandomEnemyType(float gameTime)
{
    if (enemyName == "Skeleton" && gameTime < 30f) continue;  // Game logic!
    if (enemyName == "Dragon" && gameTime < 60f) continue;    // Game logic!
}
```

**Better Separation:**
```csharp
// EnemySpawner.cs - Owns progression logic
private EnemyStats GetEnemyForCurrentPhase()
{
    var available = allEnemyTypes.Where(e => IsUnlocked(e, gameTime)).ToList();
    return available[Random.Range(0, available.Count)];
}

// EnemyPool.cs - Just manages pooling
public Enemy GetEnemy() => GetItem();  // No stats, no time, just pooling
```

---

### 🟡 Issue 5: Query Allocates New Lists Every Frame

**Problem:**
`SpatialHashGrid.Query()` creates a new `List<ICollidable>` every call:

```csharp
public List<ICollidable> Query(Vector3 position, float radius, CollisionLayer mask)
{
    var results = new List<ICollidable>(32);  // Allocation every frame!
    // ...
    return results;
}
```

With 200 projectiles querying per frame, this is 200 list allocations.

**Better Pattern:**
```csharp
// Callback-based (zero allocation)
public void Query(Vector3 position, float radius, CollisionLayer mask, Action<ICollidable> callback)
{
    // ... for each match ...
    callback(entity);  // No list needed
}

// Or pooled list
private static List<ICollidable> sharedQueryResults = new List<ICollidable>(256);
public List<ICollidable> Query(...)  // Same signature
{
    sharedQueryResults.Clear();
    // ... populate sharedQueryResults ...
    return sharedQueryResults;  // Caller must use immediately, not store
}
```

---

### 🟡 Issue 6: Magic Strings for Weapon Types

**Problem:**
Weapons are created via string matching:

```csharp
// WeaponInventory.cs
Weapon weapon = weaponType switch
{
    "ProjectileWeapon" => weaponObj.AddComponent<ProjectileWeapon>(),
    "MagicMissile" => weaponObj.AddComponent<ProjectileWeapon>(),  // Duplicate!
    "OrbiterWeapon" => weaponObj.AddComponent<OrbiterWeapon>(),
    // ...
};
```

**Better Pattern:**
Use **Weapon Registry** or **ScriptableObject**:

```csharp
[CreateAssetMenu]
public class WeaponDefinition : ScriptableObject
{
    public string weaponId;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;  // OR:
    public System.Type weaponType;  // For AddComponent<>
}

// WeaponInventory.cs
[SerializeField] private WeaponDefinition[] weaponRegistry;

public bool AddWeapon(string weaponId)
{
    var def = weaponRegistry.FirstOrDefault(w => w.weaponId == weaponId);
    if (def == null) return false;
    
    var weapon = gameObject.AddComponent(def.weaponType) as Weapon;
    weapon.Initialize(def);
    return true;
}
```

---

## Recommended Improvements

### Priority 1: Hero/Player Refactor

**Goal:** Separate identity from runtime state for cleaner architecture and future meta-progression.

**Implementation Steps:**
1. Rename `CharacterData` → `HeroDefinition`
2. Create `HeroStats` class for runtime stat modifiers
3. Refactor `Player` to only hold runtime state
4. `HeroDefinition` becomes the source of truth for base values
5. `HeroStats` accumulates bonuses from upgrades, items, buffs

**File Changes:**
- `CharacterData.cs` → `HeroDefinition.cs`
- New: `HeroStats.cs`
- Refactor: `Player.cs` → `HeroController.cs`

---

### Priority 2: Unified Collision System

**Goal:** Eliminate duplicated collision code across weapons.

**Implementation Steps:**
1. Create `ProjectileCollisionProcessor` in `CollisionManager`
2. Each pool registers its active items for collision checking
3. Damage application becomes data-driven (projectile carries all needed info)
4. Remove `IWeaponCollisionHandler` interface entirely

---

### Priority 3: UI Prefab Migration

**Goal:** Move procedural UI to prefab-based for maintainability.

**Implementation Steps:**
1. Design `CharacterSelectionCanvas` prefab in editor
2. Create `CharacterCardPrefab` with `CharacterCard` component
3. Populate at runtime via Instantiate pattern
4. Same for `LevelUpUI`, `GameOverUI`, `HUDCanvas`

---

### Priority 4: Query Optimization

**Goal:** Eliminate per-frame allocations in spatial grid.

**Implementation Steps:**
1. Use pooled/shared result list in `SpatialHashGrid.Query()`
2. Or switch to callback-based pattern
3. Profile to confirm GC reduction

---

## Performance Considerations

### Current Performance Profile
| Metric | Target | Current |
|--------|--------|---------|
| Frame Rate | 60 FPS | ✓ Achieved |
| Max Enemies | 500 | ✓ Achieved |
| Max Projectiles | 200 | ✓ Achieved |
| Collision Time | <2ms | ✓ ~1-2ms |
| GC Allocations | <10KB/frame | ⚠️ Needs profiling |

### Optimization Techniques in Use
1. **Spatial hash grid** - 90% reduction in collision checks
2. **Active list caching** - Eliminates `GetComponentsInChildren()` overhead
3. **Object pooling** - Zero instantiation during gameplay
4. **Sprite caching** - Prevents regeneration each frame
5. **Conditional logging** - `DebugLog.Verbose` compiles out in Release

### Known Performance Concerns
1. `SpatialHashGrid.Query()` allocates list per call
2. `List<Projectile>` copy in `ProjectileWeapon.CheckCollisions()`
3. LINQ usage in `EnemyPool.GetRandomEnemyType()` (allocates enumerator)
4. `FindAnyObjectByType<>()` calls during initialization (slow)

### Profiling Checkpoints
```csharp
// Key methods to profile
CollisionManager.Update()  // Should be <2ms
SpatialHashGrid.PopulateGrid()  // Should be <0.5ms
SpatialHashGrid.Query()  // Should be <0.01ms per call
Weapon.Fire()  // Should be <0.1ms per weapon
```

---

## Code Patterns & Conventions

### Naming Conventions
| Element | Convention | Example |
|---------|-----------|---------|
| Private fields | camelCase | `private float currentDamage;` |
| Public properties | PascalCase | `public float Damage => currentDamage;` |
| Methods | PascalCase | `public void ApplyUpgrade()` |
| Constants | UPPER_SNAKE | `private const int MAX_ENEMIES = 500;` |
| SerializeFields | camelCase | `[SerializeField] private float baseDamage;` |

### Logging Pattern
Use `DebugLog` instead of `Debug.Log()`:
```csharp
DebugLog.Info("Important state change");      // Always logged
DebugLog.Verbose("Per-frame data");           // Only in Development
DebugLog.Warning("Recoverable issue");        // Always logged
DebugLog.Error("Critical failure");           // Always logged
```

### Pool Access Pattern
```csharp
// ✓ Correct: Use GameServices
var enemy = GameServices.EnemyPool.GetEnemy(stats);

// ✗ Avoid: Direct FindObjectOfType
var pool = FindObjectOfType<EnemyPool>();  // Slow!
```

### Phase Check Pattern
```csharp
// ✓ Correct: Check phase before gameplay logic
protected virtual void Update()
{
    if (GamePhaseManager.CurrentPhase != GamePhase.Gameplay) return;
    if (GameState.IsPaused) return;
    
    // Actual logic here
}
```

---

## Quick Reference for AI Agents

### Before Making Changes

1. **Read relevant existing code** - Don't assume patterns, verify them
2. **Check `GamePhaseManager.CurrentPhase`** - Logic must respect phases
3. **Use existing pools** - Never `Instantiate()` during gameplay
4. **Follow active list caching** - Pools maintain their own active lists
5. **Use `DebugLog`** - Not `Debug.Log()` for conditional compilation

### Common Tasks

**Adding a New Weapon:**
```csharp
// 1. Create MyWeapon.cs inheriting from Weapon
public class MyWeapon : Weapon, IWeaponCollisionHandler
{
    protected override void Awake()
    {
        weaponName = "My Weapon";
        baseDamage = 15f;
        base.Awake();
    }
    
    protected override void Fire() { /* firing logic */ }
    
    public void CheckCollisions(...) { /* collision logic */ }
    public bool IsActive => gameObject.activeInHierarchy;
}

// 2. Add to WeaponInventory.CreateWeapon() switch statement
"MyWeapon" => weaponObj.AddComponent<MyWeapon>(),

// 3. Register with CollisionManager if needed
```

**Adding a New Enemy:**
```csharp
// 1. Create ScriptableObject asset: Assets/EnemyStats/MyEnemyStats.asset
// 2. Configure stats in Inspector
// 3. Add to EnemyPool.enemyTypes array in scene
// 4. Add unlock logic to EnemyPool.GetRandomEnemyType(float gameTime)
```

**Adding a New Hero/Character:**
```csharp
// 1. Create ScriptableObject: Assets/Resources/Characters/MyHeroData.asset
// 2. Configure stats, starting weapon, sprite type
// 3. Add sprites to Resources/Sprites/Heroes/myhero/
// 4. Character will auto-load if in Resources/Characters/
```

### File Locations Reference
| System | Location |
|--------|----------|
| Core Scripts | `Assets/Scripts/` |
| Enemy System | `Assets/Scripts/Enemies/Core/`, `Assets/Scripts/Enemies/Systems/` |
| Enemy Stats | `Assets/EnemyStats/*.asset` |
| Character Data | `Assets/Resources/Characters/*.asset` |
| Sprites | `Assets/Resources/Sprites/` |
| Scene | `Assets/Scenes/SampleScene.unity` |

### Debug Shortcuts
| Key | Action |
|-----|--------|
| F3 | Toggle hitbox visualization |
| ESC | Pause/Resume |
| R | Restart (on Game Over) |

---

## Related Documentation

- `CLAUDE.md` - Quick reference for Claude Code sessions
- `GAME_DESIGN.md` - Game design document with formulas
- `WEAPON_SYSTEM_ARCHITECTURE.md` - Deep dive into weapon system
- `SPRITE_ERROR_DIAGNOSIS_GUIDE.md` - Troubleshooting sprite loading
- `LOG_DEBUGGING_WORKFLOW.md` - Using persistent logging

---

*This document should be updated when significant architectural changes are made.*

