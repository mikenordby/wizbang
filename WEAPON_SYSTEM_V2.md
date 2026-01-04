# Weapon System V2 - Complete Design Document

**Last Updated**: 2025-12-30
**Status**: Phase 1 & 2 Implemented
**Author**: Claude (AI Assistant)

## Implementation Status

| Phase | Status | Notes |
|-------|--------|-------|
| 1. Weapon Leveling | ✅ Complete | Levels 1-3, +20% damage/+10% fire rate per level |
| 2. New Tier 1 Weapons | ✅ Complete | Flame Bolt, Heat Aura, Light Beam added |
| 3. Combination Requirements | ✅ Complete | Level 2+ required for combinations |
| 4. Tier 2 Definitions | 🔄 Partial | Circle of Fire, Laser Beam, Poisoned Flames defined |
| 5. UI Updates | ✅ Complete | Weapon upgrades show in level-up choices |

### Remaining Work
- Create weapon definitions for remaining Tier 2 combinations (Storm Shield, Arcane Gatling, etc.)
- Test full combination flow in-game
- Balance tuning for new weapons

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Weapon Leveling System](#weapon-leveling-system)
3. [Weapon Tiers](#weapon-tiers)
4. [Tier 1 Weapons (Basic)](#tier-1-weapons-basic)
5. [Tier 2 Weapons (Combined)](#tier-2-weapons-combined)
6. [Combination Recipes](#combination-recipes)
7. [Implementation Plan](#implementation-plan)
8. [Migration Notes](#migration-notes)

---

## Executive Summary

### Key Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Weapon levels | 1-5 | Simple progression, level 2 unlocks combinations |
| Combination requirement | Level 2+ | Prevents instant combining, adds progression |
| Circle of Fire | Tier 2 (combined) | Too powerful for starting weapon |
| Laser Beam | Tier 2 (combined) | Too powerful for starting weapon |
| Tier 2 stat multiplier | 1.5x | Balanced reward for combining |

### Weapon Count

| Tier | Count | Notes |
|------|-------|-------|
| Tier 1 (Basic) | 8 | Starting pool, simple behaviors |
| Tier 2 (Combined) | 10+ | Tier 1 + Tier 1, requires level 2 |
| Tier 3 (Super) | 2 | Tier 2 + Tier 2, endgame (future) |

---

## Weapon Leveling System

### Level Progression

| Level | Damage | Fire Rate | Milestone Bonus |
|-------|--------|-----------|-----------------|
| 1 | 100% | 100% | Base stats |
| 2 | +20% | +10% | **Unlocks combination eligibility** |
| 3 | +40% | +20% | +1 projectile (projectile weapons) |
| 4 | +60% | +30% | +1 pierce (projectile weapons) |
| 5 | +80% | +40% | Weapon-specific bonus |

### Level 5 Special Bonuses (Per Weapon)

| Weapon | Level 5 Bonus |
|--------|---------------|
| Magic Missile | +1 projectile, homing enabled |
| Rapid Fire | +50% fire rate |
| Orbiting Blades | +1 blade, +25% orbit radius |
| Chain Lightning | +2 max chains |
| Poison Cloud | +50% radius, +2s duration |
| Flame Bolt | Projectiles explode on hit (small AoE) |
| Heat Aura | +50% radius |
| Light Beam | +50% beam length |

### Implementation

```csharp
// In Weapon.cs
public int Level { get; private set; } = 1;
public const int MaxLevel = 5;
public bool CanCombine => Level >= 2;

public void LevelUp()
{
    if (Level >= MaxLevel) return;
    Level++;
    RecalculateStats();
    OnLevelUp?.Invoke(this, Level);
}

protected virtual void RecalculateStats()
{
    float levelMultiplier = 1f + (Level - 1) * 0.2f; // 20% per level
    float fireRateMultiplier = 1f + (Level - 1) * 0.1f; // 10% per level

    currentDamage = baseDamage * levelMultiplier * playerMultipliers;
    currentFireRate = baseFireRate * fireRateMultiplier * playerMultipliers;

    // Milestone bonuses
    if (Level >= 3) currentProjectileCount += 1;
    if (Level >= 4) currentPierce += 1;
}
```

---

## Weapon Tiers

### Tier 1 (Basic)
- Available from start of game
- Acquired through level-up choices
- Must reach level 2 to be eligible for combination
- Simple, focused behaviors

### Tier 2 (Combined)
- Created by combining two Tier 1 weapons (both level 2+)
- Inherits ALL tags from both parents (union)
- Stats = better of both parents × 1.5
- Starts at level 1 (can level to 5)
- More complex behaviors

### Tier 3 (Super Combined) - Future
- Created by combining two Tier 2 weapons
- Stats = better of both × 2.25 (1.5 × 1.5)
- Ultimate endgame weapons

---

## Tier 1 Weapons (Basic)

### Current Weapons (Keep)

| Weapon | Tags | Behavior | Base Damage | Fire Rate |
|--------|------|----------|-------------|-----------|
| Magic Missile | [Arcane] | Projectile | 10 | 1.0 |
| Rapid Fire | [Gun] | Projectile | 5 | 4.0 |
| Orbiting Blades | [Melee] | Orbiter | 15 | - |
| Chain Lightning | [Lightning] | Chain | 15 | 0.8 |
| Poison Cloud | [Poison, Area] | AoE | 3/tick | 0.3 |

### New Weapons (Add)

| Weapon | Tags | Behavior | Base Damage | Fire Rate | Description |
|--------|------|----------|-------------|-----------|-------------|
| **Flame Bolt** | [Fire] | Projectile | 8 | 1.2 | Fire projectile, auto-aim, small burn effect |
| **Heat Aura** | [Area] | AoE | 2/tick | - | Small constant damage aura, 1.5 radius |
| **Light Beam** | [Gun] | Beam | 3/tick | - | Short beam (8 units), physical damage |

### Removed from Tier 1 (Moved to Tier 2)

| Weapon | Reason | Now Obtained Via |
|--------|--------|------------------|
| Circle of Fire | Too powerful | Flame Bolt + Heat Aura |
| Laser Beam | Too powerful | Light Beam + Flame Bolt |

---

## Tier 2 Weapons (Combined)

### Fire Combinations

| Result | Parent 1 | Parent 2 | Tags | Behavior |
|--------|----------|----------|------|----------|
| **Circle of Fire** | Flame Bolt [Fire] | Heat Aura [Area] | [Fire, Area] | Permanent fire ring around player |
| **Laser Beam** | Light Beam [Gun] | Flame Bolt [Fire] | [Gun, Fire] | Long fire beam, infinite pierce |
| **Poisoned Flames** | Poison Cloud [Poison, Area] | Flame Bolt [Fire] | [Poison, Area, Fire] | Fire ring + poison DoT |

### Lightning Combinations

| Result | Parent 1 | Parent 2 | Tags | Behavior |
|--------|----------|----------|------|----------|
| **Storm Shield** | Chain Lightning [Lightning] | Orbiting Blades [Melee] | [Lightning, Melee] | Orbiting electric blades that chain |

### Arcane Combinations

| Result | Parent 1 | Parent 2 | Tags | Behavior |
|--------|----------|----------|------|----------|
| **Arcane Gatling** | Magic Missile [Arcane] | Rapid Fire [Gun] | [Arcane, Gun] | Rapid-fire homing projectiles |

### Poison Combinations

| Result | Parent 1 | Parent 2 | Tags | Behavior |
|--------|----------|----------|------|----------|
| **Toxic Barrage** | Poison Cloud [Poison, Area] | Rapid Fire [Gun] | [Poison, Area, Gun] | Rapid poison projectiles with trails |

### Multi-Element Combinations

| Result | Parent 1 | Parent 2 | Tags | Behavior |
|--------|----------|----------|------|----------|
| **Flame Chain** | Chain Lightning [Lightning] | Flame Bolt [Fire] | [Lightning, Fire] | Lightning that ignites enemies |
| **Frozen Storm** | Chain Lightning [Lightning] | Poison Cloud [Poison, Area] | [Lightning, Poison, Area] | Lightning strikes leave poison pools |

---

## Combination Recipes

### Full Recipe List

```
TIER 2 COMBINATIONS (Tier 1 + Tier 1)
=====================================

1. Circle of Fire
   Recipe: Flame Bolt [Fire] + Heat Aura [Area]
   Result Tags: [Fire, Area]
   Tier Multiplier: 1.5x

2. Laser Beam
   Recipe: Light Beam [Gun] + Flame Bolt [Fire]
   Result Tags: [Gun, Fire]
   Tier Multiplier: 1.5x

3. Poisoned Flames
   Recipe: Poison Cloud [Poison, Area] + Flame Bolt [Fire]
   Result Tags: [Poison, Area, Fire]
   Tier Multiplier: 1.5x

4. Storm Shield
   Recipe: Chain Lightning [Lightning] + Orbiting Blades [Melee]
   Result Tags: [Lightning, Melee]
   Tier Multiplier: 1.5x

5. Arcane Gatling
   Recipe: Magic Missile [Arcane] + Rapid Fire [Gun]
   Result Tags: [Arcane, Gun]
   Tier Multiplier: 1.5x

6. Toxic Barrage
   Recipe: Poison Cloud [Poison, Area] + Rapid Fire [Gun]
   Result Tags: [Poison, Area, Gun]
   Tier Multiplier: 1.5x

7. Flame Chain
   Recipe: Chain Lightning [Lightning] + Flame Bolt [Fire]
   Result Tags: [Lightning, Fire]
   Tier Multiplier: 1.5x

8. Frozen Storm (renamed from Poison Storm for clarity)
   Recipe: Chain Lightning [Lightning] + Poison Cloud [Poison, Area]
   Result Tags: [Lightning, Poison, Area]
   Tier Multiplier: 1.5x

9. Blade Blaze
   Recipe: Orbiting Blades [Melee] + Flame Bolt [Fire]
   Result Tags: [Melee, Fire]
   Tier Multiplier: 1.5x

10. Arcane Aura
    Recipe: Magic Missile [Arcane] + Heat Aura [Area]
    Result Tags: [Arcane, Area]
    Tier Multiplier: 1.5x
```

### Combination Matrix

Quick reference for what combines with what:

| | Magic Missile | Rapid Fire | Orbiters | Chain Light | Poison | Flame Bolt | Heat Aura | Light Beam |
|-|---------------|------------|----------|-------------|--------|------------|-----------|------------|
| **Magic Missile** | - | Arcane Gatling | - | - | - | - | Arcane Aura | - |
| **Rapid Fire** | Arcane Gatling | - | - | - | Toxic Barrage | - | - | - |
| **Orbiters** | - | - | - | Storm Shield | - | Blade Blaze | - | - |
| **Chain Lightning** | - | - | Storm Shield | - | Frozen Storm | Flame Chain | - | - |
| **Poison Cloud** | - | Toxic Barrage | - | Frozen Storm | - | Poisoned Flames | - | - |
| **Flame Bolt** | - | - | Blade Blaze | Flame Chain | Poisoned Flames | - | Circle of Fire | Laser Beam |
| **Heat Aura** | Arcane Aura | - | - | - | - | Circle of Fire | - | - |
| **Light Beam** | - | - | - | - | - | Laser Beam | - | - |

---

## Implementation Plan

### Phase 1: Weapon Leveling (Priority: HIGH)

**Files to modify:**
- `Weapon.cs` - Add Level property and leveling logic
- `UpgradeChoiceGenerator.cs` - Re-enable weapon upgrade choices
- `UpgradeChoice.cs` - Update for weapon upgrades
- `LevelUpUI.cs` - Show weapon level in upgrade choices

**New code needed:**
```csharp
// Weapon.cs additions
public int Level { get; protected set; } = 1;
public const int MaxLevel = 5;
public bool CanCombine => Level >= 2;
public bool CanLevelUp => Level < MaxLevel;

public virtual void LevelUp() { ... }
protected virtual float GetLevelDamageMultiplier() => 1f + (Level - 1) * 0.2f;
protected virtual float GetLevelFireRateMultiplier() => 1f + (Level - 1) * 0.1f;
```

### Phase 2: New Tier 1 Weapons (Priority: HIGH)

**Files to create/modify:**
- `DefaultWeaponDefinitions.cs` - Add Flame Bolt, Heat Aura, Light Beam
- Remove Circle of Fire and Laser Beam from Tier 1 (move to Tier 2)

**New weapon definitions:**
1. `CreateFlameBolt()` - [Fire] Projectile
2. `CreateHeatAura()` - [Area] AoE (small radius, low damage)
3. `CreateLightBeam()` - [Gun] Beam (short, physical)

### Phase 3: Combination Requirements (Priority: HIGH)

**Files to modify:**
- `CombinationManager.cs` - Add level check
- `UpgradeChoiceGenerator.cs` - Only show combos for level 2+ weapons

**Key change:**
```csharp
// In CombinationManager.GetAvailableCombinations()
if (weaponA.Level < 2 || weaponB.Level < 2)
    continue; // Skip - weapons not high enough level
```

### Phase 4: Tier 2 Weapon Definitions (Priority: MEDIUM)

**Files to create/modify:**
- `DefaultWeaponDefinitions.cs` - Add all Tier 2 weapon definitions
- Create `WeaponCombination` ScriptableObjects in `Resources/Combinations/`

### Phase 5: UI Updates (Priority: MEDIUM)

**Files to modify:**
- `LevelUpUI.cs` - Show weapon levels, show "Lv.2 required" for combos
- `CharacterCard.cs` - Display weapon level if applicable

---

## Migration Notes

### Breaking Changes

1. **Circle of Fire removed from starting pool** - Players who expected it must now combine
2. **Laser Beam removed from starting pool** - Same as above
3. **Combination requires level 2** - Can't instant-combine anymore

### Backward Compatibility

- Existing `WeaponDefinition` system remains unchanged
- `GenericWeapon` and behavior classes unchanged
- Only adding new properties, not removing existing ones

### Character Starting Weapons

Need to update character definitions:

| Character | Old Starting Weapon | New Starting Weapon |
|-----------|--------------------|--------------------|
| Knight | Orbiting Blades | Orbiting Blades (keep) |
| Wizard | Magic Missile | Magic Missile (keep) |
| (Others) | Circle of Fire? | Flame Bolt |

---

## Balance Considerations

### Power Curve by Game Time

| Time | Expected Weapons | Power Level |
|------|-----------------|-------------|
| 0-5 min | 2-3 Tier 1 (level 1-2) | Baseline |
| 5-10 min | 4-5 Tier 1 (level 2-3) | 1.4x baseline |
| 10-15 min | 3 Tier 1 + 1 Tier 2 | 2x baseline |
| 15-20 min | 2 Tier 1 + 2 Tier 2 | 3x baseline |
| 20-30 min | 1 Tier 1 + 3 Tier 2 | 4x baseline |

### Stat Inheritance Math

```
Combined Weapon Stats:
- Damage = max(parent1.damage, parent2.damage) × 1.5 × combinedWeaponLevel
- FireRate = max(parent1.fireRate, parent2.fireRate) × 1.5
- Tags = union(parent1.tags, parent2.tags)

Example:
Flame Bolt Lv.3 (damage: 8 × 1.4 = 11.2) + Heat Aura Lv.2 (damage: 2 × 1.2 = 2.4)
= Circle of Fire (damage: max(11.2, 2.4) × 1.5 = 16.8 at level 1)
```

---

## Summary

This design:
- ✅ Re-adds weapon leveling (1-5) with meaningful progression
- ✅ Gates combinations behind level 2 requirement
- ✅ Moves Circle of Fire and Laser Beam to Tier 2 (combined)
- ✅ Adds 3 new Tier 1 weapons (Flame Bolt, Heat Aura, Light Beam)
- ✅ Defines 10 Tier 2 combinations
- ✅ Maintains existing data-driven architecture

**Ready for implementation approval.**
